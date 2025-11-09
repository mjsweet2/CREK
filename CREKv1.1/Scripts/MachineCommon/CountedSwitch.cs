/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the MIT License.
 * You should have received a copy of the MIT License with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountedSwitch : MonoBehaviour
{


    public int threshold; //how many consecutive frames you need to switch states
    public int count; // counts the opposite state in order to switch
    public int value; // 0 = off, 1 = on
    public bool onThisFrame;
    public bool offThisFrame;

    public int physicalContact;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        processFrame(physicalContact);
    }

    void reset()
    {

        count = 0; //counts the opposite state in order to switch
        value = 0; // 0 = off, 1 = on
        onThisFrame = false;
        offThisFrame = false;

    }

    void processFrame(int frame)
    {
        onThisFrame = offThisFrame = false;

        if (value == 0)
        {
            if (frame == 0)
            {
                count = 0;
            }
            if (frame == 1)
            {
                count = count + 1;

                if (count >= threshold)
                {
                    value = 1;
                    count = 0;
                    onThisFrame = true;
                }
            }
        }
        else if (value == 1)
        {
            if (frame == 1)
            {
                count = 0;
            }
            if (frame == 0)
            {
                count = count + 1;

                if (count >= threshold)
                {
                    value = 0;
                    count = 0;
                    offThisFrame = true;
                }
            }
        }
    }
    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        physicalContact = 1;
    }
    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerExit(Collider other)
    {
        physicalContact = 0;
    }

    // returns a 1-hot version of value
    int[] getVV()
    {
        int[] v = { 0, 0 };
        v[value] = 1;
        return v;
    }



}
