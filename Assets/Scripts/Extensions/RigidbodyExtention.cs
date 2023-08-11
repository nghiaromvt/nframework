using UnityEngine;

namespace NFramework
{
    public static class RigidbodyExtention
    {
        public static void SetPosX(this Rigidbody rb, float x)
        {
            rb.position = new Vector3(x, rb.position.y, rb.position.z);
        }

        public static void SetPosY(this Rigidbody rb, float y)
        {
            rb.position = new Vector3(rb.position.x, y, rb.position.z);
        }

        public static void SetPosZ(this Rigidbody rb, float z)
        {
            rb.position = new Vector3(rb.position.x, rb.position.y, z);
        }

        public static void SetRotationX(this Rigidbody rb, float x)
        {
            Vector3 eulerAngle = rb.rotation.eulerAngles;
            eulerAngle.x = x;
            rb.rotation = Quaternion.Euler(eulerAngle);
        }

        public static void SetRotationY(this Rigidbody rb, float y)
        {
            Vector3 eulerAngle = rb.rotation.eulerAngles;
            eulerAngle.y = y;
            rb.rotation = Quaternion.Euler(eulerAngle);
        }

        public static void SetRotationZ(this Rigidbody rb, float z)
        {
            Vector3 eulerAngle = rb.rotation.eulerAngles;
            eulerAngle.z = z;
            rb.rotation = Quaternion.Euler(eulerAngle);
        }
    }
}
