using System;
using System.Collections.Generic;

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx.Smoothing
{
    /// <summary>
    /// A specialized list of <c>SmoothedBody</c> objects.
    /// </summary>
    public class SmoothedBodyList<T> : List<SmoothedBody<T>> where T : ISmoother
    {
        object _parameters;

        /// <summary>
        /// Create an instance of a <c>SmoothedBodyList</c> collection. Accepts an
        /// optional parameters object of the type appropriate for the specified
        /// <c>ISmoother</c>.
        /// </summary>
        public SmoothedBodyList(ISmootherParameters parameters = null)
        {
            _parameters = parameters;
        }

        /// <summary>
        /// Fill this collection with the specified number of new
        /// <c>SmoothedBody&gt;T&lt;</c> instances.
        /// </summary>
        public void Fill(int count)
        {
            this.Clear();
            for (var i = 0; i < count; i++)
            {
                this.Add((SmoothedBody<T>)Activator.CreateInstance(typeof(SmoothedBody<T>), _parameters));
            }
        }

        /// <summary>
        /// Similar to the Kinect SDK method BodyFrame.GetAndRefreshBodyData, this
        /// method uses the values from the source <c>IBody</c> collection to update
        /// the values in this collection. If the current collection does not contain
        /// the correct number of bodies, this method clears and refills this collection
        /// with new <c>SmoothedBody&gt;T&lt;</c> instances. Note that if this behavior 
        /// is undesirable, insure that the collection contains the right number of 
        /// bodies before calling this method.
        /// </summary>
        public void RefreshFromBodyList<T>(List<T> bodies) where T : IBody
        {
            if (bodies.Count != this.Count)
            {
                this.Fill(bodies.Count);
            }

            for (var i = 0; i < this.Count; i++)
            {
                this[i].Update(bodies[i]);
            }
        }

#if !NOSDK
        /// <summary>
        /// Similar to the Kinect SDK method BodyFrame.GetAndRefreshBodyData, this
        /// method uses the values from the specified <c>Body</c> array to update
        /// the values in this collection. If the current collection does not contain
        /// the correct number of bodies, this method clears and refills this collection
        /// with new <c>SmoothedBody&gt;T&lt;</c> instances. Note that if this behavior 
        /// is undesirable, insure that the collection contains the right number of 
        /// bodies before calling this method.
        /// </summary>
        public void RefreshFromBodyArray(Body[] bodies)
        {
            if (bodies.Length != this.Count)
            {
                this.Fill(bodies.Length);
            }

            for (var i = 0; i < this.Count; i++)
            {
                this[i].Update(bodies[i]);
            }
        }
#endif

    }
}
