using System;
using UnityEngine;

[Serializable]
public struct PositionLerper
{
    // Calculated start for the most recent interpolation
    [SerializeField] Vector3 m_LerpStart;

    // Calculated time elapsed for the most recent interpolation
    [SerializeField] float m_CurrentLerpTime;

    // The duration of the interpolation, in seconds
    [SerializeField, Range(0.00001f, 1f)] float duration;

    [SerializeField] float warpDistance;

    public float Duration { get => duration; set => duration = value; }

    /// <summary>
    /// Linearly interpolate between two Vector3 values.
    /// </summary>
    /// <param name="current"> Start of the interpolation. </param>
    /// <param name="target"> End of the interpolation. </param>
    /// <returns> A Vector3 value between current and target. </returns>
    public Vector3 Lerp(Vector3 current, Vector3 target)
    {
        if(Vector3.SqrMagnitude(current - target) >= warpDistance)
        {
            m_LerpStart = target;
            current = target;
            return target;
        }

        if (current != target)
        {
            m_LerpStart = current;
            m_CurrentLerpTime = 0f;
        }

        m_CurrentLerpTime += Time.deltaTime;
        if (m_CurrentLerpTime > duration)
        {
            m_CurrentLerpTime = duration;
        }

        var lerpPercentage = m_CurrentLerpTime / duration;

        return Vector3.Lerp(m_LerpStart, target, lerpPercentage);
    }
}