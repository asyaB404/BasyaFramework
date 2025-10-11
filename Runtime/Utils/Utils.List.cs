using System;
using System.Collections.Generic;

namespace BasyaFramework.Utils
{
    public static partial class Utils
    {
        public static void Shuffle<T>(this IList<T> array)
        {
            for (var i = array.Count - 1; i > 0; i--)
            {
                var j = UnityEngine.Random.Range(0, i + 1);
                (array[j], array[i]) = (array[i], array[j]);
            }
        }

        public static void Shuffle<T>(this IList<T> array, Random rand)
        {
            for (var i = array.Count - 1; i > 0; i--)
            {
                var j = rand.Next(0, i + 1);
                (array[j], array[i]) = (array[i], array[j]);
            }
        }
        
        public static int BisectLeft<T>(this IList<T> arr, T value, int low = 0, int high = -1,
            IComparer<T> comparer = null) where T : IComparable<T>
        {
            if (high == -1) high = arr.Count; 
            comparer ??= Comparer<T>.Default;
            while (low < high)
            {
                int mid = (low + high) >> 1;
                if (comparer.Compare(arr[mid], value) < 0)
                    low = mid + 1;
                else
                    high = mid;
            }

            return low;
        }
    }
}