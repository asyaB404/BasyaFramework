using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BasyaFramework.ResourceManager
{
    public class DefaultResourceManager : IResourceManager
    {
        //TODO:对象池优化
        public T Load<T>(string path) where T : Object
        {
            var load = Resources.Load<T>(path);
            if (load is GameObject)
            {
                return Object.Instantiate(load);
            }
            return load;
        }

        private async UniTask<T> LoadAsync<T>(string path) where T : Object
        {
            var asyncOperation = Resources.LoadAsync<T>(path);
            await asyncOperation;
            var load = asyncOperation.asset as T;
            if (load is GameObject)
            {
                return Object.Instantiate(load);
            }
            return load;
        }
        
        public void LoadAsync<T>(string path, Action<T> callback) where T : Object
        {
            LoadAsync<T>(path).ContinueWith(callback);
        }

        public void Clear() 
        {
            Resources.UnloadUnusedAssets();
        }
    }
}