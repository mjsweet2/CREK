/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Articulated2ZXE2ZXYK : MonoBehaviour
{

    public ArticulatedZXE leftFront;
    public ArticulatedZXE rightFront;
    public ArticulatedZXYK leftRear;
    public ArticulatedZXYK rightRear;

    public ArticulatedSegs segs;


    public Transform orientationSensor;
    public Vector3 orientation;
    public string sensorString;




    // Start is called before the first frame update
    void Start()
    {

        setAllSpring(10000);
        setAllDamper(50);

    }

    // Update is called once per frame
    void Update()
    {
        orientation = orientationSensor.rotation.eulerAngles;
        buildSensorString();

    }


    void setAllSpring(float s)
    {

        leftFront.springValue = s;
        leftFront.setSpring();

        rightFront.springValue = s;
        rightFront.setSpring();

        leftRear.springValue = s;
        leftRear.setSpring();

        rightRear.springValue = s;
        rightRear.setSpring();


    }
    void setAllDamper(float d)
    {

        leftFront.damperValue = d;
        leftFront.setDamper();

        rightFront.damperValue = d;
        rightFront.setDamper();

        leftRear.damperValue = d;
        leftRear.setDamper();

        rightRear.damperValue = d;
        rightRear.setDamper();


    }
    public void processTripleMessage(string wire, float x, float y, float z)
    {
        if (wire == "1")
        {

            leftFront.actuation.x = x;
            leftFront.actuation.y = y;
            leftFront.actuation.z = z;
        }
        if (wire == "2")
        {

            rightFront.actuation.x = x;
            rightFront.actuation.y = y;
            rightFront.actuation.z = z;
        }
        if (wire == "3")
        {

            leftRear.actuation.x = x;
            leftRear.actuation.y = y;
            leftRear.actuation.z = z;
        }
        if (wire == "4")
        {
            rightRear.actuation.x = x;
            rightRear.actuation.y = y;
            rightRear.actuation.z = z;
        }
        if (wire == "5")
        {
            segs.actuation.x = x;
            segs.actuation.y = y;
            segs.actuation.z = z;
        }


    }
    public void processMessage(string wire, string device, float value)
    {
        if (wire == "1")
        {
            if (device == "8001")
                leftFront.actuation.x = value;
            if (device == "8002")
                leftFront.actuation.y = value;
            if (device == "8003")
                leftFront.actuation.z = value;
        }
        if (wire == "2")
        {
            if (device == "8001")
                rightFront.actuation.x = value;
            if (device == "8002")
                rightFront.actuation.y = value;
            if (device == "8003")
                rightFront.actuation.z = value;
            Debug.Log("2: " + rightFront.actuation.ToString());
        }
        if (wire == "3")
        {
            if (device == "8001")
                leftRear.actuation.x = value;
            if (device == "8002")
                leftRear.actuation.y = value;
            if (device == "8003")
                leftRear.actuation.z = value;
        }
        if (wire == "4")
        {
            if (device == "8001")
                rightRear.actuation.x = value;
            if (device == "8002")
                rightRear.actuation.y = value;
            if (device == "8003")
                rightRear.actuation.z = value;
        }



    }

    //4 ints
    //4 floats

    public void buildSensorString()
    {

        sensorString = (leftFront.switchOn ? "1;" : "0;") + (rightFront.switchOn ? "1;" : "0;");
        sensorString = sensorString + (leftRear.switchOn ? "1;" : "0;") + (rightRear.switchOn ? "1;" : "0;");
        sensorString = sensorString + orientation.x.ToString() + ";" + "0.0;" + "0.0;" + "0.0";

    }
    public string getSensorString()
    {
        buildSensorString();
        return sensorString;
    }
}
