#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx
{
    /// An interface that explicitly maps all of the Kinect SDK <c>JointOrientation</c>
    /// members so that different variations of joint orientation can be used for
    /// both smoothing and recording.
    public interface IJointOrientation
    {
        /// <summary>
        /// Gets or sets the type of the joint.
        /// </summary>
        JointType JointType { get; set; }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        Vector4 Orientation { get; set; }

        /// <summary>
        /// Update the joint orientation based on the referenced <c>IJointOrientation</c>.
        /// </summary>
        /// <param name="jointOrientation">The joint orientation.</param>
        void Update(IJointOrientation jointOrientation);

#if !NOSDK
        /// <summary>
        /// Update the joint orientation based on the referenced <c>JointOrientation</c>.
        /// </summary>
        /// <param name="jointOrientation">The joint orientation.</param>
        void Update(JointOrientation jointOrientation);
#endif
    }
}
