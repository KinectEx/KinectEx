#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx
{
    public interface IJoint
    {
        JointType JointType { get; set; }
        CameraSpacePoint Position { get; set; }
        TrackingState TrackingState { get; set; }

        void Update(IJoint joint);
#if !NOSDK
        void Update(Joint joint);
#endif
    }
}
