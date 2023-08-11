using UnityEngine;

namespace NFramework
{
    /// <summary>
    /// Add this class to a Rigidbody or Rigidbody2D to be able to edit its center of mass from the inspector directly
    /// </summary>
    public class CenterOfMass : MonoBehaviour
    {
        /// the possible modes this class can start on
        public enum AutomaticSetModes { Awake, Start, ScriptOnly }

        [Header("CenterOfMass")]
        /// the offset to apply to the center of mass
        [SerializeField] private Vector3 _centerOfMassOffset;

        [Header("Automation")]
        /// whether to set the center of mass on awake, start, or via script only
        [SerializeField] private AutomaticSetModes _automaticSetMode = AutomaticSetModes.Awake;
        /// whether or not this component should auto destroy after a set
        [SerializeField] private bool _autoDestroyComponentAfterSet = true;

        [Header("Test")]
        /// the size of the gizmo point to display at the center of mass
        public float gizmoPointSize = 0.1f;

        private Vector3 gizmoCenter;
        private Rigidbody rb;
        private Rigidbody2D rb2d;

        /// <summary>
        /// On Awake we grab our components and set our center of mass if needed
        /// </summary>
        protected virtual void Awake()
        {
            if (_automaticSetMode == AutomaticSetModes.Awake)
                SetCenterOfMass();
        }

        /// <summary>
        /// On Start we set our center of mass if needed
        /// </summary>
        protected virtual void Start()
        {
            if (_automaticSetMode == AutomaticSetModes.Start)
                SetCenterOfMass();
        }

        /// <summary>
        /// Sets the center of mass on the rigidbody or rigidbody2D
        /// </summary>
        [ButtonMethod]
        public virtual void SetCenterOfMass()
        {
            rb = GetComponent<Rigidbody>();

            if (rb == null)
                rb2d = GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.centerOfMass = _centerOfMassOffset;
            else if (rb2d != null)
                rb2d.centerOfMass = _centerOfMassOffset;

            if (_autoDestroyComponentAfterSet && Application.isPlaying)
                Destroy(this);
        }

        /// <summary>
        /// On DrawGizmosSelected, we draw a yellow point at the position of our center of mass
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
        {
            gizmoCenter = this.transform.TransformPoint(_centerOfMassOffset);
            GizmosDrawer.DrawGizmoPoint(gizmoCenter, gizmoPointSize, Color.yellow);
        }
    }
}
