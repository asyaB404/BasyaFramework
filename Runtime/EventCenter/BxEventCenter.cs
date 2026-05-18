using System;
using System.Collections.Generic;
using System.Threading;
using BasyaFramework.Logger;

namespace BasyaFramework.EventCenter
{
    /// <summary>
    /// 按枚举类型隔离的事件总线：每种 <typeparamref name="TEnum"/> 使用独立的 <see cref="Instance"/>。
    /// 建议在程序集或模块内自定义枚举；无参桶键为枚举值，有参桶键为枚举 + 参数类型，派发为强类型 <c>Invoke</c>，不使用 <c>DynamicInvoke</c>。
    /// </summary>
    public sealed class BxEventCenter<TEnum> where TEnum : struct, Enum
    {
        private static readonly Lazy<BxEventCenter<TEnum>> LazyInstance =
            new(() => new BxEventCenter<TEnum>(), LazyThreadSafetyMode.ExecutionAndPublication);

        public static BxEventCenter<TEnum> Instance => LazyInstance.Value;

        private readonly Dictionary<TEnum, Action> _actionDict = new();
        private readonly Dictionary<(TEnum, Type), Delegate> _oneArgDict = new();
        private readonly Dictionary<(TEnum, Type, Type), Delegate> _twoArgDict = new();

        #region 添加事件监听器

        public void AddEventListener(TEnum eventId, Delegate callback)
        {
            if (callback is Action action)
                AddEventListener(eventId, action);
            else
                BxDebug.LogWarning($"不支持的委托类型: {callback.GetType()}, 事件: {eventId}");
        }

        public void AddEventListener(TEnum eventId, Action action)
        {
            if (_actionDict.TryGetValue(eventId, out var existingAction))
                _actionDict[eventId] = existingAction + action;
            else
                _actionDict[eventId] = action;
        }

        public void AddEventListener<T>(TEnum eventId, Action<T> action)
        {
            var key = (eventId, typeof(T));
            if (_oneArgDict.TryGetValue(key, out var existing))
            {
                if (existing is Action<T> existingAction)
                    _oneArgDict[key] = existingAction + action;
                else
                    BxDebug.LogWarning($"事件类型不匹配: {eventId}, 期望: {typeof(Action<T>)}, 实际: {existing.GetType()}");
            }
            else
            {
                _oneArgDict[key] = action;
            }
        }

        public void AddEventListener<T, T1>(TEnum eventId, Action<T, T1> action)
        {
            var key = (eventId, typeof(T), typeof(T1));
            if (_twoArgDict.TryGetValue(key, out var existing))
            {
                if (existing is Action<T, T1> existingAction)
                    _twoArgDict[key] = existingAction + action;
                else
                    BxDebug.LogWarning($"事件类型不匹配: {eventId}, 期望: {typeof(Action<T, T1>)}, 实际: {existing.GetType()}");
            }
            else
            {
                _twoArgDict[key] = action;
            }
        }

        #endregion

        #region 移除事件监听器

        public void RemoveEventListener(TEnum eventId, Action action)
        {
            if (!_actionDict.TryGetValue(eventId, out var existingAction))
                return;

            var newAction = existingAction - action;
            if (newAction == null)
                _actionDict.Remove(eventId);
            else
                _actionDict[eventId] = newAction;
        }

        public void RemoveEventListener<T>(TEnum eventId, Action<T> action)
        {
            var key = (eventId, typeof(T));
            if (!_oneArgDict.TryGetValue(key, out var existing))
                return;

            if (existing is not Action<T> existingAction)
                return;

            var newAction = existingAction - action;
            if (newAction == null)
                _oneArgDict.Remove(key);
            else
                _oneArgDict[key] = newAction;
        }

        public void RemoveEventListener<T, T1>(TEnum eventId, Action<T, T1> action)
        {
            var key = (eventId, typeof(T), typeof(T1));
            if (!_twoArgDict.TryGetValue(key, out var existing))
                return;

            if (existing is not Action<T, T1> existingAction)
                return;

            var newAction = existingAction - action;
            if (newAction == null)
                _twoArgDict.Remove(key);
            else
                _twoArgDict[key] = newAction;
        }

        public void RemoveEventListener(TEnum eventId, Delegate callback)
        {
            if (callback is Action action)
                RemoveEventListener(eventId, action);
            else
                BxDebug.LogWarning($"不支持的委托类型: {callback.GetType()}, 事件: {eventId}");
        }

        #endregion

        #region 触发事件

        public void EventTrigger(TEnum eventId)
        {
            if (_actionDict.TryGetValue(eventId, out var action))
                action?.Invoke();
        }

        public void EventTrigger<T>(TEnum eventId, T eventData)
        {
            var key = (eventId, typeof(T));
            if (_oneArgDict.TryGetValue(key, out var del) && del is Action<T> action)
                action?.Invoke(eventData);
        }

        public void EventTrigger<T, T1>(TEnum eventId, T eventData, T1 eventData1)
        {
            var key = (eventId, typeof(T), typeof(T1));
            if (_twoArgDict.TryGetValue(key, out var del) && del is Action<T, T1> action)
                action?.Invoke(eventData, eventData1);
        }

        #endregion

        #region 清理方法

        public void Clear(TEnum eventId)
        {
            _actionDict.Remove(eventId);

            var oneKeys = new List<(TEnum, Type)>();
            foreach (var key in _oneArgDict.Keys)
            {
                if (EqualityComparer<TEnum>.Default.Equals(key.Item1, eventId))
                    oneKeys.Add(key);
            }

            foreach (var key in oneKeys)
                _oneArgDict.Remove(key);

            var twoKeys = new List<(TEnum, Type, Type)>();
            foreach (var key in _twoArgDict.Keys)
            {
                if (EqualityComparer<TEnum>.Default.Equals(key.Item1, eventId))
                    twoKeys.Add(key);
            }

            foreach (var key in twoKeys)
                _twoArgDict.Remove(key);
        }

        public void Clear()
        {
            _actionDict.Clear();
            _oneArgDict.Clear();
            _twoArgDict.Clear();
        }

        #endregion
    }
}
