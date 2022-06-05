using UnityEngine;

public static class Helpers
{
    public static readonly float RPM2Rad = Mathf.PI * 2 / 60;
    public static readonly float Rad2RPM = 1 / RPM2Rad;

    public static float Map(float inRangeA, float inRangeB, float outRangeA, float outRangeB, float value)
    {
        return (value - inRangeA) * (outRangeB - outRangeA) / (inRangeB - inRangeA) + outRangeA;
    }
}