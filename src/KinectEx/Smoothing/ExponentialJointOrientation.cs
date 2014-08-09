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
    /// A class representing a <c>CustomJointOrientation</c> who's values
    /// are "smoothed" using a Double Exponential filtering algorithm.
    /// </summary>
    public class ExponentialJointOrientation : CustomJointOrientation
    {
        /// <summary>
        /// Create an exponential smoothing joint with default configuration values.
        /// </summary>
        /// <param name="jointType">The joint type to create</param>
        public ExponentialJointOrientation(JointType jointType)
            : base(jointType)
        {
        }

        /// <summary>
        /// Create an exponential smoothing joint with custom configuration values.
        /// </summary>
        /// <param name="jointType">The joint type to create</param>
        /// <param name="parameters">An <c>ExponentialSmoothingParameters</c> object</param>
        public ExponentialJointOrientation(JointType jointType, ISmootherParameters parameters = null)
            : base(jointType)
        {
        }

        private void Reset()
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
