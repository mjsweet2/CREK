/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArticulatedWheel : MonoBehaviour
{
    public enum SIDE { LEFT, RIGHT, OMNI0, OMNI120, OMNI240, MOTO };
    public SIDE side;


    public ArticulationBody hubBody;
    ArticulationDrive hubDrive;


    public float actuation;
    public float clampedActuation;
    public bool clamp;

    public float springValue;
    public float damperValue;


    public float alpha;
    public Vector2 range;


    public List<string> messages;


    // Start is called before the first frame update
    void Start()
    {
        hubDrive = hubBody.xDrive;

    }


    // Update is called once per frame
    void Update()
    {


        actHub();


    }

    // clockwise is positive, actuator shaft outwards
    // lift forward
    // left side: clockwise
    // right side: counter-clockwise
    void actHub()
    {
        if (float.IsNaN(actuation))
            return;

 
        

        if(side == SIDE.LEFT)
        {
            actuation = -actuation;
        }

        clampedActuation = actuation;
        //I'm testing this to see if spherical wheels can be used in place of omniwheels
        //doesn't work very well. I think it's suffering gymbol lock
        if (clamp)
            clampedActuation = clamp180(clampedActuation);


        hubDrive = hubBody.xDrive;
        hubDrive.target = clampedActuation * 360;  //for some reason I can't reverse the axis, so I use negative values
        hubBody.xDrive = hubDrive; //they have to be applied

    }

    float clamp180(float value)
    {
        while (value > 0.5f)
            value = value - 1f;

        while (value < -0.5f)
            value = value + 1f;

        return value;
    }


    public void setDamper()
    {
        hubDrive = hubBody.xDrive;

        hubDrive.damping = damperValue;

        //they have to be applied
        hubBody.xDrive = hubDrive;
        
    }
    public void setSpring()
    {
        hubDrive = hubBody.xDrive;

        hubDrive.stiffness = springValue;

        //they have to be applied
        hubBody.xDrive = hubDrive;  

    }



}
