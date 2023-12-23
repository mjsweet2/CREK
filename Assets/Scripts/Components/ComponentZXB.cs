
/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentZXB : MonoBehaviour
{


    public bool isSingle;

    public enum YELLOWZONE { GREEN, YELLOWOUTLOW, YELLOWOUTHIGH, YELLOWINLOW, YELLOWINHIGH };// playing with logic machine to keep the arm from flipping around
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
    public float radius;


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
    public ArticulatedZXB phys;

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
        if (isSingle)
        {
            doIkOnTargetFk();
        }

    }

    public void doFkOnInputIk()
    {
        if (side == SIDE.LEFT)
            outputFk = calcLeftFkOnIks();
        else
            outputFk = calcRightFkOnIks();

        targetFk.position = outputFk;

    }

    Vector3 calcLeftFkOnIks()
    {

        Vector3 runningPoint = Vector3.zero;

        runningPoint += new Vector3(0f, -upper, 0f);
        runningPoint = Geometry.rotateAroundXAxis(inputIk.y * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundZAxis(inputIk.x * 2 * Mathf.PI, runningPoint);

        return runningPoint;

    }
    Vector3 calcRightFkOnIks()
    {

        Vector3 runningPoint = Vector3.zero;

        runningPoint += new Vector3(0f, -upper, 0f);
        runningPoint = Geometry.rotateAroundXAxis(-inputIk.y * 2 * Mathf.PI, runningPoint);
        runningPoint = Geometry.rotateAroundZAxis(inputIk.x * 2 * Mathf.PI, runningPoint);


        return runningPoint;

    }


    public void doIkOnTargetFk()
    {

        doRightIk(targetFk.position.x, targetFk.position.y, targetFk.position.z);
        if (phys != null)
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
        return new Vector3(0, -(upper), 0);
    }

    public void loadParams(float u, float r)
    {
        upper = u;
        radius = r;
        lastValidFk = new Vector3(0, -(upper), 0);
    }

    public Geometry.GeoPacket doBal(float z)
    {
        //remember to save as rotations not degrees
        //convert z velocity as angular velocity using radius and 60 fps
        float angular = z / (Mathf.PI * 2f * radius);

        lastValidIk.z += (angular / 60f);
        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request
    }

    public Geometry.GeoPacket doLeftIk(float x, float y, float z)
    {

        Vector3 requested = new Vector3(x, y, z);
        Vector3 ik = Vector3.zero;

        //  save to send in case of bad ik.
        lastValidIk = ik;
        lastValidFk = new Vector3(x, y, z);
        previousValidIk = ik;
        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }
    public Geometry.GeoPacket doRightIk(float x, float y, float z)
    {

        Vector3 requested = new Vector3(x, y, z);
        Vector3 ik = Vector3.zero;

        //  save to send in case of bad ik.
        lastValidIk = ik;
        lastValidFk = new Vector3(x, y, z);
        previousValidIk = ik;
        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request

    }

    /*

        public Geometry.GeoPacket doRightIk(float x, float y, float z)
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
            ik.z = 0.5f - (radians.vector3.x / (2 * Mathf.PI));



            //update the End Effector intermediate as we go
            //rotate the lower bone
            intermediateEndEffector = Geometry.rotateAroundXAxis(-(Mathf.PI - radians.vector3.x), new Vector3(0f, -lower, 0f));
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
        */

    /*
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
         ik.z = 0.5f - (radians.vector3.x / (2 * Mathf.PI));
         ik.z = -ik.z;

         //update the End Effector intermediate as we go
         //rotate the lower bone
         intermediateEndEffector = Geometry.rotateAroundXAxis(-(Mathf.PI - radians.vector3.x), new Vector3(0f, -lower, 0f));
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

     */

}
