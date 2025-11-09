/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompSegs : MonoBehaviour
{

    public string compName; // self.name = "base_class"     # for logging
    public Vector3 dormantFk; // fk at zero actuation
    public Vector4 lastValidIk;
    public Vector3 lastValidFk;
    public bool is_motioning;     // is this component running a component trajectory 



    public float inputX; //requested degrees of rotation of each segment
    public float inputZ; //requested degrees of rotation of each segment

    public float inputUpperX;   //experimenting with putting 2nd set of segs into this component.

    public float range; //max degrees of rotation of each segment


    public float magToAct;  //should be 1 for digital robot, about .16 for real robot


    public float topBlockLength;    //length from the machine origin to the top of the actual segments
    public float middleBlockLength; //length between 2 section of segments
    public float bottomBlockLength; //additional length after the segemnts that influence components after the segments
    public float segLength;         //all segments have this length


    public Vector3 intermediateFk;
    public Vector3 intermediateInfluenceFK;

    
    //non-children, just for testing
    public Transform visualizeFK;
    public Transform visualizeInfluencedFk;
    public Transform inputInfluenceFk;

    public Vector3 watch01Vector3;
    public Vector3 watch02Vector3;
    public Vector3 watch03Vector3;
    public float watchScaler;
    public ArticulatedSegs articulatedSegs;

     


    // Start is called before the first frame update
    void Start()
    {
       
        //refreshParams();


    }
    // Update is called once per frame
    void Update()
    {
        //testInfluenceCalculations();



    }

    void testInfluenceCalculations()
    {
        if (articulatedSegs != null)
        {
            doIk(inputX, inputZ);
            //flip around some values for this particular setup
            articulatedSegs.actuation = new Vector3(inputZ, inputX, 0f);
        }

        visualizeFK.position = lastValidFk;
        watch01Vector3 = inputInfluenceFk.localPosition * watchScaler + new Vector3(0f, 0f, -0.45f);//convert to machine space here.
        watch03Vector3 = getLowerDeltaPositionInverse(watch01Vector3);
        visualizeInfluencedFk.position = watch03Vector3;


    }

    public Vector4 valIk()
    {
        return lastValidIk;
    }

    public Vector3 valFk()
    {
        return lastValidFk;
    }
    public void loadParams(float s, float t, float m, float b)
    {
        segLength = s;

        topBlockLength = t;
        middleBlockLength = m;
        bottomBlockLength = b; // I might not need this

        float dormant = topBlockLength + (1 * segLength) + middleBlockLength + (5 * segLength);
        dormantFk = new Vector3(0f, 0f, -dormant);
        lastValidFk = dormantFk;

    }

    //call this if I'm not calling loadParams directly
    void refreshParams()
    {
        float dormant = topBlockLength + (1 * segLength) + middleBlockLength + (5 * segLength);
        dormantFk = new Vector3(0f, 0f, -dormant);
        lastValidFk = dormantFk;

    }



    //TODO, figure out upper influence and lower influence?
    //this is the effect on the machine space fks for everything above the segs
    //and everything below.
    //I want to invert this influence and send them to the affected components
    //each of these is a different pose mode? keep the upper steady vs keep the lower steady?

    public Vector3 getLowerDeltaRotationInverse(Vector3 machineSpaceRot)
    {
        Vector3 rot = new Vector3(-inputX * Mathf.Deg2Rad * 5f, -inputZ * Mathf.Deg2Rad * 5f, 0f) + machineSpaceRot;
        return rot;
    }
    public Vector3 getLowerDeltaPositionInverse(Vector3 machineSpacePos)
    {
        //step 1: find machine space of the base of the lowest segment in zero actuation
        //step 2: subject this from the passed in machineSpaceFK. This is the start of the rotation sequence


        //topBlockLength + (1 * segLength) + middleBlockLength + (5 * segLength);

        Vector3 baseFK = machineSpacePos - dormantFk;
        watch02Vector3 = baseFK;

        float segLXAngle = -inputX;
        float segLZAngle = -inputZ;

        float segUXAngle = -inputUpperX;

        //start at the bottom and work upwards
        //the physical pivot is at the bottom of the segment, no length here?
        //unless calculating influence on other vectors
        intermediateInfluenceFK = baseFK;

        //the physical pivot is at the bottom of the segment, no length here?
        intermediateFk += new Vector3(0f, 0f, 0);

        intermediateInfluenceFK = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, intermediateInfluenceFK);
        intermediateInfluenceFK = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, intermediateInfluenceFK);

        intermediateInfluenceFK += new Vector3(0f, 0f, -segLength);

        intermediateInfluenceFK = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, intermediateInfluenceFK);
        intermediateInfluenceFK = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, intermediateInfluenceFK);

        intermediateInfluenceFK += new Vector3(0f, 0f, -segLength);

        intermediateInfluenceFK = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, intermediateInfluenceFK);
        intermediateInfluenceFK = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, intermediateInfluenceFK);

        intermediateInfluenceFK += new Vector3(0f, 0f, -segLength);

        intermediateInfluenceFK = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, intermediateInfluenceFK);
        intermediateInfluenceFK = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, intermediateInfluenceFK);

        intermediateInfluenceFK += new Vector3(0f, 0f, -segLength);

        intermediateInfluenceFK = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, intermediateInfluenceFK);
        intermediateInfluenceFK = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, intermediateInfluenceFK);

        intermediateInfluenceFK += new Vector3(0f, 0f, -segLength);



        //following is for dealing with upper segment, only 1 segment for now
        intermediateInfluenceFK += new Vector3(0f, 0f, -middleBlockLength);
        intermediateInfluenceFK = Geometry.rotateAroundXAxis(segUXAngle * Mathf.Deg2Rad, intermediateInfluenceFK);


        //single upper segment
        intermediateInfluenceFK += new Vector3(0f, 0f, -segLength);


        //top block, this keeps the Segments Component 0,0,0 in terms of Machine space.
        intermediateInfluenceFK += new Vector3(0f, 0f, -topBlockLength);


        return intermediateInfluenceFK;

    }
   

    void limitInputs()
    {


        if (inputX < -range)
            inputX = -range;
        if (inputX > range)
            inputX = range;


        if (inputZ < -range)
            inputZ = -range;
        if (inputZ > range)
            inputZ = range;


        if (inputUpperX < -range)
            inputUpperX = -range;
        if (inputUpperX > range)
            inputUpperX = range;

    }

    public Geometry.GeoPacket doIk(float x, float z)
    {


        inputX = x;
        inputZ = z;

        limitInputs();

        calcFK();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }
    public Geometry.GeoPacket doIk(float x, float z, float ux)
    {


        inputX = x;
        inputZ = z;

        inputUpperX = ux;

        limitInputs();

        calcFK();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }
    public Geometry.GeoPacket doAdditiveIk(float x, float z)
    {


        inputX += x;
        inputZ += z;


        limitInputs();

        Debug.Log("segs RL: " + inputX.ToString() + ":" + inputZ.ToString());

        calcFK();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }


    //can't multiply magnitude of rotation, have to iterate through all seperate rotations
    //probably because it suffers similiar consequence as rotation order
    void calcFK()
    {

        //topBlockLength + (1 * segLength) + middleBlockLength + (5 * segLength);

        float segLXAngle = inputX;
        float segLZAngle = inputZ;

        float segUXAngle = inputUpperX;

        //start at the bottom and work upwards
        intermediateFk = new Vector3(0f, 0f, -bottomBlockLength);

        //the physical pivot is at the bottom of the segment, no length here?
        intermediateFk += new Vector3(0f, 0f, 0);

        intermediateFk = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, intermediateFk);
        intermediateFk = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, intermediateFk);

        //unless calculating influence on other vectors
        intermediateFk += new Vector3(0f, 0f, -segLength);

        intermediateFk = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, intermediateFk);
        intermediateFk = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, intermediateFk);

        intermediateFk += new Vector3(0f, 0f, -segLength);

        intermediateFk = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, intermediateFk);
        intermediateFk = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, intermediateFk);

        intermediateFk += new Vector3(0f, 0f, -segLength);

        intermediateFk = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, intermediateFk);
        intermediateFk = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, intermediateFk);

        intermediateFk += new Vector3(0f, 0f, -segLength);

        intermediateFk = Geometry.rotateAroundYAxis(segLZAngle * Mathf.Deg2Rad, intermediateFk);
        intermediateFk = Geometry.rotateAroundXAxis(segLXAngle * Mathf.Deg2Rad, intermediateFk);

        intermediateFk += new Vector3(0f, 0f, -segLength);



        //following is for dealing with upper segment, only 1 segment for now
        intermediateFk += new Vector3(0f, 0f, -middleBlockLength);
        intermediateFk = Geometry.rotateAroundXAxis(segUXAngle * Mathf.Deg2Rad, intermediateFk);


        //single upper segment
        intermediateFk += new Vector3(0f, 0f, -segLength);


        //top block, this keeps the Segments Component 0,0,0 in terms of Machine space.
        intermediateFk += new Vector3(0f, 0f, -topBlockLength);

      

        lastValidIk = new Vector4(inputX, 0f, inputZ, inputUpperX) * magToAct;

        lastValidFk = intermediateFk;


    }



}
