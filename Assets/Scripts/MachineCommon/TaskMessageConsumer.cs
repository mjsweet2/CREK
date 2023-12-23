/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskMessageConsumer : MonoBehaviour
{


    //get messages
    public TaskMessageSubscriber taskMessageSubscriber;
    public string outerMessage;


    // Start is called before the first frame update
    void Start()
    {

        taskMessageSubscriber = GetComponent<TaskMessageSubscriber>();

    }

    // Update is called once per frame
    void Update()
    {

        processMessages();

    }
    void processMessages()
    {
        while (taskMessageSubscriber.hasMessages())
        {
            outerMessage = taskMessageSubscriber.getNextMessage();
            TopicMessageJSON incoming = JsonUtility.FromJson<TopicMessageJSON>(outerMessage);
            Debug.Log(gameObject.name + ":" + incoming.topic + ":" + incoming.messagestring);
          
        }

    }


}

