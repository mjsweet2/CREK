/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompSegPair : MonoBehaviour
{

    public string compName; // self.name = "base_class"     # for logging
    public Vector3 lastValidUpperIk;
    public Vector3 lastValidLowerIk;
    public Vector3 lastValidFk;  
    public bool is_motioning;     // is this component running a component trajectory 



    public float inputUX;
    public float inputUZ;

    public float inputLX;
    public float inputLZ;

    public float uRange;
    public float lRange;


    public float magToAct;


    public float topBlock;
    public float middleBlock;
    public float bottomBlock;
    public float segLength;



    //non-children, just for testing
    public Transform visualizeFK;
    

  
    // Start is called before the first frame update
    void Start()
    {
    
       

    }
    public Vector3 valUpperIk()
    {
        return lastValidUpperIk;
    }
    public Vector3 valLowerIk()
    {
        return lastValidLowerIk;
    }

    void rotateXZPair(float x, float z)
    {

        //for (int i = 0; i < segments.Length; i++)
        //{
        //    segments[i].localRotation = Quaternion.Euler(new Vector3(x, 0f, z));
        //
        // }

    }
    public Vector3 valFk()
    {
        return lastValidFk;
    }
    public void loadParams(float s, float t, float m, float b)
    {
        segLength = s;

        topBlock = t;
        middleBlock = m;
        bottomBlock = b;

        float dormant = bottomBlock + (5 * segLength) + middleBlock + (5 * segLength) + topBlock;
        lastValidFk = new Vector3(0f, 0f, dormant);

    }

    // Update is called once per frame
    void Update()
    {
        
        

    }



    void limitXZPair()
    {
        if (inputUX < -1f)
            inputUX = -1f;
        if (inputUX > 1f)
            inputUX = 1f;


        if (inputUZ < -1f)
            inputUZ = -1f;
        if (inputUZ > 1f)
            inputUZ = 1f;

        if (inputLX < -1f)
            inputLX = -1f;
        if (inputLX > 1f)
            inputLX = 1f;


        if (inputLZ < -1f)
            inputLZ = -1f;
        if (inputLZ > 1f)
            inputLZ = 1f;


    }

    public Geometry.GeoPacket doFk(float ux, float uz, float lx, float lz)
    {
        inputUX = ux;
        inputUZ = uz;

        inputLX = lx;
        inputLZ = lz;

        limitXZPair();

        calcFK();

        return new Geometry.GeoPacket(true, valFk()); //  add boolean for good/bad request

    }

    //can't multiply magnitude of rotation, have to iterate through all seperate rotations
    //probably because it suffers similiar consequence as rotation order
    void calcFK()
    {
        float segUXAngle = -inputUX * uRange;
        float segUZAngle = -inputUZ * uRange;

        float segLXAngle = -inputLX * lRange;
        float segLZAngle = -inputLZ * lRange;

        //start at the bottom and work upwards


        //bottom block
        Vector3 seg = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, new Vector3(0f, 0f, .18f));
        seg = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, seg);

        seg += new Vector3(0f, 0f, .03f);

        seg = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, seg);
        seg = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, seg);

        seg += new Vector3(0f, 0f, .03f);

        seg = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, seg);
        seg = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, seg);

        seg += new Vector3(0f, 0f, .03f);

        seg = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, seg);
        seg = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, seg);

        seg += new Vector3(0f, 0f, .03f);

        seg = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, seg);
        seg = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, seg);

        

        //
        //
        //
        //
        //
        //middle block/lowest segment on top segments
        seg += new Vector3(0f, 0f, .03f);
        seg += new Vector3(0f, 0f, .125f);
        seg = Geometry.rotateAroundYAxis(segUZAngle * Mathf.Deg2Rad, seg);
        seg = Geometry.rotateAroundXAxis(segUXAngle * Mathf.Deg2Rad, seg);


        seg += new Vector3(0f, 0f, .03f);

        seg = Geometry.rotateAroundYAxis(segUZAngle * Mathf.Deg2Rad, seg);
        seg = Geometry.rotateAroundXAxis(segUXAngle * Mathf.Deg2Rad, seg);

        seg += new Vector3(0f, 0f, .03f);

        seg = Geometry.rotateAroundYAxis(segUZAngle * Mathf.Deg2Rad, seg);
        seg = Geometry.rotateAroundXAxis(segUXAngle * Mathf.Deg2Rad, seg);

        seg += new Vector3(0f, 0f, .03f);

        seg = Geometry.rotateAroundYAxis(segUZAngle * Mathf.Deg2Rad, seg);
        seg = Geometry.rotateAroundXAxis(segUXAngle * Mathf.Deg2Rad, seg);

        seg += new Vector3(0f, 0f, .03f);

        seg = Geometry.rotateAroundYAxis(segUZAngle * Mathf.Deg2Rad, seg);
        seg = Geometry.rotateAroundXAxis(segUXAngle * Mathf.Deg2Rad, seg);


        //top block/highest segment in upper segments
        seg += new Vector3(0f, 0f, .03f);

        //visualizeFK.position = seg;

        lastValidUpperIk = new Vector3(inputUX, 0f, inputUZ) * magToAct;
        lastValidLowerIk = new Vector3(inputLX, 0f, inputLZ) * magToAct;

        lastValidFk = seg;


    }


   
}
