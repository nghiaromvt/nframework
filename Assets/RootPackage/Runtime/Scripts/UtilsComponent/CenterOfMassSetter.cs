using Sirenix.OdinInspector;
using UnityEngine;

namespace NFramework
{
    public class CenterOfMass : MonoBehaviour
    {
        public enum EAutomaticSetModes { Awake, Start, ScriptOnly }
        
        [SerializeField] private bool _is2D;
        
        [Header("CenterOfMass")]
        [SerializeField] private Vector3 _centerOfMassOffset;
        
        [Header("Automation")]
        [SerializeField] private EAutomaticSetModes _automaticSetMode = EAutomaticSetModes.Awake;
        [SerializeField] private bool _autoDestroyComponentAfterSet = true;

        [Header("Test")]
        [SerializeField] private float _gizmoPointSize = 0.1f;

        private Vector3 _gizmoCenter;
        
        protected virtual void Awake()
        {
            if (_automaticSetMode == EAutomaticSetModes.Awake)
                SetCenterOfMass();
        }
        
        protected virtual void Start()
        {
            if (_automaticSetMode == EAutomaticSetModes.Start)
                SetCenterOfMass();
        }

        [Button]
        public void SetCenterOfMass()
        {
            if (_is2D)
                GetComponent<Rigidbody2D>().centerOfMass = _centerOfMassOffset;
            else
                GetComponent<Rigidbody>().centerOfMass = _centerOfMassOffset;

            if (_autoDestroyComponentAfterSet && Application.isPlaying)
                Destroy(this);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            _gizmoCenter = transform.TransformPoint(_centerOfMassOffset);
            GizmosDrawer.DrawGizmoPoint(_gizmoCenter, _gizmoPointSize, Color.yellow);
        }
    }
}