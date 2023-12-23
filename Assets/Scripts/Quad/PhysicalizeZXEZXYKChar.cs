/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalizeZXEZXYKChar : MonoBehaviour
{

    public Transform subject;
    public Dictionary<string, Transform> landmarks;


    public float springValue;
    public float damperValue;

    public float springValue2;
    public float damperValue2;


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

    public void doPhysicalize()
    {
        nameLandmarks();
        findLandmarks();
        printLandmarks();

        addArticulationBodies();
        configureArticulationBodies();
        setArticulationDrive();
        setAndConfigureArticulationPlus();
        addBoxColliders();
        assignPhysScripts();

    }

    public void assignPhysScripts()
    {

        landmarks["ZXYK16Axis"].gameObject.AddComponent<Articulated2ZXE2ZXYK>();

        landmarks["LeftFrontZ"].gameObject.AddComponent<ArticulatedZXE>();
        landmarks["RightFrontZ"].gameObject.AddComponent<ArticulatedZXE>();
        landmarks["LeftRearZ"].gameObject.AddComponent<ArticulatedZXYK>();
        landmarks["RightRearZ"].gameObject.AddComponent<ArticulatedZXYK>();


        Articulated2ZXE2ZXYK parent = landmarks["ZXYK16Axis"].gameObject.GetComponent<Articulated2ZXE2ZXYK>();

        ArticulatedZXE leftfront = landmarks["LeftFrontZ"].gameObject.GetComponent<ArticulatedZXE>();
        ArticulatedZXE rightfront = landmarks["RightFrontZ"].gameObject.GetComponent<ArticulatedZXE>();
        ArticulatedZXYK leftrear = landmarks["LeftRearZ"].gameObject.GetComponent<ArticulatedZXYK>();
        ArticulatedZXYK rightrear = landmarks["RightRearZ"].gameObject.GetComponent<ArticulatedZXYK>();



        parent.leftFront = leftfront;
        parent.rightFront = rightfront;
        parent.leftRear = leftrear;
        parent.rightRear = rightrear;
        parent.orientationSensor = landmarks["topblock"];



        leftfront.side = ArticulatedZXE.SIDE.LEFT;
        rightfront.side = ArticulatedZXE.SIDE.RIGHT;
        leftrear.side = ArticulatedZXYK.SIDE.LEFT;
        rightrear.side = ArticulatedZXYK.SIDE.RIGHT;



        leftfront.rangeZ = new Vector2(-1f, 1f);
        leftfront.rangeX = new Vector2(-1f, 1f);
        leftfront.rangeE = new Vector2(-1f, 1f);

        rightfront.rangeZ = new Vector2(-1f, 1f);
        rightfront.rangeX = new Vector2(-1f, 1f);
        rightfront.rangeE = new Vector2(-1f, 1f);

        leftrear.rangeZ = new Vector2(-1f, 1f);
        leftrear.rangeX = new Vector2(-1f, 1f);
        leftrear.rangeY = new Vector2(-1f, 1f);
        leftrear.rangeK = new Vector2(-1f, 1f);

        rightrear.rangeZ = new Vector2(-1f, 1f);
        rightrear.rangeX = new Vector2(-1f, 1f);
        rightrear.rangeY = new Vector2(-1f, 1f);
        rightrear.rangeK = new Vector2(-1f, 1f);


        leftfront.shoulderZBody = landmarks["leftfront.z"].GetComponent<ArticulationBody>();
        leftfront.shoulderXBody = landmarks["leftfront.x"].GetComponent<ArticulationBody>();
        leftfront.elbowBody = landmarks["leftfront.e"].GetComponent<ArticulationBody>();

        rightfront.shoulderZBody = landmarks["rightfront.z"].GetComponent<ArticulationBody>();
        rightfront.shoulderXBody = landmarks["rightfront.x"].GetComponent<ArticulationBody>();
        rightfront.elbowBody = landmarks["rightfront.e"].GetComponent<ArticulationBody>();

        leftrear.shoulderZBody = landmarks["leftrear.z"].GetComponent<ArticulationBody>();
        leftrear.shoulderXBody = landmarks["leftrear.x"].GetComponent<ArticulationBody>();
        leftrear.shoulderYBody = landmarks["leftrear.y"].GetComponent<ArticulationBody>();
        leftrear.kneeBody = landmarks["leftrear.k"].GetComponent<ArticulationBody>();

        rightrear.shoulderZBody = landmarks["rightrear.z"].GetComponent<ArticulationBody>();
        rightrear.shoulderXBody = landmarks["rightrear.x"].GetComponent<ArticulationBody>();
        rightrear.shoulderYBody = landmarks["rightrear.k"].GetComponent<ArticulationBody>();
        rightrear.kneeBody = landmarks["rightrear.k"].GetComponent<ArticulationBody>();



        leftfront.springValue = springValue;
        leftfront.damperValue = damperValue;

        rightfront.springValue = springValue;
        rightfront.damperValue = damperValue;

        leftrear.springValue = springValue;
        leftrear.damperValue = damperValue;

        rightrear.springValue = springValue;
        rightrear.damperValue = damperValue;


        landmarks["SegmentsB"].gameObject.AddComponent<ArticulatedSegs>();

        ArticulatedSegs segs = landmarks["SegmentsB"].gameObject.GetComponent<ArticulatedSegs>();
        parent.segs = segs;

        segs.s1 = landmarks["l1"].gameObject.GetComponent<ArticulationBody>();
        segs.s2 = landmarks["l2"].gameObject.GetComponent<ArticulationBody>();
        segs.s3 = landmarks["l3"].gameObject.GetComponent<ArticulationBody>();
        segs.s4 = landmarks["l4"].gameObject.GetComponent<ArticulationBody>();
        segs.s5 = landmarks["l5"].gameObject.GetComponent<ArticulationBody>();

        segs.damping = damperValue2;
        segs.stiffness = springValue2;




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
        bc.size = bcSizeC;
        bc.center = bcCenterC;

        landmarks["rightfront.x"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["rightfront.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;

        landmarks["rightfront.e"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["rightfront.e"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;

        landmarks["leftrear.x"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["leftrear.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeD;
        bc.center = bcCenterD;

        landmarks["leftrear.k"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["leftrear.k"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeD;
        bc.center = bcCenterD;

        landmarks["rightrear.x"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["rightrear.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeD;
        bc.center = bcCenterD;

        landmarks["rightrear.k"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["rightrear.k"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeD;
        bc.center = bcCenterD;


        //add ball feet
        landmarks["leftfront.e"].gameObject.AddComponent<SphereCollider>();
        SphereCollider sc = landmarks["leftfront.e"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeE.x;
        sc.center = bcCenterE;

        landmarks["rightfront.e"].gameObject.AddComponent<SphereCollider>();
        sc = landmarks["rightfront.e"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeE.x;
        sc.center = bcCenterE;


        landmarks["leftrear.k"].gameObject.AddComponent<SphereCollider>();
        sc = landmarks["leftrear.k"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeF.x;
        sc.center = bcCenterF;

        landmarks["rightrear.k"].gameObject.AddComponent<SphereCollider>();
        sc = landmarks["rightrear.k"].gameObject.GetComponent<SphereCollider>();
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
        landmarks["leftfront.e"].gameObject.AddComponent<ArticulationBody>();

        landmarks["rightfront.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightfront.e"].gameObject.AddComponent<ArticulationBody>();


        landmarks["l1"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l2"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l3"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l4"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l5"].gameObject.AddComponent<ArticulationBody>();


        landmarks["leftrear.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftrear.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftrear.y"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftrear.k"].gameObject.AddComponent<ArticulationBody>();


        landmarks["rightrear.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightrear.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightrear.y"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightrear.k"].gameObject.AddComponent<ArticulationBody>();

        //this keeps it from falling, comment this out for characters
        //ArticulationBody parent = landmarks["topblock"].gameObject.GetComponent<ArticulationBody>();
        //parent.immovable = true;

    }

    void setArticulationDrive()
    {
        ArticulationBody lfzab = landmarks["leftfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfxab = landmarks["leftfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfeab = landmarks["leftfront.e"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody rfzab = landmarks["rightfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfxab = landmarks["rightfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfeab = landmarks["rightfront.e"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody lrzab = landmarks["leftrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lrxab = landmarks["leftrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lryab = landmarks["leftrear.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lrkab = landmarks["leftrear.k"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody rrzab = landmarks["rightrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rrxab = landmarks["rightrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rryab = landmarks["rightrear.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rrkab = landmarks["rightrear.k"].gameObject.GetComponent<ArticulationBody>();

        ArticulationDrive lfzad = lfzab.xDrive;
        ArticulationDrive lfxad = lfxab.xDrive;
        ArticulationDrive lfead = lfeab.xDrive;

        ArticulationDrive rfzad = rfzab.xDrive;
        ArticulationDrive rfxad = rfxab.xDrive;
        ArticulationDrive rfead = rfeab.xDrive;

        ArticulationDrive lrzad = lrzab.xDrive;
        ArticulationDrive lrxad = lrxab.xDrive;
        ArticulationDrive lryad = lryab.xDrive;
        ArticulationDrive lrkad = lrkab.xDrive;

        ArticulationDrive rrzad = rrzab.xDrive;
        ArticulationDrive rrxad = rrxab.xDrive;
        ArticulationDrive rryad = rryab.xDrive;
        ArticulationDrive rrkad = rrkab.xDrive;

        lfzad.stiffness = lfxad.stiffness = lfead.stiffness = springValue;
        rfzad.stiffness = rfxad.stiffness = rfead.stiffness = springValue;
        lrzad.stiffness = lrxad.stiffness = lryad.stiffness = lrkad.stiffness = springValue;
        rrzad.stiffness = rrxad.stiffness = rryad.stiffness = rrkad.stiffness = springValue;


        lfzad.damping = lfxad.damping = lfead.damping = damperValue;
        rfzad.damping = rfxad.damping = rfead.damping = damperValue;
        lrzad.damping = lrxad.damping = lryad.damping = lrkad.damping = damperValue;
        rrzad.damping = rrxad.damping = rryad.damping = rrkad.damping = damperValue;


        lfzab.xDrive = lfzad;
        lfxab.xDrive = lfxad;
        lfeab.xDrive = lfead;

        rfzab.xDrive = rfzad;
        rfxab.xDrive = rfxad;
        rfeab.xDrive = rfead;

        lrzab.xDrive = lrzad;
        lrxab.xDrive = lrxad;
        lryab.xDrive = lryad;
        lrkab.xDrive = lrkad;

        rrzab.xDrive = rrzad;
        rrxab.xDrive = rrxad;
        rryab.xDrive = rryad;
        rrkab.xDrive = rrkad;




    }
    void setAndConfigureArticulationPlus()
    {


        ArticulationBody l1ab = landmarks["l1"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody l2ab = landmarks["l2"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody l3ab = landmarks["l3"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody l4ab = landmarks["l4"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody l5ab = landmarks["l5"].gameObject.GetComponent<ArticulationBody>();

        l1ab.jointType = l2ab.jointType = l3ab.jointType = l4ab.jointType = l5ab.jointType = ArticulationJointType.SphericalJoint;


        l1ab.anchorRotation = Quaternion.Euler(-90f, 90f, 0f);
        l2ab.anchorRotation = Quaternion.Euler(-90f, 90f, 0f);
        l3ab.anchorRotation = Quaternion.Euler(-90f, 90f, 0f);
        l4ab.anchorRotation = Quaternion.Euler(-90f, 90f, 0f);
        l5ab.anchorRotation = Quaternion.Euler(-90f, 90f, 0f);


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

        //l1ab.anchorRotation = l2ab.anchorRotation = l3ab.anchorRotation = l4ab.anchorRotation = l5ab.anchorRotation = Quaternion.Euler(0f, 0f, 90f);


        l1xad.stiffness = l2xad.stiffness = l3xad.stiffness = l4xad.stiffness = l5xad.stiffness = springValue2;
        l1yad.stiffness = l2yad.stiffness = l3yad.stiffness = l4yad.stiffness = l5yad.stiffness = springValue2;
        l1zad.stiffness = l2zad.stiffness = l3zad.stiffness = l4zad.stiffness = l5zad.stiffness = springValue2;

        l1xad.damping = l2xad.damping = l3xad.damping = l4xad.damping = l5xad.damping = damperValue2;
        l1yad.damping = l2yad.damping = l3yad.damping = l4yad.damping = l5yad.damping = damperValue2;
        l1zad.damping = l2zad.damping = l3zad.damping = l4zad.damping = l5zad.damping = damperValue2;

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

    void configureArticulationBodies()
    {
        ArticulationBody lfzab = landmarks["leftfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfxab = landmarks["leftfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfeab = landmarks["leftfront.e"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody rfzab = landmarks["rightfront.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfxab = landmarks["rightfront.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfeab = landmarks["rightfront.e"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody lrzab = landmarks["leftrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lrxab = landmarks["leftrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lryab = landmarks["leftrear.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lrkab = landmarks["leftrear.k"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody rrzab = landmarks["rightrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rrxab = landmarks["rightrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rryab = landmarks["rightrear.y"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rrkab = landmarks["rightrear.k"].gameObject.GetComponent<ArticulationBody>();

        //front 3 axis
        lfzab.jointType = lfxab.jointType = lfeab.jointType = ArticulationJointType.RevoluteJoint;
        rfzab.jointType = rfxab.jointType = rfeab.jointType = ArticulationJointType.RevoluteJoint;
        //rear 4 axis
        lrzab.jointType = lrxab.jointType = lryab.jointType = lrkab.jointType = ArticulationJointType.RevoluteJoint;
        rrzab.jointType = rrxab.jointType = rryab.jointType = rrkab.jointType = ArticulationJointType.RevoluteJoint;



        lfzab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
        lfxab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        lfeab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        
        rfzab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        rfxab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        rfeab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);


        lrzab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        lrxab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        lryab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
        lrkab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);


        rrzab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
        rrxab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        rryab.anchorRotation = Quaternion.Euler(0f, -90f, 0f);
        rrkab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);


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

        landmarks.Add("ZXYK16Axis", subject);

        landmarks.Add("LeftFrontZ", subject);
        landmarks.Add("RightFrontZ", subject);
        landmarks.Add("LeftRearZ", subject);
        landmarks.Add("RightRearZ", subject);
        landmarks.Add("SegmentsB", subject);


        landmarks.Add("topblock", subject);
        landmarks.Add("l1", subject);
        landmarks.Add("l2", subject);
        landmarks.Add("l3", subject);
        landmarks.Add("l4", subject);
        landmarks.Add("l5", subject);

        landmarks.Add("leftfront.z", subject);
        landmarks.Add("leftfront.x", subject);
        landmarks.Add("leftfront.e", subject);

        landmarks.Add("rightfront.z", subject);
        landmarks.Add("rightfront.x", subject);
        landmarks.Add("rightfront.e", subject);

        landmarks.Add("leftrear.z", subject);
        landmarks.Add("leftrear.x", subject);
        landmarks.Add("leftrear.y", subject);
        landmarks.Add("leftrear.k", subject);

        landmarks.Add("rightrear.z", subject);
        landmarks.Add("rightrear.x", subject);
        landmarks.Add("rightrear.y", subject);
        landmarks.Add("rightrear.k", subject);

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
