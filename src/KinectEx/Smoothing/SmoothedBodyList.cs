using System;
using System.Collections.Generic;

namespace KinectEx.Smoothing
{
    public class SmoothedBodyList<T> : List<SmoothedBody<T>> where T : ISmoother
    {
        object _parameters;

        public SmoothedBodyList(object parameters = null)
        {
            _parameters = parameters;
        }

        public void Fill(int count)
        {
            this.Clear();
            for (var i = 0; i < count; i++)
            {
                this.Add((SmoothedBody<T>)Activator.CreateInstance(typeof(SmoothedBody<T>), _parameters));
            }
        }
    }
}
