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
    /// <summary>
    /// Contains a number of helpful extensions to Kinect Body (and KinectEx IBody)
    /// class. Included are methods to get individual bones (really start and end
    /// joints), find angles between bones (whether connected by a common joint or
    /// not), and to find the distance between joints.
    /// </summary>
    public static class BodyExtensions
    {
        private static Dictionary<JointTypeEx, JointTypeEx> _mirroredJoints = new Dictionary<JointTypeEx, JointTypeEx>();

        private static Dictionary<JointTypeEx, List<BoneTypeEx>> _bonesAt = new Dictionary<JointTypeEx, List<BoneTypeEx>>();

        /// <summary>
        /// Lists the two <c>BoneTypeEx</c> values that intersect at a given joint.
        /// </summary>
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

        /// <summary>
        /// Gets an <c>IJoint</c> object representing the joint that mirrors the 
        /// specified <c>JointTypeEx</c> (e.g., the left wrist mirrors the right wrist).
        /// </summary>
        public static IJoint GetMirroredJoint(this IBody body, JointTypeEx jointType)
        {
            return body.Joints[_mirroredJoints[jointType]];
        }

        /// <summary>
        /// Gets the <c>IJointOrientation</c> of the joint that mirrors the specified
        /// <c>JointTypeEx</c> (e.g., the left wrist mirrors the right wrist).
        /// </summary>
        public static IJointOrientation GetMirroredJointOrientation(this IBody body, JointTypeEx jointType)
        {
            return body.JointOrientations[_mirroredJoints[jointType]];
        }

        /// <summary>
        /// Get a <c>Vector3</c> (SharpDX / DirectX) object representing the vector 
        /// between two given <c>JointTypeEx</c> values..
        /// </summary>
        public static Vector3 GetVector(this IBody body, JointTypeEx jointTypeA, JointTypeEx jointTypeB)
        {
            var posA = body.Joints[jointTypeA].Position;
            var posB = body.Joints[jointTypeB].Position;
            return new Vector3(posA.X - posB.X, posA.Y - posB.Y, posA.Z - posB.Z);
        }

        /// <summary>
        /// Get a <c>Vector3</c> (SharpDX / DirectX) object representing the vector 
        /// between the joints of the specified <c>BoneTypeEx</c> value. Optionally
        /// allows for the bone to be inverted to achieve the desired orientation.
        /// </summary>
        public static Vector3 GetVector(this IBody body, BoneTypeEx boneType, bool invert = false)
        {
            if (invert)
                return body.GetVector(boneType.EndJoint, boneType.StartJoint);
            else
                return body.GetVector(boneType.StartJoint, boneType.EndJoint);
        }

        /// <summary>
        /// Gets a <c>Bone</c> structure containing the two <c>IJoint</c> values
        /// that constitute the start and end of the specified <c>BoneTypeEx</c>.
        /// </summary>
        public static Bone GetBone(this IBody body, BoneTypeEx boneType)
        {
            return new Bone(boneType, body.Joints[boneType.StartJoint], body.Joints[boneType.EndJoint]);
        }

        /// <summary>
        /// Gets a list of the two <c>Bone</c> values that share the specified
        /// <c>JointTypeEx</c> value.
        /// </summary>
        public static List<Bone> GetBonesAt(this IBody body, JointTypeEx jointType)
        {
            if (_bonesAt.ContainsKey(jointType))
                return new List<Bone>()
                {
                    body.GetBone(_bonesAt[jointType][0]),
                    body.GetBone(_bonesAt[jointType][1])
                };
            else
                return null;
        }

        /// <summary>
        /// Returns the angle (in degrees) between the two specified
        /// <c>BoneTypeEx</c> values. Optionally allows for either or
        /// both bones to be inverted to achieve the desired orientation.
        /// </summary>
        public static double GetAngleBetween(this IBody body,
                                             BoneTypeEx boneTypeA,
                                             BoneTypeEx boneTypeB,
                                             bool invertBoneA = false,
                                             bool invertBoneB = false)
        {
            return AngleBetween(body.GetVector(boneTypeA, invertBoneA), body.GetVector(boneTypeB, invertBoneB));
        }

        /// <summary>
        /// Returns the angle (in degrees) between the two bones that
        /// intersect at the specified <c>JointTypeEx</c> value.
        /// </summary>
        public static double GetAngleAt(this IBody body, JointTypeEx jointType)
        {
            if (_bonesAt.ContainsKey(jointType))
            {
                return body.GetAngleBetween(_bonesAt[jointType][0], _bonesAt[jointType][1], false, true);
            }
            else
            {
                throw new KeyNotFoundException("Joint not found");
            }
        }

        /// <summary>
        /// Returns the distance (in meters) between the two specified joints.
        /// </summary>
        public static double GetDistanceBetween(this IBody body, JointTypeEx jointTypeA, JointTypeEx jointTypeB)
        {
            return body.GetVector(jointTypeA, jointTypeB).Length();
        }

        /// <summary>
        /// Similar to the Kinect SDK method BodyFrame.GetAndRefreshBodyData, this
        /// method uses the values from the source <c>IBody</c> collection to update
        /// the values in this collection. If the current collection does not contain
        /// the correct number of bodies, this method clears and refills this collection
        /// with new bodies of type T. Note that if this behavior is undesirable,
        /// insure that the collection contains the right number of bodies before
        /// calling this method.
        /// </summary>
        public static void RefreshFromBodyList<T>(this IList<T> bodies, IList<IBody> sourceBodies) where T : IBody
        {
            if (sourceBodies.Count != bodies.Count)
            {
                bodies.Clear();
                for (var i = 0; i < sourceBodies.Count; i++)
                {
                    bodies.Add((T)Activator.CreateInstance(typeof(T)));
                }
            }

            for (var i = 0; i < sourceBodies.Count; i++)
            {
                bodies[i].Update(sourceBodies[i]);
            }
        }

#if !NOSDK
        /// <summary>
        /// Gets an <c>IJoint</c> object representing the joint that mirrors the 
        /// specified <c>JointType</c> (e.g., the left wrist mirrors the right wrist).
        /// </summary>
        public static IJoint GetMirroredJoint(this Body body, JointType jointType)
        {
            return ((KinectBody)body).GetMirroredJoint(jointType);
        }

        /// <summary>
        /// Gets the <c>IJointOrientation</c> of the joint that mirrors the specified
        /// <c>JointType</c> (e.g., the left wrist mirrors the right wrist).
        /// </summary>
        public static IJointOrientation GetMirroredJointOrientation(this Body body, JointType jointType)
        {
            return ((KinectBody)body).GetMirroredJointOrientation(jointType);
        }

        /// <summary>
        /// Get a <c>Vector3</c> (SharpDX / DirectX) object representing the vector 
        /// between two given <c>JointType</c> values..
        /// </summary>
        public static Vector3 GetVector(this Body body, JointType jointTypeA, JointType jointTypeB)
        {
            return ((KinectBody)body).GetVector(jointTypeA, jointTypeB);
        }

        /// <summary>
        /// Get a <c>Vector3</c> (SharpDX / DirectX) object representing the vector 
        /// between the joints of the specified <c>BoneTypeEx</c> value. Optionally
        /// allows for the bone to be inverted to achieve the desired orientation.
        /// </summary>
        public static Vector3 GetVector(this Body body, BoneTypeEx boneType, bool invert = false)
        {
            return ((KinectBody)body).GetVector(boneType, invert);
        }

        /// <summary>
        /// Gets a <c>Bone</c> structure containing the two <c>IJoint</c> values
        /// that constitute the start and end of the specified <c>BoneTypeEx</c>.
        /// </summary>
        public static Bone GetBone(this Body body, BoneTypeEx boneType)
        {
            return ((KinectBody)body).GetBone(boneType);
        }

        /// <summary>
        /// Gets a list of the two <c>Bone</c> values that share the specified
        /// <c>JointType</c> value.
        /// </summary>
        public static List<Bone> GetBonesAt(this Body body, JointType jointType)
        {
            return ((KinectBody)body).GetBonesAt(jointType);
        }

        /// <summary>
        /// Returns the angle (in degrees) between the two specified
        /// <c>BoneTypeEx</c> values. Optionally allows for either or
        /// both bones to be inverted to achieve the desired orientation.
        /// </summary>
        public static double GetAngleBetween(this Body body,
                                             BoneTypeEx boneTypeA,
                                             BoneTypeEx boneTypeB,
                                             bool invertBoneA = false,
                                             bool invertBoneB = false)
        {
            return ((KinectBody)body).GetAngleBetween(boneTypeA, boneTypeB, invertBoneA, invertBoneB);
        }

        /// <summary>
        /// Returns the angle (in degrees) between the two bones that
        /// intersect at the specified <c>JointType</c> value.
        /// </summary>
        public static double GetAngleAt(this Body body, JointType jointType)
        {
            return ((KinectBody)body).GetAngleAt(jointType);
        }

        /// <summary>
        /// Returns the distance (in meters) between the two specified joints.
        /// </summary>
        public static double GetDistanceBetween(this Body body, JointTypeEx jointTypeA, JointTypeEx jointTypeB)
        {
            return ((KinectBody)body).GetDistanceBetween(jointTypeA, jointTypeB);
        }

        /// <summary>
        /// Similar to the Kinect SDK method BodyFrame.GetAndRefreshBodyData, this
        /// method uses the values from the specified <c>Body</c> array to update
        /// the values in this collection. If the current collection does not contain
        /// the correct number of bodies, this method clears and refills this collection
        /// with new bodies of type T. Note that if this behavior is undesirable,
        /// insure that the collection contains the right number of bodies before
        /// calling this method.
        /// </summary>
        public static void RefreshFromBodyArray<T>(this IList<T> bodies, Body[] kinectBodies) where T : IBody
        {
            if (kinectBodies.Length != bodies.Count)
            {
                bodies.Clear();
                for (var i = 0; i < kinectBodies.Length; i++)
                {
                    bodies.Add((T)Activator.CreateInstance(typeof(T)));
                }
            }

            for (var i = 0; i < kinectBodies.Length; i++)
            {
                bodies[i].Update(kinectBodies[i]);
            }
        }
#endif

        /// <summary>
        /// Gets the angle between two <c>Vector3</c> (SharpDX / DirectX) values.
        /// Derived from System.Media.Media3D.Vector3D source code.
        /// </summary>
        public static double AngleBetween(this Vector3 vector1, Vector3 vector2)
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
    }
}
