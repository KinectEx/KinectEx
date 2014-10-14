namespace KinectEx.Smoothing
{
    /// <summary>
    /// Contains member properties used to configure a <c>KalmanSmooter</c>.
    /// </summary>
    public class KalmanSmoothingParameters : ISmootherParameters
    {
        /// <summary>
        /// The default measurement uncertainty
        /// </summary>
        private static float DEFAULT_MEASUREMENT_UNCERTAINTY = 0.001f;

        /// <summary>
        /// The default jitter radius
        /// </summary>
        private static float DEFAULT_JITTER_RADIUS = 0.03f;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="KalmanSmoothingParameters"/> class.
        /// </summary>
        public KalmanSmoothingParameters()
        {
            MeasurementUncertainty = DEFAULT_MEASUREMENT_UNCERTAINTY;
            JitterRadius = DEFAULT_JITTER_RADIUS;
        }
    }
}
