/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalizeZXYKComp : MonoBehaviour
{

    public Transform subject;
    public Dictionary<string, Transform> landmarks;


    public float springValue;
    public float damperValue;

    //box collider sizes
    public Vector3 bcSizeA; //topblock
    public Vector3 bcSizeB; //segs
    public Vector3 bcSizeC; //front upper 
    public Vector3 bcSizeD; //front lower
    public Vector3 bcSizeE; //rear upper
    public Vector3 bcSizeF; //rear lower

    public Vector3 bcCenterA; //topblock
    public Vector3 bcCenterB; //segs
    public Vector3 bcCenterC; //front upper 
    public Vector3 bcCenterD; //front lower
    public Vector3 bcCenterE; //rear upper
    public Vector3 bcCenterF; //rear lower

    public bool hasLeft;
    public bool hasRight;


    // Start is called before the first frame update
    void Start()
    {





    }

    // Update is called once per frame
    void Update()
    {



    }
    public void doPhysicalize()
    {

        if (hasLeft)
        {
            nameLandmarksLeft();
            findLandmarks();
            printLandmarks();


            addArticulationBodiesLeft();
            configureArticulationBodiesLeft();
            setArticulationDriveLeft();
            addBoxCollidersLeft();
            assignPhysScriptsLeft();
        }
        if(hasRight)
        {

            nameLandmarksRight();
            findLandmarks();
            printLandmarks();


            addArticulationBodiesRight();
            configureArticulationBodiesRight();
            setArticulationDriveRight();
            addBoxCollidersRight();
            assignPhysScriptsRight();
        }

    }

    public void assignPhysScriptsRight()
    {

        landmarks["RightRearXY.ZXYKComp"].gameObject.AddComponent<ArticulatedZXYK>();

        ArticulatedZXYK rightrear = landmarks["RightRearXY.ZXYKComp"].gameObject.GetComponent<ArticulatedZXYK>();

        rightrear.side = ArticulatedZXYK.SIDE.RIGHT;

        rightrear.rangeZ = new Vector2(-1f, 1f);
        rightrear.rangeX = new Vector2(-1f, 1f);
        rightrear.rangeY = new Vector2(-1f, 1f);
        rightrear.rangeK = new Vector2(-1f, 1f);

        rightrear.shoulderZBody = landmarks["rightrear.z"].GetComponent<ArticulationBody>();
        rightrear.shoulderXBody = landmarks["rightrear.x"].GetComponent<ArticulationBody>();
        rightrear.shoulderYBody = landmarks["rightrear.y"].GetComponent<ArticulationBody>();
        rightrear.kneeBody = landmarks["rightrear.k"].GetComponent<ArticulationBody>();

        rightrear.springValue = springValue;
        rightrear.damperValue = damperValue;



    }

    public void addBoxCollidersRight()
    {

        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(false);


        //landmarks["rightfront.z"].gameObject.AddComponent<BoxCollider>();
        //BoxCollider bc = landmarks["rightfront.z"].gameObject.GetComponent<BoxCollider>();
        //bc.size = bcSizeB;

        landmarks["rightrear.x"].gameObject.AddComponent<BoxCollider>();
        BoxCollider bc = landmarks["rightrear.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;

        landmarks["rightrear.k"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["rightrear.k"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;




        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(true);


    }

    public void addArticulationBodiesRight()
    {

        landmarks["rightrear.hub"].gameObject.AddComponent<ArticulationBody>();


        landmarks["rightrear.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightrear.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightrear.y"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightrear.k"].gameObject.AddComponent<ArticulationBody>();

        //this keeps it from falling
        ArticulationBody parent = landmarks["rightrear.hub"].gameObject.GetComponent<ArticulationBody>();
        parent.immovable = true;

    }

    public void configureArticulationBodiesRight()
    {

        ArticulationBody zab = landmarks["rightrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["rightrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody yab = landmarks["rightrear.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody kab = landmarks["rightrear.k"].gameObject.GetComponent<ArticulationBody>();

        zab.jointType = ArticulationJointType.RevoluteJoint;
        xab.jointType = ArticulationJointType.RevoluteJoint;
        yab.jointType = ArticulationJointType.RevoluteJoint;
        kab.jointType = ArticulationJointType.RevoluteJoint;

        zab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
        xab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        yab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        kab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);


    }
    public void setArticulationDriveRight()
    {

        ArticulationBody zab = landmarks["rightrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["rightrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody yab = landmarks["rightrear.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody kab = landmarks["rightrear.k"].gameObject.GetComponent<ArticulationBody>();

        ArticulationDrive zad = zab.xDrive;
        ArticulationDrive xad = xab.xDrive;
        ArticulationDrive yad = xab.xDrive;
        ArticulationDrive kad = kab.xDrive;

        zad.stiffness = xad.stiffness = yad.stiffness = kad.stiffness = springValue;

        zad.damping = xad.damping = yad.damping = kad.damping = damperValue;

        zab.xDrive = zad;
        xab.xDrive = xad;
        yab.xDrive = yad;
        kab.xDrive = kad;

    }

    public void assignPhysScriptsLeft()
    {

        landmarks["LeftRearXY.ZXYKComp"].gameObject.AddComponent<ArticulatedZXYK>();

        ArticulatedZXYK leftrear = landmarks["LeftRearXY.ZXYKComp"].gameObject.GetComponent<ArticulatedZXYK>();

        leftrear.side = ArticulatedZXYK.SIDE.LEFT;

        leftrear.rangeZ = new Vector2(-1f, 1f);
        leftrear.rangeX = new Vector2(-1f, 1f);
        leftrear.rangeY = new Vector2(-1f, 1f);
        leftrear.rangeK = new Vector2(-1f, 1f);

        leftrear.shoulderZBody = landmarks["leftrear.z"].GetComponent<ArticulationBody>();
        leftrear.shoulderXBody = landmarks["leftrear.x"].GetComponent<ArticulationBody>();
        leftrear.shoulderYBody = landmarks["leftrear.y"].GetComponent<ArticulationBody>();
        leftrear.kneeBody = landmarks["leftrear.k"].GetComponent<ArticulationBody>();

        leftrear.springValue = springValue;
        leftrear.damperValue = damperValue;

    }

    public void addBoxCollidersLeft()
    {

        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(false);


        //landmarks["leftfront.z"].gameObject.AddComponent<BoxCollider>();
        //BoxCollider bc = landmarks["leftfront.z"].gameObject.GetComponent<BoxCollider>();
        //bc.size = bcSizeB;



        landmarks["leftrear.x"].gameObject.AddComponent<BoxCollider>();
        BoxCollider bc = landmarks["leftrear.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;

        landmarks["leftrear.k"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["leftrear.k"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;




        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(true);


    }

    public void addArticulationBodiesLeft()
    {

        landmarks["leftrear.hub"].gameObject.AddComponent<ArticulationBody>();


        landmarks["leftrear.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftrear.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftrear.y"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftrear.k"].gameObject.AddComponent<ArticulationBody>();

        //this keeps it from falling
        ArticulationBody parent = landmarks["leftrear.hub"].gameObject.GetComponent<ArticulationBody>();
        parent.immovable = true;

    }

    public void configureArticulationBodiesLeft()
    {

        ArticulationBody zab = landmarks["leftrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["leftrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody yab = landmarks["leftrear.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody kab = landmarks["leftrear.k"].gameObject.GetComponent<ArticulationBody>();

        zab.jointType = ArticulationJointType.RevoluteJoint;
        xab.jointType = ArticulationJointType.RevoluteJoint;
        yab.jointType = ArticulationJointType.RevoluteJoint;
        kab.jointType = ArticulationJointType.RevoluteJoint;

        zab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        xab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        yab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
        kab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);

    }
    public void setArticulationDriveLeft()
    {

        ArticulationBody zab = landmarks["leftrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["leftrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody yab = landmarks["leftrear.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody kab = landmarks["leftrear.k"].gameObject.GetComponent<ArticulationBody>();

        ArticulationDrive zad = zab.xDrive;
        ArticulationDrive xad = xab.xDrive;
        ArticulationDrive yad = yab.xDrive;
        ArticulationDrive kad = kab.xDrive;

        zad.stiffness = springValue;
        xad.stiffness = springValue;
        yad.stiffness = springValue;
        kad.stiffness = springValue;


        zad.damping = damperValue;
        xad.damping = damperValue;
        yad.damping = damperValue;
        kad.damping = damperValue;

        zab.xDrive = zad;
        xab.xDrive = xad;
        yab.xDrive = yad;
        kab.xDrive = kad;
    }

    public void findLandmarks()
    {
        Debug.Log(subject.name);
        if (landmarks.ContainsKey(subject.name))
            landmarks[subject.name] = subject;

        if (subject.childCount > 0)
        {
            for (int i = 0; i < subject.childCount; i++)
            {

                findLandmarks(subject.GetChild(i));

            }
        }

    }
    private void findLandmarks(Transform t)
    {
        Debug.Log(t.name + " : " + t.childCount.ToString());
        if (landmarks.ContainsKey(t.name))
            landmarks[t.name] = t;

        if (t.childCount > 0)
        {
            for (int i = 0; i < t.childCount; i++)
            {

                findLandmarks(t.GetChild(i));

            }
        }
    }

    void nameLandmarksRight()
    {

        landmarks = new Dictionary<string, Transform>();


        landmarks.Add("RightRearXY.ZXYKComp", subject);

        landmarks.Add("rightrear.hub", subject);
        landmarks.Add("rightrear.z", subject);
        landmarks.Add("rightrear.x", subject);
        landmarks.Add("rightrear.y", subject);
        landmarks.Add("rightrear.k", subject);


    }

    void nameLandmarksLeft()
    {

        landmarks = new Dictionary<string, Transform>();


        landmarks.Add("LeftRearXY.ZXYKComp", subject);

        landmarks.Add("leftrear.hub", subject);
        landmarks.Add("leftrear.z", subject);
        landmarks.Add("leftrear.x", subject);
        landmarks.Add("leftrear.y", subject);
        landmarks.Add("leftrear.k", subject);


    }
    void printLandmarks()
    {

        Debug.Log("\n\n\n\n\n\nprinting landmarks...");
        foreach (var pair in landmarks)
        {
            Debug.Log(pair.Key.ToString() + " : " + pair.Value.name);

        }

    }


}
