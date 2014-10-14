using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectEx.KinectSDK
{
    public struct CameraIntrinsics
    {
        /// <summary>
        /// Gets or sets the X focal length of the camera, in pixels.
        /// </summary>
        public float FocalLengthX { get; set; }

        /// <summary>
        /// Gets or sets the Y focal length of the camera, in pixels.
        /// </summary>
        public float FocalLengthY { get; set; }

        /// <summary>
        /// Gets or sets the principal point of the camera in the X dimension, in pixels.
        /// </summary>
        public float PrincipalPointX { get; set; }

        /// <summary>
        /// Gets or sets the principal point of the camera in the Y dimension, in pixels.
        /// </summary>
        public float PrincipalPointY { get; set; }

        /// <summary>
        /// Gets or sets the fourth order radial distortion parameter of the camera.
        /// </summary>
        public float RadialDistortionFourthOrder { get; set; }

        /// <summary>
        /// Gets or sets the second order radial distortion parameter of the camera.
        /// </summary>
        public float RadialDistortionSecondOrder { get; set; }

        /// <summary>
        /// Gets or sets the sixth order radial distortion parameter of the camera.
        /// </summary>
        public float RadialDistortionSixthOrder { get; set; }
    }
}
