/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArticulizeZXYETrike : MonoBehaviour
{

    public Transform subject;
    public Dictionary<string, Transform> landmarks;



    public float springValue;
    public float damperValue;

    public float segSpringValue;
    public float segDamperValue;


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


    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {


    }

    public void doArticulize()
    {

        nameLandmarks();
        findLandmarks();
        printLandmarks();

        addArticulationBodies();
        configureArticulationZXYEs();
        setArticulationZXYEs();
        setAndConfigureArticulationSegs();

        addBoxColliders();
        addSphereColliders();
        assignPhysScripts();
        assignPhysScriptsSegs();

    }

    public void assignPhysScripts()
    {

        landmarks["ZXYETrike"].gameObject.AddComponent<Articulated2ZXYE>();

        landmarks["LeftFrontZ.ZXYETrike"].gameObject.AddComponent<ArticulatedZXYE>();
        landmarks["RightFrontZ.ZXYETrike"].gameObject.AddComponent<ArticulatedZXYE>();


        Articulated2ZXYE parent = landmarks["ZXYETrike"].gameObject.GetComponent<Articulated2ZXYE>();

        ArticulatedZXYE leftfront = landmarks["LeftFrontZ.ZXYETrike"].gameObject.GetComponent<ArticulatedZXYE>();
        ArticulatedZXYE rightfront = landmarks["RightFrontZ.ZXYETrike"].gameObject.GetComponent<ArticulatedZXYE>();


        parent.leftFront = leftfront;
        parent.rightFront = rightfront;

        parent.orientationSensor = landmarks["topblock"];


        leftfront.side = ArticulatedZXYE.SIDE.LEFT;
        rightfront.side = ArticulatedZXYE.SIDE.RIGHT;


        leftfront.rangeZ = new Vector2(-1f, 1f);
        leftfront.rangeX = new Vector2(-1f, 1f);
        leftfront.rangeY = new Vector2(-1f, 1f);
        leftfront.rangeE = new Vector2(-1f, 1f);

        rightfront.rangeZ = new Vector2(-1f, 1f);
        rightfront.rangeX = new Vector2(-1f, 1f);
        rightfront.rangeY = new Vector2(-1f, 1f);
        rightfront.rangeE = new Vector2(-1f, 1f);


        leftfront.shoulderZBody = landmarks["leftfront.z"].GetComponent<ArticulationBody>();
        leftfront.shoulderXBody = landmarks["leftfront.x"].GetComponent<ArticulationBody>();
        leftfront.shoulderYBody = landmarks["leftfront.y"].GetComponent<ArticulationBody>();
        leftfront.elbowBody = landmarks["leftfront.e"].GetComponent<ArticulationBody>();

        rightfront.shoulderZBody = landmarks["rightfront.z"].GetComponent<ArticulationBody>();
        rightfront.shoulderXBody = landmarks["rightfront.x"].GetComponent<ArticulationBody>();
        rightfront.shoulderYBody = landmarks["rightfront.y"].GetComponent<ArticulationBody>();
        rightfront.elbowBody = landmarks["rightfront.e"].GetComponent<ArticulationBody>();


        leftfront.springValue = springValue;
        leftfront.damperValue = damperValue;

        rightfront.springValue = springValue;
        rightfront.damperValue = damperValue;


    }
    public void assignPhysScriptsSegs()
    {

        landmarks["Segments.ZXYETrike"].gameObject.AddComponent<ArticulatedSegs>();


        Articulated2ZXYE parent = landmarks["ZXYETrike"].gameObject.GetComponent<Articulated2ZXYE>();

        ArticulatedSegs segs = landmarks["Segments.ZXYETrike"].gameObject.GetComponent<ArticulatedSegs>();
        parent.segs = segs;

        segs.s1 = landmarks["l1"].gameObject.GetComponent<ArticulationBody>();
        segs.s2 = landmarks["l2"].gameObject.GetComponent<ArticulationBody>();
        segs.s3 = landmarks["l3"].gameObject.GetComponent<ArticulationBody>();
        segs.s4 = landmarks["l4"].gameObject.GetComponent<ArticulationBody>();
        segs.s5 = landmarks["l5"].gameObject.GetComponent<ArticulationBody>();

        segs.stiffness = segSpringValue;
        segs.damping = segDamperValue;

    }
    public void addBoxColliders()
    {

        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(false);

        landmarks["topblock"].gameObject.AddComponent<BoxCollider>();
        BoxCollider bc = landmarks["topblock"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeA;




        landmarks["l1"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["l1"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeB;

        landmarks["l2"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["l2"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeB;

        landmarks["l3"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["l3"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeB;

        landmarks["l4"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["l4"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeB;

        landmarks["l5"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["l5"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeB;



        landmarks["leftfront.x"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["leftfront.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;

        landmarks["leftfront.e"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["leftfront.e"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeD;
        bc.center = bcCenterD;

        landmarks["rightfront.x"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["rightfront.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;

        landmarks["rightfront.e"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["rightfront.e"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeD;
        bc.center = bcCenterD;


        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(true);

    }
    public void addSphereColliders()
    {
        subject.gameObject.SetActive(false);

        landmarks["leftrear.base"].gameObject.AddComponent<SphereCollider>();
        SphereCollider sc = landmarks["leftrear.base"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeE.x;
        sc.center = bcCenterE;

        landmarks["rightrear.base"].gameObject.AddComponent<SphereCollider>();
        sc = landmarks["rightrear.base"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeE.x;
        sc.center = bcCenterE;

        landmarks["leftfront.e"].gameObject.AddComponent<SphereCollider>();
        sc = landmarks["leftfront.e"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeF.x;
        sc.center = bcCenterF;

        landmarks["rightfront.e"].gameObject.AddComponent<SphereCollider>();
        sc = landmarks["rightfront.e"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeF.x;
        sc.center = bcCenterF;


        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(true);




    }
    public void addArticulationBodies()
    {
        //topmost parent
        landmarks["topblock"].gameObject.AddComponent<ArticulationBody>();


        landmarks["leftfront.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftfront.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftfront.y"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftfront.e"].gameObject.AddComponent<ArticulationBody>();

        landmarks["rightfront.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.y"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.e"].gameObject.AddComponent<ArticulationBody>();


        landmarks["l1"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l2"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l3"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l4"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l5"].gameObject.AddComponent<ArticulationBody>();


    }
    void setArticulationZXYEs()
    {
        ArticulationBody lfzab = landmarks["leftfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfxab = landmarks["leftfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfyab = landmarks["leftfront.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfeab = landmarks["leftfront.e"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody rfzab = landmarks["rightfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfxab = landmarks["rightfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfyab = landmarks["rightfront.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfeab = landmarks["rightfront.e"].gameObject.GetComponent<ArticulationBody>();


        ArticulationDrive lfzad = lfzab.xDrive;
        ArticulationDrive lfxad = lfxab.xDrive;
        ArticulationDrive lfyad = lfyab.xDrive;
        ArticulationDrive lfead = lfeab.xDrive;

        ArticulationDrive rfzad = rfzab.xDrive;
        ArticulationDrive rfxad = rfxab.xDrive;
        ArticulationDrive rfyad = rfyab.xDrive;
        ArticulationDrive rfead = rfeab.xDrive;


        lfzad.stiffness = lfxad.stiffness = lfyad.stiffness = lfead.stiffness = springValue;
        rfzad.stiffness = rfxad.stiffness = rfyad.stiffness = rfead.stiffness = springValue;

        lfzad.damping = lfxad.damping = lfyad.damping = lfead.damping = damperValue;
        rfzad.damping = rfxad.damping = rfyad.damping = rfead.damping = damperValue;


        lfzab.xDrive = lfzad;
        lfxab.xDrive = lfxad;
        lfyab.xDrive = lfyad;
        lfeab.xDrive = lfead;

        rfzab.xDrive = rfzad;
        rfxab.xDrive = rfxad;
        rfyab.xDrive = rfyad;
        rfeab.xDrive = rfead;

    }
    void setAndConfigureArticulationSegs()
    {

        ArticulationBody l1ab = landmarks["l1"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody l2ab = landmarks["l2"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody l3ab = landmarks["l3"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody l4ab = landmarks["l4"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody l5ab = landmarks["l5"].gameObject.GetComponent<ArticulationBody>();

        l1ab.jointType = l2ab.jointType = l3ab.jointType = l4ab.jointType = l5ab.jointType = ArticulationJointType.SphericalJoint;

        ArticulationDrive l1xad = l1ab.xDrive;
        ArticulationDrive l1yad = l1ab.yDrive;
        ArticulationDrive l1zad = l1ab.zDrive;

        ArticulationDrive l2xad = l2ab.xDrive;
        ArticulationDrive l2yad = l2ab.yDrive;
        ArticulationDrive l2zad = l2ab.zDrive;

        ArticulationDrive l3xad = l3ab.xDrive;
        ArticulationDrive l3yad = l3ab.yDrive;
        ArticulationDrive l3zad = l3ab.zDrive;

        ArticulationDrive l4xad = l4ab.xDrive;
        ArticulationDrive l4yad = l4ab.yDrive;
        ArticulationDrive l4zad = l4ab.zDrive;

        ArticulationDrive l5xad = l5ab.xDrive;
        ArticulationDrive l5yad = l5ab.yDrive;
        ArticulationDrive l5zad = l5ab.zDrive;

        l1ab.anchorRotation = l2ab.anchorRotation = l3ab.anchorRotation = l4ab.anchorRotation = l5ab.anchorRotation = Quaternion.Euler(270f, 90f, 0f);

        l1xad.stiffness = l2xad.stiffness = l3xad.stiffness = l4xad.stiffness = l5xad.stiffness = segSpringValue;
        l1yad.stiffness = l2yad.stiffness = l3yad.stiffness = l4yad.stiffness = l5yad.stiffness = segSpringValue;
        l1zad.stiffness = l2zad.stiffness = l3zad.stiffness = l4zad.stiffness = l5zad.stiffness = segSpringValue;

        l1xad.damping = l2xad.damping = l3xad.damping = l4xad.damping = l5xad.damping = segDamperValue;
        l1yad.damping = l2yad.damping = l3yad.damping = l4yad.damping = l5yad.damping = segDamperValue;
        l1zad.damping = l2zad.damping = l3zad.damping = l4zad.damping = l5zad.damping = segDamperValue;


        l1ab.xDrive = l1yad;
        l1ab.yDrive = l1yad;
        l1ab.zDrive = l1zad;

        l2ab.xDrive = l2xad;
        l2ab.yDrive = l2yad;
        l2ab.zDrive = l2zad;

        l3ab.xDrive = l3xad;
        l3ab.yDrive = l3yad;
        l3ab.zDrive = l3zad;

        l4ab.xDrive = l4xad;
        l4ab.yDrive = l4yad;
        l4ab.zDrive = l4zad;

        l5ab.xDrive = l5xad;
        l5ab.yDrive = l5yad;
        l5ab.zDrive = l5zad;

    }
    void configureArticulationZXYEs()
    {
        ArticulationBody lfzab = landmarks["leftfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfxab = landmarks["leftfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfyab = landmarks["leftfront.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfeab = landmarks["leftfront.e"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody rfzab = landmarks["rightfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfxab = landmarks["rightfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfyab = landmarks["rightfront.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfeab = landmarks["rightfront.e"].gameObject.GetComponent<ArticulationBody>();


        lfzab.jointType = lfxab.jointType = lfyab.jointType = lfeab.jointType = ArticulationJointType.RevoluteJoint;
        rfzab.jointType = rfxab.jointType = rfyab.jointType = rfeab.jointType = ArticulationJointType.RevoluteJoint;


        lfzab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        lfxab.anchorRotation = Quaternion.Euler(0f, 0f, 0f);
        lfyab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
        lfeab.anchorRotation = Quaternion.Euler(0f, 0f, 0f);

        rfzab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        rfxab.anchorRotation = Quaternion.Euler(0f, 180f, 0f);
        rfyab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        rfeab.anchorRotation = Quaternion.Euler(0f, 180f, 00f);


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
    void nameLandmarks()
    {

        landmarks = new Dictionary<string, Transform>();
        landmarks.Add("ZXYETrike", subject);
   
        landmarks.Add("LeftFrontZ.ZXYETrike", subject);
        landmarks.Add("RightFrontZ.ZXYETrike", subject);
        landmarks.Add("Segments.ZXYETrike", subject);

        landmarks.Add("topblock", subject);

        landmarks.Add("l1", subject);
        landmarks.Add("l2", subject);
        landmarks.Add("l3", subject);
        landmarks.Add("l4", subject);
        landmarks.Add("l5", subject);

        landmarks.Add("leftfront.z", subject);
        landmarks.Add("leftfront.x", subject);
        landmarks.Add("leftfront.y", subject);
        landmarks.Add("leftfront.e", subject);

        landmarks.Add("rightfront.z", subject);
        landmarks.Add("rightfront.x", subject);
        landmarks.Add("rightfront.y", subject);
        landmarks.Add("rightfront.e", subject);

        landmarks.Add("leftrear.base", subject);
        landmarks.Add("rightrear.base", subject);


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
