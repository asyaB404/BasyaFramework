using System;
using System.Collections.Generic;
using BasyaFramework.Logger;

namespace BasyaFramework.EventCenter
{
    /// <summary>
    /// 简单的全局事件中心,建议外部使用时使用常量枚举定义事件名
    /// 性能优化：使用强类型字典避免 DynamicInvoke，直接调用委托提升性能
    /// </summary>
    public class BxEventCenter
    {
        private static BxEventCenter _instance;

        // 性能优化：使用强类型字典，避免 DynamicInvoke
        private readonly Dictionary<string, Action> _actionDict = new();
        private readonly Dictionary<string, object> _genericActionDict = new(); // 存储泛型 Action 的包装

        public static BxEventCenter Instance
        {
            get
            {
                _instance ??= new BxEventCenter();
                return _instance;
            }
        }
        
        #region 添加事件监听器

        public void AddEventListener(string eventName, Delegate callback)
        {
            // 尝试根据委托类型添加到对应的字典
            if (callback is Action action)
            {
                AddEventListener(eventName, action);
            }
            else
            {
                BxDebug.LogWarning($"不支持的委托类型: {callback.GetType()}, 事件名: {eventName}");
            }
        }

        public void AddEventListener(string eventName, Action action)
        {
            if (_actionDict.TryGetValue(eventName, out var existingAction))
            {
                _actionDict[eventName] = existingAction + action;
            }
            else
            {
                _actionDict[eventName] = action;
            }
        }
    
        public void AddEventListener<T>(string eventName, Action<T> action)
        {
            string key = GetGenericKey(eventName, typeof(T));
            if (_genericActionDict.TryGetValue(key, out var existingObj))
            {
                if (existingObj is Action<T> existingAction)
                {
                    _genericActionDict[key] = existingAction + action;
                }
                else
                {
                    BxDebug.LogWarning($"事件类型不匹配: {eventName}, 期望: {typeof(Action<T>)}, 实际: {existingObj.GetType()}");
                }
            }
            else
            {
                _genericActionDict[key] = action;
            }
        }
    
        public void AddEventListener<T, T1>(string eventName, Action<T, T1> action)
        {
            string key = GetGenericKey(eventName, typeof(T), typeof(T1));
            if (_genericActionDict.TryGetValue(key, out var existingObj))
            {
                if (existingObj is Action<T, T1> existingAction)
                {
                    _genericActionDict[key] = existingAction + action;
                }
                else
                {
                    BxDebug.LogWarning($"事件类型不匹配: {eventName}, 期望: {typeof(Action<T, T1>)}, 实际: {existingObj.GetType()}");
                }
            }
            else
            {
                _genericActionDict[key] = action;
            }
        }

        #endregion

        #region 移除事件监听器

        public void RemoveEventListener(string eventName, Action action)
        {
            if (_actionDict.TryGetValue(eventName, out var existingAction))
            {
                var newAction = existingAction - action;
                if (newAction == null)
                    _actionDict.Remove(eventName);
                else
                    _actionDict[eventName] = newAction;
            }
            else
            {
                BxDebug.LogWarning($"事件为空,无法被移除: {eventName}");
            }
        }
    
        public void RemoveEventListener<T>(string eventName, Action<T> action)
        {
            string key = GetGenericKey(eventName, typeof(T));
            if (_genericActionDict.TryGetValue(key, out var existingObj))
            {
                if (existingObj is Action<T> existingAction)
                {
                    var newAction = existingAction - action;
                    if (newAction == null)
                        _genericActionDict.Remove(key);
                    else
                        _genericActionDict[key] = newAction;
                }
            }
            else
            {
                BxDebug.LogWarning($"事件为空,无法被移除: {eventName}");
            }
        }
    
        public void RemoveEventListener<T, T1>(string eventName, Action<T, T1> action)
        {
            string key = GetGenericKey(eventName, typeof(T), typeof(T1));
            if (_genericActionDict.TryGetValue(key, out var existingObj))
            {
                if (existingObj is Action<T, T1> existingAction)
                {
                    var newAction = existingAction - action;
                    if (newAction == null)
                        _genericActionDict.Remove(key);
                    else
                        _genericActionDict[key] = newAction;
                }
            }
            else
            {
                BxDebug.LogWarning($"事件为空,无法被移除: {eventName}");
            }
        }
        
        public void RemoveEventListener(string eventName, Delegate callback)
        {
            if (callback is Action action)
            {
                RemoveEventListener(eventName, action);
            }
            else
            {
                BxDebug.LogWarning($"不支持的委托类型: {callback.GetType()}, 事件名: {eventName}");
            }
        }

        #endregion

        #region 触发事件（性能优化：直接调用，避免 DynamicInvoke）

        public void EventTrigger(string eventName)
        {
            if (_actionDict.TryGetValue(eventName, out var action))
            {
                action?.Invoke();
            }
        }
    
        public void EventTrigger<T>(string eventName, T eventData)
        {
            string key = GetGenericKey(eventName, typeof(T));
            if (_genericActionDict.TryGetValue(key, out var actionObj))
            {
                if (actionObj is Action<T> action)
                {
                    action?.Invoke(eventData);
                }
            }
        }
    
        public void EventTrigger<T, T1>(string eventName, T eventData, T1 eventData1)
        {
            string key = GetGenericKey(eventName, typeof(T), typeof(T1));
            if (_genericActionDict.TryGetValue(key, out var actionObj))
            {
                if (actionObj is Action<T, T1> action)
                {
                    action?.Invoke(eventData, eventData1);
                }
            }
        }

        #endregion

        #region 工具方法

        /// <summary>
        /// 生成泛型事件的唯一键
        /// </summary>
        private string GetGenericKey(string eventName, Type type1)
        {
            return $"{eventName}__{type1.FullName}";
        }

        /// <summary>
        /// 生成泛型事件的唯一键（双参数）
        /// </summary>
        private string GetGenericKey(string eventName, Type type1, Type type2)
        {
            return $"{eventName}__{type1.FullName}__{type2.FullName}";
        }

        #endregion

        #region 清理方法

        public void Clear(string eventName)
        {
            _actionDict.Remove(eventName);
            // 清理所有相关的泛型事件（以 eventName 开头的键）
            var keysToRemove = new List<string>();
            foreach (var key in _genericActionDict.Keys)
            {
                if (key.StartsWith(eventName + "__"))
                {
                    keysToRemove.Add(key);
                }
            }
            foreach (var key in keysToRemove)
            {
                _genericActionDict.Remove(key);
            }
        }

        public void Clear()
        {
            _actionDict.Clear();
            _genericActionDict.Clear();
        }

        #endregion
    }
}
