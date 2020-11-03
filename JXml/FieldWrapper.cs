using System;
using System.Reflection;
using System.Xml.Serialization;

namespace JXml
{
    public struct FieldWrapper
    {
        public static FieldWrapper Invalid { get; } = new FieldWrapper((FieldInfo) null);

        public bool IsField
        {
            get
            {
                return Field != null;
            }
        }
        public bool IsProperty
        {
            get
            {
                return Property != null;
            }
        }
        public bool IsValid
        {
            get
            {
                return IsField || IsProperty;
            }
        }

        public FieldInfo Field { get; }
        public PropertyInfo Property { get; }
        public bool HasIgnoreAttribute
        {
            get
            {
                if(_isIgnoreChecked == false)
                {
                    if (IsField)
                        _isIgnore = Field.GetCustomAttribute<XmlIgnoreAttribute>() != null;
                    if (IsProperty)
                        _isIgnore = Property.GetCustomAttribute<XmlIgnoreAttribute>() != null;
                    _isIgnoreChecked = true;
                }

                return _isIgnore;
            }
        }

        private bool _isIgnore;
        private bool _isIgnoreChecked;

        public Type FieldType
        {
            get
            {
                if (IsField)
                    return Field.FieldType;
                if (IsProperty)
                    return Property.PropertyType;
                return null;
            }
        }

        public FieldWrapper(FieldInfo field)
        {
            this.Field = field;
            this.Property = null;
            _isIgnore = false;
            _isIgnoreChecked = false;
        }

        public FieldWrapper(PropertyInfo property)
        {
            this.Field = null;
            this.Property = property;
            _isIgnore = false;
            _isIgnoreChecked = false;
        }

        public object ReadValue(object instance)
        {
            if (IsField)
                return Field.GetValue(instance);
            if (IsProperty)
                return Property.GetValue(instance);

            return null;
        }

        public void WriteValue(object instance, object value)
        {
            if (IsField)
            {
                Field.SetValue(instance, value);
            }
            else if (IsProperty)
            {
                Property.SetValue(instance, value);
            }
        }

        public override string ToString()
        {
            if (!IsValid)
                return "[Invalid Field Wrapper]";

            string path = IsField ? Field.DeclaringType.FullName : Property.DeclaringType.FullName;
            path = $"{path}.{(IsField ? Field.Name : Property.Name)}";
            string ignored = HasIgnoreAttribute ? "(XmlIgnore)" : string.Empty;
            return $"[{(IsField ? "Field" : "Property")}] {path} {ignored}";
        }
    }
}
