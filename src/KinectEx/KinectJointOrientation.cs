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

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectJointOrientation"/> class.
        /// </summary>
        /// <param name="jointOrientation">The joint orientation.</param>
        public KinectJointOrientation(JointOrientation jointOrientation)
        {
            this._jointOrientation = jointOrientation;
        }

        /// <summary>
        /// Gets or sets the type of the joint.
        /// </summary>
        public JointType JointType
        {
            get { return _jointOrientation.JointType; }
            set { }
        }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        public Vector4 Orientation
        {
            get { return _jointOrientation.Orientation; }
            set { }
        }

        /// <summary>
        /// Update the joint orientation based on the referenced <c>IJointOrientation</c>.
        /// </summary>
        /// <param name="jointOrientation"></param>
        /// <exception cref="System.NotSupportedException">Must update a KinectJointOrientation with a native Kinect SDK JointOrientation</exception>
        public void Update(IJointOrientation jointOrientation)
        {
            throw new NotSupportedException("Must update a KinectJointOrientation with a native Kinect SDK JointOrientation");
        }

        /// <summary>
        /// Update the joint orientation based on the referened <c>JointOrientation</c>.
        /// </summary>
        /// <param name="jointOrientation"></param>
        public void Update(JointOrientation jointOrientation)
        {
            this._jointOrientation = jointOrientation;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="JointOrientation"/> to <see cref="KinectJointOrientation"/>.
        /// </summary>
        /// <param name="jointOrientation">The joint orientation.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator KinectJointOrientation(JointOrientation jointOrientation)
        {
            return new KinectJointOrientation(jointOrientation);
        }
    }
}

#endif