using KinectEx.Smoothing;
using System;
using System.Collections.Generic;

#if NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif


namespace KinectEx
{
    public static class BodyFrameExtensions
    {
        private static Body[] _bodies = null;

        public static void GetAndRefreshBodyData(this BodyFrame frame, SmoothedBodyList<ISmoother> bodies)
        {
            if (bodies == null)
            {
                throw new ArgumentNullException("bodies list must not be null");
            }

            if (_bodies == null || frame.BodyCount != _bodies.Length)
            {
                _bodies = new Body[frame.BodyCount];
            }

            if (frame.BodyCount != bodies.Count)
            {
                bodies.Fill(frame.BodyCount);
            }

            frame.GetAndRefreshBodyData(_bodies);
            for (var i = 0; i < frame.BodyCount; i++)
            {
                bodies[i].Update(_bodies[i]);
            }
        }

        public static void GetAndRefreshBodyData<T>(this BodyFrame frame, IList<T> bodies) where T : IBody
        {
            if (bodies == null)
            {
                throw new ArgumentNullException("bodies list must not be null");
            }

            if (_bodies == null || frame.BodyCount != _bodies.Length)
            {
                _bodies = new Body[frame.BodyCount];
            }

            if (frame.BodyCount != bodies.Count)
            {
                bodies.Clear();
                for (var i = 0; i < frame.BodyCount; i++)
                {
                    bodies.Add((T)Activator.CreateInstance(typeof(T)));
                }
            }

            frame.GetAndRefreshBodyData(_bodies);
            for (var i = 0; i < frame.BodyCount; i++)
            {
                bodies[i].Update(_bodies[i]);
            }
        }
    }
}
