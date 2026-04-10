
using UnityEngine;

public static class TransformUtility
{
    public static Vector3 GetPlanarFoward(this Transform transform, Vector3 up)
    {
        return Vector3.ProjectOnPlane(transform.forward, up).normalized;
    }

    public static Vector3 GetPlanarRight(this Transform transform, Vector3 up)
    {
        return Vector3.ProjectOnPlane(transform.right, up).normalized;
    }
}

