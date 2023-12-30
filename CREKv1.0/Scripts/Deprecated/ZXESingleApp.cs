using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ZXESingleApp : MonoBehaviour
{

    public InputField outputIF;
    public InputField inputIF;


    public ArticulatedZXE articulatedZXE;
    public ComponentZXE componentZXE;

    public Transform target;
    public Transform intermediateA;
    public Transform intermediateB;



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }
    void processMessage(string message)
    {
        string[] values = message.Split(';');

        float z, x, e;
        float ax, ay, az;
        float bx, by, bz;
        z = float.Parse(values[0]);
        x = float.Parse(values[1]);
        e = float.Parse(values[2]);

        ax = float.Parse(values[3]);
        ay = float.Parse(values[4]);
        az = float.Parse(values[5]);

        bx = float.Parse(values[6]);
        by = float.Parse(values[7]);
        bz = float.Parse(values[8]);

        articulatedZXE.actuation = new Vector3(z, x, e);


        intermediateA.position = new Vector3(ax, ay, az);
        intermediateB.position = new Vector3(bx, by, bz);



    }
    
   




}

