using UnityEngine;

namespace NFramework
{
    public static partial class StaticUtils
    {
        public static Vector2Int ConvertIndexToIndex2Dimension(int index, int xSize, int ySize)
        {
            var x = index % xSize;
            var y = index / ySize;
            return new Vector2Int(x, y);
        }


        public static Vector2Int ConvertIndexToIndex2Dimension(int index, Vector2 size)
        {
            var x = index % (int)size.x;
            var y = index / (int)size.y;
            return new Vector2Int(x, y);
        }
    }
}
