namespace KinectEx.DVR
{
    /// <summary>
    /// Internal class with static instances of each <c>IColorCodec</c>
    /// supported by the KinectEx.DVR system. Used primarily to get quick
    /// access to properties of any individual codec.
    /// </summary>
    internal static class ColorCodecs
    {
        /// <summary>
        /// Gets a static instance of the Raw codec.
        /// </summary>
        public static IColorCodec Raw { get; private set; }

        /// <summary>
        /// Gets a static instance of the Jpeg codec.
        /// </summary>
        public static IColorCodec Jpeg { get; private set; }

        /// <summary>
        /// Initializes the <see cref="ColorCodecs"/> class.
        /// </summary>
        static ColorCodecs()
        {
            Raw = new RawColorCodec();
            Jpeg = new JpegColorCodec();
        }
    }
}
