using UnityEngine;

namespace NFramework
{
    public static class RigidbodyExtension
    {
        public static void SetPosX(this Rigidbody rb, float x) => rb.position = rb.position.WithX(x);

        public static void SetPosY(this Rigidbody rb, float y) => rb.position = rb.position.WithY(y);

        public static void SetPosZ(this Rigidbody rb, float z) => rb.position = rb.position.WithZ(z);

        public static void SetPosXY(this Rigidbody rb, float x, float y) 
            => rb.position = rb.position.WithXY(x, y);
        
        public static void SetPosXZ(this Rigidbody rb, float x, float z) 
            => rb.position = rb.position.WithXZ(x, z);
        
        public static void SetPosYZ(this Rigidbody rb, float y, float z) 
            => rb.position = rb.position.WithYZ(y, z);
        
        public static void SetEulerAnglesX(this Rigidbody rb, float x)
        {
            Vector3 eulerAngle = rb.rotation.eulerAngles.WithX(x);
            rb.rotation = Quaternion.Euler(eulerAngle);
        }

        public static void SetEulerAnglesXY(this Rigidbody rb, float x, float y)
        {
            Vector3 eulerAngle = rb.rotation.eulerAngles.WithXY(x, y);
            rb.rotation = Quaternion.Euler(eulerAngle);
        }
        
        public static void SetEulerAnglesY(this Rigidbody rb, float y)
        {
            Vector3 eulerAngle = rb.rotation.eulerAngles.WithY(y);
            rb.rotation = Quaternion.Euler(eulerAngle);
        }
        
        public static void SetEulerAnglesYZ(this Rigidbody rb, float y, float z)
        {
            Vector3 eulerAngle = rb.rotation.eulerAngles.WithYZ(y, z);
            rb.rotation = Quaternion.Euler(eulerAngle);
        }

        public static void SetEulerAnglesZ(this Rigidbody rb, float z)
        {
            Vector3 eulerAngle = rb.rotation.eulerAngles.WithZ(z);
            rb.rotation = Quaternion.Euler(eulerAngle);
        }
        
        public static void SetEulerAnglesXZ(this Rigidbody rb, float x, float z)
        {
            Vector3 eulerAngle = rb.rotation.eulerAngles.WithXZ(x, z);
            rb.rotation = Quaternion.Euler(eulerAngle);
        }
    }
}