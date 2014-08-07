using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectEx.Smoothing
{
    public class ExponentialSmoother : ISmoother
    {
        public Type CustomJointType { get { return typeof(ExponentialJoint); } }
        public Type CustomJointOrientationType { get { return null; } }
    }
}
