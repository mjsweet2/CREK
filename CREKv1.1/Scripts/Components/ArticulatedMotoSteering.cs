/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the MIT License.
 * You should have received a copy of the MIT License with this file.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArticulatedMotoSteering : MonoBehaviour
{

    public ArticulationBody steeringBody;
    ArticulationDrive steeringDrive;


    public float actuation;


    public float springValue;
    public float damperValue;


    public float range;


    public List<string> messages;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        actSteering();
    }

    void actSteering()
    {
        if (float.IsNaN(actuation))
            return;


        if (actuation > range)
            actuation = range;
        if (actuation < -range)
            actuation = -range;

        steeringDrive = steeringBody.xDrive;
        steeringDrive.target = actuation * 360;  //for some reason I can't reverse the axis, so I use negative values
        steeringBody.xDrive = steeringDrive; //they have to be applied

    }



    public void setDamper()
    {
        steeringDrive = steeringBody.xDrive;

        steeringDrive.damping = damperValue;

        //they have to be applied
        steeringBody.xDrive = steeringDrive;

    }
    public void setSpring()
    {
        steeringDrive = steeringBody.xDrive;

        steeringDrive.stiffness = springValue;

        //they have to be applied
        steeringBody.xDrive = steeringDrive;

    }

}
