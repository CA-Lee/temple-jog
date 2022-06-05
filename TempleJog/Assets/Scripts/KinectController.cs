using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kinect = Windows.Kinect;
using Math = System.Math;

static class Constants
{
    public const double LANE_SPACING = 0.5; // in meter
    public const double VERTICAL_CENTER_OFFSET = 0;// in meter
    public const double JUMP_THRESHOLD = 0.2;// in meter
    public const double TURN_THRESHOLD = 0.4;// in meter
}

public enum TempleAction
{
    Jump,
    SlideLeft,
    SlideRight,
    TurnLeft,
    TurnRight
}

enum Lane
{
    Left,
    Center,
    Right
}

public class KinectController : MonoBehaviour
{
    public string Bodyname = "KinectBody";

    private GameObject BodySourceManager;

    private BodySourceManager _BodyManager;

    private Lane _CurrentLane = Lane.Center;

    private Dictionary<TempleAction, bool>
        _Action =
            new Dictionary<TempleAction, bool> {
                { TempleAction.Jump, false },
                { TempleAction.SlideLeft, false },
                { TempleAction.SlideRight, false },
                { TempleAction.TurnLeft, false },
                { TempleAction.TurnRight, false }
            };

    private bool _RecordingLeftHand = false;
    private double _LeftHandStartPosition;
    private bool _RecordingRightHand = false;
    private double _RightHandStartPosition;

    public bool GetAction(TempleAction targetAction)
    {
        if (_Action[targetAction])
        {
            _Action[targetAction] = false; // indicate this action is processed
            return true;
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        BodySourceManager = GameObject.Find(Bodyname);
    }

    // Update is called once per frame
    void Update()
    {
        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        List<Kinect.Body> trackedBodies = new List<Kinect.Body>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedBodies.Add(body);
            }
        }
        if (trackedBodies.Count != 1)
        {
            return;
        }

        foreach (var body in trackedBodies)
        {
            // check slide LR
            // x=0 center, x=LINE_SPACING right, x=-LINE_SPACING left
            double currentX =
                body.Joints[Kinect.JointType.SpineBase].Position.X;

            double distanceToLeft =
                Math.Abs(currentX - (-Constants.LANE_SPACING));
            double distanceToCenter = Math.Abs(currentX);

            Lane newLane;
            if (distanceToLeft * 2 < distanceToCenter)
            {
                // is on left lane
                newLane = Lane.Left;
            }
            else
            {
                double distanceToRight =
                    Math.Abs(currentX - Constants.LANE_SPACING);
                if (distanceToRight * 2 < distanceToCenter)
                {
                    newLane = Lane.Right;
                }
                else if (
                    distanceToCenter * 2 < distanceToLeft &&
                    distanceToCenter * 2 < distanceToRight
                )
                {
                    newLane = Lane.Center;
                }
                else
                {
                    newLane = _CurrentLane;
                }
            }

            // update action if needed
            if (_CurrentLane != newLane)
            {
                if (
                    (_CurrentLane == Lane.Left && newLane == Lane.Center) ||
                    (_CurrentLane == Lane.Center && newLane == Lane.Right)
                )
                {
                    _Action[TempleAction.SlideRight] = true;
                }
                else if (
                    (_CurrentLane == Lane.Right && newLane == Lane.Center) ||
                    (_CurrentLane == Lane.Center && newLane == Lane.Left)
                )
                {
                    _Action[TempleAction.SlideLeft] = true;
                }
                _CurrentLane = newLane;
            }

            // check jump
            if (
                (body.Joints[Kinect.JointType.SpineBase].Position.Z
                - Constants.VERTICAL_CENTER_OFFSET)
                >= Constants.JUMP_THRESHOLD
                )
            {
                // update action if needed
                _Action[TempleAction.Jump] = true;
            }


            // check turn L
            // update recording
            if (body.HandLeftState == Kinect.HandState.Closed)
            {
                if (!_RecordingLeftHand)
                {
                    _RecordingLeftHand = true;
                    _LeftHandStartPosition = body.Joints[Kinect.JointType.HandLeft].Position.X;
                }
                // update action if needed
                else if ((body.Joints[Kinect.JointType.HandLeft].Position.X - _LeftHandStartPosition) <= -Constants.TURN_THRESHOLD)// lefter, smaller
                {
                    _Action[TempleAction.TurnLeft] = true;
                }
            }
            else
            {
                _RecordingLeftHand = false;
            }
            // check turn R
            // update recording
            if (body.HandRightState == Kinect.HandState.Closed)
            {
                if (!_RecordingRightHand)
                {
                    _RecordingRightHand = true;
                    _RightHandStartPosition = body.Joints[Kinect.JointType.HandRight].Position.X;
                }
                // update action if needed
                else if ((body.Joints[Kinect.JointType.HandRight].Position.X - _RightHandStartPosition) >= Constants.TURN_THRESHOLD)
                {
                    _Action[TempleAction.TurnRight] = true;
                }
            }
            else
            {
                _RecordingRightHand = false;
            }

            //Debug
            foreach (var act in _Action)
            {
                if (act.Value)
                {
                    Debug.Log(act.Key.ToString());
                    _Action[act.Key] = false;
                }
            }
        }
    }
}
