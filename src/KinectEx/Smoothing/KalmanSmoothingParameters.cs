namespace KinectEx.Smoothing
{
    class KalmanSmoothingParameters
    {
        public static float DEFAULT_MEASUREMENT_UNCERTAINTY = 0.001f;
        public static float DEFAULT_JITTER_RADIUS = 0.03f;

        /// <summary>
        /// The degree of "uncertainty" applied to the X, Y, and Z measurements
        /// supplied by the Kinect sensor. In effect, higher values place more
        /// emphasis on the predicted location (based on velocity and delta-velocity
        /// vectors). So higher uncertainty means smoother movement but higher
        /// latency. The default value is 0.001f.
        /// </summary>
        public float MeasurementUncertainty { get; set; }

        /// <summary>
        /// Maximum size (in meters) of movement that will be assumed to be jitter.
        /// The default value is 0.03f.
        /// </summary>
        public float JitterRadius { get; set; }

        public KalmanSmoothingParameters()
        {
            MeasurementUncertainty = DEFAULT_MEASUREMENT_UNCERTAINTY;
            JitterRadius = DEFAULT_JITTER_RADIUS;
        }
    }
}
