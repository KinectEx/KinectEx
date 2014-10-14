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
    /// An interface that explicitly maps all of the Kinect SDK <c>Body</c>
    /// members so that different variations of body can be used for
    /// both smoothing and recording.
    /// </summary>
    public interface IBody
    {
        /// <summary>
        /// Gets or sets the clipped edges.
        /// </summary>
        FrameEdges ClippedEdges { get; set; }

        /// <summary>
        /// Gets or sets the hand left confidence.
        /// </summary>
        TrackingConfidence HandLeftConfidence { get; set; }

        /// <summary>
        /// Gets or sets the state of the hand left.
        /// </summary>
        HandState HandLeftState { get; set; }

        /// <summary>
        /// Gets or sets the hand right confidence.
        /// </summary>
        TrackingConfidence HandRightConfidence { get; set; }

        /// <summary>
        /// Gets or sets the state of the hand right.
        /// </summary>
        HandState HandRightState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is restricted.
        /// </summary>
        bool IsRestricted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tracked.
        /// </summary>
        bool IsTracked { get; set; }

        /// <summary>
        /// Gets the joint orientations.
        /// </summary>
        IReadOnlyDictionary<JointType, IJointOrientation> JointOrientations { get; }

        /// <summary>
        /// Gets or sets the joints.
        /// </summary>
        IReadOnlyDictionary<JointType, IJoint> Joints { get; set; }

#if NETFX_CORE
        /// <summary>
        /// Gets or sets the lean.
        /// </summary>
        Point Lean { get; set; }
#else
        /// <summary>
        /// Gets or sets the lean.
        /// </summary>
        PointF Lean { get; set; }
#endif

        /// <summary>
        /// Gets or sets the state of the lean tracking.
        /// </summary>
        TrackingState LeanTrackingState { get; set; }

        /// <summary>
        /// Gets or sets the tracking identifier.
        /// </summary>
        ulong TrackingId { get; set; }

        /// <summary>
        /// Updates the specified body.
        /// </summary>
        /// <param name="body">The body.</param>
        void Update(IBody body);

#if !NOSDK
        /// <summary>
        /// Updates the specified body.
        /// </summary>
        /// <param name="body">The body.</param>
        void Update(Body body);
#endif
    }
}
