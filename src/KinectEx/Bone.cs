namespace KinectEx
{
    /// <summary>
    /// A structure holding both the starting and ending joint values of
    /// a "bone" in a Kinect <c>Body</c>.
    /// </summary>
    public struct Bone
    {
        private BoneTypeEx _boneType;

        /// <summary>
        /// Gets or sets the type of the bone.
        /// </summary>
        public BoneTypeEx BoneType
        {
            get { return _boneType; }
            set { _boneType = value; }
        }


        private IJoint _startJoint;

        /// <summary>
        /// Gets or sets the start joint.
        /// </summary>
        public IJoint StartJoint
        {
            get { return _startJoint; }
            set { _startJoint = value; }
        }

        private IJoint _endJoint;

        /// <summary>
        /// Gets or sets the end joint.
        /// </summary>
        public IJoint EndJoint
        {
            get { return _endJoint; }
            set { _endJoint = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bone"/> struct.
        /// </summary>
        /// <param name="boneType">Type of the bone.</param>
        /// <param name="startJoint">The start joint.</param>
        /// <param name="endJoint">The end joint.</param>
        public Bone(BoneTypeEx boneType, IJoint startJoint, IJoint endJoint)
        {
            _boneType = boneType;
            _startJoint = startJoint;
            _endJoint = endJoint;
        }
    }
}
