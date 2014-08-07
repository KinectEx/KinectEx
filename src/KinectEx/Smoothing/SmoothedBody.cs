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
    public class SmoothedBody<T> : CustomBody where T : ISmoother
    {
        public SmoothedBody()
            : base()
        {
            ISmoother smoother = (ISmoother)Activator.CreateInstance(typeof(T));

            _joints.Clear();
            foreach (JointType jointType in (JointType[])Enum.GetValues(typeof(JointType)))
            {
                _joints.Add(jointType, (IJoint)Activator.CreateInstance(smoother.CustomJointType, jointType));
            }
        }

        public SmoothedBody(object parameters = null)
            : base()
        {
            ISmoother smoother = (ISmoother)Activator.CreateInstance(typeof(T));

            _joints.Clear();
            foreach (JointType jointType in (JointType[])Enum.GetValues(typeof(JointType)))
            {
                _joints.Add(jointType, (IJoint)Activator.CreateInstance(smoother.CustomJointType, jointType, parameters));
            }
        }
    }
}
