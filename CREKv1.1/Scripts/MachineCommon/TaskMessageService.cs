/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class TaskMessageService : MonoBehaviour
{

    public bool isActive;   //this object needs to exist before, I can register subscriptions

    public M2MqttCRClient m2MqttNSCClient;

    [Serializable]
    public class TaskMessageSubscription
    {
        public TaskMessageSubscriber subscriber;
        public string topic;
        public TaskMessageSubscription(TaskMessageSubscriber s = null, string t = "notopic") { subscriber = s; topic = t; }

    }
    public List<TaskMessageSubscription> subscriptions;
    public List<string> topics;

    public Queue<string> messages;
    public List<string> messageHistory;
    public string outerMessage;
    public string topic;
    public string messagestring;

    public IntPairJSON intPair;
    public int int1;
    public int int2;
    // Start is called before the first frame update
    void Start()
    {
        subscriptions = new List<TaskMessageSubscription>();
        messages = new Queue<string>();
        messageHistory = new List<string>();
        topics = new List<string>();
        isActive = true;


    }

    // Update is called once per frame
    void Update()
    {
        processAllMessages();
    }

    public void postMessage(string m)
    {
        messages.Enqueue(m);
        messageHistory.Add(m);
    }

    void processAllMessages()
    {
        while (messages.Count > 0)
        {
            outerMessage = messages.Dequeue();
            
            TopicMessageJSON myMessageJSON = JsonUtility.FromJson<TopicMessageJSON>(outerMessage);
            topic = myMessageJSON.topic;

            //keep track of all topics I hear about from publishers and subscribers
            if (!topics.Contains(topic))
            {
                topics.Add(topic);
            }

            for (int i = 0; i < subscriptions.Count; i++)
            {
                if (subscriptions[i].topic == topic)
                {
                    subscriptions[i].subscriber.recieveMessage(outerMessage);
                }

            }

        }
    }

    public void importFromJSON()
    {
        StreamReader sReader;
        string path = Application.persistentDataPath + "/psmessage/" + gameObject.name + ".json";
        sReader = new StreamReader(path);
        string fileString = sReader.ReadToEnd();


        TopicMessageJSON myMessageJSON = JsonUtility.FromJson<TopicMessageJSON>(fileString);

        topic = myMessageJSON.topic;
        messagestring = myMessageJSON.messagestring;


        intPair = JsonUtility.FromJson<IntPairJSON>(messagestring);
        int1 = intPair.int1;
        int2 = intPair.int2;

        sReader.Close();


    }
    public void registerSubscription(TaskMessageSubscriber tmSub, string t)
    {
        // keep track of all topics I hear about from publishers and subscribers
        if (!topics.Contains(t))
        {
            topics.Add(t);
        }


        TaskMessageSubscription sub = new TaskMessageSubscription();
        sub.subscriber = tmSub;
        sub.topic = t;
        subscriptions.Add(sub);



    }
    public void subscribeNonLocal(string t)
    {
        // a TaskMessageSubsciber can subscribe to non local topics, and they will be reposted locally
        //if a Subscribe expects message from a remote source
        //subscriber.tms.registerSubscription(me,t);
        //subscriber.tms.subscribeNonLocal(t)
        //this actuall registers the local system for the non-local message,
        //then the local message will recieve from the outside then repost on the inside

        m2MqttNSCClient.subscribeTopic(t);
        
    }

    public void exportToJSON()
    {

        Debug.Log(gameObject.name);

        TopicMessageJSON myMessageJSON = new TopicMessageJSON();
        myMessageJSON.topic = topic;
        myMessageJSON.messagestring = messagestring;


        //second parameter is to print pretty
        string jsonString = JsonUtility.ToJson(myMessageJSON, true);

        Debug.Log(jsonString);

        //example
        //TrajJSON myObject = JsonUtility.FromJson<TrajJSON>(jsonString);


        StreamWriter sWriter;
        string path = Application.persistentDataPath + "/taskmessage/" + gameObject.name + ".json";
        sWriter = new StreamWriter(path);
        sWriter.Write(jsonString);
        sWriter.Close();

    }
}

