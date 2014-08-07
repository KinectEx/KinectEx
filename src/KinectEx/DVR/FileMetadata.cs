#if NETFX_CORE
using Windows.Foundation;
#elif NOSDK
using KinectEx.KinectSDK;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx.DVR
{
    internal class FileMetadata
    {
        public string Version { get; set; }
        public int ColorCodecId { get; set; }

#if NETFX_CORE
        public Point[] DepthFrameToCameraSpaceTable { get; set; }
#else
        public PointF[] DepthFrameToCameraSpaceTable { get; set; }
#endif
    }
}
