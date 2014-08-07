using System.Collections.Generic;
using System.Linq;

#if NETFX_CORE
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endif

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx
{
    public static class BodyImageExtension
    {
        /// <summary>
        /// Width of output drawing
        /// </summary>
        private static float _width;
        private static float _halfWidth;

        /// <summary>
        /// Height of our output drawing
        /// </summary>
        private static float _height;
        private static float _halfHeight;

        /// <summary>
        /// Thickness of drawn joint circles
        /// </summary>
        private const double _jointThickness = 3;
        
        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double _clipBoundsThickness = 5;

#if !NOSDK
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private static KinectSensor _kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private static CoordinateMapper _coordinateMapper = null;
#endif

        /// <summary>
        /// Initialize static values
        /// </summary>
        static BodyImageExtension()
        {
#if NOSDK
            _width = 512;
            _height = 424;
#else
            _kinectSensor = KinectSensor.GetDefault();

            if (_kinectSensor.IsAvailable)
                _coordinateMapper = _kinectSensor.CoordinateMapper;

            _width = _kinectSensor.DepthFrameSource.FrameDescription.Width;
            _height = _kinectSensor.DepthFrameSource.FrameDescription.Height;
#endif
            _halfWidth = _width / 2;
            _halfHeight = _height / 2;
        }

        /// <summary>
        /// Gets the body joints and bones drawn as a WriteableBitmap
        /// </summary>
        /// <param name="boneColor">color to use for drawing bones</param>
        /// <param name="jointColor">color to use for drawing joints</param>
        public static WriteableBitmap GetBitmap(this IBody body, Color boneColor, Color jointColor)
        {
            var bitmap = BitmapFactory.New((int)_width, (int)_height);
            AddToBitmap(body, bitmap, boneColor, jointColor);
            return bitmap;
        }

        /// <summary>
        /// Draws the body joints and bones to a WriteableBitmap
        /// </summary>
        /// <param name="bitmap">the bitmap to which to draw the body</param>
        /// <param name="boneColor">color to use for drawing bones</param>
        /// <param name="jointColor">color to use for drawing joints</param>
        public static void AddToBitmap(this IBody body, WriteableBitmap bitmap, Color boneColor, Color jointColor)
        {
            using (var context = bitmap.GetBitmapContext())
                if (body.IsTracked)
                    AddBodyToContext(body, context, boneColor, jointColor);
        }

        // TODO: Revisit to add a color per body
        
        /// <summary>
        /// Gets the body joints and bones drawn as a WriteableBitmap
        /// </summary>
        /// <param name="boneColor">color to use for drawing bones</param>
        /// <param name="jointColor">color to use for drawing joints</param>
        public static WriteableBitmap GetBitmap(this IEnumerable<IBody> bodies, Color boneColor, Color jointColor)
        {
            var bitmap = BitmapFactory.New((int)_width, (int)_height);
            AddToBitmap(bodies, bitmap, boneColor, jointColor);
            return bitmap;
        }

        /// <summary>
        /// Draws the body joints and bones to a WriteableBitmap
        /// </summary>
        /// <param name="bitmap">the bitmap to which to draw the bodies</param>
        /// <param name="boneColor">color to use for drawing bones</param>
        /// <param name="jointColor">color to use for drawing joints</param>
        public static void AddToBitmap(this IEnumerable<IBody> bodies, WriteableBitmap bitmap, Color boneColor, Color jointColor)
        {
            using (var context = bitmap.GetBitmapContext())
                foreach (var body in bodies)
                    if (body.IsTracked)
                        AddBodyToContext(body, context, boneColor, jointColor);
        }

#if !NOSDK
        /// <summary>
        /// Gets the body joints and bones drawn as a WriteableBitmap
        /// </summary>
        /// <param name="boneColor">color to use for drawing bones</param>
        /// <param name="jointColor">color to use for drawing joints</param>
        public static WriteableBitmap GetBitmap(this Body body, Color boneColor, Color jointColor)
        {
            return GetBitmap(new KinectBody(body), boneColor, jointColor);
        }

        /// <summary>
        /// Draws the body joints and bones to a WriteableBitmap
        /// </summary>
        /// <param name="bitmap">the bitmap to which to draw the body</param>
        /// <param name="boneColor">color to use for drawing bones</param>
        /// <param name="jointColor">color to use for drawing joints</param>
        public static void AddToBitmap(this Body body, WriteableBitmap bitmap, Color boneColor, Color jointColor)
        {
            using (var context = bitmap.GetBitmapContext())
                AddBodyToContext(new KinectBody(body), context, boneColor, jointColor);
        }

        // TODO: Revisit to add a color per body

        /// <summary>
        /// Gets the body joints and bones drawn as a WriteableBitmap
        /// </summary>
        /// <param name="boneColor">color to use for drawing bones</param>
        /// <param name="jointColor">color to use for drawing joints</param>
        public static WriteableBitmap GetBitmap(this IEnumerable<Body> bodies, Color boneColor, Color jointColor)
        {
            var bitmap = BitmapFactory.New((int)_width, (int)_height);
            AddToBitmap(bodies, bitmap, boneColor, jointColor);
            return bitmap;
        }

        /// <summary>
        /// Draws the body joints and bones to a WriteableBitmap
        /// </summary>
        /// <param name="bitmap">the bitmap to which to draw the bodies</param>
        /// <param name="boneColor">color to use for drawing bones</param>
        /// <param name="jointColor">color to use for drawing joints</param>
        public static void AddToBitmap(this IEnumerable<Body> bodies, WriteableBitmap bitmap, Color boneColor, Color jointColor)
        {
            using (var context = bitmap.GetBitmapContext())
                foreach (var body in bodies)
                    if (body.IsTracked)
                        AddBodyToContext(new KinectBody(body), context, boneColor, jointColor);
        }
#endif

        private static void AddBodyToContext(IBody body, BitmapContext context, Color boneColor, Color jointColor)
        {
            DrawBonesAndJoints(body, context, boneColor, jointColor);
            RenderClippedEdges(body, context);
        }

        private static void RenderClippedEdges(IBody body, BitmapContext context)
        {
            try
            {
                if (body.ClippedEdges.HasFlag(FrameEdges.Bottom))
                {
                    context.WriteableBitmap.FillRectangle(
                        0,
                        (int)_height - (int)_clipBoundsThickness, 
                        (int)_width,
                        (int)_height, 
                        Colors.Red);
                }

                if (body.ClippedEdges.HasFlag(FrameEdges.Top))
                {
                    context.WriteableBitmap.FillRectangle(
                        0,
                        0,
                        (int)_width,
                        (int)_clipBoundsThickness,
                        Colors.Red);
                }

                if (body.ClippedEdges.HasFlag(FrameEdges.Left))
                {
                    context.WriteableBitmap.FillRectangle(
                        0,
                        0,
                        (int)_clipBoundsThickness,
                        (int)_height,
                        Colors.Red);
                }

                if (body.ClippedEdges.HasFlag(FrameEdges.Right))
                {
                    context.WriteableBitmap.FillRectangle(
                        (int)_width - (int)_clipBoundsThickness,
                        0,
                        (int)_width,
                        (int)_height,
                        Colors.Red);
                }
            }
            catch { }
        }

        private static void DrawBonesAndJoints(IBody body, BitmapContext context, Color boneColor, Color jointColor)
        {
            foreach (var bone in BoneTypeEx.DrawnBones)
            {
                DrawBone(body, context, bone, boneColor);
            }

            Color inferredJointColor = jointColor;
            inferredJointColor.A = 128;

            // Render Joints
            foreach (var joint in body.Joints.Values)
            {
                if (joint.TrackingState == TrackingState.NotTracked)
                {
                    continue;
                }

                var point = MapCameraPointToDepthSpace(joint.Position);

                context.WriteableBitmap.FillEllipseCentered(
                    (int)point.X,
                    (int)point.Y,
                    (int)_jointThickness,
                    (int)_jointThickness,
                    joint.TrackingState == TrackingState.Inferred ? inferredJointColor : jointColor);
            }
        }

        private static void DrawBone(IBody body, BitmapContext context, BoneTypeEx bone, Color color)
        {
            var startJoint = body.Joints[bone.StartJoint];
            var endJoint = body.Joints[bone.EndJoint];

            // If we can't find either of these joints, exit
            if (startJoint.TrackingState == TrackingState.NotTracked ||
                endJoint.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // Don't draw if both points are inferred
            if (startJoint.TrackingState == TrackingState.Inferred &&
                endJoint.TrackingState == TrackingState.Inferred)
            {
                return;
            }

            // If either isn't tracked, it is "inferred"
            if (startJoint.TrackingState != TrackingState.Tracked || endJoint.TrackingState != TrackingState.Tracked)
            {
                color.A = 192;
            }

            var startPoint = MapCameraPointToDepthSpace(startJoint.Position);
            var endPoint = MapCameraPointToDepthSpace(endJoint.Position);

            context.WriteableBitmap.DrawLineAa(
                (int)startPoint.X,
                (int)startPoint.Y,
                (int)endPoint.X,
                (int)endPoint.Y,
                color);
        }

        private static DepthSpacePoint MapCameraPointToDepthSpace(CameraSpacePoint cameraPoint)
        {
            DepthSpacePoint depthPoint;

#if NOSDK
            depthPoint = new DepthSpacePoint()
                {
                    X = (cameraPoint.X * _halfWidth) + _halfWidth,
                    Y = (cameraPoint.Y * -_halfHeight) + _halfHeight
                };
#else
            if (_coordinateMapper != null)
            {
                depthPoint = _coordinateMapper.MapCameraPointToDepthSpace(cameraPoint);
            }
            else
            {
                depthPoint = new DepthSpacePoint()
                    {
                        X = (cameraPoint.X * _halfWidth) + _halfWidth,
                        Y = (cameraPoint.Y * -_halfHeight) + _halfHeight
                    };
            }
#endif

            return depthPoint;
        }
    }
}
