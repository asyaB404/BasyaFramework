using System;
using System.Collections.Generic;
using BasyaFramework.Logger;

namespace BasyaFramework.EventCenter
{
    /// <summary>
    /// 简单的全局事件中心,建议外部使用时使用常量枚举定义事件名
    /// </summary>
    public class BxEventCenter
    {
        private static BxEventCenter _instance;

        private readonly Dictionary<string, Delegate> _eventDict = new();

        public static BxEventCenter Instance
        {
            get
            {
                _instance ??= new BxEventCenter();
                return _instance;
            }
        }
        
        public void AddEventListener(string eventName, Delegate callback)
        {
            if (_eventDict.TryGetValue(eventName, out var existingAction))
            {
                try
                {
                    _eventDict[eventName] = Delegate.Combine(existingAction, callback);
                }
                catch (Exception ex)
                {
                    BxDebug.LogWarning($"添加事件监听器失败: {eventName}, 错误: {ex.Message}");
                }
            }
            else
            {
                _eventDict[eventName] = callback;
            }
        }

        public void AddEventListener(string eventName, Action action)
        {
            if (_eventDict.TryGetValue(eventName, out var existingAction))
            {
                _eventDict[eventName] = (Action)existingAction + action;
            }
            else
            {
                _eventDict.Add(eventName, action);
            }
        }
    
        public void AddEventListener<T>(string eventName, Action<T> action)
        {
            if (_eventDict.TryGetValue(eventName, out var existingAction))
            {
                _eventDict[eventName] = (Action<T>)existingAction + action;
            }
            else
            {
                _eventDict.Add(eventName, action);
            }
        }
    
        public void AddEventListener<T, T1>(string eventName, Action<T, T1> action)
        {
            if (_eventDict.TryGetValue(eventName, out var existingAction))
            {
                _eventDict[eventName] = (Action<T, T1>)existingAction + action;
            }
            else
            {
                _eventDict.Add(eventName, action);
            }
        }

        public void RemoveEventListener(string eventName, Action action)
        {
            if (_eventDict.TryGetValue(eventName, out var existingAction))
            {
                var newAction = (Action)existingAction - action;
                if (newAction == null)
                    _eventDict.Remove(eventName);
                else
                    _eventDict[eventName] = newAction;
            }
            else
            {
                BxDebug.LogWarning("-------->   " + eventName + "事件为空,无法被移除");
            }
        }
    
        public void RemoveEventListener<T>(string eventName, Action<T> action)
        {
            if (_eventDict.TryGetValue(eventName, out var existingAction))
            {
                var newAction = (Action<T>)existingAction - action;
                if (newAction == null)
                    _eventDict.Remove(eventName);
                else
                    _eventDict[eventName] = newAction;
            }
            else
            {
                BxDebug.LogWarning("-------->   " + eventName + "事件为空,无法被移除");
            }
        }
    
        public void RemoveEventListener<T, T1>(string eventName, Action<T, T1> action)
        {
            if (_eventDict.TryGetValue(eventName, out var existingAction))
            {
                var newAction = (Action<T, T1>)existingAction - action;
                if (newAction == null)
                    _eventDict.Remove(eventName);
                else
                    _eventDict[eventName] = newAction;
            }
            else
            {
                BxDebug.LogWarning("-------->   " + eventName + "事件为空,无法被移除");
            }
        }
        
        public void RemoveEventListener(string eventName, Delegate callback)
        {
            if (_eventDict.TryGetValue(eventName, out var existingAction))
            {
                try
                {
                    // 获取委托类型并尝试移除
                    var newAction = Delegate.Remove(existingAction, callback);
                    if (newAction == null)
                        _eventDict.Remove(eventName);
                    else
                        _eventDict[eventName] = newAction;
                }
                catch (Exception ex)
                {
                    BxDebug.LogWarning($"移除事件监听器失败: {eventName}, 错误: {ex.Message}");
                }
            }
            else
            {
                BxDebug.LogWarning("-------->   " + eventName + "事件为空,无法被移除");
            }
        }

        public void EventTrigger(string eventName)
        {
            if (_eventDict.TryGetValue(eventName, out var action))
            {
                action.DynamicInvoke();
            }
        }
    
        public void EventTrigger<T>(string eventName, T eventData)
        {
            if (_eventDict.TryGetValue(eventName, out var action))
            {
                action.DynamicInvoke(eventData);
            }
        }
    
        public void EventTrigger<T, T1>(string eventName, T eventData, T1 eventData1)
        {
            if (_eventDict.TryGetValue(eventName, out var action))
            {
                action.DynamicInvoke(eventData, eventData1);
            }
        }
    
        public void Clear(string eventName)
        {
            _eventDict.Remove(eventName);
        }

        public void Clear()
        {
            _eventDict.Clear();
        }
    }
}