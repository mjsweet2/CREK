using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArticulizeBalTrike : MonoBehaviour
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
        configArticulationZXBs();
        setArticulationZXBs();
        configAndSetArticulationSegs();

        addBoxColliders();
        addSphereColliders();
        assignPhysScripts();
        assignPhysScriptsSegs();

    }

    public void assignPhysScripts()
    {

        landmarks["BalTrike"].gameObject.AddComponent<ArticulatedBalTrike>();

        landmarks["XLeftRear"].gameObject.AddComponent<ArticulatedZXB>();
        landmarks["XRightRear"].gameObject.AddComponent<ArticulatedZXB>();


        ArticulatedBalTrike parent = landmarks["BalTrike"].gameObject.GetComponent<ArticulatedBalTrike>();

        ArticulatedZXB leftrear = landmarks["XLeftRear"].gameObject.GetComponent<ArticulatedZXB>();
        ArticulatedZXB rightrear = landmarks["XRightRear"].gameObject.GetComponent<ArticulatedZXB>();


        parent.leftRear = leftrear;
        parent.rightRear = rightrear;

        parent.orientationSensor = landmarks["leftrear.z"];


        leftrear.side = ArticulatedZXB.SIDE.LEFT;
        rightrear.side = ArticulatedZXB.SIDE.RIGHT;


        leftrear.rangeZ = new Vector2(-1f, 1f);
        leftrear.rangeX = new Vector2(-1f, 1f);
        

        rightrear.rangeZ = new Vector2(-1f, 1f);
        rightrear.rangeX = new Vector2(-1f, 1f);
        


        leftrear.shoulderZBody = landmarks["leftrear.z"].GetComponent<ArticulationBody>();
        leftrear.shoulderXBody = landmarks["leftrear.x"].GetComponent<ArticulationBody>();
        leftrear.balBody = landmarks["leftrear.bal"].GetComponent<ArticulationBody>();

        rightrear.shoulderZBody = landmarks["rightrear.z"].GetComponent<ArticulationBody>();
        rightrear.shoulderXBody = landmarks["rightrear.x"].GetComponent<ArticulationBody>();
        rightrear.balBody = landmarks["rightrear.bal"].GetComponent<ArticulationBody>();


        leftrear.springValue = new Vector3(springValue, springValue, springValue);
        leftrear.damperValue = new Vector3(damperValue, damperValue, damperValue);

        rightrear.springValue = new Vector3(springValue, springValue, springValue);
        rightrear.damperValue = new Vector3(damperValue, damperValue, damperValue);


    }
    public void assignPhysScriptsSegs()
    {

        landmarks["LowerSegments"].gameObject.AddComponent<ArticulatedSegs>();


        ArticulatedBalTrike parent = landmarks["BalTrike"].gameObject.GetComponent<ArticulatedBalTrike>();

        ArticulatedSegs segs = landmarks["LowerSegments"].gameObject.GetComponent<ArticulatedSegs>();
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

       



        landmarks["l1"].gameObject.AddComponent<BoxCollider>();
        BoxCollider bc = landmarks["l1"].gameObject.GetComponent<BoxCollider>();
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
        bc.center = bcCenterB;

        landmarks["l5"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["l5"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeB;
        bc.center = bcCenterB;




        landmarks["leftrear.x"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["leftrear.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;

        landmarks["rightrear.x"].gameObject.AddComponent<BoxCollider>();
        bc = landmarks["rightrear.x"].gameObject.GetComponent<BoxCollider>();
        bc.size = bcSizeC;
        bc.center = bcCenterC;



        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(true);

    }
    public void addSphereColliders()
    {
        subject.gameObject.SetActive(false);

        landmarks["leftrear.bal"].gameObject.AddComponent<SphereCollider>();
        SphereCollider sc = landmarks["leftrear.bal"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeE.x;
        sc.center = bcCenterE;

        landmarks["rightrear.bal"].gameObject.AddComponent<SphereCollider>();
        sc = landmarks["rightrear.bal"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeE.x;
        sc.center = bcCenterE;


        landmarks["cap04wheelfront"].gameObject.AddComponent<SphereCollider>();
        sc = landmarks["cap04wheelfront"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeF.x;
        sc.center = bcCenterF;

        landmarks["cap04wheelrear"].gameObject.AddComponent<SphereCollider>();
        sc = landmarks["cap04wheelrear"].gameObject.GetComponent<SphereCollider>();
        sc.radius = bcSizeF.x;
        sc.center = bcCenterF;

        //Adding colliders makes the object explode
        //so I'm going to disable the object until all the colliders are sized properly
        subject.gameObject.SetActive(true);




    }
    public void addArticulationBodies()
    {
        //topmost parent
        landmarks["root"].gameObject.AddComponent<ArticulationBody>();


        landmarks["leftrear.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftrear.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["leftrear.bal"].gameObject.AddComponent<ArticulationBody>();

        landmarks["rightrear.z"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightrear.x"].gameObject.AddComponent<ArticulationBody>();
        landmarks["rightrear.bal"].gameObject.AddComponent<ArticulationBody>();


        landmarks["l1"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l2"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l3"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l4"].gameObject.AddComponent<ArticulationBody>();
        landmarks["l5"].gameObject.AddComponent<ArticulationBody>();


        landmarks["cap04wheelfront"].gameObject.AddComponent<ArticulationBody>();
        landmarks["cap04wheelrear"].gameObject.AddComponent<ArticulationBody>();


    }
    void setArticulationZXBs()
    {
        ArticulationBody lfzab = landmarks["leftrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfxab = landmarks["leftrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfbab = landmarks["leftrear.bal"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody rfzab = landmarks["rightrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfxab = landmarks["rightrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfbab = landmarks["rightrear.bal"].gameObject.GetComponent<ArticulationBody>();




        ArticulationDrive lfzad = lfzab.xDrive;
        ArticulationDrive lfxad = lfxab.xDrive;
        ArticulationDrive lfbad = lfbab.xDrive;

        ArticulationDrive rfzad = rfzab.xDrive;
        ArticulationDrive rfxad = rfxab.xDrive;
        ArticulationDrive rfbad = rfbab.xDrive;


        lfzad.stiffness = lfxad.stiffness = lfbad.stiffness = springValue;
        rfzad.stiffness = rfxad.stiffness = rfbad.stiffness = springValue;

        lfzad.damping = lfxad.damping = lfbad.damping = damperValue;
        rfzad.damping = rfxad.damping = rfbad.damping = damperValue;


        lfzab.xDrive = lfzad;
        lfxab.xDrive = lfxad;
        lfbab.xDrive = lfbad;

        rfzab.xDrive = rfzad;
        rfxab.xDrive = rfxad;
        rfbab.xDrive = rfbad;

    }
    void configAndSetArticulationSegs()
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
    void configArticulationZXBs()
    {
        ArticulationBody lfzab = landmarks["leftrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfxab = landmarks["leftrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody lfbab = landmarks["leftrear.bal"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody rfzab = landmarks["rightrear.z"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfxab = landmarks["rightrear.x"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody rfbab = landmarks["rightrear.bal"].gameObject.GetComponent<ArticulationBody>();

        ArticulationBody cfab = landmarks["cap04wheelfront"].gameObject.GetComponent<ArticulationBody>();
        ArticulationBody crab = landmarks["cap04wheelrear"].gameObject.GetComponent<ArticulationBody>();


        lfzab.jointType = lfxab.jointType = lfbab.jointType = ArticulationJointType.RevoluteJoint;
        rfzab.jointType = rfxab.jointType = rfbab.jointType = ArticulationJointType.RevoluteJoint;

        cfab.jointType = crab.jointType = ArticulationJointType.RevoluteJoint;





        lfzab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
        lfxab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        lfbab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);

        rfzab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);
        rfxab.anchorRotation = Quaternion.Euler(90f, 0f, 0f);
        rfbab.anchorRotation = Quaternion.Euler(0f, 90f, 0f);


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


        landmarks.Add("BalTrike", subject);

        landmarks.Add("XLeftRear", subject);
        landmarks.Add("XRightRear", subject);
        landmarks.Add("LowerSegments", subject);


        landmarks.Add("root", subject);
        landmarks.Add("segsroot", subject);

        landmarks.Add("l1", subject);
        landmarks.Add("l2", subject);
        landmarks.Add("l3", subject);
        landmarks.Add("l4", subject);
        landmarks.Add("l5", subject);

        landmarks.Add("cap04wheelfront", subject);
        landmarks.Add("cap04wheelrear", subject);

        landmarks.Add("leftrear.z", subject);
        landmarks.Add("leftrear.x", subject);
        landmarks.Add("leftrear.bal", subject);

        landmarks.Add("rightrear.z", subject);
        landmarks.Add("rightrear.x", subject);
        landmarks.Add("rightrear.bal", subject);


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

