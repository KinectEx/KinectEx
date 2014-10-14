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

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectJoint"/> class.
        /// </summary>
        /// <param name="joint">The joint.</param>
        public KinectJoint(Joint joint)
        {
            this._joint = joint;
        }

        /// <summary>
        /// Gets or sets the type of the joint.
        /// </summary>
        public JointType JointType
        {
            get { return this._joint.JointType; }
            set { }
        }

        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public CameraSpacePoint Position
        {
            get { return this._joint.Position; }
            set { }
        }

        /// <summary>
        /// Gets or sets the state of the tracking.
        /// </summary>
        public TrackingState TrackingState
        {
            get { return this._joint.TrackingState; }
            set { }
        }

        /// <summary>
        /// Update the joint position based on the referenced <c>IJoint</c>.
        /// </summary>
        /// <param name="joint"></param>
        /// <exception cref="System.NotSupportedException">Must update a KinectJoint with a native Kinect SDK Joint</exception>
        public void Update(IJoint joint)
        {
            throw new NotSupportedException("Must update a KinectJoint with a native Kinect SDK Joint");
        }

        /// <summary>
        /// Updates the specified joint.
        /// </summary>
        /// <param name="joint">The joint.</param>
        public void Update(Joint joint)
        {
            this._joint = joint;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Joint"/> to <see cref="KinectJoint"/>.
        /// </summary>
        /// <param name="joint">The joint.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator KinectJoint(Joint joint)
        {
            return new KinectJoint(joint);
        }
    }
}

#endif