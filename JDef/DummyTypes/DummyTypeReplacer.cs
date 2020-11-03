using JXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace JDef.DummyTypes
{
    public static class DummyTypeReplacer
    {
        private static readonly Dictionary<Type, (List<FieldWrapper> dummies, List<FieldWrapper> subTypes)> dummyFields = new Dictionary<Type, (List<FieldWrapper> dummies, List<FieldWrapper> subTypes)>();
        private static readonly HashSet<Type> haveNoFields = new HashSet<Type>();

        private static bool IsDummyType(Type t)
        {
            if (t == null)
                return false;

            return t.GetCustomAttribute<CanBeDummyAttribute>() != null; // TODO cache result into hashet or similar.
        }

        private static (List<FieldWrapper> dummies, List<FieldWrapper> subTypes) GetDummyFields(Type type)
        {
            if (type == null)
                return (null, null);

            if (haveNoFields.Contains(type))
                return (null, null);

            if (dummyFields.TryGetValue(type, out var pair))
                return pair;

            var list = new List<FieldWrapper>();
            var list2 = new List<FieldWrapper>();

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                var ft = field.FieldType;
                var wrapper = new FieldWrapper(field);

                if (IsDummyType(ft))
                {
                    list.Add(wrapper);
                    continue;
                }
                if (ft.IsArray && IsDummyType(ft.GetElementType()))
                {
                    list.Add(wrapper);
                    continue;
                }
                if (typeof(IList).IsAssignableFrom(ft) && ft.IsGenericType && IsDummyType(ft.GetGenericArguments()[0]))
                {
                    list.Add(wrapper);
                    continue;
                }
                if (typeof(IDictionary).IsAssignableFrom(ft) && ft.IsGenericType)
                {
                    Type[] dictParams = ft.GetGenericArguments();
                    Type keyType = dictParams[0];
                    Type valueType = dictParams[1];
                    if (IsDummyType(keyType))
                    {
                        Console.WriteLine($"[ERROR] Dictionary key type is a dummy type '{keyType.Name}' in {type.FullName}.{field.Name}, but replacing keys is not supported.");
                    }
                    if (IsDummyType(valueType))
                    {
                        list.Add(wrapper);
                        continue;
                    }
                }

                if (ft.IsPrimitive || ft == typeof(string) || typeof(ICollection).IsAssignableFrom(ft))
                    continue;

                list2.Add(wrapper);
            }

            if (list.Count > 0 || list2.Count > 0)
            {
                dummyFields.Add(type, (list, list2));
                return (list, list2);
            }
            else
            {
                haveNoFields.Add(type);
                return (null, null);
            }
        }

        public static int ReplaceDummyTypes(object obj)
        {
            if (obj == null)
                return 0;

            Type objType = obj.GetType();
            if (objType.IsPrimitive)
                return 0;

            var (fields, needExploring) = GetDummyFields(objType);
            //Console.WriteLine($"GetDummyFields({obj.GetType().Name}) -> {fields?.Count ?? 0} dummy fields, {needExploring?.Count ?? 0} need exploring");
            if ((fields == null || fields.Count == 0) && (needExploring == null || needExploring.Count == 0))
                return 0;

            int acc = 0;

            foreach (var toReplace in fields)
            {
                bool isArray = toReplace.FieldType.IsArray;
                bool isList = typeof(IList).IsAssignableFrom(toReplace.FieldType);
                bool isDictionary = typeof(IDictionary).IsAssignableFrom(toReplace.FieldType);

                if (isArray)
                {
                    acc += HandleArray(obj, toReplace);
                }
                else if (isDictionary)
                {
                    acc += HandleDictionary(obj, toReplace);
                }
                else if (isList)
                {
                    acc += HandleList(obj, toReplace);
                }
                else
                {
                    // This is a type that has the [CanBeDummy] attribute.
                    object currentValue = toReplace.ReadValue(obj);
                    if (currentValue != null)
                    {
                        if (currentValue is IDummyType dt)
                        {
                            // If the value itself is a dummy, then it needs replacing.
                            var replacement = dt.GetRealObject();
                            toReplace.WriteValue(obj, replacement);
                            acc++;
                        }
                    }
                }
            }

            foreach (var toExplore in needExploring)
            {
                var value = toExplore.ReadValue(obj);

                // Important node:
                // Because this recursive method works 'forwards'
                // rather than 'backwards' like the XmlController,
                // fields that are structs cannot be re-assigned back to their
                // parent object, so it just won't work.
                // Uses classes instead.

                if (toExplore.FieldType.IsValueType)
                {
                    Console.WriteLine($"[ERROR] {toExplore} is a struct, so cannot be explored by the dummy replacer. Consider using classes instead.");
                    continue;
                }

                if (value != null)
                {
                    int change = ReplaceDummyTypes(value);
                    acc += change;
                }
            }

            return acc;
        }

        private static int HandleArray(object parent, FieldWrapper wrapper)
        {
            var currentValue = wrapper.ReadValue(parent);
            if (currentValue == null)
                return 0;

            int acc = 0;
            Array arr = currentValue as Array;
            for (int i = 0; i < arr.Length; i++)
            {
                var value = arr.GetValue(i);
                if (value == null)
                    continue;

                if (value is IDummyType dt)
                {
                    var newValue = dt.GetRealObject();
                    arr.SetValue(newValue, i);
                    acc++;

                    //Console.WriteLine($"Replaced array element {i}: {value ?? "null"} -> {newValue ?? "null"}");
                }
            }
            return acc;
        }

        private static int HandleList(object parent, FieldWrapper wrapper)
        {
            var currentValue = wrapper.ReadValue(parent);
            if (currentValue == null)
                return 0;

            int acc = 0;
            IList arr = currentValue as IList;
            for (int i = 0; i < arr.Count; i++)
            {
                var value = arr[i];
                if (value == null)
                    continue;

                if (value is IDummyType dt)
                {
                    var newValue = dt.GetRealObject();
                    arr[i] = newValue;
                    acc++;

                    //Console.WriteLine($"Replaced list element {i}: {value ?? "null"} -> {newValue ?? "null"}");
                }
            }
            return acc;
        }

        private static List<object> keys = new List<object>();
        private static int HandleDictionary(object parent, FieldWrapper wrapper)
        {
            var currentValue = wrapper.ReadValue(parent);
            if (currentValue == null)
                return 0;

            int acc = 0;
            IDictionary arr = currentValue as IDictionary;
            foreach (var k in arr.Keys)
                keys.Add(k);
            foreach(var key in keys)
            {
                object value = arr[key];
                if (value == null)
                    continue;

                if (value is IDummyType dt)
                {
                    var newValue = dt.GetRealObject();
                    arr[key] = newValue;
                    acc++;

                    //Console.WriteLine($"Replaced dict element {key}: {value ?? "null"} -> {newValue ?? "null"}");
                }
            }
            keys.Clear();
            return acc;
        }
    }
}
