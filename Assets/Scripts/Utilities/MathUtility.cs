using UnityEngine;

public static class MathUtility
{
    public static bool ApproximatleyEqual(Vector3 v1, Vector3 v2)
    {
        return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y) && Mathf.Approximately(v1.z, v2.z);
    }

    public static bool ApproximatleyEqual(ref Vector3 v1, ref Vector3 v2)
    {
        return Mathf.Approximately(v1.x, v2.x) && Mathf.Approximately(v1.y, v2.y) && Mathf.Approximately(v1.z, v2.z);
    }

    public static bool ApproximatleyEqual(ref Vector3 v1, ref Vector3 v2, float epsilon)
    {
        return Mathf.Abs(v1.x - v2.x) < epsilon && Mathf.Abs(v1.y - v2.y) < epsilon && Mathf.Abs(v1.z - v2.z) < epsilon;
    }

    public static bool ApproximatleyEqual(Vector3 v1, Vector3 v2, float epsilon)
    {
        return Mathf.Abs(v1.x - v2.x) < epsilon && Mathf.Abs(v1.y - v2.y) < epsilon && Mathf.Abs(v1.z - v2.z) < epsilon;
    }
}

