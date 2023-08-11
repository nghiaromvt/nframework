using UnityEngine;

namespace NFramework
{
    public class SeparatorAttribute : PropertyAttribute
    {
        public readonly string title;
        public readonly bool withOffset;
        public readonly int colorIndex;

        public SeparatorAttribute(string title = "", bool withOffset = false, int colorIndex = -1)
        {
            this.title = title;
            this.withOffset = withOffset;
            this.colorIndex = colorIndex;
        }
    }
}
