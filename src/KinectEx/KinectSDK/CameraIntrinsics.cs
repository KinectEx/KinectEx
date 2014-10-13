using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectEx.KinectSDK
{
    public struct CameraIntrinsics
    {
        // Summary:
        //     The X focal length of the camera, in pixels.
        public float FocalLengthX { get; set; }

        //
        // Summary:
        //     The Y focal length of the camera, in pixels.
        public float FocalLengthY { get; set; }

        //
        // Summary:
        //     The principal point of the camera in the X dimension, in pixels.
        public float PrincipalPointX { get; set; }

        //
        // Summary:
        //     The principal point of the camera in the Y dimension, in pixels.
        public float PrincipalPointY { get; set; }

        //
        // Summary:
        //     The fourth order radial distortion parameter of the camera.
        public float RadialDistortionFourthOrder { get; set; }

        //
        // Summary:
        //     The second order radial distortion parameter of the camera.
        public float RadialDistortionSecondOrder { get; set; }

        //
        // Summary:
        //     The sixth order radial distortion parameter of the camera.
        public float RadialDistortionSixthOrder { get; set; }
    }
}
