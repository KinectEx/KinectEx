using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx
{
    public struct CameraSpacePointEx
    {
        private CameraSpacePoint _point;

        public CameraSpacePointEx(CameraSpacePoint point)
        {
            _point = point;
        }
        
        public float X
        {
            get { return _point.X; }
            set { _point.X = value; }
        }

        public float Y
        {
            get { return _point.Y; }
            set { _point.Y = value; }
        }

        public float Z
        {
            get { return _point.Z; }
            set { _point.Z = value; }
        }

        public double Length
        {
            get
            {
                return Math.Sqrt(this.LengthSquared);
            }
        }

        public double LengthSquared
        {
            get
            {
                return _point.X * _point.X + _point.Y * _point.Y + _point.Z * _point.Z;
            }
        }

        public static bool operator ==(CameraSpacePointEx left, CameraSpacePointEx right)
        {
            return left._point == right._point;
        }

        public static bool operator !=(CameraSpacePointEx left, CameraSpacePointEx right)
        {
            return left._point != right._point;
        }

        public static bool operator ==(CameraSpacePoint left, CameraSpacePointEx right)
        {
            return left == right._point;
        }

        public static bool operator !=(CameraSpacePoint left, CameraSpacePointEx right)
        {
            return left != right._point;
        }

        public static bool operator ==(CameraSpacePointEx left, CameraSpacePoint right)
        {
            return left._point == right;
        }

        public static bool operator !=(CameraSpacePointEx left, CameraSpacePoint right)
        {
            return left._point != right;
        }

        public static implicit operator CameraSpacePoint(CameraSpacePointEx pointEx)
        {
            return pointEx._point;
        }

        public static implicit operator CameraSpacePointEx(CameraSpacePoint point)
        {
            return new CameraSpacePointEx(point);
        }

        public override bool Equals(object obj)
        {
            if (obj is CameraSpacePointEx)
                return this._point == ((CameraSpacePointEx)obj)._point;
            else if (obj is CameraSpacePoint)
                return this._point == (CameraSpacePoint)obj;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return _point.GetHashCode();
        }

        public override string ToString()
        {
            return _point.ToString();
        }
    }
}
