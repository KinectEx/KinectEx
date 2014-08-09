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
    public class CustomJointOrientation : IJointOrientation
    {
        protected JointType _jointType;
        public virtual JointType JointType
        {
            get { return _jointType; }
            set { _jointType = value; }
        }

        protected Vector4 _orientation;
        public virtual Vector4 Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        /// <summary>
        /// Create a new <c>CustomJointOrientation</c> object based on the supplied
        /// <c>JointType</c> value.
        /// </summary>
        public CustomJointOrientation(JointType type)
        {
            _jointType = type;
            _orientation = new Vector4();
        }

        /// <summary>
        /// Update the joint orientation based on the referenced <c>IJointOrientation</c>.
        /// </summary>
        public virtual void Update(IJointOrientation jointOrientation)
        {
            if (this.JointType != jointOrientation.JointType)
                throw new Exception("Cannot Update with Joint of a different Type");

            _orientation = jointOrientation.Orientation;
        }

#if !NOSDK
        /// <summary>
        /// Update the joint position based on the referened <c>Joint</c>.
        /// </summary>
        public virtual void Update(JointOrientation jointOrientation)
        {
            Update((KinectJointOrientation)jointOrientation);
        }
#endif

    }
}
