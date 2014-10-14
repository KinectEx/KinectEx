using System;
using System.Collections.Generic;
using System.Linq;

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx
{
    /// <summary>
    /// A class that contains values for each "joint" in a Kinect
    /// <c>Body</c>. The class can be used statically in much the
    /// same way as an enum. However, each value contains useful
    /// information about the joint. The class also allows for easy
    /// enumeration of joints. Can be used interchangeable with
    /// the <c>JointType</c> enum.
    /// </summary>
    [Newtonsoft.Json.JsonConverter(typeof(Json.JointTypeExConverter))]
    public class JointTypeEx
    {
        /// <summary>
        /// A short name for the joint (matches the static/enumerated name).
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        /// A friendlier display name for the joint.
        /// </summary>
        public String DisplayName { get; private set; }

        /// <summary>
        /// The actual <c>JointType</c> value for this joint.
        /// </summary>
        public JointType JointType { get; private set; }

        /// <summary>
        /// A list containing all of the joints in the body.
        /// </summary>
        public static List<JointTypeEx> AllJoints { get; private set; }

        /// <summary>
        /// A dictionary that allows retrieval of a <c>JointTypeEx</c> by name.
        /// </summary>
        public static Dictionary<string, JointTypeEx> ByName { get; private set; }

        /// <summary>
        /// A dictionary that maps a Kinect SDK <c>JointType</c> to a <c>JointTypeEx</c>.
        /// </summary>
        public static Dictionary<JointType, JointTypeEx> ByJointType { get; private set; }

        /// <summary>
        /// Gets the spine base.
        /// </summary>
        public static JointTypeEx SpineBase { get; private set; }

        /// <summary>
        /// Gets the spine mid.
        /// </summary>
        public static JointTypeEx SpineMid { get; private set; }

        /// <summary>
        /// Gets the neck.
        /// </summary>
        public static JointTypeEx Neck { get; private set; }

        /// <summary>
        /// Gets the head.
        /// </summary>
        public static JointTypeEx Head { get; private set; }

        /// <summary>
        /// Gets the shoulder left.
        /// </summary>
        public static JointTypeEx ShoulderLeft { get; private set; }

        /// <summary>
        /// Gets the elbow left.
        /// </summary>
        public static JointTypeEx ElbowLeft { get; private set; }

        /// <summary>
        /// Gets the wrist left.
        /// </summary>
        public static JointTypeEx WristLeft { get; private set; }

        /// <summary>
        /// Gets the hand left.
        /// </summary>
        public static JointTypeEx HandLeft { get; private set; }

        /// <summary>
        /// Gets the shoulder right.
        /// </summary>
        public static JointTypeEx ShoulderRight { get; private set; }

        /// <summary>
        /// Gets the elbow right.
        /// </summary>
        public static JointTypeEx ElbowRight { get; private set; }

        /// <summary>
        /// Gets the wrist right.
        /// </summary>
        public static JointTypeEx WristRight { get; private set; }

        /// <summary>
        /// Gets the hand right.
        /// </summary>
        public static JointTypeEx HandRight { get; private set; }

        /// <summary>
        /// Gets the hip left.
        /// </summary>
        public static JointTypeEx HipLeft { get; private set; }

        /// <summary>
        /// Gets the knee left.
        /// </summary>
        public static JointTypeEx KneeLeft { get; private set; }

        /// <summary>
        /// Gets the ankle left.
        /// </summary>
        public static JointTypeEx AnkleLeft { get; private set; }

        /// <summary>
        /// Gets the foot left.
        /// </summary>
        public static JointTypeEx FootLeft { get; private set; }

        /// <summary>
        /// Gets the hip right.
        /// </summary>
        public static JointTypeEx HipRight { get; private set; }

        /// <summary>
        /// Gets the knee right.
        /// </summary>
        public static JointTypeEx KneeRight { get; private set; }

        /// <summary>
        /// Gets the ankle right.
        /// </summary>
        public static JointTypeEx AnkleRight { get; private set; }

        /// <summary>
        /// Gets the foot right.
        /// </summary>
        public static JointTypeEx FootRight { get; private set; }

        /// <summary>
        /// Gets the spine shoulder.
        /// </summary>
        public static JointTypeEx SpineShoulder { get; private set; }

        /// <summary>
        /// Gets the hand tip left.
        /// </summary>
        public static JointTypeEx HandTipLeft { get; private set; }

        /// <summary>
        /// Gets the thumb left.
        /// </summary>
        public static JointTypeEx ThumbLeft { get; private set; }

        /// <summary>
        /// Gets the hand tip right.
        /// </summary>
        public static JointTypeEx HandTipRight { get; private set; }

        /// <summary>
        /// Gets the thumb right.
        /// </summary>
        public static JointTypeEx ThumbRight { get; private set; }

        /// <summary>
        /// Initializes the <see cref="JointTypeEx"/> class.
        /// </summary>
        static JointTypeEx()
        {
            SpineBase = new JointTypeEx() { Name = "SpineBase", DisplayName = "Base of Spine", JointType = JointType.SpineBase };
            SpineMid = new JointTypeEx() { Name = "SpineMid", DisplayName = "Middle of Spine", JointType = JointType.SpineMid };
            Neck = new JointTypeEx() { Name = "Neck", DisplayName = "Neck", JointType = JointType.Neck };
            Head = new JointTypeEx() { Name = "Head", DisplayName = "Head", JointType = JointType.Head };

            ShoulderLeft = new JointTypeEx() { Name = "ShoulderLeft", DisplayName = "Left Shoulder", JointType = JointType.ShoulderLeft };
            ElbowLeft = new JointTypeEx() { Name = "ElbowLeft", DisplayName = "Left Elbow", JointType = JointType.ElbowLeft };
            WristLeft = new JointTypeEx() { Name = "WristLeft", DisplayName = "Left Wrist", JointType = JointType.WristLeft };
            HandLeft = new JointTypeEx() { Name = "HandLeft", DisplayName = "Left Hand", JointType = JointType.HandLeft };

            ShoulderRight = new JointTypeEx() { Name = "ShoulderRight", DisplayName = "Right Shoulder", JointType = JointType.ShoulderRight };
            ElbowRight = new JointTypeEx() { Name = "ElbowRight", DisplayName = "Right Elbow", JointType = JointType.ElbowRight };
            WristRight = new JointTypeEx() { Name = "WristRight", DisplayName = "Right Wrist", JointType = JointType.WristRight };
            HandRight = new JointTypeEx() { Name = "HandRight", DisplayName = "Right Hand", JointType = JointType.HandRight };

            HipLeft = new JointTypeEx() { Name = "HipLeft", DisplayName = "Left Hip", JointType = JointType.HipLeft };
            KneeLeft = new JointTypeEx() { Name = "KneeLeft", DisplayName = "Left Knee", JointType = JointType.KneeLeft };
            AnkleLeft = new JointTypeEx() { Name = "AnkleLeft", DisplayName = "Left Ankle", JointType = JointType.AnkleLeft };
            FootLeft = new JointTypeEx() { Name = "FootLeft", DisplayName = "Left Foot", JointType = JointType.FootLeft };

            HipRight = new JointTypeEx() { Name = "HipRight", DisplayName = "Right Hip", JointType = JointType.HipRight };
            KneeRight = new JointTypeEx() { Name = "KneeRight", DisplayName = "Right Knee", JointType = JointType.KneeRight };
            AnkleRight = new JointTypeEx() { Name = "AnkleRight", DisplayName = "Right Ankle", JointType = JointType.AnkleRight };
            FootRight = new JointTypeEx() { Name = "FootRight", DisplayName = "Right Foot", JointType = JointType.FootRight };

            SpineShoulder = new JointTypeEx() { Name = "SpineShoulder", DisplayName = "Spine at Shoulders", JointType = JointType.SpineShoulder };

            HandTipLeft = new JointTypeEx() { Name = "HandTipLeft", DisplayName = "Left Hand Tip", JointType = JointType.HandTipLeft };
            ThumbLeft = new JointTypeEx() { Name = "ThumbLeft", DisplayName = "Left Thumb", JointType = JointType.ThumbLeft };
            HandTipRight = new JointTypeEx() { Name = "HandTipRight", DisplayName = "Right Hand Tip", JointType = JointType.HandTipRight };
            ThumbRight = new JointTypeEx() { Name = "ThumbRight", DisplayName = "Right Thumb", JointType = JointType.ThumbRight };

            AllJoints = new List<JointTypeEx>()
            {
                SpineBase,
                SpineMid,
                Head,
                Neck,
                ShoulderLeft,
                ElbowLeft,
                WristLeft,
                HandLeft,
                ShoulderRight,
                ElbowRight,
                WristRight,
                HandRight,
                HipLeft,
                KneeLeft,
                AnkleLeft,
                FootLeft,
                HipRight,
                KneeRight,
                AnkleRight,
                FootRight,
                SpineShoulder,
                HandTipLeft,
                ThumbLeft,
                HandTipRight,
                ThumbRight
            };

            ByName = new Dictionary<string, JointTypeEx>();
            foreach (var joint in AllJoints)
            {
                ByName.Add(joint.Name, joint);
            }

            ByJointType = new Dictionary<JointType, JointTypeEx>();
            foreach (var joint in AllJoints)
            {
                ByJointType.Add(joint.JointType, joint);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var joint = obj as JointTypeEx;
            return joint != null && joint.JointType == this.JointType;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="JointTypeEx"/>.
        /// </summary>
        /// <param name="jointName">Name of the joint.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator JointTypeEx(string jointName)
        {
            return ByName[jointName];
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="JointTypeEx"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="joint">The joint.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator int(JointTypeEx joint)
        {
            return (int)joint.JointType;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="JointTypeEx"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator JointTypeEx(int value)
        {
            return ByJointType[(JointType)value];
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="JointTypeEx"/> to <see cref="JointType"/>.
        /// </summary>
        /// <param name="joint">The joint.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator JointType(JointTypeEx joint)
        {
            return (JointType)joint.JointType;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="JointType"/> to <see cref="JointTypeEx"/>.
        /// </summary>
        /// <param name="joint">The joint.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator JointTypeEx(JointType joint)
        {
            return ByJointType[joint];
        }
    }
}
