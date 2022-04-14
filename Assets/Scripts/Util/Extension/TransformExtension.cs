using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UILib
{
    public static class TransformExtension
    {
        public static void SetParentAndResetTransform(this Transform tr, Transform parent)
        {
            if (tr == null)
                return;

            tr.SetParent(parent);
            tr.ResetTransform();
        }

        public static void ResetTransform(this Transform tr)
        {
            if (tr == null)
                return;

            tr.localRotation = Quaternion.identity;
            tr.localPosition = Vector3.zero;
            tr.localScale = Vector3.one;
        }
    }
}
