/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalZXE : MonoBehaviour
{
    public enum SIDE { LEFT, RIGHT };


    public SIDE side;

    public bool isPlonking;
    public HingeJoint shoulderZHinge;
    public HingeJoint shoulderXHinge;
    public HingeJoint elbowHinge;


    JointSpring shoulderZSpring;
    JointSpring shoulderXSpring;
    JointSpring elbowSpring;




    public Vector3 actuation;

    public Vector3 springValue;
    public Vector3 damperValue;


    public bool toeSwitch;
    public bool switchOn;

    public float alphaZ;
    public float alphaX;
    public float alphaE;

    public Vector2 rangeZ;
    public Vector2 rangeX;
    public Vector2 rangeE;

    public List<string> messages;






    // Start is called before the first frame update
    void Start()
    {
        shoulderZSpring = shoulderZHinge.spring;
        shoulderXSpring = shoulderXHinge.spring;
        elbowSpring = elbowHinge.spring;



    }


    // Update is called once per frame
    void Update()
    {



        actZ();
        actX();
        actE();

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

        shoulderZSpring = shoulderZHinge.spring;
        shoulderZSpring.targetPosition = actuation.x * 360;
        shoulderZHinge.spring = shoulderZSpring; //they have to be applied


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

        shoulderXSpring = shoulderXHinge.spring;
        shoulderXSpring.targetPosition = actuation.y * 360;
        shoulderXHinge.spring = shoulderXSpring; //they have to be applied


    }
    // clockwise is positive, actuator shaft outwards
    // bends ee, ie closes
    // left side: clockwise
    // right side: counter-clockwise
    void actE()
    {
        if (float.IsNaN(actuation.z))
            return;

        if (actuation.z < rangeE.x)
        {
            actuation.z = rangeE.x;
            Debug.Log("actuation.z out of low range request");
            messages.Add("actuation.z out of low range request");

        }
        if (actuation.z > rangeE.y)
        {
            actuation.z = rangeE.y;
            Debug.Log("actuation.z out of high range request");
            messages.Add("actuation.z out of high range request");

        }



        elbowSpring = elbowHinge.spring;
        elbowSpring.targetPosition = actuation.z * 360;
        elbowHinge.spring = elbowSpring; //they have to be applied

    }


    public void setDamper()
    {
        shoulderZSpring = shoulderZHinge.spring;
        shoulderXSpring = shoulderXHinge.spring;
        elbowSpring = elbowHinge.spring;


        shoulderZSpring.damper = damperValue.x;
        shoulderXSpring.damper = damperValue.y;
        elbowSpring.damper = damperValue.z;


        //they have to be applied
        shoulderZHinge.spring = shoulderZSpring;
        shoulderXHinge.spring = shoulderXSpring;
        elbowHinge.spring = elbowSpring;

    }
    public void setSpring()
    {
        shoulderZSpring = shoulderZHinge.spring;
        shoulderXSpring = shoulderXHinge.spring;
        elbowSpring = elbowHinge.spring;


        shoulderZSpring.spring = springValue.x;
        shoulderXSpring.spring = springValue.y;
        elbowSpring.spring = springValue.z;


        //they have to be applied
        shoulderZHinge.spring = shoulderZSpring;
        shoulderXHinge.spring = shoulderXSpring;
        elbowHinge.spring = elbowSpring;

    }



}