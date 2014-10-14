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
        private FrameEdges _clippedEdges;

        /// <summary>
        /// Gets or sets the clipped edges.
        /// </summary>
        public virtual FrameEdges ClippedEdges
        {
            get { return _clippedEdges; }
            set { _clippedEdges = value; }
        }

        private TrackingConfidence _handLeftConfidence;

        /// <summary>
        /// Gets or sets the hand left confidence.
        /// </summary>
        public virtual TrackingConfidence HandLeftConfidence
        {
            get { return _handLeftConfidence; }
            set { _handLeftConfidence = value; }
        }

        private HandState _handLeftState;

        /// <summary>
        /// Gets or sets the state of the hand left.
        /// </summary>
        public virtual HandState HandLeftState
        {
            get { return _handLeftState; }
            set { _handLeftState = value; }
        }

        private TrackingConfidence _handRightConfidence;

        /// <summary>
        /// Gets or sets the hand right confidence.
        /// </summary>
        public virtual TrackingConfidence HandRightConfidence
        {
            get { return _handRightConfidence; }
            set { _handRightConfidence = value; }
        }

        private HandState _handRightState;

        /// <summary>
        /// Gets or sets the state of the hand right.
        /// </summary>
        public virtual HandState HandRightState
        {
            get { return _handRightState; }
            set { _handRightState = value; }
        }

        private bool _isDisposed;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsDisposed
        {
            get { return _isDisposed; }
            set { _isDisposed = value; }
        }

        private bool _isRestricted;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is restricted.
        /// </summary>
        public virtual bool IsRestricted
        {
            get { return _isRestricted; }
            set { _isRestricted = value; }
        }

        private bool _isTracked;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tracked.
        /// </summary>
        public virtual bool IsTracked
        {
            get { return _isTracked; }
            set { _isTracked = value; }
        }

        /// <summary>
        /// The joint orientations.
        /// </summary>
        protected Dictionary<JointType, IJointOrientation> _jointOrientations;

        /// <summary>
        /// Gets the joint orientations.
        /// </summary>
        public virtual IReadOnlyDictionary<JointType, IJointOrientation> JointOrientations
        {
            get { return _jointOrientations; }
            set { _jointOrientations = value as Dictionary<JointType, IJointOrientation>; }
        }

        /// <summary>
        /// The joints.
        /// </summary>
        protected Dictionary<JointType, IJoint> _joints;

        /// <summary>
        /// Gets or sets the joints.
        /// </summary>
        public virtual IReadOnlyDictionary<JointType, IJoint> Joints
        {
            get { return _joints; }
            set { _joints = value as Dictionary<JointType, IJoint>; }
        }


#if NETFX_CORE
        private Point _lean;

        /// <summary>
        /// Gets or sets the lean.
        /// </summary>
        public virtual Point Lean
        {
            get { return _lean; }
            set { _lean = value; }
        }
#else
        private PointF _lean;

        /// <summary>
        /// Gets or sets the lean.
        /// </summary>
        public virtual PointF Lean
        {
            get { return _lean; }
            set { _lean = value; }
        }
#endif

        private TrackingState _leanTrackingState;

        /// <summary>
        /// Gets or sets the state of the lean tracking.
        /// </summary>
        public virtual TrackingState LeanTrackingState
        {
            get { return _leanTrackingState; }
            set { _leanTrackingState = value; }
        }

        private ulong _trackingId;

        /// <summary>
        /// Gets or sets the tracking identifier.
        /// </summary>
        public virtual ulong TrackingId
        {
            get { return _trackingId; }
            set { _trackingId = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomBody"/> class.
        /// </summary>
        public CustomBody()
        {
            _clippedEdges = FrameEdges.None;

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
            this.ClippedEdges = body.ClippedEdges;
            this.HandLeftConfidence = body.HandLeftConfidence;
            this.HandLeftState = body.HandLeftState;
            this.HandRightConfidence = body.HandRightConfidence;
            this.HandRightState = body.HandRightState;
            this.IsRestricted = body.IsRestricted;
            this.IsTracked = body.IsTracked;

            foreach (var key in body.JointOrientations.Keys)
            {
                _jointOrientations[key].Update(body.JointOrientations[key]);
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
