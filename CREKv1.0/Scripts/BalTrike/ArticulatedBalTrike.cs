using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArticulatedBalTrike : MonoBehaviour
{

    public ArticulatedZXB leftRear;
    public ArticulatedZXB rightRear;


    public ArticulatedSegs segs;


    public Transform orientationSensor;
    public Vector3 orientation;
    public string sensorString;


    public bool usePhysics;

    // Start is called before the first frame update
    void Start()
    {

        //setAllSpring(10000);
        //setAllDamper(50);

        if(!usePhysics)
        {
            disablePhysics();
        }

    }

    // Update is called once per frame
    void Update()
    {
        orientation = orientationSensor.rotation.eulerAngles;
        buildSensorString();

    }


    void setAllSpring(float s)
    {
        Vector3 sss = new Vector3(s, s, s);
        leftRear.springValue = sss;
        leftRear.setSpring();

        rightRear.springValue = sss;
        rightRear.setSpring();
    }
    void setAllDamper(float d)
    {
        Vector3 ddd = new Vector3(d, d, d);
        leftRear.damperValue = ddd;
        leftRear.setDamper();

        rightRear.damperValue = ddd;
        rightRear.setDamper();
    }
    public void processTripleMessage(string wire, float x, float y, float z)
    {
        if (wire == "1")
        {

            leftRear.actuation.x = x;
            leftRear.actuation.y = y;
            //leftRear.actuation.z = z;
        }
        if (wire == "2")
        {

            rightRear.actuation.x = x;
            rightRear.actuation.y = y;
            //rightRear.actuation.z = z;
        }
        if (wire == "3")
        {

            //leftRear.actuation.x = x;
            //leftRear.actuation.y = y;
            leftRear.actuation.z = z;
        }
        if (wire == "4")
        {

            //rightRear.actuation.x = x;
            //rightRear.actuation.y = y;
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
                leftRear.actuation.x = value;
            if (device == "8002")
                leftRear.actuation.y = value;
            if (device == "8003")
                leftRear.actuation.z = value;
        }
        if (wire == "2")
        {
            if (device == "8001")
                rightRear.actuation.x = value;
            if (device == "8002")
                rightRear.actuation.y = value;
            if (device == "8003")
                rightRear.actuation.z = value;
            Debug.Log("2: " + rightRear.actuation.ToString());
        }



    }

    //4 ints
    //4 floats

    public void buildSensorString()
    {

        sensorString = (leftRear.switchOn ? "1;" : "0;") + (rightRear.switchOn ? "1;" : "0;");
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
