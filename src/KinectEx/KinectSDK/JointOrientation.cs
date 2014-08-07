using System;
using System.Globalization;

namespace KinectEx.KinectSDK
{
    public struct JointOrientation
    {
        private JointType _jointType;

        public JointType JointType
        {
            get { return _jointType; }
            set { _jointType = value; }
        }

        private Vector4 _orientation;

        public Vector4 Orientation
        {
            get { return _orientation; }
            set { _orientation = value; }
        }

        public static bool operator ==(JointOrientation left, JointOrientation right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(JointOrientation left, JointOrientation right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is JointOrientation)
            {
                var jo = (JointOrientation)obj;
                return this.JointType == jo.JointType && this.Orientation == jo.Orientation;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _jointType.GetHashCode() ^ _orientation.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{{JointType={0}, Orientation={1}}}",
                _jointType.ToString(),
                _orientation.ToString());
        }
    }
}
