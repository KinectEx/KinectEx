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
    /// <summary>
    /// Contains extensions to the Kinect SDK <c>BodyFrame</c> class which enable
    /// use of KinectEx functionality using familiar KinectSDK patterns and syntax.
    /// </summary>
    public static class BodyFrameExtensions
    {
        private static Body[] _bodies = null;

        /// <summary>
        /// Similar to the Kinect SDK method of the same name, this method retrieves
        /// the array of bodies from a <c>BodyFrame</c> and updates the specified
        /// <c>SmoothedBodyList</c> collection. If the collection does not contain
        /// the correct number of bodies, this method clears and refills the collection
        /// with new bodies of type T. Note that if this behavior is undesirable,
        /// insure that the collection contains the right number of bodies before
        /// calling this method.
        /// </summary> 
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

            frame.GetAndRefreshBodyData(_bodies);

            bodies.RefreshFromBodyArray(_bodies);
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

            frame.GetAndRefreshBodyData(_bodies);

            bodies.RefreshFromBodyArray(_bodies);
        }
    }
}
