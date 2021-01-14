using JXml.Serializers;
using JXml.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace JXml
{
    public class XmlController : IDisposable
    {
        public static event Action<string> OnLog;
        internal static void Log(string msg)
        {
            OnLog?.Invoke(msg);
        }

        private Dictionary<Type, IRootTypeSerializer> rootSerializers = new Dictionary<Type, IRootTypeSerializer>();
        private Dictionary<string, FieldWrapper> cachedFields = new Dictionary<string, FieldWrapper>();
        private XmlDocument doc = new XmlDocument();
        private Dictionary<Type, Func<CustomResolverArgs, object>> customResolvers = new Dictionary<Type, Func<CustomResolverArgs, object>>();
        private Dictionary<Type, Type> customResolverTypeMap = new Dictionary<Type, Type>();

        public XmlController()
        {
            // Add default serializers.
            AddRootTypeSerializer(new ByteParser());
            AddRootTypeSerializer(new SByteParser());
            AddRootTypeSerializer(new ShortParser());
            AddRootTypeSerializer(new UShortParser());
            AddRootTypeSerializer(new IntParser());
            AddRootTypeSerializer(new UIntParser());
            AddRootTypeSerializer(new LongParser());
            AddRootTypeSerializer(new ULongParser());
            AddRootTypeSerializer(new FloatParser());
            AddRootTypeSerializer(new DoubleParser());
            AddRootTypeSerializer(new DecimalParser());
            AddRootTypeSerializer(new StringParser());
            AddRootTypeSerializer(new BoolParser());
            AddRootTypeSerializer(new CharParser());
            AddRootTypeSerializer(new Vector2Parser());
            AddRootTypeSerializer(new ColorParser());
            AddRootTypeSerializer(new TypeParser());
        }

        public void AddRootTypeSerializer(IRootTypeSerializer serializer)
        {
            if (serializer == null)
                throw new ArgumentNullException(nameof(serializer));

            var targetType = serializer.TargetType;

            if (rootSerializers.ContainsKey(targetType))
                throw new ArgumentException($"There is already a root serializer for the type {targetType.FullName}", nameof(serializer));

            rootSerializers.Add(targetType, serializer);
        }

        public IRootTypeSerializer GetRootTypeSerializer<T>()
        {
            return GetRootTypeSerializer(typeof(T));
        }

        public IRootTypeSerializer GetRootTypeSerializer(Type type)
        {
            if (type == null)
                return null;

            return rootSerializers.TryGetValue(type, out var value) ? value : null;
        }

        public bool HasRootTypeSerializer<T>()
        {
            return HasRootTypeSerializer(typeof(T));
        }

        public bool HasRootTypeSerializer(Type type)
        {
            return GetRootTypeSerializer(type) != null;
        }

        public void RemoveRootTypeSerializer<T>()
        {
            RemoveRootTypeSerializer(typeof(T));
        }

        public void RemoveRootTypeSerializer(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!rootSerializers.ContainsKey(type))
                throw new Exception($"There is no registered root serializer for type {type.FullName}, cannot remove.");

            rootSerializers.Remove(type);
        }

        public void AddCustomResolver(Type type, Func<CustomResolverArgs, object> func)
        {
            if (customResolvers.ContainsKey(type))
            {
                OnLog?.Invoke($"[ERROR] There is already a custom resolver for type '{type.FullName}'");
                return;
            }
            if(func == null)
            {
                OnLog?.Invoke($"[ERROR] Null arg {nameof(func)}.");
                return;
            }

            customResolvers.Add(type, func);
        }

        public void RemoveCustomResolver(Type type)
        {
            if (!customResolvers.ContainsKey(type))
            {
                OnLog?.Invoke($"[ERROR] There is no registered custom resolver for type '{type.FullName}'");
                return;
            }

            customResolvers.Remove(type);
        }

        public T Deserialize<T>(string xml, T toFill) where T : class
        {
            doc.LoadXml(xml);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            FieldWrapper currentField = default;
            object rootObject = null;
            object parentObject = null;

            Type rootType = toFill == null ? typeof(T) : toFill.GetType();
            T returnValue = (T)CreateAndPopulate(toFill, doc.FirstContentNode(), rootType);

            watch.Stop();
            //OnLog?.Invoke($"Took {watch.Elapsed.TotalMilliseconds:F2} ms");

            return returnValue;
            object CreateAndPopulate(object existing, XmlNode rootNode, Type type)
            {
                bool isRootAndFilling = rootObject == null && toFill != null;
                string customClassName = rootNode.TryGetAttribute("class");
                if(customClassName != null)
                {
                    customClassName = customClassName.Trim();
                    var newType = TypeResolver.Resolve(customClassName);
                    if(newType == null)
                    {
                        OnLog?.Invoke($"[ERROR] Node {rootNode.GetXPath()}: Could not find custom class '{customClassName}' for node {rootNode.Name}. Node will be ignored.");
                        return null;
                    }

                    bool currentTypeCanGoIntoRoot = isRootAndFilling && newType.IsAssignableFrom(type);

                    if (!type.IsAssignableFrom(newType) && !currentTypeCanGoIntoRoot)
                    {
                        string problem = type.IsInterface ? "does not implement interface" : "is not a subclass of";
                        OnLog?.Invoke($"[ERROR] Node {rootNode.GetXPath()}: {newType.FullName} {problem} {type.FullName}. Node will be ignored.");
                        return null;
                    }

                    type = newType;
                }

                if (type.IsRealNullable())
                {
                    Type replacement = Nullable.GetUnderlyingType(type);
                    //OnLog?.Invoke($"{type.Name} is a nullable type, using {replacement.Name} instead.");
                    type = replacement;
                }

                var loader = GetRootTypeSerializer(type);
                bool isBasic = loader != null;
                bool isArrayType = IsArrayType(type);
                bool isListType = IsListType(type);
                bool isDictType = IsDictionaryType(type);
                bool isEnumType = IsEnumType(type);
                bool tryUseCustom = !isRootAndFilling && !isBasic && !isArrayType && !isListType && !isDictType && !isEnumType && !type.IsPrimitive;

                object DoCustom(Type baseType)
                {
                    var custom = customResolvers[baseType];
                    return custom.Invoke(new CustomResolverArgs()
                    {
                        XmlNode = rootNode,
                        ExistingObject = existing,
                        Field = currentField,
                        RootObject = rootObject,
                        ParentObject = parentObject
                    });
                }

                if (tryUseCustom)
                {
                    if (customResolverTypeMap.TryGetValue(type, out var foundBaseType))
                    {
                        if(foundBaseType != null)
                            return DoCustom(foundBaseType);
                    }
                    else
                    {
                        foreach (var ct in customResolvers)
                        {
                            var crType = ct.Key;
                            if (crType.IsAssignableFrom(type))
                            {
                                // Found it!
                                customResolverTypeMap.Add(type, crType);
                                return DoCustom(crType);
                            }
                        }
                    }
                }

                if ((type.IsAbstract || type.IsInterface) && !isRootAndFilling && !isBasic)
                {
                    string problem = type.IsInterface ? "an interface" : "an abstract class";
                    OnLog?.Invoke($"[ERROR] Node {rootNode.GetXPath()}: {type.FullName} is {problem} and so cannot be instantiated. Please use the class='typeName' attribute to specify a concrete class. Node will be ignored.");
                    return null;
                }

                if (isArrayType)
                {
                    // Create the array.
                    int arrayLength = 0;
                    for(int i = 0; i < rootNode.ChildNodes.Count; i++)
                    {
                        var node = rootNode.ChildNodes.Item(i);
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            arrayLength++; 
                        }
                    }

                    Array created = CreateInstance(type, arrayLength) as Array;
                    Type arrayType = type.GetElementType();
                    for (int i = 0; i < created.Length; i++)
                    {
                        var node = rootNode.ChildNodes.Item(i);
                        object atPosition = CreateAndPopulate(null, node, arrayType);

                        created.SetValue(atPosition, i);
                    }

                    string attr = rootNode.TryGetAttribute("mode");
                    if(attr != null)
                    {
                        OnLog?.Invoke($"[ERROR] Node {rootNode.GetXPath()}: Arrays cannot use merge modes. To use merge modes, change it to a List<{arrayType.Name}>.");
                    }

                    return created;
                }

                if (isListType)
                {
                    if (!type.IsGenericType)
                    {
                        OnLog?.Invoke($"[ERROR] Node {rootNode.GetXPath()}: Non-generic list type {type.Name} is not supported.");
                        return null;
                    }

                    IList old = existing as IList;

                    Type listType = type.GetGenericArguments()[0];
                    IList created = (IList)CreateInstance(type);
                    bool allowNullValues = listType.IsNullable();
                    for (int i = 0; i < rootNode.ChildNodes.Count; i++)
                    {
                        var node = rootNode.ChildNodes.Item(i);
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            object atPosition = CreateAndPopulate(null, node, listType);

                            if (atPosition == null && !allowNullValues)
                            {
                                OnLog?.Invoke($"[ERROR] List {rootNode.GetXPath()} has a null value. This is not valid because the list type is {listType.Name}.");
                                continue;
                            }
                            created.Add(atPosition);
                        }
                    }

                    if (old == null)
                        return created;

                    ListMergeMode mode = rootNode.TryParseAttributeEnum<ListMergeMode>("mode") ?? ListMergeMode.MergeReplace;
                    if((old.IsFixedSize || old.IsReadOnly) && mode != ListMergeMode.Replace)
                    {
                        OnLog?.Invoke($"[ERROR] Node {rootNode.GetXPath()}: This List<{listType.Name}> uses merge mode {mode}, but the list instance is read-only. New list will replace old values.");
                        mode = ListMergeMode.Replace;
                    }

                    ListMergeUtils.Combine(old, created, mode);

                    return old;
                }

                if (isDictType)
                {
                    if (!type.IsGenericType)
                    {
                        OnLog?.Invoke($"[ERROR] Node {rootNode.GetXPath()}: Non-generic dictionary type {type.Name} is not supported.");
                        return null;
                    }
                    Type[] dictParams = type.GetGenericArguments();
                    Type keyType = dictParams[0];
                    Type valueType = dictParams[1];

                    IDictionary created = (IDictionary) CreateInstance(type);
                    bool? attrCompact = rootNode.TryParseAttributeBool("compact");
                    bool useCompact = keyType == typeof(string) && (attrCompact == null || attrCompact.Value);
                    if(keyType != typeof(string) && attrCompact != null && attrCompact.Value)
                    {
                        OnLog?.Invoke($"[ERROR] Node {rootNode.GetXPath()} has compact='true', but the dictionary has keys of type {keyType.Name}. They must be Strings to use compact mode.");
                    }
                    bool allowNullValues = valueType.IsNullable();
                    if (useCompact)
                    {
                        // The key is an attribute, the value is the node value.
                        for (int i = 0; i < rootNode.ChildNodes.Count; i++)
                        {
                            var node = rootNode.ChildNodes.Item(i);
                            if (node.NodeType != XmlNodeType.Element)
                                continue;

                            string key = node.TryGetAttribute("key");
                            if (key == null)
                            {
                                OnLog?.Invoke($"[ERROR] Dictionary {rootNode.GetXPath()} has element at index {i} that does not have a key attribute, such as key='hello'. Compact mode is enabled. Use compact='false' to disable compact mode on this dictionary.");
                                continue;
                            }
                            if (created.Contains(key))
                            {
                                OnLog?.Invoke($"[ERROR] Duplicate key in dictionary {rootNode.GetXPath()}: '{key}'");
                                continue;
                            }
                            object value = CreateAndPopulate(null, node, valueType);

                            if(value == null && !allowNullValues)
                            {
                                OnLog?.Invoke($"[ERROR] Dictionary {rootNode.GetXPath()} has a null value. This is not valid because the value type is {valueType.Name}.");
                                continue;
                            }

                            created.Add(key, value);
                        }
                    }
                    else
                    {
                        // The key is a node with the name K, a value is a node with the name V.
                        // K and V's Should be in sequence and should be in pairs.
                        // The key is an attribute, the value is the node value.
                        bool expectKey = true;
                        int index = -1;
                        object lastKey = null;
                        for (int i = 0; i < rootNode.ChildNodes.Count; i++)
                        {
                            var node = rootNode.ChildNodes.Item(i);
                            if (node.NodeType != XmlNodeType.Element)
                                continue;

                            index++;
                            string nodeName = node.Name.Trim().ToLower();
                            bool isKey = nodeName == "k" || nodeName == "key";
                            bool isValue = nodeName == "v" || nodeName == "value";

                            if(!isKey && !isValue)
                            {
                                OnLog?.Invoke($"[ERROR] Dictionary {rootNode.Name} has an item at index {index} called '{node.Name}'. Dictionary items, when not in compact mode, should only be named either K or V. Value will be assumed to be a {(expectKey ? "key" : "value")}.");
                                if (expectKey)
                                    isKey = true;
                                else
                                    isValue = true;
                            }

                            if(isKey && !expectKey)
                            {
                                OnLog?.Invoke($"[ERROR] Dictionary {rootNode.Name} has two keys in a row! Item order must be key, value, key, value etc.");
                                continue;
                            }
                            if (!isKey && expectKey)
                            {
                                OnLog?.Invoke($"[ERROR] Dictionary {rootNode.Name} has two values in a row! Item order must be key, value, key, value etc.");
                                continue;
                            }

                            if (isKey)
                            {
                                lastKey = CreateAndPopulate(null, node, keyType);
                                expectKey = false;
                            }
                            else
                            {
                                expectKey = true;
                                if (created.Contains(lastKey))
                                {
                                    OnLog?.Invoke($"[ERROR] Duplicate key in dictionary {rootNode.GetXPath()}: '{lastKey}'");
                                    lastKey = null;
                                    continue;
                                }
                                object value = CreateAndPopulate(null, node, valueType);
                                if (value == null && !allowNullValues)
                                {
                                    OnLog?.Invoke($"[ERROR] Dictionary {rootNode.GetXPath()} has a null value. This is not valid because the value type is {valueType.Name}.");
                                    continue;
                                }
                                created.Add(lastKey, value);
                                lastKey = null;
                            }
                        }
                    }

                    if(!(existing is IDictionary oldDict))
                        return created;

                    ListMergeMode mode = rootNode.TryParseAttributeEnum<ListMergeMode>("mode") ?? ListMergeMode.MergeReplace;
                    if ((oldDict.IsFixedSize || oldDict.IsReadOnly) && mode != ListMergeMode.Replace)
                    {
                        OnLog?.Invoke($"[ERROR] Node {rootNode.Name}: This Dictionary<{keyType.Name}, {valueType.Name}> uses merge mode {mode}, but the dictioary instance is read-only. New dictionary will replace old values.");
                        mode = ListMergeMode.Replace;
                    }

                    ListMergeUtils.Combine(oldDict, created, mode);

                    return oldDict;
                }

                if (isEnumType)
                {
                    string textContent = rootNode.InnerText;
                    try
                    {
                        var parsedEnum = Enum.Parse(type, textContent, true);
                        return parsedEnum;
                    }
                    catch
                    {
                        OnLog?.Invoke($"Failed to parse '{textContent}' as enum {type.Name}");
                    }
                }

                if (isBasic)
                {
                    var firstChild = rootNode.FirstContentNode();
                    if (firstChild == null)
                    {
                        OnLog?.Invoke($"[ERROR] Null value (empty tag) in node {rootNode.GetXPath()}. Expected a {loader.TargetType.Name}.");
                        return null;
                    }

                    try
                    {
                        var fromLoader = loader.Deserialize(firstChild);
                        return fromLoader;
                    }
                    catch (Exception e)
                    {
                        OnLog?.Invoke($"[ERROR] Exception deserializing value '{firstChild.InnerText}' as a {loader.TargetType.Name} using loader {loader.GetType().Name} for node {firstChild.GetXPath()}:\n{e}");
                        return null;
                    }
                }
                else
                {
                    // Create new object (class or struct)
                    bool didCreate = existing == null;
                    var created = existing ?? CreateInstance(type);
                    if (didCreate && created is IXmlAware aware)
                    {
                        aware.OnCreateFromNode(rootNode);
                    }
                    rootObject ??= created;
                    parentObject = created;

                    // Get all child nodes in this node.
                    var children = rootNode.ChildNodes;
                    for (int i = 0; i < children.Count; i++)
                    {
                        var node = children.Item(i);

                        //OnLog?.Invoke($"[{node.NodeType}] {node.Name}: {node.Value}");
                        if(node.NodeType != XmlNodeType.Element && node.NodeType != XmlNodeType.Comment)
                        {
                            // You should not be here... Most likely a text. However, the xml loaded correctly, so it might be safe to ignore with just a warning.
                            OnLog?.Invoke($"Unexpected node of type '{node.NodeType}' at {node.GetXPath()}. Content: '{node.Value?.Trim()}'. Please remove.");
                        }
                        if (node.NodeType != XmlNodeType.Element)
                            continue;

                        string fieldName = node.Name;
                        var field = GetField(fieldName, type);
                        if (!field.IsValid)
                        {
                            OnLog?.Invoke($"Error: Failed to find field '{type.FullName}.{fieldName}'");
                            continue;
                        }

                        Type childType = field.FieldType;

                        currentField = field;
                        var childObj = CreateAndPopulate(ReadField(field, created), node, childType);
                        currentField = FieldWrapper.Invalid;

                        WriteField(field, created, childObj, node);
                    }

                    parentObject = null;
                    return created;
                }
            }
        }

        private bool IsArrayType(Type t)
        {
            return t.IsArray;
        }

        private bool IsListType(Type t)
        {
            return typeof(IList).IsAssignableFrom(t);
        }

        private bool IsDictionaryType(Type t)
        {
            return typeof(IDictionary).IsAssignableFrom(t);
        }

        private bool IsEnumType(Type t)
        {
            return t.IsEnum;
        }

        private object CreateInstance(Type type, int arrayLength = 0)
        {
            if (type.IsArray)
            {
                return Array.CreateInstance(type.GetElementType(), arrayLength <= 0 ? throw new ArgumentException("Array length must be greater than zero.", nameof(arrayLength), null) : arrayLength);
            }

            return Activator.CreateInstance(type, true);
        }

        private FieldWrapper GetField(string fieldName, Type type)
        {
            if (type == null || fieldName == null)
                return default;

            string path = type.FullName + fieldName;
            if (cachedFields.TryGetValue(path, out var wrapper))
                return wrapper;

            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (field != null)
            {
                var w = new FieldWrapper(field);
                cachedFields.Add(path, w);
                return w;
            }
            var prop = type.GetProperty(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (prop != null)
            {
                var w = new FieldWrapper(prop);
                cachedFields.Add(path, w);
                return w;
            }

            cachedFields.Add(path, default);
            return default;
        }

        private void WriteField(FieldWrapper wrapper, object obj, object value, XmlNode node)
        {
            if (!wrapper.IsValid)
                throw new Exception("Field wrapper is invalid.");

            if (wrapper.HasIgnoreAttribute)
                throw new Exception($"Field '{wrapper}' has an [XmlIgnore] attribute, so cannot be written to from xml ({node?.GetXPath()}).");

            if (wrapper.IsField)
            {
                wrapper.Field.SetValue(obj, value);
            }
            else
            {
                wrapper.Property.SetValue(obj, value);
            }
        }

        private object ReadField(FieldWrapper wrapper, object obj)
        {
            if (!wrapper.IsValid)
                throw new Exception("Field wrapper is invalid.");

            if (wrapper.IsField)
            {
                return wrapper.Field.GetValue(obj);
            }
            else
            {
                if(wrapper.Property.CanRead)
                    return wrapper.Property.GetValue(obj);
                return null;
            }
        }

        public void Dispose()
        {
            doc = null;
            rootSerializers?.Clear();
            rootSerializers = null;
            cachedFields?.Clear();
            cachedFields = null;
            customResolvers?.Clear();
            customResolvers = null;
        }
    }
}
