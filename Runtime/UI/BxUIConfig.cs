using UnityEngine;

namespace BasyaFramework.UI
{
    [System.Serializable]
    public class BxUIConfig
    {
        /// <summary>
        /// 层级枚举
        /// </summary>
        public string[] panelLevels;

        /// <summary>
        /// 层级对应的order
        /// </summary>
        /// <returns></returns>
        public int[] panelLevelOrder;

        /// <summary>
        /// 画布
        /// </summary>
        public Canvas canvas;
        
        /// <summary>
        /// UI相机
        /// </summary>
        public Camera uiCamera;
    }
}