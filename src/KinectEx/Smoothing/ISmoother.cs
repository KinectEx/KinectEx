using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectEx.Smoothing
{
    /// <summary>
    /// Shared interface for all smoothing "strategy" objects.
    /// </summary>
    public interface ISmoother
    {
        /// <summary>
        /// Type of "smoothed" joint to create when constructing bodies 
        /// using this strategy.
        /// </summary>
        Type CustomJointType { get; }

        /// <summary>
        /// Type of "smoothed" joint orientation to create when constructing bodies 
        /// using this strategy.
        /// </summary>
        Type CustomJointOrientationType { get; }
    }
}
