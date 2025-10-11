using UnityEngine;

namespace BasyaFramework.Utils
{
    public static partial class Utils
    {
        public static void DestroyAllChildrenImmediate(this Transform transform)
        {
            for (var i = transform.childCount - 1; i >= 0; i--) Object.DestroyImmediate(transform.GetChild(i).gameObject);
        }
        
        public static void DestroyAllChildren(this Transform transform)
        {
            for (var i = transform.childCount - 1; i >= 0; i--) Object.Destroy(transform.GetChild(i).gameObject);
        }
    }
}