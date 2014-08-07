using SharpDX;
using System;

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx.Smoothing
{
    public class KalmanJoint : CustomJoint
    {
        private Vector3 _pos, _vel, _deltavel, _posvar, _velvar, _measurementUncertainty;
        private Vector3 _oldpos, _oldvel;
        private Vector3 _rawpos, _diffvec;
        float _diffval;
        float _jitterRadius;
        private bool _init = true;

        public KalmanJoint(JointType jointType)
            : base(jointType)
        {
            var parms = new KalmanSmoothingParameters();
            _measurementUncertainty.X = parms.MeasurementUncertainty;
            _measurementUncertainty.Y = parms.MeasurementUncertainty;
            _measurementUncertainty.Z = parms.MeasurementUncertainty;
            _jitterRadius = parms.JitterRadius;
        }

        public KalmanJoint(JointType jointType, object parameters = null)
            : base(jointType)
        {
            var parms = parameters as KalmanSmoothingParameters;

            if (parms == null)
                parms = new KalmanSmoothingParameters();

            _measurementUncertainty.X = parms.MeasurementUncertainty;
            _measurementUncertainty.Y = parms.MeasurementUncertainty;
            _measurementUncertainty.Z = parms.MeasurementUncertainty;
            _jitterRadius = parms.JitterRadius;
        }

        public override void Update(IJoint joint)
        {
            if (joint.TrackingState == TrackingState.NotTracked)
                return;

            this.TrackingState = joint.TrackingState;

            if (_init)
            {
                _init = false;

                _pos.X = joint.Position.X;
                _pos.Y = joint.Position.Y;
                _pos.Z = joint.Position.Z;
                _posvar.X = 1000;
                _posvar.Y = 1000;
                _posvar.Z = 1000;
                _velvar.X = 1000;
                _velvar.Y = 1000;
                _velvar.Z = 1000;
            }
            else
            {
                _oldpos = _pos;
                _oldvel = _vel;

                // Predict
                _pos = _pos + _vel;
                _vel = _vel + _deltavel;
                _posvar = _posvar + _measurementUncertainty;
                _velvar = _velvar + _measurementUncertainty;

                // Update
                _rawpos.X = joint.Position.X;
                _rawpos.Y = joint.Position.Y;
                _rawpos.Z = joint.Position.Z;

                // ... but first filter for jitter ...
                _diffvec = _rawpos - _oldpos;
                _diffval = Math.Abs(_diffvec.Length());
                var jr = joint.TrackingState == TrackingState.Tracked ? _jitterRadius : _jitterRadius * 2;
                if (_diffval <= jr)
                {
                    _rawpos = (_rawpos * (_diffval / jr)) +
                              (_oldpos * (1.0f - (_diffval / jr)));
                }

                _pos.X = (_posvar.X * _rawpos.X + _measurementUncertainty.X * _pos.X) / (_measurementUncertainty.X + _posvar.X);
                _pos.Y = (_posvar.Y * _rawpos.Y + _measurementUncertainty.Y * _pos.Y) / (_measurementUncertainty.Y + _posvar.Y);
                _pos.Z = (_posvar.Z * _rawpos.Z + _measurementUncertainty.Z * _pos.Z) / (_measurementUncertainty.Z + _posvar.Z);

                _vel.X = (_velvar.X * (_rawpos.X - _oldpos.X) + _measurementUncertainty.X * _vel.X) / (_measurementUncertainty.X + _velvar.X);
                _vel.Y = (_velvar.Y * (_rawpos.Y - _oldpos.Y) + _measurementUncertainty.Y * _vel.Y) / (_measurementUncertainty.Y + _velvar.Y);
                _vel.Z = (_velvar.Z * (_rawpos.Z - _oldpos.Z) + _measurementUncertainty.Z * _vel.Z) / (_measurementUncertainty.Z + _velvar.Z);

                _deltavel = _vel - _oldvel;

                _posvar.X = 1 / ((1 / _measurementUncertainty.X) + (1 / _posvar.X));
                _posvar.Y = 1 / ((1 / _measurementUncertainty.Y) + (1 / _posvar.Y));
                _posvar.Z = 1 / ((1 / _measurementUncertainty.Z) + (1 / _posvar.Z));

                _velvar.X = 1 / ((1 / _measurementUncertainty.X) + (1 / _velvar.X));
                _velvar.Y = 1 / ((1 / _measurementUncertainty.Y) + (1 / _velvar.Y));
                _velvar.Z = 1 / ((1 / _measurementUncertainty.Z) + (1 / _velvar.Z));
            }

            var jointPos = this.Position;
            jointPos.X = _pos.X;
            jointPos.Y = _pos.Y;
            jointPos.Z = _pos.Z;
            this.Position = jointPos;
        }
    }
}
