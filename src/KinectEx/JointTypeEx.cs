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
    [Newtonsoft.Json.JsonConverter(typeof(Json.JointTypeExConverter))]
    public class JointTypeEx
    {
        public String Name { get; private set; }
        public String DisplayName { get; private set; }
        public JointType JointType { get; private set; }

        public static List<JointTypeEx> AllJoints { get; private set; }

        public static Dictionary<string, JointTypeEx> ByName { get; private set; }

        public static Dictionary<JointType, JointTypeEx> ByJointType { get; private set; }

        public static JointTypeEx SpineBase { get; private set; }
        public static JointTypeEx SpineMid { get; private set; }
        public static JointTypeEx Neck { get; private set; }
        public static JointTypeEx Head { get; private set; }
        public static JointTypeEx ShoulderLeft { get; private set; }
        public static JointTypeEx ElbowLeft { get; private set; }
        public static JointTypeEx WristLeft { get; private set; }
        public static JointTypeEx HandLeft { get; private set; }
        public static JointTypeEx ShoulderRight { get; private set; }
        public static JointTypeEx ElbowRight { get; private set; }
        public static JointTypeEx WristRight { get; private set; }
        public static JointTypeEx HandRight { get; private set; }
        public static JointTypeEx HipLeft { get; private set; }
        public static JointTypeEx KneeLeft { get; private set; }
        public static JointTypeEx AnkleLeft { get; private set; }
        public static JointTypeEx FootLeft { get; private set; }
        public static JointTypeEx HipRight { get; private set; }
        public static JointTypeEx KneeRight { get; private set; }
        public static JointTypeEx AnkleRight { get; private set; }
        public static JointTypeEx FootRight { get; private set; }
        public static JointTypeEx SpineShoulder { get; private set; }
        public static JointTypeEx HandTipLeft { get; private set; }
        public static JointTypeEx ThumbLeft { get; private set; }
        public static JointTypeEx HandTipRight { get; private set; }
        public static JointTypeEx ThumbRight { get; private set; }

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

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            var joint = obj as JointTypeEx;
            return joint != null && joint.JointType == this.JointType;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public static implicit operator JointTypeEx(string jointName)
        {
            return ByName[jointName];
        }

        public static implicit operator int(JointTypeEx joint)
        {
            return (int)joint.JointType;
        }

        public static implicit operator JointTypeEx(int value)
        {
            return ByJointType[(JointType)value];
        }

        public static implicit operator JointType(JointTypeEx joint)
        {
            return (JointType)joint.JointType;
        }

        public static implicit operator JointTypeEx(JointType joint)
        {
            return ByJointType[joint];
        }
    }
}
