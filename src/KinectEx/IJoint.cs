#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx
{
    /// <summary>
    /// An interface that explicitly maps all of the Kinect SDK <c>Joint</c>
    /// members so that different variations of joint can be used for
    /// both smoothing and recording.
    /// </summary>
    public interface IJoint
    {
        /// <summary>
        /// Gets or sets the type of the joint.
        /// </summary>
        JointType JointType { get; set; }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        CameraSpacePoint Position { get; set; }

        /// <summary>
        /// Gets or sets the 2D depth space position.
        /// </summary>
        DepthSpacePoint DepthPosition { get; set; }

        /// <summary>
        /// Gets or sets the 2D color space position.
        /// </summary>
        ColorSpacePoint ColorPosition { get; set; }

        /// <summary>
        /// Gets or sets the state of the tracking.
        /// </summary>
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
