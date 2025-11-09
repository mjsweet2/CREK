/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;

public class M2MqttCRClient : M2MqttUnity.M2MqttUnityClient
{

    public List<TopicMessageJSON> mqttMessages;
    public List<string> uiMessages;

    public List<string> topics;
    public bool doDebugMessages;

    
    
   


    //for testing
    public float loopInterval;
    public int messageCount;
    public int messageCountMax;

    protected override void Start()
    {


        topics.Add("notopic");

        messageCount = 0;// this test app is for testing how fast many messages get processed.

        mqttMessages = new List<TopicMessageJSON>();
        uiMessages = new List<string>();
        //setUiMessage("Ready.");
        base.Start();

       

    }
    protected override void Update()
    {
        base.Update(); // call ProcessMqttEvents()


    }

    //following 2 functions are for testing
    public void publishOne()
    {
        publish("M2MQTT_Unity/FunIncoming","countingmessage");
    }
    public void publishOneHundred()
    {
        for (int i = 0; i < messageCountMax; i++)
        {
            publish("M2MQTT_Unity/FunIncoming","countingmessage");
            //yield return new WaitForSeconds(loopInterval);
        }
    }
    public void publish(string topic, string outgoingMessage)
    {
        //this publishes all my outgoing messages to the same topic "M2MQTT_Unity/fun"
        client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(outgoingMessage), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
        if(doDebugMessages)
        {
                Debug.Log("Message published");
        }
        
        //addUiMessage("Message published.");
    }

    public void SetBrokerAddress(string brokerAddressString)
    {
        this.brokerAddress = brokerAddressString;
    }

    public void SetBrokerPort(string brokerPortString)
    {
        int.TryParse(brokerPortString, out this.brokerPort);
    }

    public void SetEncrypted(bool isEncrypted)
    {
        this.isEncrypted = isEncrypted;
    }


    public void setUiMessage(string msg)
    {
        uiMessages.Clear();
        //uiMessages.Add(msg);
    }

    public void addUiMessage(string msg)
    {
        //uiMessages.Add(msg);
    }
    public bool ifHasUIMessages()
    {
        return (uiMessages.Count == 0);
    }


    protected override void OnConnecting()
    {
        base.OnConnecting();
        setUiMessage("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        setUiMessage("Connected to broker on " + brokerAddress + "\n");

    }

    protected override void SubscribeTopics()
    {
        //MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE
        //new string[] { "M2MQTT_Unity/FunIncoming" }
        //new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE }
        string[] topicsArray = new string[topics.Count];
        byte[] qosLevels = new byte[topics.Count];
        for(int i = 0; i < topics.Count; i++)
        {
            topicsArray[i] = topics[i];
            qosLevels[i] = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE;
        }    
        client.Subscribe(topicsArray, qosLevels);
    }
    
    public void subscribeTopic(string t)
    {
        //for late subscriptions
        //this allows me to manage a list of my subscriptions
        //during run time
        topics.Add(t);
        client.Subscribe(new string[] { t }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

    }
    protected override void UnsubscribeTopics()
    {
        //new string[] { "M2MQTT_Unity/FunIncoming" }

        string[] topicsArray = new string[topics.Count];
        
        for (int i = 0; i < topics.Count; i++)
        {
            topicsArray[i] = topics[i];
        }

        client.Unsubscribe(topicsArray);     
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        ;// addUiMessage("CONNECTION FAILED! " + errorMessage);
    }

    protected override void OnDisconnected()
    {
        ;// addUiMessage("Disconnected.");
    }

    protected override void OnConnectionLost()
    {
        ;// addUiMessage("CONNECTION LOST!");
    }


    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);
        //Debug.Log("Received: " + msg);
        messageCount++;
        //TODO, comment StoreMessage(msg), once everything is tested
        //StoreMessage(msg);


        
        TopicMessageJSON outGoing = new TopicMessageJSON();
        outGoing.topic = topic;
        outGoing.messagestring = msg;

        //this one strips away the topic from mqtt
        mqttMessages.Add(outGoing);
 

    }

    public bool hasMessages()
    {
        return (mqttMessages.Count > 0);
    }
    public TopicMessageJSON getNextMessage()
    {
        TopicMessageJSON next = new TopicMessageJSON();
        if (mqttMessages.Count < 1)
            return next;

        next = mqttMessages[0];
        mqttMessages.RemoveAt(0);
        return next;
    }
    private void StoreMessage(string eventMsg)
    {
        //TopicMessageJSON another = new TopicMessageJSON();
        //another.messagestring = eventMsg;
        //mqttMessages.Add(another);
    }

    private void ProcessMessage(string msg)
    {
        //in practice this function will add messages to the internal TaskMessageService
        //I think I can do this in DecodeMessage(topic), 
        messageCount++;
    }



    private void OnDestroy()
    {
        Disconnect();
    }

    private void OnValidate()
    {
        autoConnect = true;
    }
}



/*
 * 
 *     //these values are from the base class M2MqttUnityClient
        public string brokerAddress = "localhost";
        [Tooltip("Port where the broker accepts connections")]
        public int brokerPort = 1883;
        [Tooltip("Use encrypted connection")]
        public bool isEncrypted = false;
        [Tooltip("Name of the certificate with extension e.g. client.pem. StreamingAssets or accessible folder")]
        public string certificateName;
        [Header("Connection parameters")]
        [Tooltip("Connection to the broker is delayed by the the given milliseconds")]
        public int connectionDelay = 500;
        [Tooltip("Connection timeout in milliseconds")]
        public int timeoutOnConnection = MqttSettings.MQTT_CONNECT_TIMEOUT;
        [Tooltip("Connect on startup")]
        public bool autoConnect = false;
        [Tooltip("UserName for the MQTT broker. Keep blank if no user name is required.")]
        public string mqttUserName = null;
        [Tooltip("Password for the MQTT broker. Keep blank if no password is required.")]
        public string mqttPassword = null;


*/