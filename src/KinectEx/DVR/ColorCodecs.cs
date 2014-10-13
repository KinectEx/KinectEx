namespace KinectEx.DVR
{
    /// <summary>
    /// Internal class with static instances of each <c>IColorCodec</c>
    /// supported by the KinectEx.DVR system. Used primarily to get quick
    /// access to properties of any individual codec.
    /// </summary>
    internal static class ColorCodecs
    {
        public static IColorCodec Raw { get; private set; }
        public static IColorCodec Jpeg { get; private set; }
        public static IColorCodec Png { get; private set; }

        static ColorCodecs()
        {
            Raw = new RawColorCodec();
            Jpeg = new JpegColorCodec();
            Png = new PngColorCodec();
        }
    }
}
