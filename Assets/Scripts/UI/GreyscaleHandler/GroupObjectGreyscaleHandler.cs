using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public class GroupObjectGreyscaleHandler : UIGreyscaleHandler
    {
        [SerializeField] private List<GameObject> _defaultGroupObjects = new();
        [SerializeField] private List<GameObject> _greyscaleGroupObjects = new();

        public override void Toggle(bool isOn)
        {
            base.Toggle(isOn);
            _defaultGroupObjects.ForEach(go => go.SetActive(!_isOn));
            _greyscaleGroupObjects.ForEach(go => go.SetActive(_isOn));
        }
    }
}