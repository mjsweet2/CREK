/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentZXYE : MonoBehaviour
{


    public bool isSingle;
    public bool doClean;
    

    public bool testA, testB; 

    public enum YELLOWZONE { GREEN, YELLOWOUTLOW, YELLOWOUTHIGH, YELLOWINLOW, YELLOWINHIGH };// playing with logic machine to keep the arm from flipping around
    public YELLOWZONE yellowZone;
    public float zRotationYellowZoneTest; //z axis angle
    public float zAxisMin;
    public float zAxisMagnet;// area where I just zero out the z axis near the middle for upper to lower movements


    public enum SIDE { LEFT, RIGHT };
    public SIDE side;

    public enum IKMODE { ZXE, ZXYE, DIRECTIONPAIR };
    public IKMODE ikMode;

    public enum XCYCLECENTER { DOWN, UPPOSITIVE, UPNEGATIVE };
    public XCYCLECENTER xCycleCenter;

    public string compName; // self.name = "base_class"     # for logging
    public Vector4 lastValidIk;   // self.last_valid_ik = [0.0,0.0,0.0] # same as current # The Component is storing state, so I need a seperate component for each
    public Vector3 lastValidFk;   // self.last_valid_fk =  [0.0,0.0,0.0]

    public Vector4 ikLow; //used for RL Skill range clamp
    public Vector4 ikHigh; //used for RL Skill range clamp

    //component space dir vectors that desciber upper(zx) and lower(ye) arms
    //this component translates lDir to elbow space before calcing lower.
    //this component works in ik mode zxe or zxe+y, or it works in dir pair mode
    //where it takes 2 component space directions
    public Vector3 uDir;        //component space direction vector of upper component
    public Vector3 lDir;        //component space direction vector of lower component  
    public Vector3 lDirESpace;  // lDir translated into elbow space


    public Vector3 intermediateEndEffector;

    public float upper;
    public float middle;
    public float lower;


    //max rotational velocity and acceleration
    //this implementation is at the engine, not at the device
    //these are axis velocity and acceleration
    //calc'ed from the ik
    public Vector3 maxVelocity;
    public Vector3 maxAcceleration;

    //valid fk tracking interfer swith previous frame calcuation
    // so I need seperate variables
    //updates in calc, not in do
    public Vector4 previousValidIk;

    public ArticulatedZXYE phys;
    public Transform inputA, inputB;

    public Vector3 intermediateA;
    public Vector3 intermediateB;

    //for troublesooting
    public Transform iA, iB, iC, ID, IE;

    //public Geometry.GeoPacket actuation;
    void Start()
    {

        //< position name = "lfymotor" joint = "lfz_joint" gear = "6" ctrlrange = "-4.8 4.8" kp = "25" kv = "0" /> < !--range is radians 3.14 / 2 = 1.57-- >         
        //< position name = "lfxmotor" joint = "lfx_joint" gear = "6" ctrlrange = "-14.2 14.2" kp = "25" kv = "0" /> < !--prev val - 9.5 9.5" -->
        //< position name = "lfzmotor" joint = "lfy_joint" gear = "6" ctrlrange = "-9.5 9.5" kp = "25" kv = "0" />                                  
        //< position name = "lfemotor" joint = "lfe_joint" gear = "6" ctrlrange = "-16.8 0" kp = "25" kv = "0" />


        //< position name = "rfymotor" joint = "rfz_joint" gear = "6" ctrlrange = "-4.8 4.8" kp = "25" kv = "0" /> < !--range is radians 3.14 / 2 = 1.57-- >                                                         
        //< position name = "rfxmotor" joint = "rfx_joint" gear = "6" ctrlrange = "-14.2 14.2" kp = "25" kv = "0" /> < !--prev val - 9.5 9.5" -->
        //< position name = "rfzmotor" joint = "rfy_joint" gear = "6" ctrlrange = "-9.5 9.5" kp = "25" kv = "0" />                                                                              
        //< position name = "rfemotor" joint = "rfe_joint" gear = "6" ctrlrange = "0 16.8" kp = "25" kv = "0" />

        if(side == SIDE.LEFT)
        {
            ikLow.x = -4.8f / (6f * 2f * Mathf.PI);
            ikLow.y = -14.2f / (6f * 2f * Mathf.PI);
            ikLow.w = -9.5f / (6f * 2f * Mathf.PI);
            ikLow.z = -16.8f / (6f * 2f * Mathf.PI);

            ikHigh.x = 4.8f / (6f * 2f * Mathf.PI);
            ikHigh.y = 14.2f / (6f * 2f * Mathf.PI);
            ikHigh.w = 9.5f / (6f * 2f * Mathf.PI);
            ikHigh.z = 0f / (6f * 2f * Mathf.PI);
        }
        else
        {
            ikLow.x = -4.8f / (6f * 2f * Mathf.PI);
            ikLow.y = -14.2f / (6f * 2f * Mathf.PI);
            ikLow.w = -9.5f / (6f * 2f * Mathf.PI);
            ikLow.z = 0f / (6f * 2f * Mathf.PI);

            ikHigh.x = 4.8f / (6f * 2f * Mathf.PI);
            ikHigh.y = 14.2f / (6f * 2f * Mathf.PI);
            ikHigh.w = 9.5f / (6f * 2f * Mathf.PI);
            ikHigh.z = 16.8f / (6f * 2f * Mathf.PI);

        }


    }

    // Update is called once per frame
    void Update()
    {
        if (isSingle)
        {
            ;
            //if (ikMode == IKMODE.DIRECTIONPAIR)
            //{
            //    doUpperDirOnInputB();
            //    doLowerDirOnInputA();
            //    
            //}
            //else
            //{
            //    doIkOnInputA();
            //}

        }

    }
   
    void clampIK()
    {

        //< position name = "lfymotor" joint = "lfz_joint" gear = "6" ctrlrange = "-4.8 4.8" kp = "25" kv = "0" /> < !--range is radians 3.14 / 2 = 1.57-- >         
        //< position name = "lfxmotor" joint = "lfx_joint" gear = "6" ctrlrange = "-14.2 14.2" kp = "25" kv = "0" /> < !--prev val - 9.5 9.5" -->
        //< position name = "lfzmotor" joint = "lfy_joint" gear = "6" ctrlrange = "-9.5 9.5" kp = "25" kv = "0" />                                  
        //< position name = "lfemotor" joint = "lfe_joint" gear = "6" ctrlrange = "-16.8 0" kp = "25" kv = "0" />


        //< position name = "rfymotor" joint = "rfz_joint" gear = "6" ctrlrange = "-4.8 4.8" kp = "25" kv = "0" /> < !--range is radians 3.14 / 2 = 1.57-- >                                                         
        //< position name = "rfxmotor" joint = "rfx_joint" gear = "6" ctrlrange = "-14.2 14.2" kp = "25" kv = "0" /> < !--prev val - 9.5 9.5" -->
        //< position name = "rfzmotor" joint = "rfy_joint" gear = "6" ctrlrange = "-9.5 9.5" kp = "25" kv = "0" />                                                                              
        //< position name = "rfemotor" joint = "rfe_joint" gear = "6" ctrlrange = "0 16.8" kp = "25" kv = "0" />

        if (lastValidIk.x < ikLow.x)
            lastValidIk.x = ikLow.x;
        if (lastValidIk.x > ikHigh.x)
            lastValidIk.x = ikHigh.x;

        if (lastValidIk.y < ikLow.y)
            lastValidIk.y = ikLow.y;
        if (lastValidIk.y > ikHigh.y)
            lastValidIk.y = ikHigh.y;

        if (lastValidIk.w < ikLow.w)
            lastValidIk.w = ikLow.w;
        if (lastValidIk.w > ikHigh.w)
            lastValidIk.w = ikHigh.w;

        if (lastValidIk.z < ikLow.z)
            lastValidIk.z = ikLow.z;
        if (lastValidIk.z > ikHigh.z)
            lastValidIk.z = ikHigh.z;



    }
    public Vector3 calcLeftFkOnIks()
    {

        Vector3 runningPoint = Vector3.zero;

        runningPoint += new Vector3(0f, -lower, 0f);
        runningPoint = Geometry.rotateAroundXAxis(lastValidIk.z * 2 * Mathf.PI, runningPoint);
        runningPoint += new Vector3(0f, -middle, 0f);
        runningPoint = Geometry.rotateAroundYAxis(lastValidIk.w * 2 * Mathf.PI, runningPoint);
        runningPoint += new Vector3(0f, -upper, 0f);
        runningPoint = Geometry.rotateAroundXAxis(lastValidIk.y * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundZAxis(lastValidIk.x * 2 * Mathf.PI, runningPoint);


        return runningPoint;

    }
    public Vector3 calcRightFkOnIks()
    {

        Vector3 runningPoint = Vector3.zero;

        runningPoint += new Vector3(0f, -lower, 0f);
        runningPoint = Geometry.rotateAroundXAxis(-lastValidIk.z * 2 * Mathf.PI, runningPoint);
        runningPoint += new Vector3(0f, -middle, 0f);
        runningPoint = Geometry.rotateAroundYAxis(-lastValidIk.w * 2 * Mathf.PI, runningPoint);
        runningPoint += new Vector3(0f, -upper, 0f);
        runningPoint = Geometry.rotateAroundXAxis(-lastValidIk.y * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundZAxis(lastValidIk.x * 2 * Mathf.PI, runningPoint);


        return runningPoint;

    }

    public Vector3 calcLeftLDirESpaceOnIks()
    {

        Vector3 runningPoint = Vector3.zero;

        runningPoint += new Vector3(0f, -lower, 0f);
        runningPoint = Geometry.rotateAroundXAxis(lastValidIk.z * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundYAxis(lastValidIk.w * 2 * Mathf.PI, runningPoint);

        runningPoint.Normalize();

        return runningPoint;
    }

    public Vector3 calcLeftLDirOnIks()
    {
        Vector3 runningPoint = Vector3.zero;

        runningPoint += new Vector3(0f, -lower, 0f);
        runningPoint = Geometry.rotateAroundXAxis(lastValidIk.z * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundYAxis(lastValidIk.w * 2 * Mathf.PI, runningPoint);

        runningPoint = Geometry.rotateAroundXAxis(lastValidIk.y * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundZAxis(lastValidIk.x * 2 * Mathf.PI, runningPoint);

        runningPoint.Normalize();

        return runningPoint;
    }

    public Vector3 calcLeftUDirOnIks()
    {
        Vector3 runningPoint = Vector3.zero;

        //dir vectors are normalized
        runningPoint += new Vector3(0f, -1f, 0f);
        runningPoint = Geometry.rotateAroundXAxis(lastValidIk.y * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundZAxis(lastValidIk.x * 2 * Mathf.PI, runningPoint);


        return runningPoint;
    }

    public Vector3 calcRightLDirESpaceOnIks()
    {
        Vector3 runningPoint = Vector3.zero;

        runningPoint += new Vector3(0f, -lower, 0f);
        runningPoint = Geometry.rotateAroundXAxis(-lastValidIk.z * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundYAxis(-lastValidIk.w * 2 * Mathf.PI, runningPoint);

        runningPoint.Normalize();

        return runningPoint;
    }
    public Vector3 calcRightLDirOnIks()
    {

        Vector3 runningPoint = Vector3.zero;

        runningPoint += new Vector3(0f, -lower, 0f);
        runningPoint = Geometry.rotateAroundXAxis(-lastValidIk.z * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundYAxis(-lastValidIk.w * 2 * Mathf.PI, runningPoint);

        runningPoint = Geometry.rotateAroundXAxis(-lastValidIk.y * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundZAxis(lastValidIk.x * 2 * Mathf.PI, runningPoint);

        runningPoint.Normalize();

        return runningPoint;
    }

    public Vector3 calcRightUDirOnIks()
    {
        
        Vector3 runningPoint = Vector3.zero;

        //dir vectors are normalized
        runningPoint += new Vector3(0f, -1f, 0f);
        runningPoint = Geometry.rotateAroundXAxis(-lastValidIk.y * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundZAxis(lastValidIk.x * 2 * Mathf.PI, runningPoint);


        return runningPoint;
    }

    public void doIkOnInputA()
    {

        if(side == SIDE.LEFT)
        {
            doLeftIk(inputA.position.x, inputA.position.y, inputA.position.z);
        }
        else
        {
            doRightIk(inputA.position.x, inputA.position.y, inputA.position.z);
        }
        
        if (phys != null)
            phys.actuation = valIk();

    }

    public Vector4 valIk() { return lastValidIk; }
    public Vector3 valFk() { return lastValidFk; }

    public Vector3 lDirVector() { return lDir; }
    public Vector3 uDirVector() { return uDir; }
    public Vector3 lDirVectorESpace() { return lDirESpace; }

    public Vector3 dormantFk() { return new Vector3(0, -(upper + middle + lower), 0); }

    public void loadParams(float u, float m, float l)
    {
        upper = u;
        middle = m;
        lower = l;

        lastValidFk = new Vector3(0, -(upper + middle + lower), 0);
    }
    public void doUpperDirOnInputB()
    {
        if (side == SIDE.LEFT)
        {
            doLeftUpperDir(inputB.position);
        }
        else
        {
            doRightUpperDir(inputB.position);
        }

        if (phys != null)
            phys.actuation = valIk();
     
    }

    public void doLowerDirOnInputA()
    {

        if (side == SIDE.LEFT)
        {
            doLeftLowerDir(inputA.position);
        }
        else
        {
            doRightLowerDir(inputA.position);
        }

        if (phys != null)
            phys.actuation = valIk();

    }

    public Geometry.GeoPacket doLeftUpperDir(Vector3 u)
    {
        uDir = u / u.magnitude;

        float xAdj = new Vector2(uDir.x, uDir.y).magnitude;

        lastValidIk.y = Mathf.Acos(xAdj) / (Mathf.PI * 2f);

        if(uDir.z > 0f)
        {
            lastValidIk.y = -lastValidIk.y;
        }

        //this work as long as I don't pas throught the y > 0 line, it snaps
        if (uDir.y > 0f)
        {
            if (uDir.z > 0f)
            {
                lastValidIk.y = -lastValidIk.y - .5f;
            }
            else
            {
                lastValidIk.y = .5f - lastValidIk.y;
            }

        }


        lastValidIk.x = Mathf.Atan(uDir.x / uDir.y) / (Mathf.PI * 2f);
        lastValidIk.x = -lastValidIk.x;


        /*
        if (uDir.y >= 0f)
        {
            if (uDir.x < 0f)
            {
                lastValidIk.x = lastValidIk.x - .5f;
            }
            else
            {
                lastValidIk.x = .5f + lastValidIk.x;
            }
            
        }
        */


        lastValidFk = calcLeftFkOnIks();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }

    public Geometry.GeoPacket doLeftLowerDir(Vector3 l)
    {
        
        lDir = lDirESpace = l / l.magnitude;

        //convert the input to elbow space
        //reverse rotations in the reverse order.
        lDirESpace = Geometry.rotateAroundZAxis(-lastValidIk.x * (2f * Mathf.PI), lDirESpace);
        lDirESpace = Geometry.rotateAroundXAxis( -lastValidIk.y * (2f * Mathf.PI), lDirESpace);
        
    

        //don't bend elbow backwards
        if (lDirESpace.z < 0f)
            lDirESpace.z = 0;

        lastValidIk.w = Mathf.Atan(lDirESpace.x / lDirESpace.z) / (Mathf.PI * 2f);
        lastValidIk.w = -lastValidIk.w;

        float eAdj = new Vector2(lDirESpace.x, lDirESpace.z).magnitude;

        lastValidIk.z = (( Mathf.PI /2 ) - Mathf.Acos(eAdj)) / (Mathf.PI * 2f);

        lastValidIk.z = -lastValidIk.z;
        if(lDirESpace.y >= 0f)
        {
            lastValidIk.z = -.5f - lastValidIk.z;
        }


        lastValidFk = calcLeftFkOnIks();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }

    public Geometry.GeoPacket doRightUpperDir(Vector3 u)
    {
        uDir = u / u.magnitude;

        float xAdj = new Vector2(uDir.x, uDir.y).magnitude;

        lastValidIk.y = Mathf.Acos(xAdj) / (Mathf.PI * 2f);

        if (uDir.z > 0f)
        {
            lastValidIk.y = -lastValidIk.y;
        }
        //this work as long as I don't pass throught the y > 0 line, it snaps
        if (uDir.y > 0f)
        {
            if (uDir.z > 0f)
            {
                lastValidIk.y = -lastValidIk.y - .5f;
            }
            else
            {
                lastValidIk.y = .5f - lastValidIk.y;
            }

        }

        lastValidIk.y = -lastValidIk.y;


        lastValidIk.x = Mathf.Atan(uDir.x / uDir.y) / (Mathf.PI * 2f);
        lastValidIk.x = -lastValidIk.x;

        /*
        if (uDir.y >= 0f)
        {
            if (uDir.x < 0f)
            {
                lastValidIk.x = lastValidIk.x - .5f;
            }
            else
            {
                lastValidIk.x = .5f + lastValidIk.x;
            }

        }
        */


        lastValidFk = calcRightFkOnIks();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }

    public Geometry.GeoPacket doRightLowerDir(Vector3 l)
    {
     
        lDir = lDirESpace = l / l.magnitude;

        //convert the input to elbow space
        //reverse rotations in the reverse order.
        lDirESpace = Geometry.rotateAroundZAxis(-lastValidIk.x * (2f * Mathf.PI), lDirESpace);
        lDirESpace = Geometry.rotateAroundXAxis(lastValidIk.y * (2f * Mathf.PI), lDirESpace);
      

        //don't bend elbow backwards
        if (lDirESpace.z < 0f)
            lDirESpace.z = 0;

        lastValidIk.w = Mathf.Atan(lDirESpace.x / lDirESpace.z) / (Mathf.PI * 2f);
      

        float eAdj = new Vector2(lDirESpace.x, lDirESpace.z).magnitude;

        lastValidIk.z = ((Mathf.PI / 2) - Mathf.Acos(eAdj)) / (Mathf.PI * 2f);

        if (lDirESpace.y >= 0f)
        {
            lastValidIk.z = .5f - lastValidIk.z;
        }

        lastValidFk = calcRightFkOnIks();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }

    //this sets the axis values directly, for RL Skills
    public Geometry.GeoPacket doDirectRight(float x, float y, float z, float w)
    {

        Vector4 requestedIK = new Vector4(x, y, z, w);

        
        lastValidIk += requestedIK;
        

        clampIK();
        previousValidIk = lastValidIk;

        //update other input channel data
        lDir = calcRightLDirOnIks();
        lDirESpace = calcRightLDirESpaceOnIks();
        uDir = calcRightUDirOnIks();

        
        // do fk calcs
        lastValidFk = calcRightFkOnIks();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }

    public Geometry.GeoPacket doDirectLeft(float x, float y, float z, float w)
    {

        Vector4 requestedIK = new Vector4(x, y, z, w);

        
        lastValidIk += requestedIK;
        

        clampIK();
        previousValidIk = lastValidIk;

        //update other input channel data
        lDir = calcLeftLDirOnIks();
        lDirESpace = calcLeftLDirESpaceOnIks();
        uDir = calcLeftUDirOnIks();


        // do fk calcs
        lastValidFk = calcLeftFkOnIks();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }

    public Geometry.GeoPacket doRightIk(float x, float y, float z)
    {

        Vector3 requested = new Vector3(x, y, z);



        //move requests less than .04 out to .04
        if (requested.magnitude < 0.04)
        {
            Debug.Log("fk too small, adjusting: " + compName + ": " + requested.magnitude.ToString());
            requested = requested * (.04f / requested.magnitude);

        }


        if (doClean)
        {
            requested = doSimpleClean(requested);
        }


        //I'm going to make the changes to this to keep an on going track the current location of the end effector
        intermediateEndEffector = Vector3.zero;

        Vector3 ik = Vector3.zero;

        //  [0] = a, [1] = b, [2] = c
        Geometry.GeoPacket radians = Geometry.anglesFromSides(requested.magnitude, (upper + middle), lower);

        if (!radians.valid)
        {

            Debug.Log("bad angles_from_sides: " + compName + ": " + requested.magnitude.ToString() + ":" + upper.ToString() + ":" + lower.ToString());
            return new Geometry.GeoPacket(false, valIk());
        }


        //ik.z calculations - > no change from xze
        ik.z = 0.5f - (radians.vector.x / (2 * Mathf.PI));



        //update the End Effector intermediate as we go
        //rotate the lower bone
        intermediateEndEffector = Geometry.rotateAroundXAxis(-(Mathf.PI - radians.vector.x), new Vector3(0f, -lower, 0f));
        //add result to upper bone in dormant position
        intermediateEndEffector += new Vector3(0f, -(upper + middle), 0f);

        intermediateA = intermediateEndEffector;

        float zIntersectRadians = Mathf.Acos(requested.z / intermediateEndEffector.magnitude);
        Vector3 zIntersectPoint = Vector3.zero;
        zIntersectPoint.z = requested.z;
        zIntersectPoint.y = Mathf.Sin(zIntersectRadians) * intermediateEndEffector.magnitude;


        // if x = 0 and y = 0, the zIntersect.y = 0;
        if (float.IsNaN(zIntersectPoint.y))
            zIntersectPoint.y = 0;

        //I currently have a positive y, make it -y
        if (requested.y < 0)
            zIntersectPoint.y = -zIntersectPoint.y;


        //ik.y calculations, shoulder x axis
        Vector2 tempU = new Vector2(zIntersectPoint.z, zIntersectPoint.y);
        Vector2 tempV = new Vector2(intermediateEndEffector.z, intermediateEndEffector.y);
        float xAngle = Geometry.angleBetweenVectors(tempU, tempV);



        //angle between vectors doesn't tell us direction, so the easiest thing
        //is to try both directions and test which is closer to our requested
        Vector3 oneWay = Geometry.rotateAroundXAxis(xAngle, intermediateEndEffector);
        Vector3 otherWay = Geometry.rotateAroundXAxis(-xAngle, intermediateEndEffector);


        // this direction doesn't seem to make sense, but because of the direction of
        // the actuator the direction can seem reversed

        if ((oneWay - zIntersectPoint).magnitude > (otherWay - zIntersectPoint).magnitude)
        {
            ;
        }
        else
        {
            xAngle = -xAngle;
        }



        ik.y = xAngle / (2 * Mathf.PI);


        intermediateEndEffector = Geometry.rotateAroundXAxis(-ik.y * (2 * Mathf.PI), intermediateEndEffector);

        intermediateB = intermediateEndEffector;

        //ik.x calculations, shoulder z axis
        Vector2 zTempU = new Vector2(requested.x, requested.y);
        Vector2 zTempV = new Vector2(intermediateEndEffector.x, intermediateEndEffector.y);
        float zAngle = Geometry.angleBetweenVectors(zTempU, zTempV);

        //angle between vectors doesn't tell us direction, so the easiest thing
        //is to try both directions and test which is closer to our requesteds
        Vector3 zOneWay = Geometry.rotateAroundZAxis(zAngle, intermediateEndEffector);
        Vector3 zOtherWay = Geometry.rotateAroundZAxis(-zAngle, intermediateEndEffector);


        // this direction doesn't seem to make sense, but because of the direction of
        // the actuator the direction can seem reversed
        if ((zOneWay - requested).magnitude < (zOtherWay - requested).magnitude)
        {
            ;
        }
        else
        {
            zAngle = -zAngle;
        }


        ik.x = zAngle / (2 * Mathf.PI);
        ik.x = -ik.x;

        //selectively move x axis cycle point, z = 0, y > 0 to z = 0, y < 0
        if (xCycleCenter == XCYCLECENTER.UPPOSITIVE)
        {
            if (ik.y < 0f)
                ik.y += 1f;
        }
        if (xCycleCenter == XCYCLECENTER.UPNEGATIVE)
        {
            if (ik.y > 0f)
                ik.y -= 1f;
        }


        if (iA != null)
            iA.position = intermediateA;

        if (iB != null)
            iB.position = intermediateB;


        //  save to send in case of bad ik.
        lastValidIk = ik;
        lastValidFk = new Vector3(x, y, z);
        previousValidIk = ik;

        //update other input channel data
        lDir = calcRightLDirOnIks();
        lDirESpace = calcRightLDirESpaceOnIks();
        uDir = calcRightUDirOnIks();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }
    public Geometry.GeoPacket doLeftIk(float x, float y, float z)
    {

        Vector3 requested = new Vector3(x, y, z);




        //move requests less than .04 out to .04
        if (requested.magnitude < 0.04)
        {
            Debug.Log("fk too small, adjusting: " + compName + ": " + requested.magnitude.ToString());
            requested = requested * (.04f / requested.magnitude);

        }


        if (doClean)
        {
            requested = doSimpleClean(requested);
        }


        //I'm going to make the changes to this to keep an on going track the current location of the end effector
        intermediateEndEffector = Vector3.zero;

        Vector3 ik = Vector3.zero;

      
        Geometry.GeoPacket radians = Geometry.anglesFromSides(requested.magnitude, (upper + middle), lower);

        if (!radians.valid)
        {

            Debug.Log("bad angles_from_sides: " + compName + ": " + requested.magnitude.ToString() + ":" + upper.ToString() + ":" + lower.ToString());
            return new Geometry.GeoPacket(false, valIk());
        }


        //ik.z calculations - > no change from xze
        ik.z = 0.5f - (radians.vector.x / (2 * Mathf.PI));
        ik.z = -ik.z;

        //update the End Effector intermediate as we go
        //rotate the lower bone
        intermediateEndEffector = Geometry.rotateAroundXAxis(-(Mathf.PI - radians.vector.x), new Vector3(0f, -lower, 0f));
        //add result to upper bone in dormant position
        intermediateEndEffector += new Vector3(0f, -(upper + middle), 0f);





        float zIntersectRadians = Mathf.Acos(requested.z / intermediateEndEffector.magnitude);
        Vector3 zIntersectPoint = Vector3.zero;
        zIntersectPoint.z = requested.z;
        zIntersectPoint.y = Mathf.Sin(zIntersectRadians) * intermediateEndEffector.magnitude;


        // if x = 0 and y = 0, the zIntersect.y = 0;
        if (float.IsNaN(zIntersectPoint.y))
            zIntersectPoint.y = 0;
        //I currently have a positive y
        //if my requested.y > 0 keep it
        //if my requested.y < 0 negate it
        if (requested.y < 0f)
            zIntersectPoint.y = -zIntersectPoint.y;


        //ik.y calculations, shoulder x axis
        Vector2 tempU = new Vector2(zIntersectPoint.z, zIntersectPoint.y);
        Vector2 tempV = new Vector2(intermediateEndEffector.z, intermediateEndEffector.y);
        float xAngle = Geometry.angleBetweenVectors(tempU, tempV);



        //angle between vectors doesn't tell us direction, so the easiest thing
        //is to try both directions and test which is closer to our requested
        Vector3 oneWay = Geometry.rotateAroundXAxis(xAngle, intermediateEndEffector);
        Vector3 otherWay = Geometry.rotateAroundXAxis(-xAngle, intermediateEndEffector);



        // this direction doesn't seem to make sense, but because of the direction of
        // the actuator the direction can seem reversed
        //
        //if((oneWay.z - requested.z) > (otherWay.z - requested.z))
        if ((oneWay - zIntersectPoint).magnitude < (otherWay - zIntersectPoint).magnitude)
        {
            ;
        }
        else
        {
            xAngle = -xAngle;
        }

        ik.y = xAngle / (2 * Mathf.PI);


        intermediateEndEffector = Geometry.rotateAroundXAxis(xAngle, intermediateEndEffector);


        intermediateA = intermediateEndEffector;




        //ik.x calculations, shoulder x axis
        Vector2 zTempU = new Vector2(requested.x, requested.y);
        Vector2 zTempV = new Vector2(intermediateEndEffector.x, intermediateEndEffector.y);
        float zAngle = Geometry.angleBetweenVectors(zTempU, zTempV);

        //angle between vectors doesn't tell us direction, so the easiest thing
        //is to try both directions and test which is closer to our requesteds
        Vector3 zOneWay = Geometry.rotateAroundZAxis(zAngle, intermediateEndEffector);
        Vector3 zOtherWay = Geometry.rotateAroundZAxis(-zAngle, intermediateEndEffector);


        // this direction doesn't seem to make sense, but because of the direction of
        // the actuator the direction can seem reversed
        if ((zOneWay - requested).magnitude < (zOtherWay - requested).magnitude)
        {
            ;
        }
        else
        {
            zAngle = -zAngle;
        }

        ik.x = zAngle / (2 * Mathf.PI);
        ik.x = -ik.x;


        //selectively move x axis cycle point, z = 0, y > 0 to z = 0, y < 0
        if (xCycleCenter == XCYCLECENTER.UPPOSITIVE)
        {
            if(ik.y < 0f)
                ik.y += 1f;
        }
        if (xCycleCenter == XCYCLECENTER.UPNEGATIVE)
        {
            if (ik.y > 0f)
                ik.y -= 1f;
        }




        //ik = applyMaxVelocity(ik);

        //  save to send in case of bad ik.
        lastValidIk = ik;
        lastValidFk = new Vector3(x, y, z);
        previousValidIk = ik;

        //update other input channel data
        lDir = calcLeftLDirOnIks();
        lDirESpace = calcLeftLDirESpaceOnIks();
        uDir = calcLeftUDirOnIks();

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }
    public Vector3 doSimpleClean(Vector3 vInput)
    {
        //Clean fk request
        //1. see if we're switching from above to below.
        //   test if you are very close to x = 0, y = 0, this is the z axis, and it's flippy.
        //   look in a thin rectangulare box, magnet the value to x = 0, so I can cross y = 0 safely,


        Vector3 vReturn = vInput;

        float yMargin = 0.008f;
        float xMargin = 0.0005f;

        if (0f <= vReturn.y && vReturn.y < yMargin)
        {

            vReturn.y = yMargin;

            if (0f <= vReturn.x && vReturn.x < xMargin)
            {
                vReturn.x = 0f;
            }
            if (-xMargin < vReturn.x && vReturn.x < 0f)
            {
                vReturn.x = 0f;
            }

        }
        if (-yMargin < vReturn.y && vReturn.y < 0f)
        {
            vReturn.y = -yMargin;

            if (0f <= vReturn.x && vReturn.x < xMargin)
            {
                vReturn.x = 0f;
            }
            if (-xMargin < vReturn.x && vReturn.x < 0f)
            {
                vReturn.x = 0f;
            }

        }


        return vReturn;
    }
    public Vector3 doRightCleanNotWorking(Vector3 vInput)
    {

        //Clean fk request
        //1. see if we're switching from above to below.
        //   test if you are very close to x = 0, y = 0, this is the z axis, and it's flippy.
        //   look in a thin rectangulare box, magnet the value to x = 0, so I can cross y = 0 safely,

        //2. except for step 1, keep the rest of fk requests away from the z axis, about 4 cm cylinder
        //   extending the entire z axis

        //3. keep the z axis rotations above a minimum, and don't let fk requests cross the y = 0 plane
        //   unless they go to x = 0. This is the "yellow zone test"

        //TODO limit on max z axis rotation, to prevent flipping at the y=0 line
        //if an out of bounds request is made, adjust the requested to the point closest at the bounds
        //also if a request is made on the other side of the bounds, stay on this side of the bounds
        //??use yellowZone.YELLOWOUTLOW, etc to track zone crossing, I can through the x = 0, y = 0 line
        // x = 0, y = 0 is green, it takes me out of the yellow zone
        zRotationYellowZoneTest = Mathf.Abs(Mathf.Atan(vInput.y / vInput.x) * Mathf.Rad2Deg);


        if (yellowZone == YELLOWZONE.YELLOWOUTLOW || yellowZone == YELLOWZONE.YELLOWINLOW)
        {
            if (zRotationYellowZoneTest < 30f)
            {
                vInput.y = -Mathf.Abs(Mathf.Tan(30f * Mathf.Deg2Rad) * vInput.x);
                //requested.x = Mathf.Abs(requested.x);//force x to be in original quadrants
            }
            else
            {
                if (vInput.y >= 0)
                    vInput.y = -Mathf.Abs(Mathf.Tan(30f * Mathf.Deg2Rad) * vInput.x);
                else
                    yellowZone = YELLOWZONE.GREEN;
            }

        }
        if (yellowZone == YELLOWZONE.YELLOWOUTHIGH || yellowZone == YELLOWZONE.YELLOWINHIGH)
        {
            if (zRotationYellowZoneTest < 30f)
            {
                vInput.y = Mathf.Abs(Mathf.Tan(30f * Mathf.Deg2Rad) * vInput.x);
                //requested.x = Mathf.Abs(requested.x);//force x to be in original quadrants

            }
            else
            {
                if (vInput.y < 0)
                    vInput.y = Mathf.Abs(Mathf.Tan(30f * Mathf.Deg2Rad) * vInput.x);
                else
                    yellowZone = YELLOWZONE.GREEN;
            }

        }

        if (yellowZone == YELLOWZONE.GREEN)
        {
            if (zRotationYellowZoneTest < 30f)
            {
                if (vInput.x >= 0 && vInput.y < 0)
                {
                    yellowZone = YELLOWZONE.YELLOWOUTLOW;
                    vInput.y = -Mathf.Abs(Mathf.Tan(30f * Mathf.Deg2Rad) * vInput.x);
                }
                if (vInput.x >= 0 && vInput.y >= 0)
                {
                    yellowZone = YELLOWZONE.YELLOWOUTHIGH;
                    vInput.y = Mathf.Abs(Mathf.Tan(30f * Mathf.Deg2Rad) * vInput.x);
                }
                if (vInput.x < 0 && vInput.y < 0)
                {
                    yellowZone = YELLOWZONE.YELLOWINLOW;
                    vInput.y = -Mathf.Abs(Mathf.Tan(30f * Mathf.Deg2Rad) * vInput.x);
                }
                if (vInput.x < 0 && vInput.y >= 0)
                {
                    yellowZone = YELLOWZONE.YELLOWINHIGH;
                    vInput.y = Mathf.Abs(Mathf.Tan(30f * Mathf.Deg2Rad) * vInput.x);
                }
            }

        }






        return vInput;
    }

    public Vector3 doLeftCleanNotWorking(Vector3 vInput)
    {
        //Clean fk request
        //1. see if we're switching from above to below.
        //   test if you are very close to x = 0, y = 0, this is the z axis, and it's flippy.
        //   look in a thin rectangulare box, magnet the value to x = 0, so I can cross y = 0 safely,

        //2. except for step 1, keep the rest of fk requests away from the z axis, about 4 cm cylinder
        //   extending the entire z axis

        //3. keep the z axis rotations above a minimum, and don't let fk requests cross the y = 0 plane
        //   unless they go to x = 0. This is the "yellow zone test"


        return vInput;
    }


}

