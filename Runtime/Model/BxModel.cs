using System;
using BasyaFramework.Core;

namespace BasyaFramework.Model
{
    public class BxModel
    {
        public Action OnClear;
        public Action OnUpdate;

        public static T GetModel<T>() where T : BxModel
        {
            return BxGame.ModelManager.GetModel<T>();
        }

        public BxModel()
        {
        }

        public virtual void Init()
        {
        }
    }
}