using System;
using System.Collections;
using System.Collections.Generic;
using BasyaFramework.EventCenter;
using BasyaFramework.Logger;
using UnityEngine;

namespace BasyaFramework.Core
{
    /// <summary>
    /// 基础行为类，继承自MonoBehaviour，提供常用的封装功能
    /// </summary>
    public class BxBehaviour : MonoBehaviour
    {
        #region 生命周期管理

        /// <summary>
        /// 初始化标记
        /// </summary>
        private bool _isInitialized;

        /// <summary>
        /// 生命周期事件列表
        /// </summary>
        private readonly List<IEnumerator> _lifeCycleCoroutines = new List<IEnumerator>();

        private void Awake()
        {
            OnInit();
        }

        private void Start()
        {
            OnStart();
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized)
                return;
            OnUpdate(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (!_isInitialized)
                return;
            OnFixedUpdate(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            if (!_isInitialized)
                return;
            OnLateUpdate(Time.deltaTime);
        }

        private void OnDestroy()
        {
            OnDispose();
            UnsubscribeAllEvents();
            StopAllCoroutines();
            _lifeCycleCoroutines.Clear();
        }

        /// <summary>
        /// 初始化回调
        /// </summary>
        protected virtual void OnInit()
        {
        }

        /// <summary>
        /// 开始回调
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// 更新回调
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        protected virtual void OnUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// 固定更新回调
        /// </summary>
        /// <param name="fixedDeltaTime">固定时间增量</param>
        protected virtual void OnFixedUpdate(float fixedDeltaTime)
        {
        }

        /// <summary>
        /// 延迟更新回调
        /// </summary>
        /// <param name="deltaTime">时间增量</param>
        protected virtual void OnLateUpdate(float deltaTime)
        {
        }

        /// <summary>
        /// 销毁回调
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        #endregion

        #region 事件中心

        private readonly List<Action> _eventUnsubscribers = new List<Action>();

        /// <summary>
        /// 订阅无参事件；销毁时自动退订。
        /// </summary>
        protected void SubscribeEvent<TEnum>(TEnum eventId, Action callback) where TEnum : struct, Enum
        {
            var hub = BxEventCenter<TEnum>.Instance;
            hub.AddEventListener(eventId, callback);
            _eventUnsubscribers.Add(() => hub.RemoveEventListener(eventId, callback));
        }

        /// <summary>
        /// 订阅单参数事件；销毁时自动退订。
        /// </summary>
        protected void SubscribeEvent<TEnum, T>(TEnum eventId, Action<T> callback) where TEnum : struct, Enum
        {
            var hub = BxEventCenter<TEnum>.Instance;
            hub.AddEventListener(eventId, callback);
            _eventUnsubscribers.Add(() => hub.RemoveEventListener(eventId, callback));
        }

        /// <summary>
        /// 订阅双参数事件；销毁时自动退订。
        /// </summary>
        protected void SubscribeEvent<TEnum, T, T1>(TEnum eventId, Action<T, T1> callback) where TEnum : struct, Enum
        {
            var hub = BxEventCenter<TEnum>.Instance;
            hub.AddEventListener(eventId, callback);
            _eventUnsubscribers.Add(() => hub.RemoveEventListener(eventId, callback));
        }

        protected void UnsubscribeEvent<TEnum>(TEnum eventId, Action callback) where TEnum : struct, Enum
        {
            BxEventCenter<TEnum>.Instance.RemoveEventListener(eventId, callback);
        }

        protected void UnsubscribeEvent<TEnum, T>(TEnum eventId, Action<T> callback) where TEnum : struct, Enum
        {
            BxEventCenter<TEnum>.Instance.RemoveEventListener(eventId, callback);
        }

        protected void UnsubscribeEvent<TEnum, T, T1>(TEnum eventId, Action<T, T1> callback) where TEnum : struct, Enum
        {
            BxEventCenter<TEnum>.Instance.RemoveEventListener(eventId, callback);
        }

        protected void EventTrigger<TEnum>(TEnum eventId) where TEnum : struct, Enum
        {
            BxEventCenter<TEnum>.Instance.EventTrigger(eventId);
        }

        protected void EventTrigger<TEnum, T>(TEnum eventId, T eventData) where TEnum : struct, Enum
        {
            BxEventCenter<TEnum>.Instance.EventTrigger(eventId, eventData);
        }

        protected void EventTrigger<TEnum, T, T1>(TEnum eventId, T eventData, T1 eventData1) where TEnum : struct, Enum
        {
            BxEventCenter<TEnum>.Instance.EventTrigger(eventId, eventData, eventData1);
        }

        private void UnsubscribeAllEvents()
        {
            for (var i = _eventUnsubscribers.Count - 1; i >= 0; i--)
                _eventUnsubscribers[i]?.Invoke();

            _eventUnsubscribers.Clear();
        }

        #endregion

        #region 协程管理

        /// <summary>
        /// 启动协程并添加到生命周期管理
        /// </summary>
        /// <param name="coroutine">协程</param>
        /// <returns>协程引用</returns>
        protected Coroutine StartLifeCycleCoroutine(IEnumerator coroutine)
        {
            _lifeCycleCoroutines.Add(coroutine);
            return StartCoroutine(coroutine);
        }

        /// <summary>
        /// 停止生命周期协程
        /// </summary>
        /// <param name="coroutine">协程</param>
        protected void StopLifeCycleCoroutine(IEnumerator coroutine)
        {
            if (_lifeCycleCoroutines.Contains(coroutine))
            {
                StopCoroutine(coroutine);
                _lifeCycleCoroutines.Remove(coroutine);
            }
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="delay">延迟时间（秒）</param>
        /// <param name="action">执行的动作</param>
        protected Coroutine Delay(float delay, Action action)
        {
            return StartCoroutine(DelayCoroutine(delay, action));
        }

        /// <summary>
        /// 延迟执行协程
        /// </summary>
        /// <param name="delay">延迟时间（秒）</param>
        /// <param name="action">执行的动作</param>
        /// <returns>协程迭代器</returns>
        private IEnumerator DelayCoroutine(float delay, Action action)
        {
            yield return new WaitForSeconds(delay);
            action?.Invoke();
        }

        /// <summary>
        /// 帧延迟执行
        /// </summary>
        /// <param name="frameCount">延迟帧数</param>
        /// <param name="action">执行的动作</param>
        protected Coroutine DelayFrame(int frameCount, Action action)
        {
            return StartCoroutine(DelayFrameCoroutine(frameCount, action));
        }

        /// <summary>
        /// 帧延迟执行协程
        /// </summary>
        /// <param name="frameCount">延迟帧数</param>
        /// <param name="action">执行的动作</param>
        /// <returns>协程迭代器</returns>
        private IEnumerator DelayFrameCoroutine(int frameCount, Action action)
        {
            for (int i = 0; i < frameCount; i++)
            {
                yield return null;
            }

            action?.Invoke();
        }

        #endregion

        #region 游戏对象管理

        /// <summary>
        /// 安全设置激活状态
        /// </summary>
        /// <param name="active">激活状态</param>
        protected void SetActiveSafe(bool active)
        {
            if (gameObject != null && gameObject.activeSelf != active)
            {
                gameObject.SetActive(active);
            }
        }

        /// <summary>
        /// 安全销毁游戏对象
        /// </summary>
        /// <param name="obj">游戏对象</param>
        /// <param name="delay">延迟时间（秒）</param>
        protected void DestroySafe(GameObject obj, float delay = 0f)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                {
                    if (delay > 0f)
                    {
                        Destroy(obj, delay);
                    }
                    else
                    {
                        Destroy(obj);
                    }
                }
                else
                {
                    DestroyImmediate(obj);
                }
            }
        }

        /// <summary>
        /// 安全销毁组件
        /// </summary>
        /// <param name="component">组件</param>
        /// <param name="delay">延迟时间（秒）</param>
        protected void DestroyComponentSafe(Component component, float delay = 0f)
        {
            if (component != null && component.gameObject != null)
            {
                if (Application.isPlaying)
                {
                    if (delay > 0f)
                    {
                        Destroy(component, delay);
                    }
                    else
                    {
                        Destroy(component);
                    }
                }
                else
                {
                    DestroyImmediate(component);
                }
            }
        }

        #endregion
    }
}