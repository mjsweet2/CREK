/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSPoseController : MonoBehaviour
{

    public UDPSocketClient udpSocketClient;
    public M2MqttCRClient m2MqttCRClient;
    public enum INPUTMODE { NONE, MOUSE, UDP, MQTT }; //NONE is internal
    public INPUTMODE inputMode;

    public TopicMessageJSON currentMessage;
    public string currentMessage2;



    public Vector3 track;
    public Vector3 subject;
    public Vector3 pivot;
    public bool isTracking;
    public bool isTrackingCompleted;//mostly using this witch RSPoseController, the button is on the mouse
    public float scaler;
    

    


    // Start is called before the first frame update
    void Start()
    {
        currentMessage = new TopicMessageJSON();


    }

    // Update is called once per frame
    void Update()
    {

        if (inputMode == INPUTMODE.MQTT)
        {

            getMessagesFromMQTT();
            processState();

        }

        if (inputMode == INPUTMODE.MOUSE)
        {
            getPositionFromMouse();
            processState();
        }
    }

    public void resetTracking() { isTrackingCompleted = false; }

    void getPositionFromMouse()
    {
        track = Input.mousePosition;
    }
    void processState()
    {

        

        //this value needs to be reset by consumer
        //should this be an event?
        if (isTrackingCompleted)
            return;

        // this way toggles the tracking on mouse clickes
        // no need to hold the button
        if(Input.GetMouseButtonUp(1))
        {
            if(!isTracking)
            {
                isTracking = true;
                pivot = track;
            }
            else
            {
                isTracking = false;
                pivot = subject = Vector3.zero;
                isTrackingCompleted = true;
            }
        }

        if (isTracking)
        {
            subject = scaler * (track - pivot);
        }

    }

    void getMessagesFromMQTT()
    {

        if (m2MqttCRClient == null)
            return;

        while (m2MqttCRClient.hasMessages())
        {
            currentMessage = m2MqttCRClient.getNextMessage();
            //put this message in the local taskMessageService


            TopicMessageJSON inside = JsonUtility.FromJson<TopicMessageJSON>(currentMessage.messagestring);
            currentMessage2 = inside.messagestring;

            PRFrameJSON frame = JsonUtility.FromJson<PRFrameJSON>(inside.messagestring);

            track = frame.position;

            


        }



    }
}
