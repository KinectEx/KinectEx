namespace KinectEx.DVR
{
    internal static class ColorCodecs
    {
        public static IColorCodec Raw { get; private set; }
        public static IColorCodec Jpeg { get; private set; }

        static ColorCodecs()
        {
            Raw = new RawColorCodec();
            Jpeg = new JpegColorCodec();
        }
    }
}
