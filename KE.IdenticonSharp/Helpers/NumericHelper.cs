namespace IdenticonSharp.Helpers
{
    public static class NumericHelper
    {
        public static double Map(this double value, double fromMin, double fromMax, double toMin, double toMax) =>
            (value - fromMin) * (toMax - toMin) / (fromMax - fromMin + toMin);

        public static double Map(this int value, double fromMin, double fromMax, double toMin, double toMax) =>
            Map((double)value, fromMin, fromMax, toMin, toMax);

        public static double Map(this byte value, double fromMin, double fromMax, double toMin, double toMax) =>
            Map((double)value, fromMin, fromMax, toMin, toMax);
    }
}
