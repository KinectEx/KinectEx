namespace KinectEx.Smoothing
{
    class KalmanSmoothingParameters
    {
        const float DEFAULT_MEASUREMENT_UNCERTAINTY = 0.001f;
        const float DEFAULT_JITTER_RADIUS = 0.03f;

        public float MeasurementUncertainty { get; set; }
        public float JitterRadius { get; set; }

        public KalmanSmoothingParameters()
        {
            MeasurementUncertainty = DEFAULT_MEASUREMENT_UNCERTAINTY;
            JitterRadius = DEFAULT_JITTER_RADIUS;
        }
    }
}
