using SharpDX;
using System;
using System.Collections.Generic;

#if NOSDK
using KinectEx.KinectSDK;
#elif NETFX_CORE
using WindowsPreview.Kinect;
#else
using Microsoft.Kinect;
#endif

namespace KinectEx
{
    public static class BodyExtensions
    {
        private static Dictionary<JointTypeEx, JointTypeEx> _mirroredJoints = new Dictionary<JointTypeEx, JointTypeEx>();

        private static Dictionary<JointTypeEx, List<BoneTypeEx>> _bonesAt = new Dictionary<JointTypeEx, List<BoneTypeEx>>();

        public static Dictionary<JointTypeEx, List<BoneTypeEx>> BonesAt
        {
            get { return _bonesAt; }
        }

        static BodyExtensions()
        {
            _mirroredJoints.Add(JointTypeEx.AnkleLeft, JointTypeEx.AnkleRight);
            _mirroredJoints.Add(JointTypeEx.AnkleRight, JointTypeEx.AnkleLeft);
            _mirroredJoints.Add(JointTypeEx.ElbowLeft, JointTypeEx.ElbowRight);
            _mirroredJoints.Add(JointTypeEx.ElbowRight, JointTypeEx.ElbowLeft);
            _mirroredJoints.Add(JointTypeEx.FootLeft, JointTypeEx.FootRight);
            _mirroredJoints.Add(JointTypeEx.FootRight, JointTypeEx.FootLeft);
            _mirroredJoints.Add(JointTypeEx.HandLeft, JointTypeEx.HandRight);
            _mirroredJoints.Add(JointTypeEx.HandRight, JointTypeEx.HandLeft);
            _mirroredJoints.Add(JointTypeEx.Head, JointTypeEx.Head);
            _mirroredJoints.Add(JointTypeEx.SpineBase, JointTypeEx.SpineBase);
            _mirroredJoints.Add(JointTypeEx.HipLeft, JointTypeEx.HipRight);
            _mirroredJoints.Add(JointTypeEx.HipRight, JointTypeEx.HipLeft);
            _mirroredJoints.Add(JointTypeEx.KneeLeft, JointTypeEx.KneeRight);
            _mirroredJoints.Add(JointTypeEx.KneeRight, JointTypeEx.KneeLeft);
            _mirroredJoints.Add(JointTypeEx.SpineShoulder, JointTypeEx.SpineShoulder);
            _mirroredJoints.Add(JointTypeEx.ShoulderLeft, JointTypeEx.ShoulderRight);
            _mirroredJoints.Add(JointTypeEx.ShoulderRight, JointTypeEx.ShoulderLeft);
            _mirroredJoints.Add(JointTypeEx.SpineMid, JointTypeEx.SpineMid);
            _mirroredJoints.Add(JointTypeEx.ThumbLeft, JointTypeEx.ThumbRight);
            _mirroredJoints.Add(JointTypeEx.ThumbRight, JointTypeEx.ThumbLeft);
            _mirroredJoints.Add(JointTypeEx.WristLeft, JointTypeEx.WristRight);
            _mirroredJoints.Add(JointTypeEx.WristRight, JointTypeEx.WristLeft);

            _bonesAt.Add(JointTypeEx.AnkleLeft, new List<BoneTypeEx>() { BoneTypeEx.FootLeft, BoneTypeEx.LegLowerLeft });
            _bonesAt.Add(JointTypeEx.KneeLeft, new List<BoneTypeEx>() { BoneTypeEx.LegLowerLeft, BoneTypeEx.LegUpperLeft });
            _bonesAt.Add(JointTypeEx.HipLeft, new List<BoneTypeEx>() { BoneTypeEx.HipLeft, BoneTypeEx.LegUpperLeft });
            _bonesAt.Add(JointTypeEx.AnkleRight, new List<BoneTypeEx>() { BoneTypeEx.FootRight, BoneTypeEx.LegLowerRight });
            _bonesAt.Add(JointTypeEx.KneeRight, new List<BoneTypeEx>() { BoneTypeEx.LegLowerRight, BoneTypeEx.LegUpperRight });
            _bonesAt.Add(JointTypeEx.HipRight, new List<BoneTypeEx>() { BoneTypeEx.HipRight, BoneTypeEx.LegUpperRight });
            _bonesAt.Add(JointTypeEx.SpineBase, new List<BoneTypeEx>() { BoneTypeEx.HipFull, BoneTypeEx.SpineLower });
            _bonesAt.Add(JointTypeEx.SpineMid, new List<BoneTypeEx>() { BoneTypeEx.SpineLower, BoneTypeEx.SpineUpper });
            _bonesAt.Add(JointTypeEx.SpineShoulder, new List<BoneTypeEx>() { BoneTypeEx.ShoulderFull, BoneTypeEx.Head });
            _bonesAt.Add(JointTypeEx.ShoulderLeft, new List<BoneTypeEx>() { BoneTypeEx.ShoulderLeft, BoneTypeEx.ArmUpperLeft });
            _bonesAt.Add(JointTypeEx.ElbowLeft, new List<BoneTypeEx>() { BoneTypeEx.ArmUpperLeft, BoneTypeEx.ArmLowerLeft });
            _bonesAt.Add(JointTypeEx.WristLeft, new List<BoneTypeEx>() { BoneTypeEx.ArmLowerLeft, BoneTypeEx.HandLeft });
            _bonesAt.Add(JointTypeEx.ShoulderRight, new List<BoneTypeEx>() { BoneTypeEx.ShoulderRight, BoneTypeEx.ArmUpperRight });
            _bonesAt.Add(JointTypeEx.ElbowRight, new List<BoneTypeEx>() { BoneTypeEx.ArmUpperRight, BoneTypeEx.ArmLowerRight });
            _bonesAt.Add(JointTypeEx.WristRight, new List<BoneTypeEx>() { BoneTypeEx.ArmLowerRight, BoneTypeEx.HandRight });
        }

        public static IJoint GetMirroredJoint(this IBody body, JointTypeEx joint)
        {
            return body.Joints[_mirroredJoints[joint]];
        }

        public static JointOrientation GetMirroredJointOrientation(this IBody body, JointTypeEx BodyJoints)
        {
            return body.JointOrientations[_mirroredJoints[BodyJoints]];
        }

        public static Vector3 GetVector(this IBody body, JointTypeEx jointA, JointTypeEx jointB)
        {
            var posA = body.Joints[jointA].Position;
            var posB = body.Joints[jointB].Position;
            return new Vector3(posA.X - posB.X, posA.Y - posB.Y, posA.Z - posB.Z);
        }

        public static Vector3 GetVector(this IBody body, BoneTypeEx bone, bool invert = false)
        {
            if (invert)
                return body.GetVector(bone.EndJoint, bone.StartJoint);
            else
                return body.GetVector(bone.StartJoint, bone.EndJoint);
        }

        public static List<BoneTypeEx> GetBonesAt(this IBody body, JointTypeEx joint)
        {
            if (_bonesAt.ContainsKey(joint))
                return _bonesAt[joint];
            else
                return null;
        }

        public static double GetAngleBetween(this IBody body, BoneTypeEx boneA, BoneTypeEx boneB, bool invertBoneA = false, bool invertBoneB = false)
        {
            return AngleBetween(body.GetVector(boneA, invertBoneA), body.GetVector(boneB, invertBoneB));
        }

        internal static double AngleBetween(Vector3 vector1, Vector3 vector2)
        {
            vector1.Normalize();
            vector2.Normalize();

            double ratio = Vector3.Dot(vector1, vector2);

            // The "straight forward" method of acos(u.v) has large precision
            // issues when the dot product is near +/-1.  This is due to the
            // steep slope of the acos function as we approach +/- 1.  Slight
            // precision errors in the dot product calculation cause large
            // variation in the output value.
            //
            //        |                   |
            //         \__                |
            //            ---___          |
            //                  ---___    |
            //                        ---_|_
            //                            | ---___
            //                            |       ---___
            //                            |             ---__
            //                            |                  \
            //                            |                   |
            //       -|-------------------+-------------------|-
            //       -1                   0                   1
            //
            //                         acos(x)
            //
            // To avoid this we use an alternative method which finds the
            // angle bisector by (u-v)/2:
            //
            //                            _>
            //                       u  _-  \ (u-v)/2
            //                        _-  __-v
            //                      _=__--
            //                    .=----------->
            //                            v
            //
            // Because u and v and unit vectors, (u-v)/2 forms a right angle
            // with the angle bisector.  The hypotenuse is 1, therefore
            // 2*asin(|u-v|/2) gives us the angle between u and v.
            //
            // The largest possible value of |u-v| occurs with perpendicular
            // vectors and is sqrt(2)/2 which is well away from extreme slope
            // at +/-1.
            //
            // (See Windows OS Bug #1706299 for details)

            double theta;

            if (ratio < 0)
            {
                theta = Math.PI - 2.0 * Math.Asin((-vector1 - vector2).Length() / 2.0);
            }
            else
            {
                theta = 2.0 * Math.Asin((vector1 - vector2).Length() / 2.0);
            }

            return theta * (180.0 / Math.PI);
        }

        public static double GetAngleAt(this IBody body, JointTypeEx joint)
        {
            if (_bonesAt.ContainsKey(joint))
            {
                return body.GetAngleBetween(_bonesAt[joint][0], _bonesAt[joint][1], false, true);
            }
            else
            {
                throw new KeyNotFoundException("Joint not found");
            }
        }

        public static double GetDistanceBetween(this IBody body, JointTypeEx jointA, JointTypeEx jointB)
        {
            return body.GetVector(jointA, jointB).Length();
        }

#if !NOSDK
        public static double GetAngleAt(this Body body, JointTypeEx joint)
        {
            return ((KinectBody)body).GetAngleAt(joint);
        }

        public static double GetAngleBetween(this Body body, BoneTypeEx boneA, BoneTypeEx boneB, bool invertBoneA = false, bool invertBoneB = false)
        {
            return ((KinectBody)body).GetAngleBetween(boneA, boneB, invertBoneA, invertBoneB);
        }

        public static double GetDistanceBetween(this Body body, JointTypeEx jointA, JointTypeEx jointB)
        {
            return ((KinectBody)body).GetDistanceBetween(jointA, jointB);
        }
#endif
    }
}
