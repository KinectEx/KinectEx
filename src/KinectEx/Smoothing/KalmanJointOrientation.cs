using SharpDX;
using System;

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx.Smoothing
{
    /// <summary>
    /// A class representing a <c>CustomJoint</c> who's position values
    /// are "smoothed" using a Kalman-like filtering algorithm.
    /// </summary>
    public class KalmanJointOrientation : CustomJointOrientation
    {
        /// <summary>
        /// Create a Kalman smoothing joint with default configuration values.
        /// </summary>
        /// <param name="jointType">The joint type to create</param>
        public KalmanJointOrientation(JointType jointType)
            : base(jointType)
        {
        }

        /// <summary>
        /// Create a Kalman smoothing joint with custom configuration values.
        /// </summary>
        /// <param name="jointType">The joint type to create</param>
        /// <param name="parameters">An <c>ExponentialSmoothingParameters</c> object</param>
        public KalmanJointOrientation(JointType jointType, ISmootherParameters parameters = null)
            : base(jointType)
        {
        }

        /// <summary>
        /// Update (and filter) the joint orientation based on the referenced <c>IJointOrientation</c>.
        /// </summary>
        public override void Update(IJointOrientation jointOrientation)
        {
            _orientation = jointOrientation.Orientation;
        }
    }
}
