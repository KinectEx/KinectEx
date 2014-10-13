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
    internal class FileMetadata
    {
        public string Version { get; set; }
        public int ColorCodecId { get; set; }
        public CameraIntrinsics DepthCameraIntrinsics { get; set; }
#if NETFX_CORE
        public Point[] DepthFrameToCameraSpaceTable { get; set; }
#else
        public PointF[] DepthFrameToCameraSpaceTable { get; set; }
#endif
    }
}
