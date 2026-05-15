using UnityEngine;

namespace BasyaFramework.Timer
{
    /// <summary>
    /// 每帧驱动 <see cref="BxTimerManager"/>，由管理器在首次添加定时器时创建。
    /// </summary>
    internal sealed class BxTimerDriver : MonoBehaviour
    {
        private void Update()
        {
            BxTimerManager.Tick();
        }
    }
}
