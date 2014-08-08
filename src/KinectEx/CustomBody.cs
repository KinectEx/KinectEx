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

        public virtual FrameEdges ClippedEdges { get; set; }

        public virtual DetectionResult Engaged { get; set; }

        protected Dictionary<Expression, DetectionResult> _expressions;
        public virtual IReadOnlyDictionary<Expression, DetectionResult> Expressions
        {
            get { return _expressions; }
            set { _expressions = value as Dictionary<Expression, DetectionResult>; }
        }

        public virtual TrackingConfidence HandLeftConfidence { get; set; }

        public virtual HandState HandLeftState { get; set; }

        public virtual TrackingConfidence HandRightConfidence { get; set; }

        public virtual HandState HandRightState { get; set; }

        public virtual bool IsDisposed { get; set; }

        public virtual bool IsRestricted { get; set; }

        public virtual bool IsTracked { get; set; }

        protected Dictionary<JointType, JointOrientation> _jointOrientations;
        public virtual IReadOnlyDictionary<JointType, JointOrientation> JointOrientations
        {
            get { return _jointOrientations; }
            set { _jointOrientations = value as Dictionary<JointType, JointOrientation>; }
        }

        protected Dictionary<JointType, IJoint> _joints;
        public virtual IReadOnlyDictionary<JointType, IJoint> Joints
        {
            get { return _joints; }
            set { _joints = value as Dictionary<JointType, IJoint>; }
        }


#if NETFX_CORE
        public virtual Point Lean { get; set; }
#else
        public virtual PointF Lean { get; set; }
#endif

        public virtual TrackingState LeanTrackingState { get; set; }

        public virtual ulong TrackingId { get; set; }

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

            this.ClippedEdges = FrameEdges.None;
            this.Engaged = DetectionResult.Unknown;

            _expressions = new Dictionary<Expression, DetectionResult>();
            foreach (var expression in (Expression[])Enum.GetValues(typeof(Expression)))
            {
                _expressions.Add(expression, DetectionResult.Unknown);
            }

            this.HandLeftConfidence = TrackingConfidence.Low;
            this.HandLeftState = HandState.Unknown;
            this.HandRightConfidence = TrackingConfidence.Low;
            this.HandRightState = HandState.Unknown;
            this.IsDisposed = false;
            this.IsRestricted = false;
            this.IsTracked = false;

            _joints = new Dictionary<JointType, IJoint>();
            _jointOrientations = new Dictionary<JointType, JointOrientation>();
            foreach (var jointType in JointTypeEx.AllJoints)
            {
                _joints.Add(jointType, new CustomJoint(jointType));
                _jointOrientations.Add(jointType, new JointOrientation());
            }

#if NETFX_CORE
            this.Lean = new Point();
#else
            this.Lean = new PointF();
#endif
            this.LeanTrackingState = TrackingState.NotTracked;
            this.TrackingId = ulong.MaxValue;
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
