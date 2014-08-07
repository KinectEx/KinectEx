using System;

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx
{
    public class CustomJoint : IJoint
    {
        public JointType JointType { get; set; }

        public CameraSpacePoint Position { get; set; }
                
        public TrackingState TrackingState { get; set; }

        public CustomJoint(JointType type, object parameters = null)
        {
            this.JointType = type;
            this.Position = new CameraSpacePoint();
            this.TrackingState = TrackingState.NotTracked;
        }

        public virtual void Update(IJoint joint)
        {
            if (this.JointType != joint.JointType)
                throw new Exception("Cannot Update with Joint of a different Type");

            this.TrackingState = joint.TrackingState;
            this.Position = joint.Position;
        }

#if !NOSDK
        public virtual void Update(Joint joint)
        {
            Update((KinectJoint)joint);
        }
#endif
    }
}
