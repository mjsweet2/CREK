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
    public enum SIDE { LEFT, RIGHT };
    public SIDE side;


    public ArticulationBody hubBody;
    ArticulationDrive hubDrive;


    public float actuation; 


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

        hubDrive = hubBody.xDrive;
        hubDrive.target = actuation * 360;  //for some reason I can't reverse the axis, so I use negative values
        hubBody.xDrive = hubDrive; //they have to be applied

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
