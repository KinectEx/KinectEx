using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectEx.Smoothing
{
    public interface ISmoother
    {
        Type CustomJointType { get; }
        Type CustomJointOrientationType { get; }
    }
}
