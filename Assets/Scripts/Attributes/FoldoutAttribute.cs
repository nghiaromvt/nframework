using UnityEngine;

namespace NFramework
{
    public class FoldoutAttribute : PropertyAttribute
    {
        public readonly string name;
        public readonly bool foldEverything;

        /// <summary>Adds the property to the specified foldout group.</summary>
        /// <param name="name">Name of the foldout group.</param>
        /// <param name="foldEverything">Toggle to put all properties to the specified group</param>
        public FoldoutAttribute(string name, bool foldEverything = false)
        {
            this.foldEverything = foldEverything;
            this.name = name;
        }
    }
}
