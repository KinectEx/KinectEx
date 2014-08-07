using System;
using System.Globalization;

namespace KinectEx.KinectSDK
{
    public struct CameraSpacePoint
    {
        private float _x;

        public float X
        {
            get { return _x; }
            set { _x = value; }
        }

        private float _y;

        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }

        private float _z;

        public float Z
        {
            get { return _z; }
            set { _z = value; }
        }

        public CameraSpacePoint(float x, float y, float z)
        {
            this._x = x;
            this._y = y;
            this._z = z;
        }

        public static bool operator ==(CameraSpacePoint left, CameraSpacePoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CameraSpacePoint left, CameraSpacePoint right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is CameraSpacePoint)
            {
                var point = (CameraSpacePoint)obj;
                return this.X == point.X && this.Y == point.Y && this.Z == point.Z;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ (_y.GetHashCode() ^ (_z.GetHashCode()));
        }

        public override string ToString()
        {
            return String.Format("{{X={0}, Y={1}, Z={2}}}",
                _x.ToString(CultureInfo.CurrentCulture),
                _y.ToString(CultureInfo.CurrentCulture),
                _z.ToString(CultureInfo.CurrentCulture));
        }
    }
}
