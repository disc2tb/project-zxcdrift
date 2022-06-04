public static class Helpers
{
    public static float Map(float inRangeA, float inRangeB, float outRangeA, float outRangeB, float value)
    {
        return (value - inRangeA) * (outRangeB - outRangeA) / (inRangeB - inRangeA) + outRangeA;
    }
}