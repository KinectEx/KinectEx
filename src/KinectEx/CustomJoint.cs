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
        /// <summary>
        /// The type of the joint.
        /// </summary>
        protected JointType _jointType;

        /// <summary>
        /// Gets or sets the type of the joint.
        /// </summary>
        public virtual JointType JointType
        {
            get { return _jointType; }
            set { _jointType = value; }
        }

        /// <summary>
        /// The position.
        /// </summary>
        protected CameraSpacePoint _position;

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public virtual CameraSpacePoint Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary>
        /// The state of the tracking.
        /// </summary>
        protected TrackingState _trackingState;

        /// <summary>
        /// Gets or sets the state of the tracking.
        /// </summary>
        public virtual TrackingState TrackingState
        {
            get { return _trackingState; }
            set { _trackingState = value; }
        }

        /// <summary>
        /// Create a new <c>CustomJoint</c> object based on the supplied
        /// <c>JointType</c> value.
        /// </summary>
        public CustomJoint(JointType type)
        {
            _jointType = type;
            _position = new CameraSpacePoint();
            _trackingState = TrackingState.NotTracked;
        }

        /// <summary>
        /// Update the joint position based on the referenced <c>IJoint</c>.
        /// </summary>
        public virtual void Update(IJoint joint)
        {
            if (this.JointType != joint.JointType)
                throw new Exception("Cannot Update with Joint of a different Type");

            _trackingState = joint.TrackingState;
            _position = joint.Position;
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
