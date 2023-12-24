/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArticulatedBalDiff : MonoBehaviour
{

    public ArticulatedWheel left;
    public ArticulatedWheel right;

    public Transform orientationSensor;
    public Vector3 orientation;
    public string sensorString;


    public bool usePhysics;

    // Start is called before the first frame update
    void Start()
    {

        //setAllSpring(10000);
        //setAllDamper(50);

        if (!usePhysics)
        {
            disablePhysics();
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (orientationSensor != null)
        {
            orientation = orientationSensor.rotation.eulerAngles;
        }
        buildSensorString();

    }


    void setAllSpring(float s)
    {

        left.springValue = s;
        left.setSpring();

        right.springValue = s;
        right.setSpring();
    }
    void setAllDamper(float d)
    {
  
        left.damperValue = d;
        left.setDamper();

        right.damperValue = d;
        right.setDamper();
    }
    public void processTripleMessage(string wire, float x, float y, float z)
    {
        if (wire == "1")
        {
            left.actuation = x;
        }
        if (wire == "2")
        {
            right.actuation = x;
        }
      
    }


    //4 ints
    //4 floats

    public void buildSensorString()
    {

        sensorString = sensorString + orientation.x.ToString() + ";" + "0.0;" + "0.0;" + "0.0";

    }
    public string getSensorString()
    {
        buildSensorString();
        return sensorString;
    }


    void disablePhysics()
    {

        ArticulationBody ab = transform.GetComponent<ArticulationBody>();
        if (ab != null)
        {
            ab.useGravity = false;
            ab.immovable = ab.isRoot;
            Debug.Log(transform.name);
        }

        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                disablePhysics(transform.GetChild(i));
            }
        }
    }

    private void disablePhysics(Transform t)
    {

        ArticulationBody ab = t.GetComponent<ArticulationBody>();
        if (ab != null)
        {
            ab.useGravity = false;
            ab.immovable = ab.isRoot;
            Debug.Log(t.name);
        }

        if (t.childCount > 0)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                disablePhysics(t.GetChild(i));
            }
        }
    }

}
