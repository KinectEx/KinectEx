#if NETFX_CORE
using Windows.Foundation;
#endif

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx.DVR
{
    /// <summary>
    /// Records information related to a specific KDVR file.
    /// </summary>
    internal class FileMetadata
    {
        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Gets or sets the color codec identifier.
        /// </summary>
        public int ColorCodecId { get; set; }

        /// <summary>
        /// Gets or sets the depth camera intrinsics.
        /// </summary>
        public CameraIntrinsics DepthCameraIntrinsics { get; set; }

#if NETFX_CORE
        /// <summary>
        /// Gets or sets the depth frame to camera space table.
        /// </summary>
        public Point[] DepthFrameToCameraSpaceTable { get; set; }
#else
        /// <summary>
        /// Gets or sets the depth frame to camera space table.
        /// </summary>
        public PointF[] DepthFrameToCameraSpaceTable { get; set; }
#endif
    }
}
