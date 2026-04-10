using System;
using UnityEngine;

namespace MinMax
{
    [Serializable]
    public class MinMaxFloat : MinMax<float>
    {
        public MinMaxFloat(float min, float max) : base(min, max) { }

        public override float Lerp(float t)
        {
            return Mathf.Lerp(min, max, t);
        }

        public override float Random()
        {
            return UnityEngine.Random.Range(min, max);
        }
    }
}
