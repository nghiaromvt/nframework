using System;
using System.Linq;
using UnityEngine;

namespace NFramework
{
    /// <summary>
    /// Conditionally Show/Hide field in inspector, based on some other field value
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ConditionalFieldAttribute : PropertyAttribute
    {
        public readonly string fieldToCheck;
        public readonly string[] compareValues;
        public readonly bool inverse;

        /// <param name="fieldToCheck">String name of field to check value</param>
        /// <param name="inverse">Inverse check result</param>
        /// <param name="compareValues">On which values field will be shown in inspector</param>
        public ConditionalFieldAttribute(string fieldToCheck, bool inverse = false, params object[] compareValues)
        {
            this.fieldToCheck = fieldToCheck;
            this.inverse = inverse;
            this.compareValues = compareValues.Select(c => c.ToString().ToUpper()).ToArray();
        }
    }
}
