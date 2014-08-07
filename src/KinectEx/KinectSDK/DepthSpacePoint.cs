using System;
using System.Globalization;

namespace KinectEx.KinectSDK
{
    public struct DepthSpacePoint
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

        public DepthSpacePoint(float x, float y)
        {
            this._x = x;
            this._y = y;
        }

        public static bool operator ==(DepthSpacePoint left, DepthSpacePoint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DepthSpacePoint left, DepthSpacePoint right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is DepthSpacePoint)
            {
                var point = (DepthSpacePoint)obj;
                return this.X == point.X && this.Y == point.Y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{{X={0}, Y={1}}}",
                _x.ToString(CultureInfo.CurrentCulture),
                _y.ToString(CultureInfo.CurrentCulture));
        }
    }
}
