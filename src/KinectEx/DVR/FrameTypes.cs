namespace KinectEx.DVR
{
    /// <summary>
    /// Enumerates the Kinect Frame Types
    /// </summary>
    public enum FrameTypes
    {
        /// <summary>
        /// The color frame type.
        /// </summary>
        Color = 0,

        /// <summary>
        /// The depth frame type.
        /// </summary>
        Depth = 1,

        /// <summary>
        /// The body frame type.
        /// </summary>
        Body = 2,

        /// <summary>
        /// The body index frame type (not currently implemented in KinectEx).
        /// </summary>
        BodyIndex = 3,

        /// <summary>
        /// The infrared frame type.
        /// </summary>
        Infrared = 4
    }
}
