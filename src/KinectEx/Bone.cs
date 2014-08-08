namespace KinectEx
{
    /// <summary>
    /// A structure holding both the starting and ending joint values of
    /// a "bone" in a Kinect <c>Body</c>.
    /// </summary>
    public struct Bone
    {
        private BoneTypeEx _boneType;

        public BoneTypeEx BoneType
        {
            get { return _boneType; }
            set { _boneType = value; }
        }
        

        private IJoint _startJoint;

        public IJoint StartJoint
        {
            get { return _startJoint; }
            set { _startJoint = value; }
        }

        private IJoint _endJoint;

        public IJoint EndJoint
        {
            get { return _endJoint; }
            set { _endJoint = value; }
        }
        
        public Bone(BoneTypeEx boneType, IJoint startJoint, IJoint endJoint)
        {
            _boneType = boneType;
            _startJoint = startJoint;
            _endJoint = endJoint;
        }
    }
}
