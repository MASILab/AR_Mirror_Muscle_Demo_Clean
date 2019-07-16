/* Created by: Alex Wang, Anjie Wang
 * Date: 07/01/2019
 * MySkeletonRenderer is responsible for creating and rendering the muscles, bones, and joints.
 * It is adapted from the original SkeletonRenderer from the Astra Orbbec SDK 2.0.16.
 */
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MySkeletonRenderer : MonoBehaviour
{
    private long _lastFrameIndex = -1;

    private Astra.Body[] _bodies;
    private Dictionary<int, GameObject[]> _bodySkeletons;
    private Dictionary<int, GameObject[]> _bodyBones;

    private readonly Vector3 NormalPoseScale = new Vector3(0.01f, 0.01f, 0.01f);
    private readonly Vector3 GripPoseScale = new Vector3(0.2f, 0.2f, 0.2f);

    public GameObject JointPrefab;
    public Transform JointRoot;

    public Toggle ToggleSeg = null;
    public Toggle ToggleSegBody = null;
    public Toggle ToggleSegBodyHand = null;

    public Toggle ToggleProfileFull = null;
    public Toggle ToggleProfileUpperBody = null;
    public Toggle ToggleProfileBasic = null;

    public Toggle ToggleOptimizationAccuracy = null;
    public Toggle ToggleOptimizationBalanced = null;
    public Toggle ToggleOptimizationMemory = null;
    public Slider SliderOptimization = null;

    private Astra.BodyTrackingFeatures _previousTargetFeatures = Astra.BodyTrackingFeatures.HandPose;
    private Astra.SkeletonProfile _previousSkeletonProfile = Astra.SkeletonProfile.Full;
    private Astra.SkeletonOptimization _previousSkeletonOptimization = Astra.SkeletonOptimization.BestAccuracy;

    #region 3d body model prefabs
    //Bone Prefabs
    public GameObject Prefab_Head_Neck;
    public GameObject Prefab_MidSpine_ShoulderSpine;
    public GameObject Prefab_BaseSpine_MidSpine;
    public GameObject Prefab_LeftShoulder_LeftElbow;
    public GameObject Prefab_LeftElbow_LeftWrist;
    public GameObject Prefab_ShoudlerSpine_LeftShoulder;
    public GameObject Prefab_ShoulderSpine_RightShoulder;
    public GameObject Prefab_RightShoulder_RightElbow;
    public GameObject Prefab_RightElbow_RightWrist;
    public GameObject Prefab_ShoulderSpine_Neck;
    public GameObject Prefab_BaseSpine_LeftHip;
    public GameObject Prefab_LeftHip_LeftKnee;
    public GameObject Prefab_LeftKnee_LeftFoot;
    public GameObject Prefab_BaseSpine_RightHip;
    public GameObject Prefab_RightHip_RightKnee;
    public GameObject Prefab_RightKnee_RightFoot;
    public GameObject Prefab_Head_bone;
    public GameObject Prefab_LeftHand;
    public GameObject Prefab_RightHand;

    private readonly float BoneThickness = 1f;
    private readonly float MuscleThickness = 2f;
    private readonly float HeadThickness = 0.15f;

    #endregion


    void Start()
    {
        _bodySkeletons = new Dictionary<int, GameObject[]>();
        _bodyBones = new Dictionary<int, GameObject[]>();
        _bodies = new Astra.Body[Astra.BodyFrame.MaxBodies];
    }

    public void OnNewFrame(Astra.BodyStream bodyStream, Astra.BodyFrame frame)
    {
        if (frame.Width == 0 ||
            frame.Height == 0)
        {
            return;
        }

        if (_lastFrameIndex == frame.FrameIndex)
        {
            return;
        }

        _lastFrameIndex = frame.FrameIndex;

        frame.CopyBodyData(ref _bodies);
        UpdateSkeletonsFromBodies(_bodies);
        UpdateBodyFeatures(bodyStream, _bodies);
        UpdateSkeletonProfile(bodyStream);
        UpdateSkeletonOptimization(bodyStream);
    }


    void UpdateSkeletonsFromBodies(Astra.Body[] bodies)
    {
        foreach (var body in bodies)
        {

            if (body.Status == Astra.BodyStatus.NotTracking)
            {
                continue;
            }


            GameObject[] joints;
            GameObject[] bones;
            bool newBody = false;

            if (!_bodySkeletons.ContainsKey(body.Id) && !_bodyBones.ContainsKey(body.Id))
            {
                //Instantiate joint gameobjects
                joints = new GameObject[body.Joints.Length];
                for (int i = 0; i < joints.Length; i++)
                {
                    joints[i] = (GameObject)Instantiate(JointPrefab, Vector3.zero, Quaternion.identity);
                    joints[i].transform.SetParent(JointRoot);
                }
                _bodySkeletons.Add(body.Id, joints);

                //Instantiate bone gameobjects
                bones = new GameObject[Bones.Length];
                for (int i = 0; i < bones.Length; i++)
                {
                    bones[0] = (GameObject)Instantiate(Prefab_BaseSpine_MidSpine, Vector3.zero, Quaternion.identity);
                    bones[1] = (GameObject)Instantiate(Prefab_MidSpine_ShoulderSpine, Vector3.zero, Quaternion.identity);
                    bones[2] = (GameObject)Instantiate(Prefab_ShoulderSpine_Neck, Vector3.zero, Quaternion.identity);
                    bones[3] = (GameObject)Instantiate(Prefab_Head_bone, Vector3.zero, Quaternion.identity);
                    bones[4] = (GameObject)Instantiate(Prefab_ShoudlerSpine_LeftShoulder, Vector3.zero, Quaternion.identity);
                    bones[5] = (GameObject)Instantiate(Prefab_LeftShoulder_LeftElbow, Vector3.zero, Quaternion.identity);
                    bones[6] = (GameObject)Instantiate(Prefab_LeftElbow_LeftWrist, Vector3.zero, Quaternion.identity);
                    bones[7] = (GameObject)Instantiate(Prefab_LeftHand, Vector3.zero, Quaternion.identity);
                    bones[8] = (GameObject)Instantiate(Prefab_ShoulderSpine_RightShoulder, Vector3.zero, Quaternion.identity);
                    bones[9] = (GameObject)Instantiate(Prefab_RightShoulder_RightElbow, Vector3.zero, Quaternion.identity);
                    bones[10] = (GameObject)Instantiate(Prefab_RightElbow_RightWrist, Vector3.zero, Quaternion.identity);
                    bones[11] = (GameObject)Instantiate(Prefab_RightHand, Vector3.zero, Quaternion.identity);
                    bones[12] = (GameObject)Instantiate(Prefab_BaseSpine_LeftHip, Vector3.zero, Quaternion.identity);
                    bones[13] = (GameObject)Instantiate(Prefab_LeftHip_LeftKnee, Vector3.zero, Quaternion.identity);
                    bones[14] = (GameObject)Instantiate(Prefab_LeftKnee_LeftFoot, Vector3.zero, Quaternion.identity);
                    bones[15] = (GameObject)Instantiate(Prefab_BaseSpine_RightHip, Vector3.zero, Quaternion.identity);
                    bones[16] = (GameObject)Instantiate(Prefab_RightHip_RightKnee, Vector3.zero, Quaternion.identity);
                    bones[17] = (GameObject)Instantiate(Prefab_RightKnee_RightFoot, Vector3.zero, Quaternion.identity);

                }
                _bodyBones.Add(body.Id, bones);

                newBody = true;
            }
            else
            {
                joints = _bodySkeletons[body.Id];
                bones = _bodyBones[body.Id];
            }

            //Log if a new body is detected
            if (newBody)
            {
                StartCoroutine(GetRequest("https://docs.google.com/forms/d/e/1FAIpQLSe9t2ffOIQF2zNo-W3mGsA0jW0Fpba65AW1vk8C8YI9o1Akyg/formResponse?entry.365241968=MUSCLEDEMO&fvv=1"));
            }

            //Render the joints
            for (int i = 0; i < body.Joints.Length; i++)
            {
                var skeletonJoint = joints[i];
                var bodyJoint = body.Joints[i];

                if (bodyJoint.Status != Astra.JointStatus.NotTracked)
                {
                    if (!skeletonJoint.activeSelf)
                    {
                        skeletonJoint.SetActive(true);
                    }

                    ///*
                    skeletonJoint.transform.localPosition =
                        new Vector3(bodyJoint.WorldPosition.X / 1000f,
                                    bodyJoint.WorldPosition.Y / 1000f,
                                    bodyJoint.WorldPosition.Z / 1000f);
                    //*/

                    /*
                    skeletonJoint.transform.localPosition =
                         new Vector3(bodyJoint.DepthPosition.X / 1000f,
                                     -bodyJoint.DepthPosition.Y / 1000f);
                    //skeletonJoint.transform.localScale = NormalPoseScale;
                    */

                    //skel.Joints[i].Orient.Matrix:
                    // 0, 			1,	 		2,
                    // 3, 			4, 			5,
                    // 6, 			7, 			8
                    // -------
                    // right(X),	up(Y), 		forward(Z)

                    //Vector3 jointRight = new Vector3(
                    //    bodyJoint.Orientation.M00,
                    //    bodyJoint.Orientation.M10,
                    //    bodyJoint.Orientation.M20);

                    Vector3 jointUp = new Vector3(
                        bodyJoint.Orientation.M01,
                        bodyJoint.Orientation.M11,
                        bodyJoint.Orientation.M21);

                    Vector3 jointForward = new Vector3(
                        bodyJoint.Orientation.M02,
                        bodyJoint.Orientation.M12,
                        bodyJoint.Orientation.M22);

                    skeletonJoint.transform.rotation =
                        Quaternion.LookRotation(jointForward, jointUp);

                    skeletonJoint.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                }
                else
                {
                    if (skeletonJoint.activeSelf) skeletonJoint.SetActive(false);
                }
            }

            //Render the bones
            for (int i = 0; i < Bones.Length; i++)
            {
                //actual gameobject bones
                var skeletonBone = bones[i];
                //bones a body should have
                var bodyBone = Bones[i];
                int startIndex = FindJointIndex(body, bodyBone._startJoint);
                int endIndex = FindJointIndex(body, bodyBone._endJoint);
                var startJoint = body.Joints[startIndex];
                var endJoint = body.Joints[endIndex];

                if (startJoint.Status != Astra.JointStatus.NotTracked && endJoint.Status != Astra.JointStatus.NotTracked)
                {
                    if (!skeletonBone.activeSelf)
                    {
                        skeletonBone.SetActive(true);
                    }


                    #region Draw all bones
                    Vector3 startPosition = joints[startIndex].transform.position;
                    Vector3 endPosition = joints[endIndex].transform.position;

                    float squaredMagnitude = Mathf.Pow(endPosition.x - startPosition.x, 2) + Mathf.Pow(endPosition.y - startPosition.y, 2);
                    float magnitude = Mathf.Sqrt(squaredMagnitude);

                    skeletonBone.transform.position = (startPosition + endPosition) / 2.0f;
                    skeletonBone.transform.localEulerAngles = new Vector3(0, 0, find2DAngles(endPosition.x - startPosition.x, endPosition.y - startPosition.y));

                    //Scale the head
                    if (startJoint.Type == Astra.JointType.Neck)
                    {
                        skeletonBone.transform.localScale = new Vector3(HeadThickness, magnitude * 1.5f, HeadThickness);
                    }
                    //Scale the arms and the legs
                    else if (startJoint.Type == Astra.JointType.LeftShoulder || startJoint.Type == Astra.JointType.RightShoulder ||
                        startJoint.Type == Astra.JointType.LeftElbow || startJoint.Type == Astra.JointType.RightElbow ||
                        startJoint.Type == Astra.JointType.LeftHip || startJoint.Type == Astra.JointType.RightHip ||
                        startJoint.Type == Astra.JointType.LeftKnee || startJoint.Type == Astra.JointType.RightKnee)
                    {
                        skeletonBone.transform.localScale = new Vector3(MuscleThickness, magnitude, MuscleThickness);
                    }
                    //Scale other bones
                    else
                    {
                        skeletonBone.transform.localScale = new Vector3(BoneThickness , magnitude, BoneThickness);
                    }
                    #endregion
                }
                else
                {
                    if (skeletonBone.activeSelf) skeletonBone.SetActive(false);
                }

            }

        }
    }

    #region Helper Methods
    private static int FindJointIndex(Astra.Body body, Astra.JointType jointType)
    {
        for (int i = 0; i < body.Joints.Length; i++)
        {
            if (body.Joints[i].Type == jointType)
            {
                return i;
            }
        }
        return -1;
    }

    private float find2DAngles(float x, float y)
    {
        return -RadiansToDegrees((float)Mathf.Atan2(x, y));
    }

    private float RadiansToDegrees(float radians)
    {
        float angle = radians * 180 / (float)Mathf.PI;
        return angle;
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
            }
        }
    }
    #endregion

    private void UpdateHandPoseVisual(GameObject skeletonJoint, Astra.HandPose pose)
    {
        Vector3 targetScale = NormalPoseScale;
        if (pose == Astra.HandPose.Grip)
        {
            targetScale = GripPoseScale;
        }
        skeletonJoint.transform.localScale = targetScale;
    }

    private void UpdateBodyFeatures(Astra.BodyStream bodyStream, Astra.Body[] bodies)
    {
        if (ToggleSeg != null &&
            ToggleSegBody != null &&
            ToggleSegBodyHand != null)
        {
            Astra.BodyTrackingFeatures targetFeatures = Astra.BodyTrackingFeatures.Segmentation;
            if (ToggleSegBodyHand.isOn)
            {
                targetFeatures = Astra.BodyTrackingFeatures.HandPose;
            }
            else if (ToggleSegBody.isOn)
            {
                targetFeatures = Astra.BodyTrackingFeatures.Skeleton;
            }

            if (targetFeatures != _previousTargetFeatures)
            {
                _previousTargetFeatures = targetFeatures;
                foreach (var body in bodies)
                {
                    if (body.Status != Astra.BodyStatus.NotTracking)
                    {
                        bodyStream.SetBodyFeatures(body.Id, targetFeatures);
                    }
                }
                bodyStream.SetDefaultBodyFeatures(targetFeatures);
            }
        }
    }

    private void UpdateSkeletonProfile(Astra.BodyStream bodyStream)
    {
        if (ToggleProfileFull != null &&
            ToggleProfileUpperBody != null &&
            ToggleProfileBasic != null)
        {
            Astra.SkeletonProfile targetSkeletonProfile = Astra.SkeletonProfile.Full;
            if (ToggleProfileFull.isOn)
            {
                targetSkeletonProfile = Astra.SkeletonProfile.Full;
            }
            else if (ToggleProfileUpperBody.isOn)
            {
                targetSkeletonProfile = Astra.SkeletonProfile.UpperBody;
            }
            else if (ToggleProfileBasic.isOn)
            {
                targetSkeletonProfile = Astra.SkeletonProfile.Basic;
            }

            if (targetSkeletonProfile != _previousSkeletonProfile)
            {
                _previousSkeletonProfile = targetSkeletonProfile;
                bodyStream.SetSkeletonProfile(targetSkeletonProfile);
            }
        }
    }

    private void UpdateSkeletonOptimization(Astra.BodyStream bodyStream)
    {
        if (ToggleOptimizationAccuracy != null &&
            ToggleOptimizationBalanced != null &&
            ToggleOptimizationMemory != null &&
            SliderOptimization != null)
        {
            int targetOptimizationValue = (int)_previousSkeletonOptimization;
            if (ToggleOptimizationAccuracy.isOn)
            {
                targetOptimizationValue = (int)Astra.SkeletonOptimization.BestAccuracy;
            }
            else if (ToggleOptimizationBalanced.isOn)
            {
                targetOptimizationValue = (int)Astra.SkeletonOptimization.Balanced;
            }
            else if (ToggleOptimizationMemory.isOn)
            {
                targetOptimizationValue = (int)Astra.SkeletonOptimization.MinimizeMemory;
            }

            if (targetOptimizationValue != (int)_previousSkeletonOptimization)
            {
                Debug.Log("Set optimization slider: " + targetOptimizationValue);
                SliderOptimization.value = targetOptimizationValue;
            }

            Astra.SkeletonOptimization targetSkeletonOptimization = Astra.SkeletonOptimization.Balanced;
            int sliderValue = (int)SliderOptimization.value;

            switch (sliderValue)
            {
                case 1:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization1;
                    break;
                case 2:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization2;
                    break;
                case 3:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization3;
                    break;
                case 4:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization4;
                    break;
                case 5:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization5;
                    break;
                case 6:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization6;
                    break;
                case 7:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization7;
                    break;
                case 8:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization8;
                    break;
                case 9:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization9;
                    break;
                default:
                    targetSkeletonOptimization = Astra.SkeletonOptimization.Optimization9;
                    SliderOptimization.value = 9;
                    break;
            }

            if (targetSkeletonOptimization != _previousSkeletonOptimization)
            {
                UpdateOptimizationToggles(targetSkeletonOptimization);

                Debug.Log("SetSkeletonOptimization: " + targetSkeletonOptimization);
                _previousSkeletonOptimization = targetSkeletonOptimization;
                bodyStream.SetSkeletonOptimization(targetSkeletonOptimization);
            }
        }
    }

    private void UpdateOptimizationToggles(Astra.SkeletonOptimization optimization)
    {
        ToggleOptimizationMemory.isOn = optimization == Astra.SkeletonOptimization.MinimizeMemory;
        ToggleOptimizationBalanced.isOn = optimization == Astra.SkeletonOptimization.Balanced;
        ToggleOptimizationAccuracy.isOn = optimization == Astra.SkeletonOptimization.BestAccuracy;
    }

    #region Bone Data Structure
    /// <summary>
    /// Bone is connector of two joints
    /// </summary>
    private struct Bone
    {
        public Astra.JointType _startJoint;
        public Astra.JointType _endJoint;

        public Bone(Astra.JointType startJoint, Astra.JointType endJoint)
        {
            _startJoint = startJoint;
            _endJoint = endJoint;
        }
    };

    /// <summary>
    /// Skeleton structure = list of bones = list of joint connectors
    /// </summary>
    private Bone[] Bones = new Bone[]
    {
            // spine, neck, and head
            new Bone(Astra.JointType.BaseSpine, Astra.JointType.MidSpine),
            new Bone(Astra.JointType.MidSpine, Astra.JointType.ShoulderSpine),
            new Bone(Astra.JointType.ShoulderSpine, Astra.JointType.Neck),
            new Bone(Astra.JointType.Neck, Astra.JointType.Head),
            // left arm
            new Bone(Astra.JointType.ShoulderSpine, Astra.JointType.LeftShoulder),
            new Bone(Astra.JointType.LeftShoulder, Astra.JointType.LeftElbow),
            new Bone(Astra.JointType.LeftElbow, Astra.JointType.LeftWrist),
            new Bone(Astra.JointType.LeftWrist, Astra.JointType.LeftHand),
            // right arm
            new Bone(Astra.JointType.ShoulderSpine, Astra.JointType.RightShoulder),
            new Bone(Astra.JointType.RightShoulder, Astra.JointType.RightElbow),
            new Bone(Astra.JointType.RightElbow, Astra.JointType.RightWrist),
            new Bone(Astra.JointType.RightWrist, Astra.JointType.RightHand),
            // left leg
            new Bone(Astra.JointType.BaseSpine, Astra.JointType.LeftHip),
            new Bone(Astra.JointType.LeftHip, Astra.JointType.LeftKnee),
            new Bone(Astra.JointType.LeftKnee, Astra.JointType.LeftFoot),
            // right leg
            new Bone(Astra.JointType.BaseSpine, Astra.JointType.RightHip),
            new Bone(Astra.JointType.RightHip, Astra.JointType.RightKnee),
            new Bone(Astra.JointType.RightKnee, Astra.JointType.RightFoot),
    };
    #endregion
}
