using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace vietlabs.fr2
{
    /// <summary>
    /// Centralized asset filtering logic to eliminate complex conditional statements
    /// scattered throughout the codebase. Provides fluent API for asset validation.
    /// </summary>
    internal static class FR2_AssetFilter
    {
        private static readonly HashSet<string> SPECIAL_USE_ASSETS = new HashSet<string>
        {
            "Assets/link.xml",
            "Assets/csc.rsp",
            "Assets/mcs.rsp",
            "Assets/gmcs.rsp"
        };

        private static readonly HashSet<string> SPECIAL_EXTENSIONS = new HashSet<string>
        {
            ".dll", ".so", ".dylib", ".bundle"
        };

        /// <summary>
        /// Checks if an asset is valid for unused asset scanning
        /// </summary>
        public static bool IsValidForUnusedScan(this FR2_Asset asset, HashSet<string> unusedAssets = null, HashSet<string> addressableAssets = null)
        {
            if (asset == null) return false;
            
            // Skip if already marked as unused
            if (unusedAssets?.Contains(asset.guid) == true) return false;
            
            // Basic exclusion criteria
            if (asset.IsMissing || asset.inEditor || asset.IsScript || 
                asset.inResources || asset.inPlugins || asset.inStreamingAsset || 
                asset.IsFolder) return false;
                
            // Must be in Assets folder
            if (!asset.assetPath.StartsWith("Assets/")) return false;
            
            // Skip forced build inclusions
            if (asset.forcedIncludedInBuild) return false;
            
            // Skip special files
            if (asset.assetName == "LICENSE") return false;
            
            // Skip ignored assets
            if (asset.IsIgnoredAsset()) return false;
            
            // Skip assets with unknown extensions
            if (asset.HasUnknownExtension()) return false;
            
            // Skip special use assets
            if (SPECIAL_USE_ASSETS.Contains(asset.assetPath)) return false;
            
            // Skip special extensions
            if (SPECIAL_EXTENSIONS.Contains(asset.extension)) return false;
            
            // Skip certain asset types
            if (asset.type == FR2_Asset.AssetType.DLL ||
                asset.type == FR2_Asset.AssetType.SCRIPT ||
                asset.type == FR2_Asset.AssetType.UNKNOWN) return false;
                
            // Skip addressable assets
            if (addressableAssets?.Contains(asset.guid) == true) return false;
            
            // Skip excluded assets
            if (asset.IsExcluded) return false;
            
            return true;
        }
        
        /// <summary>
        /// Checks if an asset is in an ignored path
        /// </summary>
        public static bool IsIgnoredAsset(this FR2_Asset asset)
        {
            return FR2_Setting.IgnoreAsset.Any(ignore =>
                asset.assetPath.Equals(ignore, StringComparison.OrdinalIgnoreCase) ||
                asset.assetPath.StartsWith(ignore + "/", StringComparison.OrdinalIgnoreCase)
            );
        }
        
        /// <summary>
        /// Checks if an asset has an unknown or missing extension
        /// </summary>
        public static bool HasUnknownExtension(this FR2_Asset asset)
        {
            string ext = Path.GetExtension(asset.assetPath);
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(asset.assetPath);
            return string.IsNullOrEmpty(ext) || assetType == typeof(DefaultAsset);
        }
        
        /// <summary>
        /// Fluent API for building complex asset filters
        /// </summary>
        public class AssetFilterBuilder
        {
            private readonly List<Func<FR2_Asset, bool>> _filters = new List<Func<FR2_Asset, bool>>();
            
            public AssetFilterBuilder ExcludeIfMissing()
            {
                _filters.Add(asset => !asset.IsMissing);
                return this;
            }
            
            public AssetFilterBuilder ExcludeIfInEditor()
            {
                _filters.Add(asset => !asset.inEditor);
                return this;
            }
            
            public AssetFilterBuilder ExcludeIfScript()
            {
                _filters.Add(asset => !asset.IsScript);
                return this;
            }
            
            public AssetFilterBuilder ExcludeIfInResources()
            {
                _filters.Add(asset => !asset.inResources);
                return this;
            }
            
            public AssetFilterBuilder ExcludeIfInPlugins()
            {
                _filters.Add(asset => !asset.inPlugins);
                return this;
            }
            
            public AssetFilterBuilder RequireInAssetsFolder()
            {
                _filters.Add(asset => asset.assetPath.StartsWith("Assets/"));
                return this;
            }
            
            public AssetFilterBuilder ExcludeIfIgnored()
            {
                _filters.Add(asset => !asset.IsIgnoredAsset());
                return this;
            }
            
            public AssetFilterBuilder ExcludeIfUnknownExtension()
            {
                _filters.Add(asset => !asset.HasUnknownExtension());
                return this;
            }
            
            public AssetFilterBuilder ExcludeIfInSet(HashSet<string> guids)
            {
                if (guids != null)
                {
                    _filters.Add(asset => !guids.Contains(asset.guid));
                }
                return this;
            }
            
            public AssetFilterBuilder ExcludeIfType(params FR2_Asset.AssetType[] types)
            {
                var typeSet = new HashSet<FR2_Asset.AssetType>(types);
                _filters.Add(asset => !typeSet.Contains(asset.type));
                return this;
            }
            
            public bool Test(FR2_Asset asset)
            {
                return _filters.All(filter => filter(asset));
            }
            
            public IEnumerable<FR2_Asset> Filter(IEnumerable<FR2_Asset> assets)
            {
                return assets.Where(Test);
            }
        }
        
        /// <summary>
        /// Creates a new fluent filter builder
        /// </summary>
        public static AssetFilterBuilder CreateFilter()
        {
            return new AssetFilterBuilder();
        }
    }
}
