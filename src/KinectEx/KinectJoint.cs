#if !NOSDK

using System;

#if NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx
{
    /// <summary>
    /// This class just wraps the Kinect SDK's Joint struct, but does so with
    /// a class that implements the common <c>IJoint</c> interface. This makes
    /// it possible to use a standard Kinect SDK object with many of the utility
    /// functions provided by the KinectEx library.
    /// </summary>
    public class KinectJoint : IJoint
    {
        private Joint _joint;

        public KinectJoint(Joint joint)
        {
            this._joint = joint;
        }

        public JointType JointType
        {
            get { return this._joint.JointType; }
            set { }
        }

        public CameraSpacePoint Position
        {
            get { return this._joint.Position; }
            set { }
        }

        public TrackingState TrackingState
        {
            get { return this._joint.TrackingState; }
            set { }
        }

        public void Update(IJoint joint)
        {
            throw new NotSupportedException("Must update a KinectJoint with a native Kinect SDK Joint");
        }

        public void Update(Joint joint)
        {
            this._joint = joint;
        }

        public static implicit operator KinectJoint(Joint joint)
        {
            return new KinectJoint(joint);
        }
    }
}

#endif