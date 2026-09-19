using System;
using System.Collections.Generic;
using System.IO;

namespace vietlabs.fr2
{
    internal static class FR2_FileExtensions
    {
        private static readonly HashSet<string> ScriptExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".js", ".boo", ".h", ".java", ".cpp", ".m", ".mm", ".cginclude", ".shadersubgraph"
        };

        private static readonly HashSet<string> ReferencableExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".anim", ".controller", ".mat", ".unity", ".guiskin", ".prefab",
            ".asset", ".overrideController", ".mask", ".flare", ".fontsettings",
            ".physicMaterial", ".physicsMaterial2D", ".asset", ".prefs", ".spriteatlas",
            ".mixer", ".playable", ".signal", ".preset", ".shadervariants"
        };

        private static readonly HashSet<string> YamlExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".anim", ".controller", ".mat", ".unity", ".guiskin", ".prefab", ".asset", 
            ".overrideController", ".mask", ".flare", ".fontsettings", ".physicMaterial", 
            ".physicsMaterial2D", ".prefs", ".spriteatlas", ".mixer", ".playable", ".signal", ".preset"
        };

        private static readonly HashSet<string> BinaryExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".fbx", ".dae", ".3ds", ".dxf", ".obj", ".skp", ".max", ".blend"
        };

        private static readonly HashSet<string> ShaderExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".shader", ".cginc", ".hlsl", ".glslinc", ".compute"
        };

        private static readonly HashSet<string> UIToolkitExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".uxml", ".uss"
        };

        private static readonly HashSet<string> MetaExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".meta"
        };

        private static readonly HashSet<string> NonReferenceExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".tiff", ".gif", ".psd", ".exr", ".hdr",
            ".wav", ".mp3", ".ogg", ".aif", ".aiff", ".mod", ".it", ".s3m", ".xm",
            ".ttf", ".otf", ".dfont", ".txt", ".bytes", ".json", ".xml", ".csv"
        };

        internal static bool IsScriptExtension(this string extension)
        {
            return ScriptExtensions.Contains(extension);
        }

        internal static bool IsReferencableExtension(this string extension)
        {
            return ReferencableExtensions.Contains(extension);
        }

        internal static bool IsYamlExtension(this string extension)
        {
            return YamlExtensions.Contains(extension);
        }

        internal static bool IsBinaryExtension(this string extension)
        {
            return BinaryExtensions.Contains(extension);
        }

        internal static bool IsShaderExtension(this string extension)
        {
            return ShaderExtensions.Contains(extension);
        }

        internal static bool IsUIToolkitExtension(this string extension)
        {
            return UIToolkitExtensions.Contains(extension);
        }

        internal static bool IsMetaExtension(this string extension)
        {
            return MetaExtensions.Contains(extension);
        }

        internal static bool IsNonReferenceExtension(this string extension)
        {
            return NonReferenceExtensions.Contains(extension);
        }

        internal static bool IsReadableExtension(this string extension)
        {
            return extension.IsYamlExtension() || extension.IsUIToolkitExtension() || 
                   extension.IsShaderExtension() || extension.IsMetaExtension();
        }
    }
}
