/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RSPoseController : MonoBehaviour
{

    public UDPSocketClient udpSocketClient;
    public M2MqttCRClient m2MqttCRClient;
    public enum INPUTMODE { NONE, UDP, MQTT, CONTROLLER }; //NONE is internal
    public INPUTMODE inputMode;
    
    public Transform subject; //output
    public Transform track; // input
    public Transform pivot; //delta = track - pivot

    public TopicMessageJSON currentMessage;
    public string currentMessage2;

    public bool isTracking;

    public Vector3 inputPosition;
    public Quaternion inputRotation;
    public Vector3 inputRotationVector3;
    public Vector3 positionRotationFix;

    public float rotationScaler;
    public float positionScaler;



    // Start is called before the first frame update
    void Start()
    {
        currentMessage = new TopicMessageJSON();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputMode == INPUTMODE.UDP)
        {
            if (udpSocketClient.runNetwork)
            {
                getMessagesFromUDP();
                processState();
            }
        }

        if (inputMode == INPUTMODE.MQTT)
        {

            getMessagesFromMQTT();
            processState();
            
        }

        if (inputMode == INPUTMODE.CONTROLLER)
        {

            getMessagesFromController();
            processState();

        }
    }
    void processState()
    {


        // this way toggles the tracking on mouse clickes
        // no need to hold the button
        if (Input.GetMouseButtonUp(1))
        {
            if (!isTracking)
            {
                isTracking = true;
                pivot.position = track.position;
                pivot.rotation = track.rotation;
            }
            else
            {
                isTracking = false;   
            }
        }


        if (isTracking)
        {
            subject.position = track.position - pivot.position;
            subject.rotation = track.rotation * Quaternion.Inverse(pivot.rotation);
        }

        //rotate position by rotation
        //inputRotationVector3 = subject.rotation.eulerAngles;
        //positionRotationFix = track.rotation.eulerAngles;
        //subject.position = geo.rotateZXY(positionRotationFix*Mathf.Deg2Rad, subject.position);


        
    }

    void getMessagesFromController()
    {
        float inputLXValue = Input.GetAxis("Vertical");
        float inputLYValue = Input.GetAxis("Horizontal");
        float inputRXValue = Input.GetAxis("RHorizontal");
        float inputRYValue = Input.GetAxis("RVertical");


        inputRotation = Quaternion.Euler(inputLXValue * rotationScaler, inputLYValue * rotationScaler, 0);

        inputPosition = new Vector3(inputRXValue * positionScaler, 0, inputRYValue * positionScaler);

        track.position = inputPosition;
        track.rotation = inputRotation;


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

            track.position = inputPosition;
            track.rotation = inputRotation;

        }

        */

    }

    void getMessagesFromMQTT()
    {

        if(m2MqttCRClient == null)
            return;

        while(m2MqttCRClient.hasMessages())
        {
            currentMessage = m2MqttCRClient.getNextMessage();
            //put this message in the local taskMessageService

           
            TopicMessageJSON inside = JsonUtility.FromJson<TopicMessageJSON>(currentMessage.messagestring);
            currentMessage2 = inside.messagestring;
           
            PRFrameJSON frame = JsonUtility.FromJson<PRFrameJSON>(inside.messagestring);
        
            
            inputRotation = frame.rotation;
            inputPosition = frame.position;

            track.position = inputPosition;
            track.rotation = inputRotation; 

        }

    }
    public void connectUDPClient()
    {

        udpSocketClient.startNetwork();


    }


}