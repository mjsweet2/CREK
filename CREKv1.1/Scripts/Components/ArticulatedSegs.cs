/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArticulatedSegs : MonoBehaviour
{
    public ArticulationBody root; //not currently used

    //can I put the upper segs into this component without breaking it?
    public ArticulationBody us1;

    public ArticulationBody s1;
    public ArticulationBody s2;
    public ArticulationBody s3;
    public ArticulationBody s4;
    public ArticulationBody s5;

    public ArticulationDrive xadu1;

    public ArticulationDrive xad1;
    public ArticulationDrive xad2;
    public ArticulationDrive xad3;
    public ArticulationDrive xad4;
    public ArticulationDrive xad5;

    public ArticulationDrive yad1;
    public ArticulationDrive yad2;
    public ArticulationDrive yad3;
    public ArticulationDrive yad4;
    public ArticulationDrive yad5;

    public ArticulationDrive zad1;
    public ArticulationDrive zad2;
    public ArticulationDrive zad3;
    public ArticulationDrive zad4;
    public ArticulationDrive zad5;

    public float stiffness;
    public float damping;

    public enum SEGSCONFIG { NONE, SINGLE, PAIR };
    public SEGSCONFIG segsConfig;
    public Vector3 actuation;
    public Vector3 actuationUpper;

    // Start is called before the first frame update
    void Start()
    {

        if(segsConfig == SEGSCONFIG.PAIR )
        {
            xadu1 = us1.xDrive;
        }
    

        xad1 = s1.xDrive;
        xad2 = s2.xDrive;
        xad3 = s3.xDrive;
        xad4 = s4.xDrive;
        xad5 = s5.xDrive;

        yad1 = s1.yDrive;
        yad2 = s2.yDrive;
        yad3 = s3.yDrive;
        yad4 = s4.yDrive;
        yad5 = s5.yDrive;

        zad1 = s1.zDrive;
        zad2 = s2.zDrive;
        zad3 = s3.zDrive;
        zad4 = s4.zDrive;
        zad5 = s5.zDrive;


        setXStiffness(stiffness);
        setYStiffness(stiffness);
        setZStiffness(stiffness);

        setXDamp(damping);
        setYDamp(damping);
        setZDamp(damping);

    }

    // Update is called once per frame
    void Update()
    {

        actUX();
        actX();
        actY();
        actZ();
    }

    void actUX()
    {
        if (segsConfig == SEGSCONFIG.PAIR)
        {
            xadu1.target = actuationUpper.x;
            us1.xDrive = xadu1;
        }
       
       
    }
    void actX()
    {

        xad1.target = actuation.x;
        xad2.target = actuation.x;
        xad3.target = actuation.x;
        xad4.target = actuation.x;
        xad5.target = actuation.x;


        s1.xDrive = xad1;
        s2.xDrive = xad2;
        s3.xDrive = xad3;
        s4.xDrive = xad4;
        s5.xDrive = xad5;


    }
    void actY()
    {

        yad1.target = actuation.y;
        yad2.target = actuation.y;
        yad3.target = actuation.y;
        yad4.target = actuation.y;
        yad5.target = actuation.y;


        s1.yDrive = yad1;
        s2.yDrive = yad2;
        s3.yDrive = yad3;
        s4.yDrive = yad4;
        s5.yDrive = yad5;

    }

    void actZ()
    {

        zad1.target = actuation.z;
        zad2.target = actuation.z;
        zad3.target = actuation.z;
        zad4.target = actuation.z;
        zad5.target = actuation.z;


        s1.zDrive = zad1;
        s2.zDrive = zad2;
        s3.zDrive = zad3;
        s4.zDrive = zad4;
        s5.zDrive = zad5;

    }

    public void setXStiffness(float s)
    {
        stiffness = s;


        if (segsConfig == SEGSCONFIG.PAIR)
        {
            xadu1.stiffness = stiffness;
            us1.xDrive = xadu1;
        }

        

        xad1.stiffness = stiffness;
        xad2.stiffness = stiffness;
        xad3.stiffness = stiffness;
        xad4.stiffness = stiffness;
        xad5.stiffness = stiffness;


        

        s1.xDrive = xad1;
        s2.xDrive = xad2;
        s3.xDrive = xad3;
        s4.xDrive = xad4;
        s5.xDrive = xad5;

    }

    public void setYStiffness(float s)
    {
        stiffness = s;


    
        yad1.stiffness = stiffness;
        yad2.stiffness = stiffness;
        yad3.stiffness = stiffness;
        yad4.stiffness = stiffness;
        yad5.stiffness = stiffness;


      
        s1.yDrive = yad1;
        s2.yDrive = yad2;
        s3.yDrive = yad3;
        s4.yDrive = yad4;
        s5.yDrive = yad5;

    }

    public void setZStiffness(float s)
    {
        stiffness = s;



        zad1.stiffness = stiffness;
        zad2.stiffness = stiffness;
        zad3.stiffness = stiffness;
        zad4.stiffness = stiffness;
        zad5.stiffness = stiffness;


        s1.zDrive = zad1;
        s2.zDrive = zad2;
        s3.zDrive = zad3;
        s4.zDrive = zad4;
        s5.zDrive = zad5;


    }

    public void setXDamp(float d)
    {
        stiffness = d;


        if (segsConfig == SEGSCONFIG.PAIR)
        {
            xadu1.damping = damping;
            us1.xDrive = xadu1;
        }

       

        xad1.damping = damping;
        xad2.damping = damping;
        xad3.damping = damping;
        xad4.damping = damping;
        xad5.damping = damping;


        

        s1.xDrive = xad1;
        s2.xDrive = xad2;
        s3.xDrive = xad3;
        s4.xDrive = xad4;
        s5.xDrive = xad5;

    }

    public void setYDamp(float d)
    {
        damping = d;

        yad1.damping = damping;
        yad2.damping = damping;
        yad3.damping = damping;
        yad4.damping = damping;
        yad5.damping = damping;


        s1.yDrive = yad1;
        s2.yDrive = yad2;
        s3.yDrive = yad3;
        s4.yDrive = yad4;
        s5.yDrive = yad5;

    }

    public void setZDamp(float d)
    {
        damping = d;

        zad1.damping = damping;
        zad2.damping = damping;
        zad3.damping = damping;
        zad4.damping = damping;
        zad5.damping = damping;


        s1.zDrive = zad1;
        s2.zDrive = zad2;
        s3.zDrive = zad3;
        s4.zDrive = zad4;
        s5.zDrive = zad5;


    }
}
