using System;
using UnityEngine;

namespace NFramework
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ButtonMethodAttribute : PropertyAttribute
    {
        public enum EDrawOrder
        {
            BeforeInspector,
            AfterInspector
        }

        public readonly EDrawOrder drawOrder;

        public ButtonMethodAttribute(EDrawOrder drawOrder = EDrawOrder.AfterInspector)
        {
            this.drawOrder = drawOrder;
        }
    }
}
