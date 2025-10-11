using System;
using Object = UnityEngine.Object;

namespace BasyaFramework.ResourceManager
{
    /// <summary>
    /// 资源管理器接口
    /// </summary>
    public interface IResourceManager
    {
        T Load<T>(string path) where T : Object;
        void LoadAsync<T>(string path, Action<T> callback) where T : Object;
        void Clear();
    }
}