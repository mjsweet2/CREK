/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MotionController : MonoBehaviour
{
    public bool isKeyboard;
    public Vector3 lMotionInput;
    public Vector3 rMotionInput;
    public float maxValue; //calcs the maximum value of the 4 input floats for single axis conditions
    public float minValue;
    public float controlValue;

    public float lDir; // this is the angle in degrees of the left thumb controller 0-360, 0 is up
    public float rDir; // this is the angle in degrees of the left thumb controller 0-360, 0 is up
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isKeyboard)
        {
            processMotionInputOnKeyboard();
        }
        else
        {
            processMotionInputOnController();
        }


    }
    float maximum(float a, float b)
    {
        if (a > b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }
    float minimum(float a, float b)
    {
        if (a < b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    float maxAbs(float a, float b)
    {
        if (Mathf.Abs(a) > Mathf.Abs(b))
        {
            return a;
        }
        else
        {
            return b;
        }

    }

    //lMotionInput
    //rMotionInput
    void processMotionInputOnController()
    {

        lMotionInput.x = Input.GetAxis("Horizontal");
        lMotionInput.y = Input.GetAxis("Vertical");

        rMotionInput.x = Input.GetAxis("RHorizontal");
        rMotionInput.y = Input.GetAxis("RVertical");

        maxValue = maximum(maximum(lMotionInput.x, lMotionInput.y), maximum(rMotionInput.x, rMotionInput.y));
        minValue = minimum(minimum(lMotionInput.x, lMotionInput.y), minimum(rMotionInput.x, rMotionInput.y));
        controlValue = maxAbs(maxAbs(lMotionInput.x, lMotionInput.y), maxAbs(rMotionInput.x, rMotionInput.y));
    }
    void processMotionInputOnKeyboard()
    {

        //lMotionInput
        //rMotionInput


        lMotionInput.x = 0f;
        if (Input.GetKey(KeyCode.D))
        {
            lMotionInput.x = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            lMotionInput.x = -1f;
        }

        lMotionInput.y = 0f;
        if (Input.GetKey(KeyCode.W))
        {
            lMotionInput.y = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            lMotionInput.y = -1f;
        }

        maxValue = maximum(maximum(lMotionInput.x, lMotionInput.y), maximum(rMotionInput.x, rMotionInput.y));
        minValue = minimum(minimum(lMotionInput.x, lMotionInput.y), minimum(rMotionInput.x, rMotionInput.y));
        controlValue = maxAbs(maxAbs(lMotionInput.x, lMotionInput.y), maxAbs(rMotionInput.x, rMotionInput.y));

    }
}
