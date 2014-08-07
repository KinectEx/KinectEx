using System;
using System.Globalization;

namespace KinectEx.KinectSDK
{
    public struct Vector4
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

        private float _w;

        public float W
        {
            get { return _w; }
            set { _w = value; }
        }

        public Vector4(float x, float y, float z, float w)
        {
            _x = x;
            _y = y;
            _z = z;
            _w = w;
        }

        public static bool operator ==(Vector4 left, Vector4 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector4 left, Vector4 right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector4)
            {
                var vector = (Vector4)obj;
                return _x == vector.X && _y == vector.Y && _z == vector.Z && _w == vector.W;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ (_y.GetHashCode() ^ (_z.GetHashCode() ^ _w.GetHashCode()));
        }

        public override string ToString()
        {
            return String.Format("{{X={0}, Y={1}, Z={2}, W={3}}",
                _x.ToString(CultureInfo.CurrentCulture),
                _y.ToString(CultureInfo.CurrentCulture),
                _z.ToString(CultureInfo.CurrentCulture),
                _w.ToString(CultureInfo.CurrentCulture));
        }
    }
}
