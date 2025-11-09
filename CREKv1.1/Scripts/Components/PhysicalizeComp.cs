/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalizeComp : MonoBehaviour
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

    public bool isLeft;



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
        nameLandmarksRight();
        findLandmarks();
        printLandmarks();


        if (isLeft)
        {
            addArticulationBodiesLeft();
            configureArticulationBodiesLeft();
            setArticulationDriveLeft();
            addBoxCollidersLeft();
            assignPhysScriptsLeft();
        }
        else
        {
            addArticulationBodiesRight();
            configureArticulationBodiesRight();
            setArticulationDriveRight();
            addBoxCollidersRight();
            assignPhysScriptsRight();
        }



    }

    public void assignPhysScriptsRight()
    {

        landmarks["RightFrontZ"].gameObject.AddComponent<ArticulatedZXE>();

        ArticulatedZXE rightfront = landmarks["RightFrontZ"].gameObject.GetComponent<ArticulatedZXE>();

        rightfront.side = ArticulatedZXE.SIDE.RIGHT;

        rightfront.rangeZ = new Vector2(-1f, 1f);
        rightfront.rangeX = new Vector2(-1f, 1f);
        rightfront.rangeE = new Vector2(-1f, 1f);

        rightfront.shoulderZBody = landmarks["rightfront.z"].GetComponent<ArticulationBody>();
        rightfront.shoulderXBody = landmarks["rightfront.x"].GetComponent<ArticulationBody>();
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

        landmarks["root"].gameObject.AddComponent<ArticulationBody>();


        landmarks["rightfront.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.e"].gameObject.AddComponent<ArticulationBody>();

        //this keeps it from falling
        ArticulationBody parent = landmarks["root"].gameObject.GetComponent<ArticulationBody>();
        parent.immovable = true;

    }

    public void configureArticulationBodiesRight()
    {

        ArticulationBody zab = landmarks["rightfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["rightfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody eab = landmarks["rightfront.e"].gameObject.GetComponent<ArticulationBody>();

        zab.jointType = ArticulationJointType.RevoluteJoint;
        xab.jointType = ArticulationJointType.RevoluteJoint;
        eab.jointType = ArticulationJointType.RevoluteJoint;

        zab.anchorRotation = Quaternion.Euler(0f, 0f, 90f);
        xab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        eab.anchorRotation = Quaternion.Euler(90f, 0f, 00f);


    }
    public void setArticulationDriveRight()
    {

        ArticulationBody zab = landmarks["rightfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["rightfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody eab = landmarks["rightfront.e"].gameObject.GetComponent<ArticulationBody>();

        ArticulationDrive zad = zab.xDrive;
        ArticulationDrive xad = xab.xDrive;
        ArticulationDrive ead = eab.xDrive;

        zad.stiffness = springValue;
        xad.stiffness = springValue;
        ead.stiffness = springValue;

        zad.damping = damperValue;
        xad.damping = damperValue;
        ead.damping = damperValue;

        zab.xDrive = zad;
        xab.xDrive = xad;
        eab.xDrive = ead;

    }




    public void assignPhysScriptsLeft()
    {

        landmarks["LeftFrontZ"].gameObject.AddComponent<ArticulatedZXE>();

        ArticulatedZXE leftfront = landmarks["LeftFrontZ"].gameObject.GetComponent<ArticulatedZXE>();

        leftfront.side = ArticulatedZXE.SIDE.LEFT;

        leftfront.rangeZ = new Vector2(-1f, 1f);
        leftfront.rangeX = new Vector2(-1f, 1f);
        leftfront.rangeE = new Vector2(-1f, 1f);

        leftfront.shoulderZBody = landmarks["leftfront.z"].GetComponent<ArticulationBody>();
        leftfront.shoulderXBody = landmarks["leftfront.x"].GetComponent<ArticulationBody>();
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

        landmarks["root"].gameObject.AddComponent<ArticulationBody>();


        landmarks["leftfront.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftfront.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftfront.e"].gameObject.AddComponent<ArticulationBody>();

        //this keeps it from falling
        ArticulationBody parent = landmarks["root"].gameObject.GetComponent<ArticulationBody>();
        parent.immovable = true;

    }

    public void configureArticulationBodiesLeft()
    {

        ArticulationBody zab = landmarks["leftfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["leftfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody eab = landmarks["leftfront.e"].gameObject.GetComponent<ArticulationBody>();

        zab.jointType = ArticulationJointType.RevoluteJoint;
        xab.jointType = ArticulationJointType.RevoluteJoint;
        eab.jointType = ArticulationJointType.RevoluteJoint;

        zab.anchorRotation = Quaternion.Euler(0f, 0f, 90f);
        xab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        eab.anchorRotation = Quaternion.Euler(90f, 0f, 00f);


    }
    public void setArticulationDriveLeft()
    {

        ArticulationBody zab = landmarks["leftfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody xab = landmarks["leftfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody eab = landmarks["leftfront.e"].gameObject.GetComponent<ArticulationBody>();

        ArticulationDrive zad = zab.xDrive;
        ArticulationDrive xad = xab.xDrive;
        ArticulationDrive ead = eab.xDrive;

        zad.stiffness = springValue;
        xad.stiffness = springValue;
        ead.stiffness = springValue;

        zad.damping = damperValue;
        xad.damping = damperValue;
        ead.damping = damperValue;

        zab.xDrive = zad;
        xab.xDrive = xad;
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

    void nameLandmarksRight()
    {

        landmarks = new Dictionary<string, Transform>();


        landmarks.Add("RightFrontZ", subject);

        landmarks.Add("root", subject);
        landmarks.Add("rightfront.z", subject);
        landmarks.Add("rightfront.x", subject);
        landmarks.Add("rightfront.e", subject);


    }

    void nameLandmarksLeft()
    {

        landmarks = new Dictionary<string, Transform>();


        landmarks.Add("LeftFrontZ", subject);

        landmarks.Add("root", subject);
        landmarks.Add("leftfront.z", subject);
        landmarks.Add("leftfront.x", subject);
        landmarks.Add("leftfront.e", subject);


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
