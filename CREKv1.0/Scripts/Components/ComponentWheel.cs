/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentWheel : MonoBehaviour
{

    public bool isSingle;

    public float fps;
    public float radius;
    public float circumference;

    public float position; //taken at the hub, the hub is @ (0,0,0) Component Space, along Z axis
    public float velocity; //taken at the hub, the hub is @ (0,0,0) Component Space, along Z axis
    public float radians;
    public float radianVelocity;
    public float positionCumulative;
    public float radiansCumulative;
    public float turns; //I don't need this, but I might as well hold on to it

    //for calculation velocity
    public float prevPosition;
    public float prevRadians;



    public enum TESTMODE { NONE, POSITION, RADIANS, VELOCITY, RADIANVELOCITY }
    public TESTMODE testMode;
    public float testInputPosition;
    public float testInputVelocity;
    public float testInputRadians;
    public float testInputRadianVelocity;



    public enum SIDE { LEFT, RIGHT };
    public SIDE side;

    //I don't know if I'm going to use this
    public enum IKMODE { RADIAN, POSITION, RADIANVELOCITY, POSITIONVELOCITY };
    public IKMODE ikMode;

    public string compName; // self.name = "base_class"     # for logging
    public Vector3 lastValidIk;   // self.last_valid_ik = [0.0,0.0,0.0] # same as current # The Component is storing state, so I need a seperate component for each
    public Vector3 lastValidFk;


    //valid fk tracking interfer swith previous frame calcuation
    // so I need seperate variables
    //updates in calc, not in do
    public Vector4 previousValidIk;

    public ArticulatedWheel phys;
    public Transform inputA, inputB;

    public Vector3 intermediateA;
    public Vector3 intermediateB;


    void Start()
    {
        circumference = radius * 2f * Mathf.PI;

    }

    // Update is called once per frame
    void Update()
    {
       if(isSingle)
        {
            doTest();
        }
    }

    public void doTest()
    {
        if (testMode == TESTMODE.POSITION)
        {
            doPosition(testInputPosition);
            //testInputPosition = position;
            testInputVelocity = velocity;
            testInputRadians = radians;
            testInputRadianVelocity = radianVelocity;
        }
        if (testMode == TESTMODE.RADIANS)
        {
            doRadians(testInputRadians);
            testInputPosition = position;
            testInputVelocity = velocity;
            //testInputRadians = radians;
            testInputRadianVelocity = radianVelocity;
        }

        if (testMode == TESTMODE.VELOCITY)
        {
            doVelocity(testInputVelocity);
            testInputPosition = position;
            //testInputVelocity = velocity;
            testInputRadians = radians;
            testInputRadianVelocity = radianVelocity;
        }
        if (testMode == TESTMODE.RADIANVELOCITY)
        {
            doRadianVelocity(testInputRadianVelocity);
            testInputPosition = position;
            testInputVelocity = velocity;
            testInputRadians = radians;
            //testInputRadianVelocity = radianVelocity;
        }


    }
    public void loadParams(float r)
    {
        radius = r;
        circumference = 2f * Mathf.PI * radius;
    }

    public Vector4 valIk() { return lastValidIk; }
    public Vector3 valFk() { return lastValidFk; }



    //don't forget that ik is the actuation that you send to the armature
    // these following functions are technically ik solvers based on requested wheel position or velocity
    public Geometry.GeoPacket doPosition(float reqP)
    {
        // position //taken at the hub, the hub is @ (0,0,0) Component Space, along Z axis
        // velocity //taken at the hub, the hub is @ (0,0,0) Component Space, along Z axis
        // radians
        // radianVelocity
        // positionCumulative
        // radiansCumulative
        // turns
        // prevPosition
        // prevRadians

        prevPosition = positionCumulative;
        prevRadians = radiansCumulative;

        //keep position in [-circumference,circumference] or a full rotation in each direction
        //keep radians in [-2*pi,2*pi] or a full rotation in each direction

        positionCumulative = reqP;
        turns = Mathf.Floor(positionCumulative / circumference);
        if (positionCumulative < 0f)
            turns++;
        position = positionCumulative - (turns*circumference);
        radiansCumulative = reqP / radius;
        radians = position / radius;


        velocity = (positionCumulative - prevPosition) * fps;
        radianVelocity = (radiansCumulative - prevRadians) * fps;


        //unit is rotations save to send in case of bad ik.
        previousValidIk = lastValidIk;
        lastValidIk.x = radiansCumulative / (2f*Mathf.PI);
        lastValidFk.z = positionCumulative;

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request
    }
    public Geometry.GeoPacket doVelocity(float reqV)
    {

        // position //taken at the hub, the hub is @ (0,0,0) Component Space, along Z axis
        // velocity //taken at the hub, the hub is @ (0,0,0) Component Space, along Z axis
        // radians
        // radianVelocity
        // positionCumulative
        // radiansCumulative
        // turns
        // prevPosition
        // prevRadians


        prevPosition = positionCumulative;
        prevRadians = radiansCumulative;

        //keep position in [-circumference,circumference] or a full rotation in each direction
        //keep radians in [-2*pi,2*pi] or a full rotation in each direction

        velocity = reqV;
        radianVelocity = reqV * (2f * Mathf.PI);

        positionCumulative += (velocity / fps);
        radiansCumulative += (radianVelocity / fps);
        turns = Mathf.Floor(positionCumulative / circumference);
        if (positionCumulative < 0f)
            turns++;

        position = positionCumulative - (turns * circumference);
        radians = position / radius;


        //unit is rotations save to send in case of bad ik.
        previousValidIk = lastValidIk;
        lastValidIk.x = radiansCumulative / (2f * Mathf.PI);
        lastValidFk.z = positionCumulative;

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request
    }
    public Geometry.GeoPacket doRadians(float reqR)
    {
        // position //taken at the hub, the hub is @ (0,0,0) Component Space, along Z axis
        // velocity //taken at the hub, the hub is @ (0,0,0) Component Space, along Z axis
        // radians
        // radianVelocity
        // positionCumulative
        // radiansCumulative
        // turns
        // prevPosition
        // prevRadians

        prevPosition = positionCumulative;
        prevRadians = radiansCumulative;


        //keep position in [-circumference,circumference] or a full rotation in each direction
        //keep radians in [-2*pi,2*pi] or a full rotation in each direction

        positionCumulative = reqR * radius;
        turns = Mathf.Floor(positionCumulative / circumference);
        if (positionCumulative < 0f)
            turns++;
        position = positionCumulative - (turns * circumference);
        radiansCumulative = reqR;
        radians = position / radius;


        velocity = (positionCumulative - prevPosition) * fps;
        radianVelocity = (radiansCumulative - prevRadians) * fps;


        //unit is rotations save to send in case of bad ik.
        previousValidIk = lastValidIk;
        lastValidIk.x = radiansCumulative / (2f * Mathf.PI);
        lastValidFk.z = positionCumulative;

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request
    }

    public Geometry.GeoPacket doRadianVelocity(float reqRV)
    {

        // position //taken at the hub, the hub is @ (0,0,0) Component Space, along Z axis
        // velocity //taken at the hub, the hub is @ (0,0,0) Component Space, along Z axis
        // radians
        // radianVelocity
        // positionCumulative
        // radiansCumulative
        // turns
        // prevPosition
        // prevRadians

        prevPosition = positionCumulative;
        prevRadians = radiansCumulative;

        //keep position in [-circumference,circumference] or a full rotation in each direction
        //keep radians in [-2*pi,2*pi] or a full rotation in each direction
        velocity = reqRV / (2f * Mathf.PI);
        radianVelocity = reqRV;

        positionCumulative += (velocity / fps);
        radiansCumulative += (radianVelocity / fps);
        turns = Mathf.Floor(positionCumulative / circumference);
        if (positionCumulative < 0f)
            turns++;

        position = positionCumulative - (turns * circumference);
        radians = position / radius;


        //unit is rotations save to send in case of bad ik.
        previousValidIk = lastValidIk;
        lastValidIk.x = radiansCumulative / (2f * Mathf.PI);
        lastValidFk.z = positionCumulative;

        return new Geometry.GeoPacket(true, valIk()); //  add boolean for good/bad request
    }



}
