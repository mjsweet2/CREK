/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArticulatedZXYK : MonoBehaviour
{
    public enum SIDE { LEFT, RIGHT };


    public SIDE side;

    public bool isPlonking;
    public ArticulationBody shoulderZBody;
    public ArticulationBody shoulderXBody;
    public ArticulationBody shoulderYBody;
    public ArticulationBody kneeBody;


    ArticulationDrive shoulderZDrive;
    ArticulationDrive shoulderXDrive;
    ArticulationDrive shoulderYDrive;
    ArticulationDrive kneeDrive;


    public Vector4 actuation; //(x,y,z,w), x,y,z is same as 3 axis comp. w is y of zxyk
    

    public float springValue;
    public float damperValue;


    public bool toeSwitch;
    public bool switchOn;

    public float alphaZ;
    public float alphaX;
    public float alphaY;
    public float alphaK;

    public Vector2 rangeZ;
    public Vector2 rangeX;
    public Vector2 rangeY;
    public Vector2 rangeK;

    public List<string> messages;



    // Start is called before the first frame update
    void Start()
    {
        shoulderZDrive = shoulderZBody.xDrive;
        shoulderXDrive = shoulderXBody.xDrive;
        shoulderYDrive = shoulderYBody.xDrive;
        kneeDrive = kneeBody.xDrive;


    }


    // Update is called once per frame
    void Update()
    {
        

        actZ();
        actX();
        actY();
        actK();

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

    void actY()
    {

        if (float.IsNaN(actuation.w))
            return;
        if (actuation.w < rangeY.x)
        {
            actuation.w = rangeY.x;
            Debug.Log("actuation.w out of low range request");
            messages.Add("actuation.w out of low range request");

        }
        if (actuation.w > rangeY.y)
        {
            actuation.w = rangeY.y;
            Debug.Log("actuation.w out of high range request");
            messages.Add("actuation.w out of high range request");

        }

        //for some reason I can't reverse the axis, so I use negative values
        //reverse right and side direction
        if (side == SIDE.LEFT)// actuation.y = -actuation.y;
        {
            shoulderYDrive = shoulderYBody.xDrive;
            shoulderYDrive.target = actuation.w * 360;
            shoulderYBody.xDrive = shoulderYDrive; //they have to be applied
        }
        else
        {
            shoulderYDrive = shoulderYBody.xDrive;
            shoulderYDrive.target = -actuation.w * 360;
            shoulderYBody.xDrive = shoulderYDrive; //they have to be applied

        }

    }


    // clockwise is positive, actuator shaft outwards
    // bends ee, ie closes
    // left side: clockwise
    // right side: counter-clockwise
    void actK()
    {
        if (float.IsNaN(actuation.z))
            return;

        if (actuation.z < rangeK.x)
        {
            actuation.z = rangeK.x;
            Debug.Log("actuation.z out of low range request");
            messages.Add("actuation.z out of low range request");

        }
        if (actuation.z > rangeK.y)
        {
            actuation.z = rangeK.y;
            Debug.Log("actuation.z out of high range request");
            messages.Add("actuation.z out of high range request");

        }

        //for some reason I can't reverse the axis, so I use negative values
        //reverse right and side direction
        if (side == SIDE.LEFT)// actuation.z = -actuation.z;
        {
            kneeDrive = kneeBody.xDrive;
            kneeDrive.target = actuation.z * 360;  //for some reason I can't reverse the axis, so I use negative values
            kneeBody.xDrive = kneeDrive; //they have to be applied

        }
        else
        {
            kneeDrive = kneeBody.xDrive;
            kneeDrive.target = -actuation.z * 360;  //for some reason I can't reverse the axis, so I use negative values
            kneeBody.xDrive = kneeDrive; //they have to be applied

        }



    }


    public void setDamper()
    {
        shoulderZDrive = shoulderZBody.xDrive;
        shoulderXDrive = shoulderXBody.xDrive;
        kneeDrive = kneeBody.xDrive;


        shoulderZDrive.damping = damperValue;
        shoulderXDrive.damping = damperValue;
        kneeDrive.damping = damperValue;


        //they have to be applied
        shoulderZBody.xDrive = shoulderZDrive;
        shoulderXBody.xDrive = shoulderXDrive;
        kneeBody.xDrive = kneeDrive;

    }
    public void setSpring()
    {
        shoulderZDrive = shoulderZBody.xDrive;
        shoulderXDrive = shoulderXBody.xDrive;
        kneeDrive = kneeBody.xDrive;


        shoulderZDrive.stiffness = springValue;
        shoulderXDrive.stiffness = springValue;
        kneeDrive.stiffness = springValue;


        //they have to be applied
        shoulderZBody.xDrive = shoulderZDrive;
        shoulderXBody.xDrive = shoulderXDrive;
        kneeBody.xDrive = kneeDrive;

    }



}
