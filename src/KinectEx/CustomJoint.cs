using System;

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
    /// This class effectively mimics the Kinect SDK's <c>Joint</c> struct, but does
    /// so with a class that can be serialized and is not sealed. These capabilities
    /// are needed to support both the smoothing / filtering and the DVR functions of
    /// the KinectEx library.
    /// </summary>
    public class CustomJoint : IJoint
    {
        public JointType JointType { get; set; }

        public CameraSpacePoint Position { get; set; }

        public TrackingState TrackingState { get; set; }

        /// <summary>
        /// Create a new <c>CustomJoint</c> object based on the supplied
        /// <c>JointType</c> value.
        /// </summary>
        public CustomJoint(JointType type)
        {
            this.JointType = type;
            this.Position = new CameraSpacePoint();
            this.TrackingState = TrackingState.NotTracked;
        }

        /// <summary>
        /// Update the joint position based on the referenced <c>IJoint</c>.
        /// </summary>
        public virtual void Update(IJoint joint)
        {
            if (this.JointType != joint.JointType)
                throw new Exception("Cannot Update with Joint of a different Type");

            this.TrackingState = joint.TrackingState;
            this.Position = joint.Position;
        }

#if !NOSDK
        /// <summary>
        /// Update the joint position based on the referened <c>Joint</c>.
        /// </summary>
        public virtual void Update(Joint joint)
        {
            Update((KinectJoint)joint);
        }
#endif
    }
}
