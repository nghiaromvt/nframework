using System.Collections.Generic;
using UnityEngine;

namespace NFramework
{
    public static class VectorExtension
    {
        #region Set X/Y/Z

		public static Vector3 WithX(this Vector3 vector, float x) => new Vector3(x, vector.y, vector.z);

		public static Vector2 WithX(this Vector2 vector, float x) => new Vector2(x, vector.y);

		public static Vector3 WithY(this Vector3 vector, float y) => new Vector3(vector.x, y, vector.z);
		
		public static Vector2 WithY(this Vector2 vector, float y) => new Vector2(vector.x, y);

		public static Vector3 WithZ(this Vector3 vector, float z) => new Vector3(vector.x, vector.y, z);
		
		public static Vector3 WithXY(this Vector3 vector, float x, float y) => new Vector3(x, y, vector.z);
		
		public static Vector3 WithXZ(this Vector3 vector, float x, float z) => new Vector3(x, vector.y, z);
		
		public static Vector3 WithYZ(this Vector3 vector, float y, float z) => new Vector3(vector.x, y, z);
		
		#endregion
		
		#region Offset X/Y/Z
		
		public static Vector3 OffsetX(this Vector3 vector, float x) => new Vector3(vector.x + x, vector.y, vector.z);

		public static Vector2 OffsetX(this Vector2 vector, float x) => new Vector2(vector.x + x, vector.y);

		public static Vector3 OffsetY(this Vector3 vector, float y) => new Vector3(vector.x, vector.y + y, vector.z);

		public static Vector2 OffsetY(this Vector2 vector, float y) => new Vector2(vector.x, vector.y + y);
		
		public static Vector3 OffsetZ(this Vector3 vector, float z) => new Vector3(vector.x, vector.y, vector.z + z);

		public static Vector3 OffsetXY(this Vector3 vector, float x, float y) => new Vector3(vector.x + x, vector.y + y, vector.z);
		
		public static Vector2 OffsetXY(this Vector2 vector, float x, float y) => new Vector2(vector.x + x, vector.y + y);
		
		public static Vector3 OffsetXZ(this Vector3 vector, float x, float z) => new Vector3(vector.x + x, vector.y, vector.z + z);
		
		public static Vector3 OffsetYZ(this Vector3 vector, float y, float z) => new Vector3(vector.x, vector.y + y, vector.z + z);

		#endregion

		#region Clamp X/Y

		public static Vector3 ClampX(this Vector3 vector, float min, float max) => vector.WithX(Mathf.Clamp(vector.x, min, max));

		public static Vector2 ClampX(this Vector2 vector, float min, float max) => vector.WithX(Mathf.Clamp(vector.x, min, max));

		public static Vector3 ClampY(this Vector3 vector, float min, float max) => vector.WithY(Mathf.Clamp(vector.y, min, max));

		public static Vector2 ClampY(this Vector2 vector, float min, float max) => vector.WithY(Mathf.Clamp(vector.y, min, max));

		public static Vector3 ClampZ(this Vector3 vector, float min, float max) => vector.WithZ(Mathf.Clamp(vector.z, min, max));

		#endregion

		#region Invert

		public static Vector3 InvertX(this Vector3 vector) => vector.WithX(-vector.x);
		
		public static Vector2 InvertX(this Vector2 vector) => vector.WithX(-vector.x);
		
		public static Vector3 InvertY(this Vector3 vector) => vector.WithY(-vector.y);
		
		public static Vector2 InvertY(this Vector2 vector) => vector.WithY(-vector.y);
		
		public static Vector3 InvertZ(this Vector3 vector) => vector.WithZ(-vector.z);

		#endregion

		#region Convert

		public static Vector2 ToVector2(this Vector3 vector) => new Vector2(vector.x, vector.y);

		public static Vector3 ToVector3(this Vector2 vector) => new Vector3(vector.x, vector.y);

		public static Vector2 ToVector2(this Vector2Int vector) => new Vector2(vector.x, vector.y);

		public static Vector3 ToVector3(this Vector3Int vector) => new Vector3(vector.x, vector.y, vector.z);

		public static Vector2Int ToVector2Int(this Vector2 vector) 
			=> new Vector2Int(vector.x.RoundToInt(), vector.y.RoundToInt());

		public static Vector3Int ToVector3Int(this Vector3 vector) 
			=> new Vector3Int(vector.x.RoundToInt(), vector.y.RoundToInt(), vector.z.RoundToInt());

		#endregion

		#region Snap

		/// <summary>
		/// Snap to grid of snapValue
		/// </summary>
		public static Vector3 SnapValue(this Vector3 val, float snapValue) 
			=> new Vector3(val.x.Snap(snapValue), val.y.Snap(snapValue), val.z.Snap(snapValue));

		/// <summary>
		/// Snap to grid of snapValue
		/// </summary>
		public static Vector2 SnapValue(this Vector2 val, float snapValue) 
			=> new Vector2(val.x.Snap(snapValue), val.y.Snap(snapValue));

		#endregion

		#region Average

		public static Vector3 AverageVector(this Vector3[] vectors)
		{
			if (vectors.IsNullOrEmpty()) return Vector3.zero;

			float x = 0f, y = 0f, z = 0f;
			for (var i = 0; i < vectors.Length; i++)
			{
				x += vectors[i].x;
				y += vectors[i].y;
				z += vectors[i].z;
			}

			return new Vector3(x / vectors.Length, y / vectors.Length, z / vectors.Length);
		}

		public static Vector2 AverageVector(this Vector2[] vectors)
		{
			if (vectors.IsNullOrEmpty()) return Vector2.zero;

			float x = 0f, y = 0f;
			for (var i = 0; i < vectors.Length; i++)
			{
				x += vectors[i].x;
				y += vectors[i].y;
			}

			return new Vector2(x / vectors.Length, y / vectors.Length);
		}

		#endregion

		#region Approximately

		public static bool Approximately(this Vector3 vector, Vector3 compared, float threshold = 0.1f)
		{
			var xDiff = Mathf.Abs(vector.x - compared.x);
			var yDiff = Mathf.Abs(vector.y - compared.y);
			var zDiff = Mathf.Abs(vector.z - compared.z);

			return xDiff <= threshold && yDiff <= threshold && zDiff <= threshold;
		}

		public static bool Approximately(this Vector2 vector, Vector2 compared, float threshold = 0.1f)
		{
			var xDiff = Mathf.Abs(vector.x - compared.x);
			var yDiff = Mathf.Abs(vector.y - compared.y);

			return xDiff <= threshold && yDiff <= threshold;
		}

		#endregion

		#region Get Closest

		/// <summary>
		/// Finds the position closest to the given one.
		/// </summary>
		/// <param name="position">World position.</param>
		/// <param name="otherPositions">Other world positions.</param>
		/// <returns>Closest position.</returns>
		public static Vector3 GetClosest(this Vector3 position, IEnumerable<Vector3> otherPositions)
		{
			var closest = Vector3.zero;
			var shortestDistance = Mathf.Infinity;

			foreach (var otherPosition in otherPositions)
			{
				var distance = (position - otherPosition).sqrMagnitude;

				if (distance < shortestDistance)
				{
					closest = otherPosition;
					shortestDistance = distance;
				}
			}

			return closest;
		}

		public static Vector3 GetClosest(this IEnumerable<Vector3> positions, Vector3 position) => position.GetClosest(positions);

		#endregion

		#region To

		/// <summary>
		/// Get vector from source to destination
		/// </summary>
		public static Vector4 To(this Vector4 source, Vector4 destination) => destination - source;

		/// <summary>
		/// Get vector from source to destination
		/// </summary>
		public static Vector3 To(this Vector3 source, Vector3 destination) => destination - source;

		/// <summary>
		/// Get vector from source to destination
		/// </summary>
		public static Vector2 To(this Vector2 source, Vector2 destination) => destination - source;

		#endregion

		#region Pow

		/// <summary>
		/// Raise each component of the source Vector2 to the specified power.
		/// </summary>
		public static Vector2 Pow(this Vector2 source, float exponent) 
			=> new Vector2(Mathf.Pow(source.x, exponent), Mathf.Pow(source.y, exponent));

		/// <summary>
		/// Raise each component of the source Vector3 to the specified power.
		/// </summary>
		public static Vector3 Pow(this Vector3 source, float exponent) 
			=> new Vector3(Mathf.Pow(source.x, exponent), Mathf.Pow(source.y, exponent), Mathf.Pow(source.z, exponent));

		/// <summary>
		/// Raise each component of the source Vector3 to the specified power.
		/// </summary>
		public static Vector4 Pow(this Vector4 source, float exponent) 
			=> new Vector4(Mathf.Pow(source.x, exponent), Mathf.Pow(source.y, exponent), Mathf.Pow(source.z, exponent), Mathf.Pow(source.w, exponent));

		#endregion

		#region ScaleBy

		/// <summary>
		/// Immutably returns the result of the source vector multiplied with
		/// another vector component-wise.
		/// </summary>
		public static Vector2 ScaleBy(this Vector2 source, Vector2 right) => Vector2.Scale(source, right);

		/// <summary>
		/// Immutably returns the result of the source vector multiplied with
		/// another vector component-wise.
		/// </summary>
		public static Vector3 ScaleBy(this Vector3 source, Vector3 right) => Vector3.Scale(source, right);

		/// <summary>
		/// Immutably returns the result of the source vector multiplied with
		/// another vector component-wise.
		/// </summary>
		public static Vector4 ScaleBy(this Vector4 source, Vector4 right) => Vector4.Scale(source, right);

		#endregion

		#region Round

		public static Vector3 Round(this Vector3 source) => new Vector3(source.x.Round(), source.y.Round(), source.z.Round());
		
		public static Vector2 Round(this Vector2 source) => new Vector2(source.x.Round(), source.y.Round());

		#endregion
		
		#region Vector2 Rotate

		public static float GetAngleInRadian(this Vector2 v1, Vector2 v2) => Mathf.Atan2(v2.y - v1.y, v2.x - v1.x);

		public static float GetAngleInDegree(this Vector2 v1, Vector2 v2) => 
			Mathf.Atan2(v2.y - v1.y, v2.x - v1.x) * Mathf.Rad2Deg;

		public static Vector2 Rotate(this Vector2 v, float angle) => 
			Quaternion.AngleAxis(angle, Vector3.back) * v;

		public static float GetAngleInDegree(this Vector2 v1) => Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg;

		#endregion

		#region Vector3 Rotate

		public static float GetAngleInRadian(this Vector3 v1, Vector3 v2) => Mathf.Atan2(v2.y - v1.y, v2.x - v1.x);

		public static float GetAngleInDegree(this Vector3 v1, Vector3 v2) => 
			Mathf.Atan2(v2.y - v1.y, v2.x - v1.x) * Mathf.Rad2Deg;

		public static float GetAngleInDegree(this Vector3 v1) => Mathf.Atan2(v1.y, v1.x) * Mathf.Rad2Deg;

		public static Vector3 Rotate(this Vector3 v, float angle)
		{
			v = Quaternion.AngleAxis(angle, Vector3.back) * v;
			return v;
		}

		#endregion
    }
}