#if !NOSDK

using System;
using System.Collections.Generic;

#if NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

#if NETFX_CORE
using Windows.Foundation;
#endif

namespace KinectEx
{
    /// <summary>
    /// This class just wraps the Kinect SDK's Joint struct, but does so with
    /// a class that implements the common <c>IJoint</c> interface. This makes
    /// it possible to use a standard Kinect SDK object with many of the utility
    /// functions provided by the KinectEx library.
    /// </summary>
    public class KinectBody : IBody
    {
        internal Body _body;
        private Dictionary<JointType, IJoint> _joints = null;

        public IReadOnlyDictionary<Activity, DetectionResult> Activities
        {
            get { return _body.Activities; }
        }

        public IReadOnlyDictionary<Appearance, DetectionResult> Appearance
        {
            get { return _body.Appearance; }
        }

        public FrameEdges ClippedEdges
        {
            get { return _body.ClippedEdges; }
            set { }
        }

        public DetectionResult Engaged
        {
            get { return _body.Engaged; }
            set { }
        }

        public IReadOnlyDictionary<Expression, DetectionResult> Expressions
        {
            get { return _body.Expressions; }
            set { }
        }

        public TrackingConfidence HandLeftConfidence
        {
            get { return _body.HandLeftConfidence; }
            set { }
        }

        public HandState HandLeftState
        {
            get { return _body.HandLeftState; }
            set { }
        }

        public TrackingConfidence HandRightConfidence
        {
            get { return _body.HandRightConfidence; }
            set { }
        }

        public HandState HandRightState
        {
            get { return _body.HandRightState; }
            set { }
        }

        public bool IsRestricted
        {
            get { return _body.IsRestricted; }
            set { }
        }

        public bool IsTracked
        {
            get { return _body.IsTracked; }
            set { }
        }

        public IReadOnlyDictionary<JointType, JointOrientation> JointOrientations
        {
            get { return _body.JointOrientations; }
            set { }
        }

        public IReadOnlyDictionary<JointType, IJoint> Joints
        {
            get
            {
                if (_joints == null)
                {
                    _joints = new Dictionary<JointType, IJoint>();
                    foreach (var key in _body.Joints.Keys)
                    {
                        _joints.Add(key, new KinectJoint(_body.Joints[key]));
                    }
                }
                return _joints;
            }
            set { }
        }

#if NETFX_CORE
        public Point Lean
#else
        public PointF Lean
#endif
        {
            get { return _body.Lean; }
            set { }
        }

        public TrackingState LeanTrackingState
        {
            get { return _body.LeanTrackingState; }
            set { }
        }

        public ulong TrackingId
        {
            get { return _body.TrackingId; }
            set { }
        }

        public KinectBody() { }

        public KinectBody(Body body)
        {
            this._body = body;
        }

        public void Update(IBody body)
        {
            throw new NotSupportedException("Must update a KinectBody with a native Kinect SDK Body");
        }

        public void Update(Body body)
        {
            this._body = body;
        }

        public static implicit operator KinectBody(Body body)
        {
            return new KinectBody(body);
        }
    }
}

#endif