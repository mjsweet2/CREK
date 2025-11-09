/* Copyright (C) 2024 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SixDofPoseController : MonoBehaviour
{

    public UDPSocketClient udpSocketClient;
    public M2MqttCRClient m2MqttCRClient;
    public enum INPUTMODE { NONE, UDP, MQTT, MOUSE, CONTROLLER }; //NONE is internal
    public INPUTMODE inputMode;

    public enum TRACKINGSTATE { NONE, WAITING, TRACKING, COMPLETE };
    public TRACKINGSTATE trackingState;

    public Vector3 subjectPos; //output
    public Vector3 trackPos; //input
    public Vector3 pivotPos; //delta = track - pivot

    public Quaternion subjectRot; //output
    public Quaternion trackRot; //input
    public Quaternion pivotRot; //delta = track - pivot

    public Transform subjectMarker; //output
    public Transform trackMarker; //input
    public Transform pivotMarker; //delta = track - pivot

    public TopicMessageJSON currentMessage;
    public string currentMessage2;


    public Vector3 inputPosition;
    public Quaternion inputRotation;
    public Vector3 inputRotationVector3;
    public Vector3 positionRotationFix;

    public float rotationScaler;
    public float positionScaler;



    // Start is called before the first frame update
    void Start()
    {
        trackingState = TRACKINGSTATE.WAITING;
        currentMessage = new TopicMessageJSON();
    }

    // Update is called once per frame
    void Update()
    {
        processMouse();


        if (inputMode == INPUTMODE.UDP)
        {
            if (udpSocketClient.runNetwork)
            {
                getMessagesFromUDP();
                doTrack();
            }
        }

        if (inputMode == INPUTMODE.MQTT)
        {

            getMessagesFromMQTT();
            doTrack();

        }

        if (inputMode == INPUTMODE.CONTROLLER)
        {

            getMessagesFromController();
            doTrack();

        }
        if( inputMode == INPUTMODE.MOUSE)
        {
            getMessagesFromMouse();
            doTrack();
        }
    }
    public void resetTracking() { trackingState = TRACKINGSTATE.WAITING; }
    public bool isTrackingComplete() {  return (trackingState == TRACKINGSTATE.COMPLETE); }
    public bool isTracking() { return (trackingState == TRACKINGSTATE.TRACKING); }
    void processMouse()
    {
        if (Input.GetMouseButtonUp(1))
        {
            toggleTracking();
        }
    }
    void toggleTracking()
    {
        if (trackingState == TRACKINGSTATE.WAITING)
        {
            trackingState = TRACKINGSTATE.TRACKING;
            pivotPos = trackPos;
            pivotRot = trackRot;
        }
        else if (trackingState == TRACKINGSTATE.TRACKING)
        {
            trackingState = TRACKINGSTATE.COMPLETE;
            pivotPos = subjectPos = Vector3.zero;
            pivotRot = subjectRot = Quaternion.identity;
            
        }

    }
    void doTrack()
    {

        //this value needs to be reset by consumer
        if (trackingState == TRACKINGSTATE.COMPLETE)
            return;

        if (trackingState == TRACKINGSTATE.TRACKING)
        {
            subjectPos = trackPos - pivotPos;
            subjectRot = trackRot * Quaternion.Inverse(pivotRot);
        }

    }
    void getMessagesFromMouse()
    {

        inputPosition = Input.mousePosition * positionScaler;
        trackPos = inputPosition;

    }
    void getMessagesFromController()
    {
        float inputLX = Input.GetAxis("Horizontal");
        float inputLY = Input.GetAxis("Vertical");
        float inputRX = Input.GetAxis("RHorizontal");
        float inputRY = Input.GetAxis("RVertical");


        //inputRotation = Quaternion.Euler(inputLX * rotationScaler, inputLY * rotationScaler, 0);

        inputPosition = new Vector3(inputLX * positionScaler, inputLY * positionScaler, inputRY * positionScaler);

        trackPos = inputPosition;
        trackRot = inputRotation;


    }

    void getMessagesFromUDP()
    {
        /*
        if (udpSocketClient == null)
            return;

        while (udpSocketClient.hasMessages())
        {
            currentMessage = udpSocketClient.getNextMessage();

            TopicMessageJSON tm = JsonUtility.FromJson<TopicMessageJSON>(currentMessage);
            currentMessage2 = tm.messagestring;

            PRFrameJSON frame = JsonUtility.FromJson<PRFrameJSON>(tm.messagestring);


            inputRotation = frame.rotation;
            inputPosition = frame.position;

            trackPos = inputPosition;
            trackRot = inputRotation;

        }
        */

    }

    void getMessagesFromMQTT()
    {

        if (m2MqttCRClient == null)
            return;

        while (m2MqttCRClient.hasMessages())
        {
            currentMessage = m2MqttCRClient.getNextMessage();
            TopicMessageJSON inside = JsonUtility.FromJson<TopicMessageJSON>(currentMessage.messagestring);
            
            if (inside.topic == "frame")
            {
                PRFrameJSON frame = JsonUtility.FromJson<PRFrameJSON>(inside.messagestring);

                inputPosition = frame.position;
                inputRotation = frame.rotation;

                trackPos = inputPosition;
                trackRot = inputRotation;

            }
            else if (inside.topic == "command")
            {
                if (inside.messagestring == "togglepose")
                {
                    toggleTracking();
                }
            }

        }

    }
    public void connectUDPClient()
    {

        udpSocketClient.startNetwork();

    }


}
