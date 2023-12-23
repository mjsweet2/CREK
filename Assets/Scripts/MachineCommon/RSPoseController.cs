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
    public Transform subject;
    public Transform track;
    public Transform pivot;
    public string currentMessage;
    public bool isTracking;

    public Vector3 mPosition;
    public Quaternion mRotation;
    public Vector3 mRotationVector3;
    public Vector3 positionRotationFix;

   

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (udpSocketClient.runNetwork)
        {
            getMessages();
            processMessages();
        }
    }
    void processMessages()
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
        //mRotationVector3 = subject.rotation.eulerAngles;
        //positionRotationFix = track.rotation.eulerAngles;
        //subject.position = geo.rotateZXY(positionRotationFix*Mathf.Deg2Rad, subject.position);


        
    }
    void getMessages()
    {

        while (udpSocketClient.hasMessages())
        {
            currentMessage = udpSocketClient.getNextMessage();

            string[] lines = currentMessage.Split(';');

            float rx = float.Parse(lines[2]);
            float ry = float.Parse(lines[3]);
            float rz = float.Parse(lines[4]);
            float rw = float.Parse(lines[5]);

            float x = float.Parse(lines[6]);
            float y = float.Parse(lines[7]);
            float z = float.Parse(lines[8]);

            mRotation = new Quaternion(-rx, -ry, -rz, rw);
            mPosition = new Vector3(x, y, z);

            track.position = mPosition;
            track.rotation = mRotation;            

        }

    }
    public void connectUDPClient()
    {

        udpSocketClient.startNetwork();


    }


}