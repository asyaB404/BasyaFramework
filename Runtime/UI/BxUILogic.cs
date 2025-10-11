using System;
using System.Collections.Generic;
using BasyaFramework.Core;
using BasyaFramework.Logger;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BasyaFramework.UI
{
    /// <summary>
    /// UI逻辑类，所有UI逻辑类继承自此类
    /// </summary>
    public abstract partial class BxUILogic
    {
        protected BxUIManager UIManager => BxGame.UIManager;
        protected List<GameObject> _loadObjects = new List<GameObject>();

        /// <summary>
        /// 同步加载UI预制体
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public BxUIPanel LoadUI(string path)
        {
            if (GetLevelIndex() == -1)
            {
                BxDebug.LogError("找不到对应层级");
                return null;
            }

            var load = UIManager.LoadUI(path, GetLevelIndex());
            _loadObjects.Add(load.gameObject);
            return load;
        }

        /// <summary>
        /// 异步加载UI预制体
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        public void LoadUIAsync(string path, Action<BxUIPanel> callback)
        {
            if (GetLevelIndex() == -1)
            {
                BxDebug.LogError("找不到对应层级");
                return;
            }

            UIManager.LoadUIAsync(path, GetLevelIndex(), (uiPanel) =>
            {
                callback?.Invoke(uiPanel);
                _loadObjects.Add(uiPanel.gameObject);
            });
        }

        /// <summary>
        /// UIConfig中配置的层级名称
        /// </summary>
        public abstract string UILevel { get; }

        public virtual void OnOpen()
        {
        }

        private int GetLevelIndex()
        {
            string levelName = UILevel;
            if (UIManager.levelOrderPairs != null)
            {
                for (int i = 0; i < UIManager.levelOrderPairs.Count; i++)
                {
                    if (UIManager.levelOrderPairs[i].name == levelName)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public virtual void OnClose()
        {
            for (int i = _loadObjects.Count - 1; i > -1; i--)
            {
                Object.Destroy(_loadObjects[i]);
            }
        }
    }
}