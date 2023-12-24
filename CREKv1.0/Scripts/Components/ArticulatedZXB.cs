/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArticulatedZXB : MonoBehaviour
{
    public enum SIDE { LEFT, RIGHT };


    public SIDE side;

    public bool isPlonking;
    public ArticulationBody shoulderZBody;
    public ArticulationBody shoulderXBody;
    public ArticulationBody balBody;


    ArticulationDrive shoulderZDrive;
    ArticulationDrive shoulderXDrive;
    ArticulationDrive balDrive;


    public Vector3 actuation;

    public Vector3 springValue;
    public Vector3 damperValue;


    public bool toeSwitch;
    public bool switchOn;

    public float alphaZ;
    public float alphaX;
    public float alphaB;

    public Vector2 rangeZ;
    public Vector2 rangeX;


    public List<string> messages;



    // Start is called before the first frame update
    void Start()
    {
        shoulderZDrive = shoulderZBody.xDrive;
        shoulderXDrive = shoulderXBody.xDrive;
        balDrive = balBody.xDrive;

    }


    // Update is called once per frame
    void Update()
    {


        actZ();
        actX();
        actB();

    }

    //experimenting with modes




    // clockwise is positive, actuator shaft outwards
    // lift forward
    // left side: clockwise
    // right side: counter-clockwise
    void actZ()
    {
        if (float.IsNaN(actuation.x))
            return;

        if (actuation.x < rangeZ.x)
        {
            actuation.x = rangeZ.x;
            Debug.Log("actuation.x out of low range request");
            messages.Add("actuation.x out of low range request");

        }
        if (actuation.x > rangeZ.y)
        {
            actuation.x = rangeZ.y;
            Debug.Log("actuation.x out of high range request");
            messages.Add("actuation.x out of high range request");

        }

        //this direction is correct for right and left sides, since both actuators face same direction

        shoulderZDrive = shoulderZBody.xDrive;
        shoulderZDrive.target = actuation.x * 360;  //for some reason I can't reverse the axis, so I use negative values
        shoulderZBody.xDrive = shoulderZDrive; //they have to be applied


    }
    // clockwise is positive, actuator shaft backwards
    // lifts
    // left side: clockwise
    // right side: counter-clockwise
    void actX()
    {

        if (float.IsNaN(actuation.y))
            return;
        if (actuation.y < rangeX.x)
        {
            actuation.y = rangeX.x;
            Debug.Log("actuation.y out of low range request");
            messages.Add("actuation.y out of low range request");

        }
        if (actuation.y > rangeX.y)
        {
            actuation.y = rangeX.y;
            Debug.Log("actuation.y out of high range request");
            messages.Add("actuation.y out of high range request");

        }

        //for some reason I can't reverse the axis, so I use negative values
        //reverse right and side direction
        if (side == SIDE.LEFT)// actuation.y = -actuation.y;
        {
            shoulderXDrive = shoulderXBody.xDrive;
            shoulderXDrive.target = actuation.y * 360;
            shoulderXBody.xDrive = shoulderXDrive; //they have to be applied
        }
        else
        {
            shoulderXDrive = shoulderXBody.xDrive;
            shoulderXDrive.target = -actuation.y * 360;
            shoulderXBody.xDrive = shoulderXDrive; //they have to be applied

        }




    }
    // clockwise is positive, actuator shaft outwards
    // bends ee, ie closes
    // left side: clockwise
    // right side: counter-clockwise
    void actB()
    {
        if (float.IsNaN(actuation.z))
            return;

        

        //for some reason I can't reverse the axis, so I use negative values
        //reverse right and side direction
        if (side == SIDE.LEFT)// actuation.z = -actuation.z;
        {
            balDrive = balBody.xDrive;
            balDrive.target = actuation.z * 360;  //for some reason I can't reverse the axis, so I use negative values
            balBody.xDrive = balDrive; //they have to be applied

        }
        else
        {
            balDrive = balBody.xDrive;
            balDrive.target = -actuation.z * 360;  //for some reason I can't reverse the axis, so I use negative values
            balBody.xDrive = balDrive; //they have to be applied

        }



    }


    public void setDamper()
    {
        shoulderZDrive = shoulderZBody.xDrive;
        shoulderXDrive = shoulderXBody.xDrive;
        balDrive = balBody.xDrive;


        shoulderZDrive.damping = damperValue.x;
        shoulderXDrive.damping = damperValue.y;
        balDrive.damping = damperValue.z;


        //they have to be applied
        shoulderZBody.xDrive = shoulderZDrive;
        shoulderXBody.xDrive = shoulderXDrive;
        balBody.xDrive = balDrive;

    }
    public void setSpring()
    {
        shoulderZDrive = shoulderZBody.xDrive;
        shoulderXDrive = shoulderXBody.xDrive;
        balDrive = balBody.xDrive;


        shoulderZDrive.stiffness = springValue.x;
        shoulderXDrive.stiffness = springValue.y;
        balDrive.stiffness = springValue.z;


        //they have to be applied
        shoulderZBody.xDrive = shoulderZDrive;
        shoulderXBody.xDrive = shoulderXDrive;
        balBody.xDrive = balDrive;

    }



}
