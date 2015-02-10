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
    /// are "smoothed" using a Double Exponential filtering algorithm.
    /// </summary>
    public class ExponentialJoint : CustomJoint
    {
        float _smoothing,
              _correction,
              _prediction,
              _jitterRadius,
              _maxDeviationRadius;
        FilterDoubleExponentialData _history;

        /// <summary>
        /// Create an exponential smoothing joint with default configuration values.
        /// </summary>
        /// <param name="jointType">The joint type to create</param>
        public ExponentialJoint(JointType jointType)
            : base(jointType)
        {
            var parms = new ExponentialSmoothingParameters();

            _smoothing = parms.Smoothing;
            _correction = parms.Correction;
            _prediction = parms.Prediction;
            _jitterRadius = parms.JitterRadius;
            _maxDeviationRadius = parms.MaxDeviationRadius;
            _history = new FilterDoubleExponentialData();
        }

        /// <summary>
        /// Create an exponential smoothing joint with custom configuration values.
        /// </summary>
        /// <param name="jointType">The joint type to create</param>
        /// <param name="parameters">An <c>ExponentialSmoothingParameters</c> object</param>
        public ExponentialJoint(JointType jointType, ISmootherParameters parameters = null)
            : base(jointType)
        {
            var parms = parameters as ExponentialSmoothingParameters;

            if (parms == null)
                parms = new ExponentialSmoothingParameters();

            _smoothing = parms.Smoothing;
            _correction = parms.Correction;
            _prediction = parms.Prediction;
            // Check for divide by zero. Use an epsilon of a 10th of a millimeter
            _jitterRadius = Math.Max(0.0001f, parms.JitterRadius);
            _maxDeviationRadius = parms.MaxDeviationRadius;
            _history = new FilterDoubleExponentialData();
        }

        private void Reset()
        {
            _history = new FilterDoubleExponentialData();
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
                Reset();
                return;
            }

            // If not tracked, we smooth a bit more by using a bigger jitter radius
            // Always filter feet highly as they are so noisy
            var jitterRadius = _jitterRadius;
            var maxDeviationRadius = _maxDeviationRadius;
            if (joint.TrackingState != TrackingState.Tracked)
            {
                jitterRadius *= 2.0f;
                maxDeviationRadius *= 2.0f;
            }

            Vector3 filteredPosition,
                    positionDelta,
                    trend;
            float diffVal;

            var reportedPosition = new Vector3(joint.Position.X, joint.Position.Y, joint.Position.Z);

            var prevFilteredPosition = _history.FilteredPosition;
            var prevTrend = _history.Trend;
            var prevRawPosition = _history.ReportedPosition;

            // If joint is invalid, reset the filter
            if (reportedPosition.X == 0 && reportedPosition.Y == 0 && reportedPosition.Z == 0)
            {
                Reset();
            }

            // Initial start values
            if (this._history.FrameCount == 0)
            {
                filteredPosition = reportedPosition;
                trend = new Vector3();
            }
            else if (this._history.FrameCount == 1)
            {
                filteredPosition = (reportedPosition + prevRawPosition) * 0.5f;
                positionDelta = filteredPosition - prevFilteredPosition;
                trend = (positionDelta * _correction) + (prevTrend * (1.0f - _correction));
            }
            else
            {
                // First apply jitter filter
                positionDelta = reportedPosition - prevFilteredPosition;
                diffVal = Math.Abs(positionDelta.Length());

                if (diffVal <= jitterRadius)
                {
                    filteredPosition = (reportedPosition * (diffVal / jitterRadius)) +
                                       (prevFilteredPosition * (1.0f - (diffVal / jitterRadius)));
                }
                else
                {
                    filteredPosition = reportedPosition;
                }

                // Now the double exponential smoothing filter
                filteredPosition = (filteredPosition * (1.0f - _smoothing)) +
                                   ((prevFilteredPosition + prevTrend) * _smoothing);

                positionDelta = filteredPosition - prevFilteredPosition;
                trend = (positionDelta * _correction) + (prevTrend * (1.0f - _correction));
            }

            // Predict into the future to reduce latency
            var predictedPosition = filteredPosition + (trend * _prediction);

            // Check that we are not too far away from raw data
            positionDelta = predictedPosition - reportedPosition;
            diffVal = Math.Abs(positionDelta.Length());

            if (diffVal > maxDeviationRadius)
            {
                predictedPosition = (predictedPosition * (maxDeviationRadius / diffVal)) +
                                    (reportedPosition * (1.0f - (maxDeviationRadius / diffVal)));
            }

            // Save the data from this frame
            this._history.ReportedPosition = reportedPosition;
            this._history.FilteredPosition = filteredPosition;
            this._history.Trend = trend;
            this._history.FrameCount++;

            // Set the filtered data back into the joint
            _position.X = predictedPosition.X;
            _position.Y = predictedPosition.Y;
            _position.Z = predictedPosition.Z;

            _depthPosition = new DepthSpacePoint();
            _colorPosition = new ColorSpacePoint();
        }

        private struct FilterDoubleExponentialData
        {
            public Vector3 ReportedPosition;
            public Vector3 FilteredPosition;
            public Vector3 Trend;
            public uint FrameCount;
        }
    }
}
