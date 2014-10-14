using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectEx.Smoothing
{
    /// <summary>
    /// A smoothing "strategy" that, when applied to a <c>SmoothedBody</c> 
    /// will "smooth" the joints using a Double Exponential filtering algorithm.
    /// </summary>
    public class ExponentialSmoother : ISmoother
    {
        /// <summary>
        /// Type of "smoothed" joint to create when constructing bodies
        /// using this strategy.
        /// </summary>
        public Type CustomJointType { get { return typeof(ExponentialJoint); } }

        /// <summary>
        /// Type of "smoothed" joint orientation to create when constructing bodies
        /// using this strategy.
        /// </summary>
        public Type CustomJointOrientationType { get { return typeof(ExponentialJointOrientation); } }
    }
}
