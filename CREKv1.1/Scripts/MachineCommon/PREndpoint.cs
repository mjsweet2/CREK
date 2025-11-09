/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the MIT License.
 * You should have received a copy of the MIT License with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PREndpointFrameJSON
{
    public List<Vector3> positions;
    public List<Quaternion> rotations;

    public PREndpointFrameJSON() { positions = new List<Vector3>(); rotations = new List<Quaternion>(); }

}

public class PREndpoint : MonoBehaviour
{

    public M2MqttCRClient m2MqttCRClient;
    public string sendTopic;
    public PREndpointFrameJSON prEndpointFrameJSON;
    public TopicMessageJSON topicMessageJSON;
    public string messageString;
    public enum ENDPOINTTYPE { NONE, SENDER, RECEIVER };
    public ENDPOINTTYPE endpointType;

    

    public List<Transform> transforms;

    // Start is called before the first frame update
    void Start()
    {
        prEndpointFrameJSON = new PREndpointFrameJSON();
        topicMessageJSON = new TopicMessageJSON();

        m2MqttCRClient = GetComponent<M2MqttCRClient>();

        if (endpointType == ENDPOINTTYPE.SENDER)
        {
            setupAsSender();
        }
        else if (endpointType == ENDPOINTTYPE.RECEIVER)
        {
            setupAsReceiver();
        }


    }

    // Update is called once per frame
    void Update()
    {

        if(endpointType == ENDPOINTTYPE.SENDER)
        {
            doSend();
        }
        else if(endpointType == ENDPOINTTYPE.RECEIVER)
        {
            doReceive();
        }

    }

    void doSend()
    {
        if (transforms.Count != prEndpointFrameJSON.positions.Count)
        {
            Debug.Log("sender and reciever count do not match");
            return;
        }

        for (int i = 0; i < transforms.Count; i++)
        {
            prEndpointFrameJSON.positions[i] = transforms[i].position;
            prEndpointFrameJSON.rotations[i] = transforms[i].rotation;
        }

        messageString = JsonUtility.ToJson(prEndpointFrameJSON);

        m2MqttCRClient.publish(sendTopic, messageString);

    }
    void doReceive()
    {
        if (transforms.Count != prEndpointFrameJSON.positions.Count)
        {
            Debug.Log("sender and reciever count do not match");
            return;
        }

        while(m2MqttCRClient.hasMessages())
        {
            topicMessageJSON = m2MqttCRClient.getNextMessage();
            prEndpointFrameJSON = JsonUtility.FromJson<PREndpointFrameJSON>(topicMessageJSON.messagestring);
        }
        //I don't push the transform position for each message, only the last one
        for (int i = 0; i < transforms.Count; i++)
        {
            transforms[i].position = prEndpointFrameJSON.positions[i];
            transforms[i].rotation = prEndpointFrameJSON.rotations[i];
        }
    }
    void setupAsSender()
    {
        prEndpointFrameJSON.positions.Clear();
        prEndpointFrameJSON.rotations.Clear();
        for (int i = 0; i < transforms.Count; i++)
        {
            prEndpointFrameJSON.positions.Add(transforms[i].position);
            prEndpointFrameJSON.rotations.Add(transforms[i].rotation);
        }

    }
    void setupAsReceiver()
    {
        prEndpointFrameJSON.positions.Clear();
        prEndpointFrameJSON.rotations.Clear();
        for (int i = 0; i < transforms.Count; i++)
        {
            prEndpointFrameJSON.positions.Add(transforms[i].position);
            prEndpointFrameJSON.rotations.Add(transforms[i].rotation);
        }

    }
}
