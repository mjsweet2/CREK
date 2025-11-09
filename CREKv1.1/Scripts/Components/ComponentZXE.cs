/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//uses linear albegra
// cos(angle) = dot(u,v) / (u.magnitude)*(v.magnitude)
//  these store ik as rotation (1.0 = 2*pi radians) because thats what I send down the line
public class ComponentZXE : MonoBehaviour
{

    
    public bool isSingle;
    public bool doClean;

    public enum YELLOWZONE { GREEN, YELLOWOUTLOW, YELLOWOUTHIGH,YELLOWINLOW, YELLOWINHIGH };// playing with logic machine to keep the arm from flipping around
    public YELLOWZONE yellowZone;
    public float zRotationYellowZoneTest; //z axis angle
    public float zAxisMin;
    public float zAxisMagnet;// area where I just zero out the z axis near the middle for upper to lower movements


    public enum SIDE { LEFT, RIGHT };
    public SIDE side;

    public string compName; // self.name = "base_class"     # for logging
    public Vector3 lastValidIk;   // self.last_valid_ik = [0.0,0.0,0.0] # same as current # The Component is storing state, so I need a seperate component for each
    public Vector3 lastValidFk;   // self.last_valid_fk =  [0.0,0.0,0.0]
    public Vector3 inputIk;     // used for fk calcs, not used for ik calcs,
    public Vector3 outputFk;    // used for fk calcs, not used for ik calcs,
    //public bool isMotioning;     // self.is_motioning = False # is this component running a component trajectory 

    public Vector3 intermediateEndEffector;

    public float upper;
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
    public Vector3 previousValidIk;


    public Transform targetFk;
    public ArticulatedZXE phys;

    public Vector3 intermediateA;
    public Vector3 intermediateB;

    public Transform markerA, markerB;

    public Vector3 wV01, wV02, wV03, wV04;

    public Geometry.GeoPacket actuation;
    void Start()
    {
  

    }

    // Update is called once per frame
    void Update()
    {
        if(isSingle)
        {
            doIkOnTargetFk();
        }
        
    }

    public void doFkOnInputIk()
    {
        if(side == SIDE.LEFT)
            outputFk = calcLeftFkOnIks();
        else
            outputFk = calcRightFkOnIks();

        targetFk.position = outputFk;

        
    }

    Vector3 calcLeftFkOnIks()
    {

        Vector3 runningPoint = Vector3.zero;

        runningPoint += new Vector3(0f, -lower, 0f);
        runningPoint = Geometry.rotateAroundXAxis(inputIk.z * 2*Mathf.PI, runningPoint);
        runningPoint += new Vector3(0f, -upper, 0f);
        runningPoint = Geometry.rotateAroundXAxis(inputIk.y * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundZAxis(inputIk.x * 2 * Mathf.PI, runningPoint);


        return runningPoint;

    }
    Vector3 calcRightFkOnIks()
    {

        Vector3 runningPoint = Vector3.zero;

        runningPoint += new Vector3(0f, -lower, 0f);
        runningPoint = Geometry.rotateAroundXAxis(-inputIk.z * 2 * Mathf.PI, runningPoint);
        runningPoint += new Vector3(0f, -upper, 0f);
        runningPoint = Geometry.rotateAroundXAxis(-inputIk.y * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundZAxis(inputIk.x * 2 * Mathf.PI, runningPoint);


        return runningPoint;

    }



    public void doIkOnTargetFk()
    {

        doRightIk(targetFk.position.x, targetFk.position.y, targetFk.position.z);
        if(phys != null)
            phys.actuation = valIk();

    }
    public Vector3 valIk()
    {
        return lastValidIk;
    }
    public Vector3 valFk()
    {
        return lastValidFk;
    }
    public Vector3 dormantFk()
    {
        return new Vector3(0, -(upper + lower), 0);
    }

    public void loadParams(float u, float l)
    {
        upper = u;
        lower = l;
        lastValidFk = new Vector3(0, -(upper + lower), 0);
    }

    public Geometry.GeoPacket doRightIk(float x, float y, float z)
    {

        Vector3 requested = new Vector3(x, y, z);


        //if zero, set to non-zero
        if (requested.magnitude < .01)
        {
            Debug.Log("fk almost zero, adjusting: " + compName + ": " + requested.magnitude.ToString());
            requested = new Vector3(0f,.01f,0f);

        }


        //move requests less than .04 out to .04
        if (requested.magnitude < 0.04)
        {
            Debug.Log("fk too small, adjusting: " + compName + ": " + requested.magnitude.ToString());
            requested = requested * (.04f/requested.magnitude);

        }


       if(doClean)
        {
            requested = doSimpleClean(requested);
        }
    

        //I'm going to make the changes to this to keep an on going track the current location of the end effector
        intermediateEndEffector = Vector3.zero;

        Vector3 ik = Vector3.zero;

        //  [0] = a, [1] = b, [2] = c
        Geometry.GeoPacket radians = Geometry.anglesFromSides(requested.magnitude, upper, lower);

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
        intermediateEndEffector += new Vector3(0f, -upper, 0f);

        intermediateA = intermediateEndEffector;

        float zIntersectRadians = Mathf.Acos(requested.z / intermediateEndEffector.magnitude);
        Vector3 zIntersectPoint = Vector3.zero;
        zIntersectPoint.z = requested.z;
        zIntersectPoint.y = Mathf.Sin(zIntersectRadians) * intermediateEndEffector.magnitude;


        // if x = 0 and y = 0, the zIntersect.y = 0;
        if (float.IsNaN(zIntersectPoint.y))
            zIntersectPoint.y = 0;

        //I currently have a positive y, make it -y
        if(requested.y < 0)
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


        intermediateEndEffector = Geometry.rotateAroundXAxis(-ik.y*(2*Mathf.PI), intermediateEndEffector);

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



        if (markerA != null)
            markerA.position = intermediateA;

        if (markerB != null)
            markerB.position = intermediateB;


        //  save to send in case of bad ik.
        lastValidIk = ik;
        lastValidFk = new Vector3(x, y, z);
        previousValidIk = ik;
        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }
    public Geometry.GeoPacket doLeftIk(float x, float y, float z)
    {

        Vector3 requested = new Vector3(x, y, z);


        //if zero, set to non-zero
        if (requested.magnitude < .01)
        {
            Debug.Log("fk almost zero, adjusting: " + compName + ": " + requested.magnitude.ToString());
            requested = new Vector3(0f, .01f, 0f);

        }


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
        Geometry.GeoPacket radians = Geometry.anglesFromSides(requested.magnitude, upper, lower);

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
        intermediateEndEffector += new Vector3(0f, -upper, 0f);




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




        //ik = applyMaxVelocity(ik);

        //  save to send in case of bad ik.
        lastValidIk = ik;
        lastValidFk = new Vector3(x, y, z);
        previousValidIk = ik;
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
