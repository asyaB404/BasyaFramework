using System;
using System.Collections.Generic;
using BasyaFramework.Logger;
using UnityEngine;

namespace BasyaFramework.Timer
{
    /// <summary>
    /// 全局计时器管理：基于 <see cref="Time.unscaledDeltaTime"/> 累加，快照迭代避免回调内改表。
    /// </summary>
    public class BxTimerManager
    {
        private int _timeId;
        private readonly Dictionary<int, TimerEntry> _timeDic = new();
        private readonly Dictionary<int, TimerEntry> _timeDicAct = new();
        private readonly List<int> _removeList = new();

        private sealed class TimerEntry
        {
            public Action act_void;
            public Action<double> act;
            public bool b_MonoBehaviour;
            public double dt;
            public double sdt;
            public double duration;
            public double total;

            public TimerEntry(Action cb, double duration, double total)
            {
                act_void = cb;
                act = null;
                this.duration = duration;
                this.total = total;
                dt = 0;
                sdt = 0;
                b_MonoBehaviour = cb != null && cb.Target is MonoBehaviour;
            }

            public TimerEntry(Action<double> cb, double duration, double total)
            {
                act = cb;
                act_void = null;
                this.duration = duration;
                this.total = total;
                dt = 0;
                sdt = 0;
                b_MonoBehaviour = cb != null && cb.Target is MonoBehaviour;
            }
        }

        /// <summary>
        /// 由 <c>BxGame.Update</c> 每帧调用一次；需保证全局每帧只进入一次。
        /// </summary>
        public void Update()
        {
            var t = Time.unscaledDeltaTime;
            _removeList.Clear();
            _timeDicAct.Clear();

            foreach (var kv in _timeDic)
            {
                _timeDicAct.Add(kv.Key, kv.Value);
            }

            foreach (var kv in _timeDicAct)
            {
                var timer = kv.Value;

                if (timer.act_void != null && timer.act_void.Target != null)
                {
                    if (timer.b_MonoBehaviour)
                    {
                        var mb = timer.act_void.Target as MonoBehaviour;
                        if (mb == null)
                        {
                            _removeList.Add(kv.Key);
                            continue;
                        }
                    }
                }

                if (timer.act != null && timer.act.Target != null)
                {
                    if (timer.b_MonoBehaviour)
                    {
                        var mb = timer.act.Target as MonoBehaviour;
                        if (mb == null)
                        {
                            _removeList.Add(kv.Key);
                            continue;
                        }
                    }
                }

                timer.dt += t;
                timer.sdt += t;
                var bcall = false;
                if (timer.dt >= timer.duration)
                {
                    timer.dt = 0;

                    if (timer.act_void != null)
                    {
                        timer.act_void.Invoke();
                    }
                    else
                    {
                        var left = timer.total - timer.sdt;
                        var remainder = left % timer.duration;
                        if (remainder > timer.duration / 2)
                        {
                            left = left - remainder + timer.duration;
                        }
                        else
                        {
                            left = left - remainder;
                        }

                        if (left <= 0)
                        {
                            left = 0;
                            bcall = true;
                        }

                        timer.act?.Invoke(left);
                    }
                }

                if (timer.total > 0)
                {
                    if (timer.sdt >= timer.total)
                    {
                        _removeList.Add(kv.Key);
                        if (!bcall && timer.act != null)
                        {
                            timer.act.Invoke(0);
                        }
                    }
                }
            }

            foreach (var tid in _removeList)
            {
                if (_timeDic.Remove(tid))
                {
                    BxDebug.Log($"remove timer {tid}");
                }
            }
        }

        /// <summary>
        /// 按固定间隔执行 <paramref name="cb"/>。
        /// </summary>
        /// <param name="duration">间隔（秒，不受 timeScale 影响）</param>
        /// <param name="cb">回调</param>
        /// <param name="loop">true 时无限循环；false 时总共触发一次（total 与 duration 相同）</param>
        /// <param name="callImmediately">为 true 时在注册时立即调用一次 <paramref name="cb"/></param>
        /// <returns>定时器 id，用于 <see cref="DelTimer"/></returns>
        public int AddTimer(double duration, Action cb, bool loop = true, bool callImmediately = false)
        {
            var tid = ++_timeId;

            var timer = new TimerEntry(cb, duration, loop ? -1 : duration);
            _timeDic[tid] = timer;

            if (callImmediately)
                cb?.Invoke();

            return tid;
        }

        /// <summary>
        /// 在 <paramref name="total"/> 时间内按 <paramref name="duration"/> 节拍报告剩余量（回调参数为取整后的剩余时间）。
        /// </summary>
        public int AddTimer(double total, Action<double> cb, float duration = 1.0f, bool callImmediately = true)
        {
            var tid = ++_timeId;

            var timer = new TimerEntry(cb, duration, total);
            _timeDic[tid] = timer;

            if (callImmediately)
                cb?.Invoke(total);

            return tid;
        }

        public void DelTimer(int tid)
        {
            if (_timeDic.ContainsKey(tid))
            {
                _timeDic.Remove(tid);
                BxDebug.Log($"remove timer {tid}");
            }
        }

        public bool HasTimer(int tid)
        {
            return _timeDic.ContainsKey(tid);
        }
    }
}
