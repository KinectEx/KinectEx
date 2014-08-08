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

        /// <summary>
        /// Update the joint position based on the referenced <c>IJoint</c>.
        /// </summary>
        void Update(IJoint joint);

#if !NOSDK
        /// <summary>
        /// Update the joint position based on the referened <c>Joint</c>.
        /// </summary>
        void Update(Joint joint);
#endif
    }
}
