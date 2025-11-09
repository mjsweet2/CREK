/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the MIT License.
 * You should have received a copy of the MIT License with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quadrilateral : MonoBehaviour
{

    public float upperLength;
    public float stanceLength;

    public float sideLength;


    public float inputRotationX; // applied to left side
    public float inputRotationY;
    public float inputRotationZ;

    public Vector3 dormantLeftPosition;
    public Vector3 dormantRightPosition;

    public float outputRotationX;
    public float outputRotationZ;

    public Vector3 circle01Position;
    public float circle01Radius;
    public Vector3 circle02Position;
    public float circle02Radius;
    public Vector3 circle03Position;
    public float circle03Radius;

    public Vector3 lowerLeftPosition;
    public Vector3 upperLeftPosition;

    public Vector3 lowerRightPosition;
    public Vector3 upperRightPosition;
    
    //markers for dev and troubleshooting
    public Transform upperLeft;
    public Transform upperRight;
    public Transform lowerLeft;
    public Transform lowerRight;

    public MarkerCircle circle01, circle02, circle03;


    public float watchFloat01;


    // Start is called before the first frame update
    void Start()
    {

        setDormantPositions(lowerLeftPosition, lowerRightPosition);

        circle01Position = upperLeftPosition;
        circle01Radius = sideLength;

        circle02Position = upperRightPosition;
        circle02Radius = sideLength;

        circle03Position = lowerLeftPosition;
        circle03Radius = stanceLength;


    }

    // Update is called once per frame
    void Update()
    {



        //doRotations();
        

    }
    public void setDormantPositions(Vector3 left, Vector3 right)
    {

        dormantLeftPosition = left;
        dormantRightPosition = right;





    }
    public void setStance(float s)
    {
        stanceLength = s;

    }
    public void doRotations()
    {

        //before I begin calculations, put end effectors in dormant positions
        lowerLeftPosition = dormantLeftPosition;
        lowerRightPosition = dormantRightPosition;


        rotateLeftOnZInput();
        rotateRightOnZInputAndStance();


        //draw Circles is based on the previous calcs, and can't be messed up by next calcs
        //so I draw them between step 2 and 3
        drawCircles();


        rotateLeftAndRightOnXInput();
        pushToMarkers();

        updateOutputs();


    }

    void rotateLeftOnZInput()
    {
        Vector3 startingPosition = dormantLeftPosition - upperLeftPosition;
        Vector3 newPosition = Geometry.rotateAroundZAxis(inputRotationZ, startingPosition);

        lowerLeftPosition = upperLeftPosition + newPosition;


        circle03Position = lowerLeftPosition;
        circle03Radius = stanceLength;

    }
    void rotateRightOnZInputAndStance()
    {

        Vector2 radical = Geometry.intersect2Circles(circle02Position.x,
                                                        circle02Position.y,
                                                        circle02Radius,
                                                        circle03Position.x,
                                                        circle03Position.y,
                                                        circle03Radius);

        
        Vector2 intersectPair = Geometry.intersectLineCircle(radical.x,radical.y,
                                            circle02Position.x,
                                            circle02Position.y,
                                            circle02Radius);

        //choose the one further to the right;
        float intersectX = intersectPair.x;
        if (intersectX < intersectPair.y)
            intersectX = intersectPair.y;


        //returns positive value that I will need the negative version of
        float intersectY = Geometry.yFromXOnCircleComponentSpace(circle02Radius, intersectX - circle02Position.x);
        intersectY = -intersectY;
        intersectY += circle02Position.y;

        //this is the machine space position of the end effector
        lowerRightPosition = new Vector3(intersectX, intersectY, 0f);


        

    }

    void rotateLeftAndRightOnXInput()
    {
        //convert to component space
        Vector3 l = lowerLeftPosition - upperLeftPosition;
        Vector3 r = lowerRightPosition - upperRightPosition;


        //rotate about the x axis
        l = Geometry.rotateAroundXAxis(inputRotationX, l);
        r = Geometry.rotateAroundXAxis(inputRotationX, r);

        //convert to machine space
        lowerLeftPosition = l + upperLeftPosition;
        lowerRightPosition = r + upperRightPosition;



    }

    void updateOutputs()
    {


    }
    void pushToMarkers()
    {
        if (lowerLeft != null)
        {
            lowerLeft.position = lowerLeftPosition;
            upperLeft.position = upperLeftPosition;
            lowerRight.position = lowerRightPosition;
            upperRight.position = upperRightPosition;

        }
    }
    void drawCircles()
    {
        

        if (circle01 != null)
        {

            circle01.center = upperLeftPosition;
            circle01.radius = sideLength;
            circle01.drawCircle();

            circle02.center = upperRightPosition;
            circle02.radius = sideLength;
            circle02.drawCircle();

            circle03.center = lowerLeftPosition;
            circle03.radius = stanceLength;
            circle03.drawCircle();
        }
       




    }
}
