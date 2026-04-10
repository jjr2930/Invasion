using System;
using System.Collections.Generic;
using UnityEngine;

namespace MinMax
{
    /// <summary>
    /// 1. min and max values of a type T.
    /// 2. IsInRange(T value) min <= value <= max
    /// 3. Random value between min and max. (if T is numeric or DateTime)
    /// 4. Clamp(T value) if value < min return min, if value > max return max, else return value.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class MinMax<T>
    {
        [SerializeField] protected T min;
        [SerializeField] protected T max;

        public MinMax(T min, T max)
        {
            this.min = min;
            this.max = max;
        }

        public bool IsInRange(T value)
        {
            return Comparer<T>.Default.Compare(value, min) >= 0 && Comparer<T>.Default.Compare(value, max) <= 0;
        }

        public T Clamp(T value)
        {
            if (Comparer<T>.Default.Compare(value, min) < 0)
                return min;
            if (Comparer<T>.Default.Compare(value, max) > 0)
                return max;
            return value;
        }

        public virtual T Random()
        {
            throw new NotImplementedException();
        }

        public virtual T Lerp(float t)
        {
            throw new NotImplementedException();
        }
    }
}