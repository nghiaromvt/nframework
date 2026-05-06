using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    public class UIGreyscaleHandler : MonoBehaviour
    {
        public const string GREYSCALE_MAT_RESOURCES_PATH = "Greyscale";
        
        [SerializeField] protected bool _isOn;
        [SerializeField] protected List<UIGreyscaleHandler> _children = new();
        
        protected virtual void OnValidate() => Toggle(_isOn);

        [Button]
        public virtual void Toggle(bool isOn)
        {
            _isOn = isOn;
            _children.ForEach(x =>
            {
                if (x != this)
                    x.Toggle(isOn);
            });
        }

        [Button]
        protected void GetChildren(bool includeInactive = true)
        {
            var components= GetComponentsInChildren<UIGreyscaleHandler>(includeInactive);
            _children.Clear();
            components.ForEach(x =>
            {
                if (x != this)
                    _children.Add(x);
            });
        }
    }
}