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
        private Dictionary<JointType, IJointOrientation> _jointOrientations = null;

        /// <summary>
        /// Gets or sets the clipped edges.
        /// </summary>
        public FrameEdges ClippedEdges
        {
            get { return _body.ClippedEdges; }
            set { }
        }

        /// <summary>
        /// Gets or sets the hand left confidence.
        /// </summary>
        public TrackingConfidence HandLeftConfidence
        {
            get { return _body.HandLeftConfidence; }
            set { }
        }

        /// <summary>
        /// Gets or sets the state of the hand left.
        /// </summary>
        public HandState HandLeftState
        {
            get { return _body.HandLeftState; }
            set { }
        }

        /// <summary>
        /// Gets or sets the hand right confidence.
        /// </summary>
        public TrackingConfidence HandRightConfidence
        {
            get { return _body.HandRightConfidence; }
            set { }
        }

        /// <summary>
        /// Gets or sets the state of the hand right.
        /// </summary>
        public HandState HandRightState
        {
            get { return _body.HandRightState; }
            set { }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is restricted.
        /// </summary>
        public bool IsRestricted
        {
            get { return _body.IsRestricted; }
            set { }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tracked.
        /// </summary>
        public bool IsTracked
        {
            get { return _body.IsTracked; }
            set { }
        }

        /// <summary>
        /// Gets the joint orientations.
        /// </summary>
        public IReadOnlyDictionary<JointType, IJointOrientation> JointOrientations
        {
            get
            {
                if (_jointOrientations == null)
                {
                    _jointOrientations = new Dictionary<JointType, IJointOrientation>();
                    foreach (var key in _body.Joints.Keys)
                    {
                        _jointOrientations.Add(key, new KinectJointOrientation(_body.JointOrientations[key]));
                    }
                }
                return _jointOrientations;
            }
            set { }
        }

        /// <summary>
        /// Gets or sets the joints.
        /// </summary>
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
        /// <summary>
        /// Gets or sets the lean.
        /// </summary>
        public Point Lean
#else
        /// <summary>
        /// Gets or sets the lean.
        /// </summary>
        public PointF Lean
#endif
        {
            get { return _body.Lean; }
            set { }
        }

        /// <summary>
        /// Gets or sets the state of the lean tracking.
        /// </summary>
        public TrackingState LeanTrackingState
        {
            get { return _body.LeanTrackingState; }
            set { }
        }

        /// <summary>
        /// Gets or sets the tracking identifier.
        /// </summary>
        public ulong TrackingId
        {
            get { return _body.TrackingId; }
            set { }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectBody"/> class.
        /// </summary>
        public KinectBody() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="KinectBody"/> class.
        /// </summary>
        /// <param name="body">The body.</param>
        public KinectBody(Body body)
        {
            this._body = body;
        }

        /// <summary>
        /// Updates the specified body.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <exception cref="System.NotSupportedException">Must update a KinectBody with a native Kinect SDK Body</exception>
        public void Update(IBody body)
        {
            throw new NotSupportedException("Must update a KinectBody with a native Kinect SDK Body");
        }

        /// <summary>
        /// Updates the specified body.
        /// </summary>
        /// <param name="body">The body.</param>
        public void Update(Body body)
        {
            this._body = body;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Body"/> to <see cref="KinectBody"/>.
        /// </summary>
        /// <param name="body">The body.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator KinectBody(Body body)
        {
            return new KinectBody(body);
        }
    }
}

#endif