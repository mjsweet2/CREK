/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskMessageSubscriber : MonoBehaviour
{

    public TaskMessageService taskMessageService;
    public string[] topics;
    public string[] nonLocalTopics;

    public Queue<string> messages;
    public string currentMessage;



 
    // Start is called before the first frame update
    void Start()
    {
        messages = new Queue<string>();
        StartCoroutine(regMe());
    }

    IEnumerator regMe()
    {

        while (!taskMessageService.isActive)
        {
            yield return new WaitForSeconds(1);
            Debug.Log("not yet");
        }


        for (int i = 0; i < topics.Length; i++)
        {
            taskMessageService.registerSubscription(this, topics[i]);
            Debug.Log(topics[i]);

        }

        for (int i = 0; i < nonLocalTopics.Length; i++)
        {
            taskMessageService.subscribeNonLocal(nonLocalTopics[i]);
            Debug.Log(topics[i]);

        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void postMessage(string outgoing)
    {
        taskMessageService.postMessage(outgoing);
    }

    //since single subscriber can register for more than one topic,
    //the messages contains the topic.
    public void recieveMessage(string message)
    {
        messages.Enqueue(message);
    }

    public bool hasMessages()
    {
        return (messages.Count > 0);
    }
    public string getNextMessage()
    {

        if (messages.Count > 0)
        {
            currentMessage = messages.Dequeue();

            return currentMessage;
        }
        else
            return "nomessage";



    }

}
