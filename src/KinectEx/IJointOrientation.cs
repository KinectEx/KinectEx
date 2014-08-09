#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx
{
    public interface IJointOrientation
    {
        JointType JointType { get; set; }
        Vector4 Orientation { get; set; }

        /// <summary>
        /// Update the joint orientation based on the referenced <c>IJointOrientation</c>.
        /// </summary>
        void Update(IJointOrientation jointOrientation);

#if !NOSDK
        /// <summary>
        /// Update the joint orientation based on the referened <c>JointOrientation</c>.
        /// </summary>
        void Update(JointOrientation jointOrientation);
#endif
    }
}
