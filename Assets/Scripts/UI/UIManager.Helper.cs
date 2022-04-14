using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UILib
{
    public partial class UIManager
    {
        private string GetPrefabAssetPath<T>() where T : MonoBehaviour
        {
            return GetPrefabPath<T>("AssetPath");
        }

        private string GetPrefabAssetPath(Type t)
        {
            var property = GetPropertyValue(t, "AssetPath");
            if (property is string assetPath)
            {
                return assetPath.ToLower();
            }
            return string.Empty;
        }

        private string GetPrefabAssetName<T>() where T : MonoBehaviour
        {
            var assetPath = GetPrefabAssetPath<T>();
            return string.IsNullOrEmpty(assetPath) ? string.Empty : GetPrefabAssetName(assetPath);
        }

        private string GetPrefabAssetName(Type t)
        {
            var assetPath = GetPrefabAssetPath(t);
            if (string.IsNullOrEmpty(assetPath))
                return string.Empty;

            return string.IsNullOrEmpty(assetPath) ? string.Empty : GetPrefabAssetName(assetPath);
        }

        private string GetPrefabAssetName(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            
            var split = path.Split('/');
            return split.Length <= 0 ? string.Empty : split[split.Length - 1];
        }
        
        private string GetPrefabPath<T>(string path) where T : MonoBehaviour
        {
            var property = GetPropertyValue<T>(path);
            if (property is string assetPath)
            {
                return assetPath.ToLower();
            }
            return string.Empty;
        }
        
        private object GetPropertyValue<T>(string property)
        {
            var path = typeof(T).GetProperty(property);
            return path == null ? null : path.GetValue(null, null);
        }
        
        private object GetPropertyValue(Type t, string property)
        {
            var path = t.GetProperty(property);
            return path == null ? null : path.GetValue(null, null);
        }
    }
}
