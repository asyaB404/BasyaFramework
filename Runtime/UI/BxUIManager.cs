using System;
using System.Collections.Generic;
using BasyaFramework.Core;
using BasyaFramework.Logger;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BasyaFramework.UI
{
    /// <summary>
    /// UI管理器
    /// </summary>
    /// <example>
    /// 1.使用流程为打开UILogic类,UILogic类中包含加载预制体逻辑，得到UI预制体面板<br></br>
    /// 2.UI预制体面板类有编辑器方法，能够快速完成UI控件绑定和生成控件代码初始化方法，默认粘贴至剪切板中<br></br>
    /// </example>
    public class BxUIManager
    {
        private BxUIConfig _uiConfig;
        public BxUIConfig uiConfig => _uiConfig;
        private List<(string name, int order)> _levelOrderPairs;
        public List<(string name, int order)> levelOrderPairs => _levelOrderPairs;
        private List<RectTransform> _panelLevels;

        private Dictionary<Type, BxUILogic> _openLogics;


        public BxUIManager(BxUIConfig uiConfig)
        {
            _uiConfig = uiConfig;
            Object.DontDestroyOnLoad(_uiConfig.canvas.gameObject);
            Object.DontDestroyOnLoad(_uiConfig.uiCamera.gameObject);
            _panelLevels = new List<RectTransform>(_uiConfig.panelLevels.Length);
            _openLogics = new Dictionary<Type, BxUILogic>();

            // 检查panelLevelOrder数组是否有效
            if (_uiConfig.panelLevelOrder == null || _uiConfig.panelLevelOrder.Length != _uiConfig.panelLevels.Length)
            {
                // 如果panelLevelOrder数组无效，则使用默认顺序
                foreach (var panelLevel in _uiConfig.panelLevels)
                {
                    GameObject panelLevelGameObject = new(panelLevel);
                    panelLevelGameObject.transform.SetParent(_uiConfig.canvas.transform, false);
                    _panelLevels.Add(panelLevelGameObject.GetComponent<RectTransform>());
                }
            }
            else
            {
                // 创建层级名称和order的配对列表
                _levelOrderPairs = new List<(string, int)>();
                for (int i = 0; i < _uiConfig.panelLevels.Length; i++)
                {
                    levelOrderPairs.Add((_uiConfig.panelLevels[i], _uiConfig.panelLevelOrder[i]));
                }

                // 按照order值升序排序（order低的先添加，这样在transform中会排在前面）
                levelOrderPairs.Sort((a, b) => a.order.CompareTo(b.order));

                // 按照排序后的顺序创建层级GameObject
                foreach (var (name, order) in levelOrderPairs)
                {
                    GameObject panelLevelGameObject = new(name);
                    panelLevelGameObject.transform.SetParent(_uiConfig.canvas.transform, false);
                    
                    var rect = panelLevelGameObject.AddComponent<RectTransform>();
                    rect.anchorMin = Vector2.zero;   // 左下角锚点
                    rect.anchorMax = Vector2.one;    // 右上角锚点
                    rect.offsetMin = Vector2.zero;   // Left和Bottom设为0
                    rect.offsetMax = Vector2.zero;   // Right和Top设为0
                    rect.anchoredPosition = Vector2.zero;
                    _panelLevels.Add(rect);
                }
            }
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public static T Open<T>() where T : BxUILogic, new()
        {
            if (BxGame.UIManager._openLogics.TryGetValue(typeof(T), out BxUILogic result))
            {
                return result as T;
            }

            T logic = new T();
            BxGame.UIManager._openLogics.Add(typeof(T), logic);
            logic.OnOpen();
            return logic;
        }

        public static void Close<T>() where T : BxUILogic
        {
            if (BxGame.UIManager._openLogics.TryGetValue(typeof(T), out BxUILogic logic))
            {
                logic.OnClose();
                BxGame.UIManager._openLogics.Remove(typeof(T));
            }
        }

        public static T GetUILogic<T>() where T : BxUILogic
        {
            if (BxGame.UIManager._openLogics.TryGetValue(typeof(T), out BxUILogic logic))
            {
                return logic as T;
            }
            
            return null;
        }
        
        public BxUIPanel LoadUI(string path, int levelIndex)
        {
            var panelObj = BxGame.ResourceManager.Load<GameObject>(path);
            var bxUIPanel = panelObj.GetComponent<BxUIPanel>();
            if (!bxUIPanel)
            {
                Object.Destroy(panelObj);
                BxDebug.LogError("预制体缺少BxUIPanel组件: " + path);
                return null;
            }

            bxUIPanel.transform.SetParent(_panelLevels[levelIndex], false);

            return bxUIPanel;
        }

        public void LoadUIAsync(string path, int levelIndex, Action<BxUIPanel> callback)
        {
            BxGame.ResourceManager.LoadAsync<GameObject>(path, (panelObj) =>
            {
                var bxUIPanel = panelObj.GetComponent<BxUIPanel>();
                if (!bxUIPanel)
                {
                    Object.Destroy(panelObj);
                    BxDebug.LogError("预制体缺少BxUIPanel组件: " + path);
                    return;
                }

                bxUIPanel.transform.SetParent(_panelLevels[levelIndex], false);
                if (callback != null)
                    callback(bxUIPanel);
            });
        }
    }
}