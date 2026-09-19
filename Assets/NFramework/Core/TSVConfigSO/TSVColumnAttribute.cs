using System;

namespace NFramework
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class TSVColumnAttribute : Attribute
    {
        public string Name { get; }

        public TSVColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
