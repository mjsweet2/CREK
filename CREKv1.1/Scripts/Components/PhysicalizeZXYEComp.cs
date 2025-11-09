/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalizeZXYEComp : MonoBehaviour
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
        nameLandmarksCommon();
        

        if (hasLeft)
        {
            nameLandmarksLeft();
        }
        if (hasRight)
        {
            nameLandmarksRight();
        }

        findLandmarks();
        printLandmarks();



        addArticulationBodiesCommon();

        if (hasLeft)
        {
            addArticulationBodiesLeft();
            configureArticulationBodiesLeft();
            setArticulationDriveLeft();
            addBoxCollidersLeft();
            assignPhysScriptsLeft();
        }
        if (hasRight)
        {
            addArticulationBodiesRight();
            configureArticulationBodiesRight();
            setArticulationDriveRight();
            addBoxCollidersRight();
            assignPhysScriptsRight();
        }

    }

    public void addArticulationBodiesCommon()
    {
        landmarks["topblock"].gameObject.AddComponent<ArticulationBody>();

        //this keeps it from falling
        ArticulationBody parent = landmarks["topblock"].gameObject.GetComponent<ArticulationBody>();
        parent.immovable = true;
    }

    public void assignPhysScriptsRight()
    {

        landmarks["RightFrontZ.ZXYETrike"].gameObject.AddComponent<ArticulatedZXYE>();

        ArticulatedZXYE rightfront = landmarks["RightFrontZ.ZXYETrike"].gameObject.GetComponent<ArticulatedZXYE>();

        rightfront.side = ArticulatedZXYE.SIDE.RIGHT;

        rightfront.rangeZ = new Vector2(-1f, 1f);
        rightfront.rangeX = new Vector2(-1f, 1f);
        rightfront.rangeY = new Vector2(-1f, 1f);
        rightfront.rangeE = new Vector2(-1f, 1f);

        rightfront.shoulderZBody = landmarks["rightfront.z"].GetComponent<ArticulationBody>();
        rightfront.shoulderXBody = landmarks["rightfront.x"].GetComponent<ArticulationBody>();
        rightfront.shoulderYBody = landmarks["rightfront.y"].GetComponent<ArticulationBody>();
        rightfront.elbowBody = landmarks["rightfront.e"].GetComponent<ArticulationBody>();

        rightfront.springValue = springValue;
        rightfront.damperValue = damperValue;


    }

    public void addBoxCollidersRight()
    {

        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(false);


        //landmarks["rightfront.z"].gameObject.AddComponent<BoxCollider>();
        //BoxCollider bc = landmarks["rightfront.z"].gameObject.GetComponent<BoxCollider>();
        //bc.size = bcSizeB;

        landmarks["rightfront.x"].gameObject.AddComponent<BoxCollider>();
        BoxCollider bc = landmarks["rightfront.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;

        landmarks["rightfront.e"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["rightfront.e"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;




        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(true);


    }

    public void addArticulationBodiesRight()
    {

        landmarks["rightfront.hub"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.y"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.e"].gameObject.AddComponent<ArticulationBody>();


    }

    public void configureArticulationBodiesRight()
    {

        ArticulationBody zab = landmarks["rightfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["rightfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody yab = landmarks["rightfront.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody eab = landmarks["rightfront.e"].gameObject.GetComponent<ArticulationBody>();

        zab.jointType = ArticulationJointType.RevoluteJoint;
        xab.jointType = ArticulationJointType.RevoluteJoint;
        yab.jointType = ArticulationJointType.RevoluteJoint;
        eab.jointType = ArticulationJointType.RevoluteJoint;

        zab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        xab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        yab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        eab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);


    }
    public void setArticulationDriveRight()
    {

        ArticulationBody zab = landmarks["rightfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["rightfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody yab = landmarks["rightfront.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody eab = landmarks["rightfront.e"].gameObject.GetComponent<ArticulationBody>();

        ArticulationDrive zad = zab.xDrive;
        ArticulationDrive xad = xab.xDrive;
        ArticulationDrive yad = xab.xDrive;
        ArticulationDrive ead = eab.xDrive;

        zad.stiffness = xad.stiffness = yad.stiffness = ead.stiffness = springValue;

        zad.damping = xad.damping = yad.damping = ead.damping = damperValue;

        zab.xDrive = zad;
        xab.xDrive = xad;
        yab.xDrive = yad;
        eab.xDrive = ead;

    }

    public void assignPhysScriptsLeft()
    {

        landmarks["LeftFrontZ.ZXYETrike"].gameObject.AddComponent<ArticulatedZXYE>();

        ArticulatedZXYE leftfront = landmarks["LeftFrontZ.ZXYETrike"].gameObject.GetComponent<ArticulatedZXYE>();

        leftfront.side = ArticulatedZXYE.SIDE.LEFT;

        leftfront.rangeZ = new Vector2(-1f, 1f);
        leftfront.rangeX = new Vector2(-1f, 1f);
        leftfront.rangeY = new Vector2(-1f, 1f);
        leftfront.rangeE = new Vector2(-1f, 1f);

        leftfront.shoulderZBody = landmarks["leftfront.z"].GetComponent<ArticulationBody>();
        leftfront.shoulderXBody = landmarks["leftfront.x"].GetComponent<ArticulationBody>();
        leftfront.shoulderYBody = landmarks["leftfront.y"].GetComponent<ArticulationBody>();
        leftfront.elbowBody = landmarks["leftfront.e"].GetComponent<ArticulationBody>();

        leftfront.springValue = springValue;
        leftfront.damperValue = damperValue;

    }

    public void addBoxCollidersLeft()
    {

        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(false);


        //landmarks["leftfront.z"].gameObject.AddComponent<BoxCollider>();
        //BoxCollider bc = landmarks["leftfront.z"].gameObject.GetComponent<BoxCollider>();
        //bc.size = bcSizeB;



        landmarks["leftfront.x"].gameObject.AddComponent<BoxCollider>();
        BoxCollider bc = landmarks["leftfront.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;

        landmarks["leftfront.e"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["leftfront.e"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;




        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(true);


    }

    public void addArticulationBodiesLeft()
    {

        landmarks["leftfront.hub"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftfront.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftfront.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftfront.y"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftfront.e"].gameObject.AddComponent<ArticulationBody>();

    }
    public void configureArticulationBodiesLeft()
    {

        ArticulationBody zab = landmarks["leftfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["leftfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody yab = landmarks["leftfront.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody eab = landmarks["leftfront.e"].gameObject.GetComponent<ArticulationBody>();

        zab.jointType = ArticulationJointType.RevoluteJoint;
        xab.jointType = ArticulationJointType.RevoluteJoint;
        yab.jointType = ArticulationJointType.RevoluteJoint;
        eab.jointType = ArticulationJointType.RevoluteJoint;

        zab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        xab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        yab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
        eab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);

    }
    public void setArticulationDriveLeft()
    {

        ArticulationBody zab = landmarks["leftfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["leftfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody yab = landmarks["leftfront.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody eab = landmarks["leftfront.e"].gameObject.GetComponent<ArticulationBody>();

        ArticulationDrive zad = zab.xDrive;
        ArticulationDrive xad = xab.xDrive;
        ArticulationDrive yad = yab.xDrive;
        ArticulationDrive ead = eab.xDrive;

        zad.stiffness = springValue;
        xad.stiffness = springValue;
        yad.stiffness = springValue;
        ead.stiffness = springValue;


        zad.damping = damperValue;
        xad.damping = damperValue;
        yad.damping = damperValue;
        ead.damping = damperValue;

        zab.xDrive = zad;
        xab.xDrive = xad;
        yab.xDrive = yad;
        eab.xDrive = ead;
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

    void nameLandmarksCommon()
    {
        landmarks = new Dictionary<string, Transform>();

        //top most parent of object
        landmarks.Add("ZXYETrike", subject);

        //top most parent of bones
        landmarks.Add("topblock", subject);
    }
    void nameLandmarksRight()
    {

        landmarks.Add("rightfront.hub", subject);
        landmarks.Add("rightfront.z", subject);
        landmarks.Add("rightfront.x", subject);
        landmarks.Add("rightfront.y", subject);
        landmarks.Add("rightfront.e", subject);
        landmarks.Add("RightFrontZ.ZXYETrike", subject); //holds Comp Scripts

    }

    void nameLandmarksLeft()
    {

        landmarks.Add("leftfront.hub", subject);
        landmarks.Add("leftfront.z", subject);
        landmarks.Add("leftfront.x", subject);
        landmarks.Add("leftfront.y", subject);
        landmarks.Add("leftfront.e", subject);
        landmarks.Add("LeftFrontZ.ZXYETrike", subject); //holds Comp Scripts

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