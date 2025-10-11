using System;

namespace BasyaFramework.Utils
{
    public static partial class Utils
    {
        public static T GetRandomEnumValue<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }

        public static T GetRandomEnumValue<T>(Random random) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(random.Next(values.Length));
        }

        public static int GetRandomEnumValueIndex<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return UnityEngine.Random.Range(0, values.Length);
        }
        
        public static int GetRandomEnumValueIndex<T>(Random random) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            return random.Next(values.Length);
        }
    }
}