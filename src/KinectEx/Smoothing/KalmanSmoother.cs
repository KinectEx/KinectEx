using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectEx.Smoothing
{
    /// <summary>
    /// A smoothing "strategy" that, when applied to a <c>SmoothedBody</c> 
    /// will "smooth" the joints using a Kalman-like filtering algorithm.
    /// </summary>
    public class KalmanSmoother : ISmoother
    {
        /// <summary>
        /// Type of "smoothed" joint to create when constructing bodies 
        /// using this strategy.
        /// </summary>
        public Type CustomJointType { get { return typeof(KalmanJoint); } }

        /// <summary>
        /// Type of "smoothed" joint orientation to create when constructing bodies 
        /// using this strategy.
        /// </summary>
        public Type CustomJointOrientationType { get { return typeof(KalmanJointOrientation); } }
    }
}
