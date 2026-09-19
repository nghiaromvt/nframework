using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace vietlabs.fr2
{
    /// <summary>
    /// Strongly-typed GUID value object to eliminate primitive obsession with strings.
    /// Provides validation, type safety, and clear intent.
    /// </summary>
    [Serializable]
    public struct FR2_GUID : IEquatable<FR2_GUID>
    {
        [SerializeField] private string _value;
        
        public string Value => _value ?? string.Empty;
        public bool IsEmpty => string.IsNullOrEmpty(_value);
        public bool IsValid => !IsEmpty && _value.Length == 32;
        
        public FR2_GUID(string guid)
        {
            _value = ValidateAndNormalize(guid);
        }
        
        private static string ValidateAndNormalize(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return string.Empty;
            
            // Remove hyphens if present (Unity GUIDs don't use hyphens)
            guid = guid.Replace("-", "");
            
            // Validate length
            if (guid.Length != 32)
            {
                FR2_LOG.LogWarning($"Invalid GUID length: {guid} (expected 32 characters)");
                return string.Empty;
            }
            
            // Validate hex characters
            foreach (char c in guid)
            {
                if (!IsHexChar(c))
                {
                    FR2_LOG.LogWarning($"Invalid GUID format: {guid} (contains non-hex characters)");
                    return string.Empty;
                }
            }
            
            return guid.ToLowerInvariant();
        }
        
        private static bool IsHexChar(char c)
        {
            return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }
        
        public static FR2_GUID FromAssetPath(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return Empty;
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            return new FR2_GUID(guid);
        }
        
        public static FR2_GUID FromObject(UnityEngine.Object obj)
        {
            if (obj == null) return Empty;
            string path = AssetDatabase.GetAssetPath(obj);
            return FromAssetPath(path);
        }
        
        public FR2_AssetPath ToAssetPath()
        {
            if (IsEmpty) return FR2_AssetPath.Empty;
            string path = AssetDatabase.GUIDToAssetPath(_value);
            return new FR2_AssetPath(path);
        }
        
        public static readonly FR2_GUID Empty = new FR2_GUID(string.Empty);
        
        // Implicit conversions for backward compatibility
        public static implicit operator string(FR2_GUID guid) => guid.Value;
        public static implicit operator FR2_GUID(string guid) => new FR2_GUID(guid);
        
        // Equality and comparison
        public bool Equals(FR2_GUID other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
        public override bool Equals(object obj) => obj is FR2_GUID other && Equals(other);
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        public override string ToString() => _value ?? string.Empty;
        
        public static bool operator ==(FR2_GUID left, FR2_GUID right) => left.Equals(right);
        public static bool operator !=(FR2_GUID left, FR2_GUID right) => !left.Equals(right);
    }
    
    /// <summary>
    /// Strongly-typed asset path value object to eliminate primitive obsession with strings.
    /// Provides validation, normalization, and path-specific operations.
    /// </summary>
    [Serializable]
    public struct FR2_AssetPath : IEquatable<FR2_AssetPath>
    {
        [SerializeField] private string _value;
        
        public string Value => _value ?? string.Empty;
        public bool IsEmpty => string.IsNullOrEmpty(_value);
        public bool IsValid => !IsEmpty && _value.StartsWith("Assets/");
        public bool IsUserAsset => IsValid && !_value.StartsWith("Packages/");
        public bool IsInResources => IsValid && _value.Contains("/Resources/");
        public bool IsInEditor => IsValid && _value.Contains("/Editor/");
        public bool IsInPlugins => IsValid && _value.Contains("/Plugins/");
        public bool IsInStreamingAssets => IsValid && _value.Contains("/StreamingAssets/");
        
        public string Extension => IsEmpty ? string.Empty : System.IO.Path.GetExtension(_value);
        public string FileName => IsEmpty ? string.Empty : System.IO.Path.GetFileName(_value);
        public string FileNameWithoutExtension => IsEmpty ? string.Empty : System.IO.Path.GetFileNameWithoutExtension(_value);
        public string Directory => IsEmpty ? string.Empty : System.IO.Path.GetDirectoryName(_value);
        
        public FR2_AssetPath(string path)
        {
            _value = ValidateAndNormalize(path);
        }
        
        private static string ValidateAndNormalize(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;
            
            // Normalize path separators
            path = path.Replace('\\', '/');
            
            // Remove duplicate slashes
            while (path.Contains("//"))
            {
                path = path.Replace("//", "/");
            }
            
            // Validate that it's a proper Unity asset path
            if (!path.StartsWith("Assets/") && !path.StartsWith("Packages/"))
            {
                FR2_LOG.LogWarning($"Invalid asset path: {path} (must start with Assets/ or Packages/)");
                return string.Empty;
            }
            
            return path;
        }
        
        public FR2_GUID ToGUID()
        {
            if (IsEmpty) return FR2_GUID.Empty;
            string guid = AssetDatabase.AssetPathToGUID(_value);
            return new FR2_GUID(guid);
        }
        
        public UnityEngine.Object LoadAsset<T>() where T : UnityEngine.Object
        {
            if (IsEmpty) return null;
            return AssetDatabase.LoadAssetAtPath<T>(_value);
        }
        
        public UnityEngine.Object LoadAsset()
        {
            if (IsEmpty) return null;
            return AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_value);
        }
        
        public bool Exists()
        {
            if (IsEmpty) return false;
            return System.IO.File.Exists(_value) || System.IO.Directory.Exists(_value);
        }
        
        public static readonly FR2_AssetPath Empty = new FR2_AssetPath(string.Empty);
        
        // Implicit conversions for backward compatibility
        public static implicit operator string(FR2_AssetPath path) => path.Value;
        public static implicit operator FR2_AssetPath(string path) => new FR2_AssetPath(path);
        
        // Equality and comparison
        public bool Equals(FR2_AssetPath other) => string.Equals(_value, other._value, StringComparison.OrdinalIgnoreCase);
        public override bool Equals(object obj) => obj is FR2_AssetPath other && Equals(other);
        public override int GetHashCode() => _value?.GetHashCode() ?? 0;
        public override string ToString() => _value ?? string.Empty;
        
        public static bool operator ==(FR2_AssetPath left, FR2_AssetPath right) => left.Equals(right);
        public static bool operator !=(FR2_AssetPath left, FR2_AssetPath right) => !left.Equals(right);
    }
    
    /// <summary>
    /// Strongly-typed file size value object with human-readable formatting
    /// </summary>
    [Serializable]
    public struct FR2_FileSize : IEquatable<FR2_FileSize>, IComparable<FR2_FileSize>
    {
        [SerializeField] private long _bytes;
        
        public long Bytes => _bytes;
        public double KB => _bytes / 1024.0;
        public double MB => _bytes / (1024.0 * 1024.0);
        public double GB => _bytes / (1024.0 * 1024.0 * 1024.0);
        
        public FR2_FileSize(long bytes)
        {
            _bytes = Math.Max(0, bytes);
        }
        
        public string ToHumanReadable()
        {
            if (_bytes < 1024) return $"{_bytes} B";
            if (_bytes < 1024 * 1024) return $"{KB:F1} KB";
            if (_bytes < 1024 * 1024 * 1024) return $"{MB:F1} MB";
            return $"{GB:F1} GB";
        }
        
        public static readonly FR2_FileSize Zero = new FR2_FileSize(0);
        
        // Implicit conversions
        public static implicit operator long(FR2_FileSize size) => size._bytes;
        public static implicit operator FR2_FileSize(long bytes) => new FR2_FileSize(bytes);
        
        // Arithmetic operations
        public static FR2_FileSize operator +(FR2_FileSize left, FR2_FileSize right) => new FR2_FileSize(left._bytes + right._bytes);
        public static FR2_FileSize operator -(FR2_FileSize left, FR2_FileSize right) => new FR2_FileSize(left._bytes - right._bytes);
        
        // Comparison operations
        public bool Equals(FR2_FileSize other) => _bytes == other._bytes;
        public override bool Equals(object obj) => obj is FR2_FileSize other && Equals(other);
        public override int GetHashCode() => _bytes.GetHashCode();
        public override string ToString() => ToHumanReadable();
        
        public int CompareTo(FR2_FileSize other) => _bytes.CompareTo(other._bytes);
        
        public static bool operator ==(FR2_FileSize left, FR2_FileSize right) => left.Equals(right);
        public static bool operator !=(FR2_FileSize left, FR2_FileSize right) => !left.Equals(right);
        public static bool operator <(FR2_FileSize left, FR2_FileSize right) => left._bytes < right._bytes;
        public static bool operator >(FR2_FileSize left, FR2_FileSize right) => left._bytes > right._bytes;
        public static bool operator <=(FR2_FileSize left, FR2_FileSize right) => left._bytes <= right._bytes;
        public static bool operator >=(FR2_FileSize left, FR2_FileSize right) => left._bytes >= right._bytes;
    }
}
