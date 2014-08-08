namespace KinectEx.Smoothing
{
    public class ExponentialSmoothingParameters
    {
        public static float DEFAULT_SMOOTHING = 0.25f;
        public static float DEFAULT_CORRECTION = 0.25f;
        public static float DEFAULT_PREDICTION = 0.25f;
        public static float DEFAULT_JITTER_RADIUS = 0.03f;
        public static float DEFAULT_MAX_DEVIATION_RADIUS = 0.25f;

        /// <summary>
        /// How much soothing will occur. Will lag when too high.
        /// </summary>
        public float Smoothing { get; set; }

        /// <summary>
        /// How much to correct back from prediction. Can make things springy.
        /// </summary>
        public float Correction { get; set; }

        /// <summary>
        /// Amount of prediction into the future to use. Can over shoot when too high.
        /// </summary>
        public float Prediction { get; set; }

        /// <summary>
        /// Maximum size of movement that will be assumed to be jitter.
        /// </summary>
        public float JitterRadius { get; set; }

        /// <summary>
        /// Size of the max prediction radius. Can snap back to noisy data when too high.
        /// </summary>
        public float MaxDeviationRadius { get; set; }

        public ExponentialSmoothingParameters()
        {
            Smoothing = DEFAULT_SMOOTHING;
            Correction = DEFAULT_CORRECTION;
            Prediction = DEFAULT_PREDICTION;
            JitterRadius = DEFAULT_JITTER_RADIUS;
            MaxDeviationRadius = DEFAULT_MAX_DEVIATION_RADIUS;
        }
    }
}
