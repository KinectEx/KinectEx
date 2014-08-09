using System;
using System.Collections.Generic;

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
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
    /// This class effectively mimics the Kinect SDK's <c>Body</c> struct, but does
    /// so with a class that can be serialized and is not sealed. These capabilities
    /// are needed to support both the smoothing / filtering and the DVR functions of
    /// the KinectEx library.
    /// </summary>
    public class CustomBody : IBody
    {
        protected Dictionary<Activity, DetectionResult> _activities;
        public virtual IReadOnlyDictionary<Activity, DetectionResult> Activities 
        { 
            get { return _activities; }
            set { _activities = value as Dictionary<Activity, DetectionResult>; }
        }

        protected Dictionary<Appearance, DetectionResult> _appearance;
        public virtual IReadOnlyDictionary<Appearance, DetectionResult> Appearance
        { 
            get { return _appearance; }
            set { _appearance = value as Dictionary<Appearance, DetectionResult>; }
        }

        protected FrameEdges _clippedEdges;
        public virtual FrameEdges ClippedEdges
        {
            get { return _clippedEdges; }
            set { _clippedEdges = value; }
        }

        protected DetectionResult _engaged;
        public virtual DetectionResult Engaged
        {
            get { return _engaged; }
            set { _engaged = value; }
        }
        
        protected Dictionary<Expression, DetectionResult> _expressions;
        public virtual IReadOnlyDictionary<Expression, DetectionResult> Expressions
        {
            get { return _expressions; }
            set { _expressions = value as Dictionary<Expression, DetectionResult>; }
        }

        protected TrackingConfidence _handLeftConfidence;
        public virtual TrackingConfidence HandLeftConfidence
        {
            get { return _handLeftConfidence; }
            set { _handLeftConfidence = value; }
        }

        protected HandState _handLeftState;
        public virtual HandState HandLeftState
        {
            get { return _handLeftState; }
            set { _handLeftState = value; }
        }

        protected TrackingConfidence _handRightConfidence;
        public virtual TrackingConfidence HandRightConfidence
        {
            get { return _handRightConfidence; }
            set { _handRightConfidence = value; }
        }

        protected HandState _handRightState;
        public virtual HandState HandRightState
        {
            get { return _handRightState; }
            set { _handRightState = value; }
        }

        protected bool _isDisposed;
        public virtual bool IsDisposed
        {
            get { return _isDisposed; }
            set { _isDisposed = value; }
        }

        protected bool _isRestricted;
        public virtual bool IsRestricted
        {
            get { return _isRestricted; }
            set { _isRestricted = value; }
        }

        protected bool _isTracked;
        public virtual bool IsTracked
        {
            get { return _isTracked; }
            set { _isTracked = value; }
        }
        
        protected Dictionary<JointType, IJointOrientation> _jointOrientations;
        public virtual IReadOnlyDictionary<JointType, IJointOrientation> JointOrientations
        {
            get { return _jointOrientations; }
            set { _jointOrientations = value as Dictionary<JointType, IJointOrientation>; }
        }

        protected Dictionary<JointType, IJoint> _joints;
        public virtual IReadOnlyDictionary<JointType, IJoint> Joints
        {
            get { return _joints; }
            set { _joints = value as Dictionary<JointType, IJoint>; }
        }


#if NETFX_CORE
        protected Point _lean;
        public virtual Point Lean
        {
            get { return _lean; }
            set { _lean = value; }
        }
#else
        protected PointF _lean;
        public virtual PointF Lean
        {
            get { return _lean; }
            set { _lean = value; }
        }
#endif

        protected TrackingState _leanTrackingState;
        public virtual TrackingState LeanTrackingState
        {
            get { return _leanTrackingState; }
            set { _leanTrackingState = value; }
        }

        protected ulong _trackingId;
        public virtual ulong TrackingId
        {
            get { return _trackingId; }
            set { _trackingId = value; }
        }
        
        public CustomBody()
        {
            _activities = new Dictionary<Activity, DetectionResult>();
            foreach (var activity in (Activity[])Enum.GetValues(typeof(Activity)))
            {
                _activities.Add(activity, DetectionResult.Unknown);
            }

            _appearance = new Dictionary<Appearance, DetectionResult>();
            foreach (var appearance in (Appearance[])Enum.GetValues(typeof(Appearance)))
            {
                _appearance.Add(appearance, DetectionResult.Unknown);
            }

            _clippedEdges = FrameEdges.None;
            _engaged = DetectionResult.Unknown;

            _expressions = new Dictionary<Expression, DetectionResult>();
            foreach (var expression in (Expression[])Enum.GetValues(typeof(Expression)))
            {
                _expressions.Add(expression, DetectionResult.Unknown);
            }

            _handLeftConfidence = TrackingConfidence.Low;
            _handLeftState = HandState.Unknown;
            _handRightConfidence = TrackingConfidence.Low;
            _handRightState = HandState.Unknown;
            _isDisposed = false;
            _isRestricted = false;
            _isTracked = false;

            _joints = new Dictionary<JointType, IJoint>();
            _jointOrientations = new Dictionary<JointType, IJointOrientation>();
            foreach (var jointType in JointTypeEx.AllJoints)
            {
                _joints.Add(jointType, new CustomJoint(jointType));
                _jointOrientations.Add(jointType, new CustomJointOrientation(jointType));
            }

#if NETFX_CORE
            _lean = new Point();
#else
            _lean = new PointF();
#endif
            _leanTrackingState = TrackingState.NotTracked;
            _trackingId = ulong.MaxValue;
        }

        /// <summary>
        /// Updates the values of this <c>CustomBody</c> with the values contained
        /// in the referenced <c>IBody</c>.
        /// </summary>
        public virtual void Update(IBody body)
        {
            foreach (var key in body.Activities.Keys)
            {
                _activities[key] = body.Activities[key];
            }

            foreach (var key in body.Appearance.Keys)
            {
                _appearance[key] = body.Appearance[key];
            }

            this.ClippedEdges = body.ClippedEdges;
            this.Engaged = body.Engaged;

            foreach (var key in body.Expressions.Keys)
            {
                _expressions[key] = body.Expressions[key];
            }
            this.HandLeftConfidence = body.HandLeftConfidence;
            this.HandLeftState = body.HandLeftState;
            this.HandRightConfidence = body.HandRightConfidence;
            this.HandRightState = body.HandRightState;
            this.IsRestricted = body.IsRestricted;
            this.IsTracked = body.IsTracked;

            foreach (var key in body.JointOrientations.Keys)
            {
                _jointOrientations[key] = body.JointOrientations[key];
            }

            foreach (var joint in body.Joints.Values)
            {
                Joints[joint.JointType].Update(joint);
            }

            this.Lean = body.Lean;
            this.LeanTrackingState = body.LeanTrackingState;
            this.TrackingId = body.TrackingId;
        }

#if !NOSDK
        /// <summary>
        /// Updates the values of this <c>CustomBody</c> with the values contained
        /// in the referenced <c>Body</c>.
        /// </summary>
        public virtual void Update(Body body)
        {
            Update((KinectBody)body);
        }
#endif
    }
}
