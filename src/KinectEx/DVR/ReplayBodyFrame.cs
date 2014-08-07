using KinectEx.Smoothing;
using System;
using System.Collections.Generic;
using System.IO;

#if NETFX_CORE
using Windows.Foundation;
#endif

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx.DVR
{
    public class ReplayBodyFrame : ReplayFrame
    {
        public int BodyCount { get; internal set; }

        public Vector4 FloorClipPlane { get; internal set; }

        public List<CustomBody> Bodies { get; internal set; }

        internal ReplayBodyFrame() { }

#if !NOSDK
        internal ReplayBodyFrame(BodyFrame frame)
        {
            this.FrameType = FrameTypes.Body;
            this.RelativeTime = frame.RelativeTime;
            this.BodyCount = frame.BodyCount;
            this.FloorClipPlane = frame.FloorClipPlane;
            this.Bodies = new List<CustomBody>(frame.BodyCount);
            frame.GetAndRefreshBodyData(this.Bodies);
        }

        internal ReplayBodyFrame(BodyFrame frame, List<CustomBody> bodies)
        {
            this.FrameType = FrameTypes.Body;
            this.RelativeTime = frame.RelativeTime;
            this.BodyCount = frame.BodyCount;
            this.FloorClipPlane = frame.FloorClipPlane;
            this.Bodies = bodies;
        }
#endif

        internal static ReplayBodyFrame FromReader(BinaryReader reader)
        {
            var frame = new ReplayBodyFrame();

            frame.FrameType = FrameTypes.Body;
            frame.RelativeTime = TimeSpan.FromMilliseconds(reader.ReadDouble());
            frame.FrameSize = reader.ReadInt64();

            long frameStartPos = reader.BaseStream.Position;

            frame.BodyCount = reader.ReadInt32();
            frame.FloorClipPlane = new Vector4()
            {
                W = reader.ReadSingle(),
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
            };

            frame.Bodies = new List<CustomBody>();
            for (var i = 0; i < frame.BodyCount; i++)
            {
                frame.Bodies.Add(CreateBodyFromReader(reader));
            }

            // Do Frame Integrity Check
            if (reader.ReadString() != ReplayFrame.EndOfFrameMarker)
            {
                System.Diagnostics.Debug.WriteLine("BAD FRAME...RESETTING");
                reader.BaseStream.Position = frameStartPos + frame.FrameSize;
                if (reader.ReadString() != ReplayFrame.EndOfFrameMarker)
                {
                    throw new IOException("The recording appears to be corrupt.");
                }
                return null;
            }
            
            return frame;
        }

        public void GetAndRefreshBodyData<T>(SmoothedBodyList<T> bodies) where T : ISmoother
        {
            if (bodies == null)
            {
                throw new ArgumentNullException("bodies list must not be null");
            }

            if (this.BodyCount != bodies.Count)
            {
                bodies.Fill(this.BodyCount);
            }

            for (var i = 0; i < this.BodyCount; i++)
            {
                bodies[i].Update(this.Bodies[i]);
            }
        }

        public void GetAndRefreshBodyData<T>(IList<T> bodies) where T : IBody
        {
            if (bodies == null)
            {
                throw new ArgumentNullException("bodies list must not be null");
            }

            if (this.BodyCount != bodies.Count)
            {
                bodies.Clear();
                for (var i = 0; i < this.BodyCount; i++)
                {
                    bodies.Add((T)Activator.CreateInstance(typeof(T)));
                }
            }

            for (var i = 0; i < this.BodyCount; i++)
            {
                bodies[i].Update(this.Bodies[i]);
            }
        }

        private static CustomBody CreateBodyFromReader(BinaryReader reader)
        {
            CustomBody body = new CustomBody();

            try
            {
                body.IsTracked = reader.ReadBoolean();

                if (body.IsTracked)
                {
                    int activityCount = reader.ReadInt32();
                    for (var i = 0; i < activityCount; i++)
                    {
                        var key = (Activity)reader.ReadInt32();
                        var value = (DetectionResult)reader.ReadInt32();
                        var dict = body.Activities as IDictionary<Activity, DetectionResult>;
                        if (dict != null)
                            dict[key] = value;
                    }

                    int appearanceCount = reader.ReadInt32();
                    for (var i = 0; i < appearanceCount; i++)
                    {
                        var key = (Appearance)reader.ReadInt32();
                        var value = (DetectionResult)reader.ReadInt32();
                        var dict = body.Appearance as IDictionary<Appearance, DetectionResult>;
                        if (dict != null)
                            dict[key] = value;
                    }

                    body.ClippedEdges = (FrameEdges)reader.ReadInt32();
                    body.Engaged = (DetectionResult)reader.ReadInt32();

                    int expressionCount = reader.ReadInt32();
                    for (var i = 0; i < expressionCount; i++)
                    {
                        var key = (Expression)reader.ReadInt32();
                        var value = (DetectionResult)reader.ReadInt32();
                        var dict = body.Expressions as IDictionary<Expression, DetectionResult>;
                        if (dict != null)
                            dict[key] = value;
                    }

                    body.HandLeftConfidence = (TrackingConfidence)reader.ReadInt32();
                    body.HandLeftState = (HandState)reader.ReadInt32();
                    body.HandRightConfidence = (TrackingConfidence)reader.ReadInt32();
                    body.HandRightState = (HandState)reader.ReadInt32();
                    body.IsRestricted = reader.ReadBoolean();

                    int orientationCount = reader.ReadInt32();
                    for (var i = 0; i < orientationCount; i++)
                    {
                        var key = (JointType)reader.ReadInt32();
                        var value = new JointOrientation();
                        value.JointType = (JointType)reader.ReadInt32();
                        var vector = new Vector4();
                        vector.W = reader.ReadSingle();
                        vector.X = reader.ReadSingle();
                        vector.Y = reader.ReadSingle();
                        vector.Z = reader.ReadSingle();
                        value.Orientation = vector;
                        var dict = body.JointOrientations as IDictionary<JointType, JointOrientation>;
                        if (dict != null)
                            dict[key] = value;
                    }

                    int jointCount = reader.ReadInt32();
                    for (var i = 0; i < jointCount; i++)
                    {
                        var key = (JointType)reader.ReadInt32();
                        var value = new CustomJoint(key);
                        value.JointType = (JointType)reader.ReadInt32();
                        var position = new CameraSpacePoint();
                        position.X = reader.ReadSingle();
                        position.Y = reader.ReadSingle();
                        position.Z = reader.ReadSingle();
                        value.Position = position;
                        value.TrackingState = (TrackingState)reader.ReadInt32();
                        var dict = body.Joints as IDictionary<JointType, IJoint>;
                        if (dict != null)
                            dict[key] = value;
                    }

#if NETFX_CORE
                    var point = new Point();
#else
                    var point = new PointF();
#endif
                    point.X = reader.ReadSingle();
                    point.Y = reader.ReadSingle();
                    body.Lean = point;

                    body.LeanTrackingState = (TrackingState)reader.ReadInt32();
                    body.TrackingId = reader.ReadUInt64();
                }
            }
            catch (Exception ex)
            {
                // TODO: Log This
                System.Diagnostics.Debug.WriteLine("EXCEPTION in CreateBodyFromReader");
                System.Diagnostics.Debug.WriteLine(ex);
            }

            return body;
        }
    }
}
