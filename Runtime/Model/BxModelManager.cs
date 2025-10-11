using System;
using System.Collections.Generic;
using System.Reflection;
using BasyaFramework.Logger;

namespace BasyaFramework.Model
{
    /// <summary>
    /// Model管理器
    /// </summary>
    /// <example>
    /// 游戏开始时会通过反射自动构造所有继承了BxModel的Model实例<br></br>
    /// Model实例会自动初始化，并添加到字典中<br></br>
    /// Model管理器在Runtime期间运行，会自动调用所有模型实例的Update/等回调方法
    /// </example>
    public class BxModelManager
    {
        private readonly Dictionary<Type, BxModel> _modelMap = new Dictionary<Type, BxModel>();
        
        /// <summary>
        /// 获取指定类型的模型实例
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <returns>模型实例，如果未找到则返回null</returns>
        public T GetModel<T>() where T : BxModel
        {
            if (_modelMap.TryGetValue(typeof(T), out BxModel model))
            {
                return (T)model;
            }
            return null;
        }

        public void Init()
        {
            InitModels();
        }

        public void Update()
        {
            foreach (BxModel model in _modelMap.Values)
            {
                if (model.OnUpdate != null)
                    model.OnUpdate();
            }
        }

        public void Clear()
        {
            foreach (BxModel model in _modelMap.Values)
            {
                if (model.OnClear != null)
                    model.OnClear();
            }
            
            _modelMap.Clear();
        }


        private void InitModels()
        {
            _modelMap.Clear();

            // 获取所有BxModel的子类
            List<Type> modelTypes = GetBxModelSubclasses();

            // 创建实例并添加到字典
            for (int i = 0; i < modelTypes.Count; i++)
            {
                Type type = modelTypes[i];
                try
                {
                    BxModel modelInstance = Activator.CreateInstance(type) as BxModel;
                    if (modelInstance != null)
                    {
                        _modelMap[modelInstance.GetType()] = modelInstance;
                        modelInstance.Init();
                    }
                }
                catch (Exception ex)
                {
                    BxDebug.LogError("无法创建Model实例: " + type.Name + ", 错误: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// 通过反射获取所有BxModel的子类
        /// </summary>
        /// <returns>BxModel子类列表</returns>
        private List<Type> GetBxModelSubclasses()
        {
            List<Type> result = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                Assembly assembly = assemblies[i];
                try
                {
                    Type[] types = assembly.GetTypes();
                    for (int j = 0; j < types.Length; j++)
                    {
                        Type type = types[j];
                        if (type.IsClass && !type.IsAbstract && typeof(BxModel).IsAssignableFrom(type))
                        {
                            result.Add(type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }
            }

            return result;
        }
    }
}