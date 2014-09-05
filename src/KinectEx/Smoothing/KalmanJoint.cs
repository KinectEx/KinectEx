using SharpDX;
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
    /// A class representing a <c>CustomJoint</c> who's position values
    /// are "smoothed" using a Kalman-like filtering algorithm.
    /// </summary>
    public class KalmanJoint : CustomJoint
    {
        private Vector3 _filteredPosition,
                        _velocity,
                        _velocityDelta,
                        _positionVariance,
                        _velocityVariance,
                        _measurementUncertainty;
        float _jitterRadius;
        private bool _init = true;

        /// <summary>
        /// Create a Kalman smoothing joint with default configuration values.
        /// </summary>
        /// <param name="jointType">The joint type to create</param>
        public KalmanJoint(JointType jointType)
            : base(jointType)
        {
            var parms = new KalmanSmoothingParameters();
            _measurementUncertainty = new Vector3(parms.MeasurementUncertainty);
            _jitterRadius = parms.JitterRadius;
        }

        /// <summary>
        /// Create a Kalman smoothing joint with custom configuration values.
        /// </summary>
        /// <param name="jointType">The joint type to create</param>
        /// <param name="parameters">An <c>ExponentialSmoothingParameters</c> object</param>
        public KalmanJoint(JointType jointType, ISmootherParameters parameters = null)
            : base(jointType)
        {
            var parms = parameters as KalmanSmoothingParameters;

            if (parms == null)
                parms = new KalmanSmoothingParameters();

            _measurementUncertainty = new Vector3(parms.MeasurementUncertainty);
            _jitterRadius = parms.JitterRadius;
        }

        /// <summary>
        /// Update (and filter) the joint position based on the referenced <c>IJoint</c>.
        /// </summary>
        public override void Update(IJoint joint)
        {
            _trackingState = joint.TrackingState;

            if (_trackingState == TrackingState.NotTracked)
            {
                _position = new CameraSpacePoint();
                return;
            }

            if (_init)
            {
                _init = false;

                _filteredPosition = new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);
                _positionVariance = new Vector3(1000f);
                _velocityVariance = new Vector3(1000f);
            }
            else
            {
                var oldPosition = _filteredPosition;
                var oldVelocity = _velocity;

                // Predict
                _filteredPosition = _filteredPosition + _velocity;
                _velocity = _velocity + _velocityDelta;
                _positionVariance = _positionVariance + _measurementUncertainty;
                _velocityVariance = _velocityVariance + _measurementUncertainty;

                // Update
                var reportedPosition = new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);

                // ... but first filter for jitter ...
                var positionDelta = reportedPosition - oldPosition;
                var differenceLength = Math.Abs((float)positionDelta.Length());
                var jitterRadiusMod = joint.TrackingState == TrackingState.Tracked ? _jitterRadius : _jitterRadius * 2;
                if (differenceLength <= jitterRadiusMod)
                {
                    reportedPosition = (reportedPosition * (differenceLength / jitterRadiusMod)) +
                             (oldPosition * (1.0f - (differenceLength / jitterRadiusMod)));
                }

                _filteredPosition = (_positionVariance * reportedPosition + _measurementUncertainty * _filteredPosition) / (_measurementUncertainty + _positionVariance);
                _velocity = (_velocityVariance * (reportedPosition - oldPosition) + _measurementUncertainty * _velocity) / (_measurementUncertainty + _velocityVariance);

                _velocityDelta = _velocity - oldVelocity;

                _positionVariance = Vector3.One / ((Vector3.One / _measurementUncertainty) + (Vector3.One / _positionVariance));
                _velocityVariance = Vector3.One / ((Vector3.One / _measurementUncertainty) + (Vector3.One / _velocityVariance));
            }

            _position.X = _filteredPosition.X;
            _position.Y = _filteredPosition.Y;
            _position.Z = _filteredPosition.Z;
        }
    }
}
