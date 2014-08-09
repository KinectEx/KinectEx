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
    /// This class just wraps the Kinect SDK's JointOrientation struct, but does so with
    /// a class that implements the common <c>IJointOrientation</c> interface. This makes
    /// it possible to use a standard Kinect SDK object with many of the utility
    /// functions provided by the KinectEx library.
    /// </summary>
    public class KinectJointOrientation : IJointOrientation
    {
        private JointOrientation _jointOrientation;

        public KinectJointOrientation(JointOrientation jointOrientation)
        {
            this._jointOrientation = jointOrientation;
        }

        public JointType JointType
        {
            get { return _jointOrientation.JointType; }
            set { }
        }

        public Vector4 Orientation
        {
            get { return _jointOrientation.Orientation; }
            set { }
        }

        public void Update(IJointOrientation jointOrientation)
        {
            throw new NotSupportedException("Must update a KinectJointOrientation with a native Kinect SDK JointOrientation");
        }

        public void Update(JointOrientation jointOrientation)
        {
            this._jointOrientation = jointOrientation;
        }

        public static implicit operator KinectJointOrientation(JointOrientation jointOrientation)
        {
            return new KinectJointOrientation(jointOrientation);
        }
    }
}

#endif