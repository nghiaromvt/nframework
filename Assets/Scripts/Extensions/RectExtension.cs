using UnityEngine;

namespace NFramework
{
    public static class RectExtension
    {
        public static bool Intersects(this Rect thisRectangle, Rect otherRectangle)
        {
            return !((thisRectangle.x > otherRectangle.xMax) || (thisRectangle.xMax < otherRectangle.x)
                || (thisRectangle.y > otherRectangle.yMax) || (thisRectangle.yMax < otherRectangle.y));
        }
    }
}

