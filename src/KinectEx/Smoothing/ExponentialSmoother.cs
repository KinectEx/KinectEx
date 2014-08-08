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
        public Type CustomJointType { get { return typeof(ExponentialJoint); } }
        public Type CustomJointOrientationType { get { return null; } }
    }
}
