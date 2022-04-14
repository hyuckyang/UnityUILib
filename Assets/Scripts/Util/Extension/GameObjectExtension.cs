using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UILib
{
    public static class GameObjectExtension 
    {
        public static void ForeachFindChildComponent<T>(this GameObject gameObject, Action<T> action) where T : Component
        {
            var comp = gameObject.GetComponentsInChildren<T>(true);
            if (comp == null || comp.Length <= 0) 
                return;
            
            foreach (var c in comp)
            {
                action?.Invoke(c);
            }
        }
    }
}

