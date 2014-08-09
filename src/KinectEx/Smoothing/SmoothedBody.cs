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
    /// A generic class represents a <c>CustomBody</c> that implements
    /// the smoothing "strategy" represented by the specified <c>ISmoother</c>.
    /// </summary>
    public class SmoothedBody<T> : CustomBody where T : ISmoother
    {
        /// <summary>
        /// Creates an instance of a <c>SmoothedBody</c> using the default parameters
        /// for the specified <c>ISmoother</c>.
        /// </summary>
        public SmoothedBody()
            : base()
        {
            ISmoother smoother = (ISmoother)Activator.CreateInstance(typeof(T));

            _joints.Clear();
            _jointOrientations.Clear();
            foreach (var jointType in JointTypeEx.AllJoints)
            {
                _joints.Add(jointType, (IJoint)Activator.CreateInstance(smoother.CustomJointType, (JointType)jointType));
                _jointOrientations.Add(jointType, (IJointOrientation)Activator.CreateInstance(smoother.CustomJointOrientationType, (JointType)jointType));
            }
        }

        /// <summary>
        /// Creates an instance of a <c>SmoothedBody</c> using the referenced smoothing
        /// parameters object.
        /// </summary>
        public SmoothedBody(ISmootherParameters parameters)
            : base()
        {
            ISmoother smoother = (ISmoother)Activator.CreateInstance(typeof(T));

            _joints.Clear();
            _jointOrientations.Clear();
            foreach (var jointType in JointTypeEx.AllJoints)
            {
                _joints.Add(jointType, (IJoint)Activator.CreateInstance(smoother.CustomJointType, (JointType)jointType, parameters));
                _jointOrientations.Add(jointType, (IJointOrientation)Activator.CreateInstance(smoother.CustomJointOrientationType, (JointType)jointType, parameters));
            }
        }
    }
}
