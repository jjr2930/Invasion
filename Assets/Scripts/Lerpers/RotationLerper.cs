using System;
using UnityEngine;

[Serializable]
public struct RotationLerper
{
    // Calculated start for the most recent interpolation
    [SerializeField] Quaternion m_LerpStart;

    // Calculated time elapsed for the most recent interpolation
    [SerializeField] float m_CurrentLerpTime;

    // The duration of the interpolation, in seconds
    [SerializeField, Range(0.00001f, 1f)] float duration;

    /// <summary>
    /// Linearly interpolate between two Quaternion values.
    /// </summary>
    /// <param name="current"> Start of the interpolation. </param>
    /// <param name="target"> End of the interpolation. </param>
    /// <returns> A Quaternion value between current and target. </returns>
    public Quaternion Lerp(Quaternion current, Quaternion target)
    {
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

        return Quaternion.Slerp(m_LerpStart, target, lerpPercentage);
    }
}