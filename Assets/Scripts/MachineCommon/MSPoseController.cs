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
    public Vector3 track;
    public Vector3 pivot;
    public bool isTracking;
    public bool isTrackingCompleted;//mostly using this witch RSPoseController, the button is on the mouse
    public float scaler;
    

    


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        processMouseInput();
    }

    public void resetTracking() { isTrackingCompleted = false; }
    void processMouseInput()
    {

        /*
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isTracking = true;
            pivot = Input.mousePosition;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isTracking = false;
            pivot = Vector3.zero;
            track = Vector3.zero;
        }
        */

        //this value need to be reset by consumer
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
                pivot = Input.mousePosition;
            }
            else
            {
                isTracking = false;
                pivot = track = Vector3.zero;
                isTrackingCompleted = true;
            }
        }

        if (isTracking)
        {
            track = scaler * (Input.mousePosition - pivot);
        }

    }
}
