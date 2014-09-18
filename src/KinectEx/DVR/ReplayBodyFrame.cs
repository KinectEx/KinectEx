using KinectEx.Smoothing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
    /// <summary>
    /// A recordable / replayable version of a <c>BodyFrame</c>.
    /// </summary>
    public class ReplayBodyFrame : ReplayFrame
    {
        /// <summary>
        /// Total number of <c>CustomBody</c> objects in the frame.
        /// </summary>
        public int BodyCount { get; internal set; }

        /// <summary>
        /// The plane of the floor as detected by the Kinect sensor.
        /// </summary>
        public Vector4 FloorClipPlane { get; internal set; }

        /// <summary>
        /// The list of <c>CustomBody</c> objects in the frame.
        /// </summary>
        public List<CustomBody> Bodies { get; internal set; }
        
        /// <summary>
        /// <para>
        /// Similar to the Kinect SDK method of the same name, this method updates the 
        /// specified <c>SmoothedBodyList</c> collection with the values contained in the
        /// Bodies list of this <c>ReplayBodyFrame</c>. If the collection does not contain
        /// the correct number of bodies, this method clears and refills the collection
        /// with new bodies of type T. Note that if this behavior is undesirable,
        /// insure that the collection contains the right number of bodies before
        /// calling this method.
        /// </para>
        /// <para>
        /// Note that this method is really only needed if you wish to use smoothing
        /// during replay. Otherwise, directly accessing the Bodies list of this
        /// <c>ReplayBodyFrame</c> is both acceptable and more efficient.
        /// </para>
        /// </summary> 
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

        /// <summary>
        /// <para>
        /// Similar to the Kinect SDK method of the same name, this method updates the 
        /// specified list of <c>IBody</c> instances with the values contained in the
        /// Bodies list of this <c>ReplayBodyFrame</c>. If the collection does not contain
        /// the correct number of bodies, this method clears and refills the collection
        /// with new bodies of type T. Note that if this behavior is undesirable,
        /// insure that the collection contains the right number of bodies before
        /// calling this method.
        /// </para>
        /// <para>
        /// Note that this method is really only needed if you wish to use smoothing
        /// during replay. Otherwise, directly accessing the Bodies list of this
        /// <c>ReplayBodyFrame</c> is both acceptable and more efficient.
        /// </para>
        /// </summary> 
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

        // Multiple Constructor options

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

        internal ReplayBodyFrame(BodyFrame frame, Body[] bodies)
        {
            this.FrameType = FrameTypes.Body;
            this.RelativeTime = frame.RelativeTime;
            this.BodyCount = frame.BodyCount;
            this.FloorClipPlane = frame.FloorClipPlane;
            var bodyList = new List<CustomBody>(bodies.Length);
            bodyList.RefreshFromBodyArray(bodies);
            this.Bodies = bodyList;
        }
#endif

        // and a factory method

        internal static ReplayBodyFrame FromReader(BinaryReader reader, Version version)
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
                frame.Bodies.Add(CreateBodyFromReader(reader, version));
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

        private static CustomBody CreateBodyFromReader(BinaryReader reader, Version version)
        {
            CustomBody body = new CustomBody();

            try
            {
                body.IsTracked = reader.ReadBoolean();

                if (body.IsTracked)
                {
                    #region Old Version
                    if (version.Minor == 1)
                    {
                        int count;
                        count = reader.ReadInt32(); // Activities.Count
                        for (int i = 0; i < count; i++)
                        {
                            reader.ReadInt32(); // key
                            reader.ReadInt32(); // value
                        }
                        count = reader.ReadInt32(); // Appearance.Count
                        for (int i = 0; i < count; i++)
                        {
                            reader.ReadInt32(); // key
                            reader.ReadInt32(); // value
                        }
                    }
                    #endregion

                    body.ClippedEdges = (FrameEdges)reader.ReadInt32();

                    #region Old Version
                    if (version.Minor == 1)
                    {
                        reader.ReadInt32(); // Engaged
                        int count;
                        count = reader.ReadInt32(); // Expressions.Count
                        for (int i = 0; i < count; i++)
                        {
                            reader.ReadInt32(); // key
                            reader.ReadInt32(); // value
                        }
                    }
                    #endregion

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
