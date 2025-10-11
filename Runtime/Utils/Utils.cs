using UnityEngine;

namespace BasyaFramework.Utils
{
    public static partial class Utils
    {
        public static void SetActive(GameObject gameObject, bool active)
        {
            if (gameObject != null && gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }
    }
}