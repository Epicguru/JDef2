using System;

namespace JDef.DummyTypes
{
    /// <summary>
    /// When placed on a class or a struct, indicates that fields of this type can
    /// be have dummy values, and those values should be replaced.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class CanBeDummyAttribute : Attribute
    {
    }
}
