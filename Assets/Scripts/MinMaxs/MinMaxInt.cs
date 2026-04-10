using System;
using UnityEngine;

namespace MinMax
{
    [Serializable]
    public class MinMaxInt : MinMax<int>
    {
        public MinMaxInt(int min, int max) : base(min, max)
        {
        }

        public override int Lerp(float t)
        {
            return Mathf.RoundToInt(Mathf.Lerp(min, max, t));
        }

        public override int Random()
        {
            return UnityEngine.Random.Range(min, max + 1);
        }
    }
}
