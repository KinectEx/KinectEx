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
    public class ExponentialJoint : CustomJoint
    {
        float _smoothing, _correction, _prediction, _jitterRadius,_maxDeviationRadius;
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
        public ExponentialJoint(JointType jointType, object parameters = null)
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
            this._history = new FilterDoubleExponentialData();
        }

        public override void Update(IJoint joint)
        {
            if (joint.TrackingState == TrackingState.NotTracked)
            {
                Reset();
                return;
            }

            this.TrackingState = joint.TrackingState;

            // If not tracked, we smooth a bit more by using a bigger jitter radius
            // Always filter feet highly as they are so noisy
            var jitterRadius = _jitterRadius;
            var maxDeviationRadius = _maxDeviationRadius;
            if (joint.TrackingState != TrackingState.Tracked)
            {
                jitterRadius *= 2.0f;
                maxDeviationRadius *= 2.0f;
            }

            Vector3 filteredPosition;
            Vector3 diffvec;
            Vector3 trend;
            float diffVal;

            Vector3 rawPosition = new Vector3()
                {
                    X = joint.Position.X,
                    Y = joint.Position.Y,
                    Z = joint.Position.Z
                };
            Vector3 prevFilteredPosition = this._history.FilteredPosition;
            Vector3 prevTrend = this._history.Trend;
            Vector3 prevRawPosition = this._history.RawPosition;

            // If joint is invalid, reset the filter
            if (rawPosition.X == 0 && rawPosition.Y == 0 && rawPosition.Z == 0)
            {
                Reset();
            }

            // Initial start values
            if (this._history.FrameCount == 0)
            {
                filteredPosition = rawPosition;
                trend = new Vector3();
            }
            else if (this._history.FrameCount == 1)
            {
                filteredPosition = (rawPosition + prevRawPosition) * 0.5f;
                diffvec = filteredPosition - prevFilteredPosition;
                trend = (diffvec * _correction) + (prevTrend * (1.0f - _correction));
            }
            else
            {
                // First apply jitter filter
                diffvec = rawPosition - prevFilteredPosition;
                diffVal = Math.Abs(diffvec.Length());

                if (diffVal <= jitterRadius)
                {
                    filteredPosition = (rawPosition * (diffVal / jitterRadius)) +
                                       (prevFilteredPosition * (1.0f - (diffVal / jitterRadius)));
                }
                else
                {
                    filteredPosition = rawPosition;
                }

                // Now the double exponential smoothing filter
                filteredPosition = (filteredPosition * (1.0f - _smoothing)) +
                                   ((prevFilteredPosition + prevTrend) * _smoothing);

                diffvec = filteredPosition - prevFilteredPosition;
                trend = (diffvec * _correction) + (prevTrend * (1.0f - _correction));
            }

            // Predict into the future to reduce latency
            Vector3 predictedPosition = filteredPosition + (trend * _prediction);

            // Check that we are not too far away from raw data
            diffvec = predictedPosition - rawPosition;
            diffVal = Math.Abs(diffvec.Length());

            if (diffVal > maxDeviationRadius)
            {
                predictedPosition = (predictedPosition * (maxDeviationRadius / diffVal)) +
                                    (rawPosition * (1.0f - (maxDeviationRadius / diffVal)));
            }

            // Save the data from this frame
            this._history.RawPosition = rawPosition;
            this._history.FilteredPosition = filteredPosition;
            this._history.Trend = trend;
            this._history.FrameCount++;

            // Set the filtered data back into the joint
            var jointPos = this.Position;
            jointPos.X = predictedPosition.X;
            jointPos.Y = predictedPosition.Y;
            jointPos.Z = predictedPosition.Z;
            this.Position = jointPos;
        }

        private struct FilterDoubleExponentialData
        {
            public Vector3 RawPosition;
            public Vector3 FilteredPosition;
            public Vector3 Trend;
            public uint FrameCount;
        }
    }
}
