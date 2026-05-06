using UnityEngine;

namespace NFramework
{
    public static class QuaternionExtension
    {
        public static Quaternion SetEulerX(this Quaternion quaternion, float x) 
            => Quaternion.Euler(quaternion.eulerAngles.WithX(x));
		
        public static Quaternion SetEulerY(this Quaternion quaternion, float y)
            => Quaternion.Euler(quaternion.eulerAngles.WithY(y));
		
        public static Quaternion SetEulerZ(this Quaternion quaternion, float z)
            => Quaternion.Euler(quaternion.eulerAngles.WithZ(z));
		
        public static Quaternion SetEulerXY(this Quaternion quaternion, float x, float y)
            => Quaternion.Euler(quaternion.eulerAngles.WithXY(x, y));
		
        public static Quaternion SetEulerXZ(this Quaternion quaternion, float x, float z)
            => Quaternion.Euler(quaternion.eulerAngles.WithXZ(x, z));
		
        public static Quaternion SetEulerYZ(this Quaternion quaternion, float y, float z)
            => Quaternion.Euler(quaternion.eulerAngles.WithYZ(y, z));
    }
}