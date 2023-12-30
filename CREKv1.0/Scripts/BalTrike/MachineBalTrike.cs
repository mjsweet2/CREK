using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class MachineBalTrike : MonoBehaviour
{

    public ArticulatedBalTrike phys;
    public UDPSocketClient remotePhys;
    public string sendPhysString;
    public string recievePhysString;
    public Transform remoteTransform;

    public RSPoseController rsPoseController;
    public MSPoseController msPoseController;
    public InputDeck inputDeck;
    public MotionController motionController; //input into motion fsm
    public BalTrikePlanner planner;
    //public BalTrikeShell shell; // 4/9/23 I don't need this here? 

   
    public string paramJSONName;
    public BLBoneLengthList paramJSON;
    public Dictionary<string, float> boneLengths;
    public Dictionary<string, Vector3> boneHeads;


    public Transform testMarker;
    public List<Transform> testMarkers;

    public string machineName;

    public enum MOTIONMODE { TRAJECTORY, MOTION, TASK, TRAJTEST };
    public MOTIONMODE motionMode;
    public enum POSEMODE
    {
        TRANSLATE, ROTATE, SEGMENTSBLIND, SEGMENTSINFLUENCED, TANDRSEGMENETSBLIND, TANDRSEGMENTSINFLUENCED,
        REAR,
        FRONTSEGMENTS,
        LEFTREARRIGHTREAR
    }
    public POSEMODE poseMode;

    public enum BALANCEMODE { NOTBALANCING, BALANCING, BALANCINGCENTER, BALANCINGXZ, BALANCINGSEGS, BALANCINGXZSEGS };
    public BALANCEMODE balanceMode;
    public enum BALANCEMOVESTATE { STILL, FORWARD, BACKWARD, LEFTWARD, RIGHTWARD };
    public BALANCEMOVESTATE balanceMoveState;

    public PID pid;



    public Transform balanceSensor;
    public Vector3 balanceRotation;
    public Vector3 remoteBalanceRotation;
    public float balanceThreshold;
    public float balanceOpposite;
    public float balanceOppositeGoal;
    public float balanceForward;
    public float balanceBackward;
    public float balanceNeutral;
    public float frameRadius;
    public float wheelRadius;
    public float turnRate;



    //for center balance
    public float balanceThresholdCenter;
    public float balanceOppositeCenter;
    public float balanceOppositeGoalCenter;
    public float balanceForwardCenter;
    public float balanceBackwardCenter;
    public float balanceNeutralCenter;
    public float frameRadiusCenter;
    public float wheelRadiusCenter;
    public float turnRateCenter;


    public float balRotOffset;


    public ComponentZXB leftRear;
    public ComponentZXB rightRear;
    public CompSegs segs;

    public Vector3 lrPos;// self.lr_pos = []
    public Vector3 rrPos;// self.rr_pos = []

   
    public Vector3 lrRot;// self.lr_rot = []
    public Vector3 rrRot;// self.rr_rot = []

    
    public Vector3 lrStance;// self.stance_lr = [0.0,0.0,0.0]
    public Vector3 rrStance;// self.stance_rr = [0.0,0.0,0.0] 

    public Vector3 lrInfluecedPosition;
    public Vector3 rrInfluecedPosition;

    public Vector3 lrInfluecedRotation;
    public Vector3 rrInfluecedRotation;

    public Vector3 lrDeltaInflueced;
    public Vector3 rrDeltaInflueced;
    public Vector3 lrInflueceInverse;
    public Vector3 rrInflueceInverse;

    public bool isStanceValid;// self.is_stance_valid = False
    public bool isDeckValid;


    // realtime input for pose, usually from mouse and 6 dof controller
    public Vector3 posePos; // poseLeft;
    public Vector3 poseRot; // poseRight;
    public Vector3 poseSegs;

    //realtime input for motion input, usually from dual analog controller
    public Vector2 lMotionInput;
    public Vector2 rMotionInput;


    //component space fks
    public Vector3 cwFk;
    public Vector3 lrFk;
    public Vector3 rrFk;
    public Vector3 segsFk;

    //component iks
    Geometry.GeoPacket cwIk;
    Geometry.GeoPacket lrIk;
    Geometry.GeoPacket rrIk;
    Geometry.GeoPacket segsIk;


    public bool isRunning;

    public Trajectory spTrajectory; // set point trajectory
    public Trajectory lbTrajectory;
    public Trajectory rbTrajectory;
    public Trajectory lrTrajectory;
    public Trajectory rrTrajectory;
    public Trajectory segsTrajectory;


    public Motion spMotion; // set point motion
    public Motion lbMotion;
    public Motion rbMotion;
    public Motion lrMotion;
    public Motion rrMotion;
    public Motion segsMotion;


    public delegate void ShellCmd();
    public Dictionary<string, ShellCmd> functions;

    // Start is called before the first frame update
    void Start()
    {
        buildTables();
        loadParams();
        planner.buildTables();

        pid = new PID(0.2f, 2.1f, 0f);
    }

    // Update is called once per frame
    void Update()
    {

        processDiff();

        processMotionInput();

        if (motionMode == MOTIONMODE.TRAJECTORY)
        {
            processTrajectory();
        }

        if (motionMode == MOTIONMODE.MOTION)
        {
            processMotions(lMotionInput.y);
        }

        processPoseInput();


        

        if (remotePhys.runNetwork)
        {
            getRemotePhysMessages();
            
        }


        broadcastMessage();
    }
    public void processWSADCenterBalanced()
    {

        //balanceThresholdCenter;
        //balanceOppositeCenter;
        //balanceOppositeGoalCenter;
        //balanceForwardCenter;
        //balanceBackwardCenter;
        //balanceNeutralCenter;
        //frameRadiusCenter;
        //wheelRadiusCenter;
        //turnRateCenter;

        if (balanceMoveState != BALANCEMOVESTATE.STILL)
        {
            return;
        }


        if (Input.GetKey(KeyCode.W))
        {
            balanceOppositeGoalCenter = balanceForwardCenter;
        }
        if (Input.GetKey(KeyCode.S))
        {
            balanceOppositeGoalCenter = balanceBackwardCenter;
        }


        //I"m not sure how to turn yet?

        /*
        if (Input.GetKey(KeyCode.A))
        {
            leftRear.lastValidIk.z += -1 * (turnRate * (1 / 60f));
            rightRear.lastValidIk.z += 1 * (turnRate * (1 / 60f));
        }
        if (Input.GetKey(KeyCode.D))
        {
            leftRear.lastValidIk.z += 1 * (turnRate * (1 / 60f));
            rightRear.lastValidIk.z += -1 * (turnRate * (1 / 60f));
        }
        */

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            balanceOppositeGoalCenter = balanceNeutralCenter;
        }


    }
    public void processWSADBalanced()
    {

        //balanceOppositeGoal
        //balanceForward
        //balanceBackward
        //balanceNeutral
        if (balanceMoveState != BALANCEMOVESTATE.STILL)
        {
            return;
        }

        
        if (Input.GetKey(KeyCode.W))
        {
            balanceOppositeGoal = balanceForward;
        }
        if (Input.GetKey(KeyCode.S))
        {
            balanceOppositeGoal = balanceBackward;
        }

        if (Input.GetKey(KeyCode.A))
        { 
            leftRear.lastValidIk.z += -1 * (turnRate * (1 / 60f));
            rightRear.lastValidIk.z += 1 * (turnRate * (1 / 60f));
        }
        if (Input.GetKey(KeyCode.D))
        {
            leftRear.lastValidIk.z += 1 * (turnRate * (1 / 60f));
            rightRear.lastValidIk.z += -1 * (turnRate * (1 / 60f));
        }

        if(Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            balanceOppositeGoal = balanceNeutral;
        }


    }
    public void processWSADUnbalanced()
    {

        //balanceOppositeGoal
        //balanceForward
        //balanceBackward
        //balanceNeutral
        if (balanceMoveState != BALANCEMOVESTATE.STILL)
        {
            return;
        }

        balanceOppositeGoal = balanceNeutral;
        if (Input.GetKey(KeyCode.W))
        {
            leftRear.lastValidIk.z += 1 * (turnRate * (1 / 60f));
            rightRear.lastValidIk.z += 1 * (turnRate * (1 / 60f));
        }
        if (Input.GetKey(KeyCode.S))
        {
            leftRear.lastValidIk.z += -1 * (turnRate * (1 / 60f));
            rightRear.lastValidIk.z += -1 * (turnRate * (1 / 60f));
        }

        if (Input.GetKey(KeyCode.A))
        {
            leftRear.lastValidIk.z += -1 * (turnRate * (1 / 60f));
            rightRear.lastValidIk.z += 1 * (turnRate * (1 / 60f));
        }
        if (Input.GetKey(KeyCode.D))
        {
            leftRear.lastValidIk.z += 1 * (turnRate * (1 / 60f));
            rightRear.lastValidIk.z += -1 * (turnRate * (1 / 60f));
        }


    }

    void balance()
    {

        //I need to use the x value of this
        balanceRotation = balanceSensor.rotation.eulerAngles;

        //if I'm connected to the remote, use the value sent in through the remote connection
        if(remotePhys.runNetwork)
        {
            balanceRotation = remoteBalanceRotation;
        }
        

        balanceRotation.x -= balRotOffset;

        if(balanceRotation.x < -balanceThreshold || balanceRotation.x > balanceThreshold)
        {
            balanceMode = BALANCEMODE.NOTBALANCING;
            return;
        }


        balanceOpposite = Mathf.Sin(balanceRotation.x * Mathf.Deg2Rad) * frameRadius;

        float wheelCirc = (Mathf.PI * 2 * wheelRadius);

        float pidControlled = pid.Update(balanceOppositeGoal, balanceOpposite, 1 / 60f);

        leftRear.lastValidIk.z -= ((pidControlled / wheelCirc) * 1f);
        rightRear.lastValidIk.z -= ((pidControlled / wheelCirc) * 1f);
       

    }
    
    public void runCmd(string cmd)
    {
        if (functions.ContainsKey(cmd))
        {
            functions[cmd]();
        }
    }
    public bool cmdExists(string cmd)
    {
        return functions.ContainsKey(cmd);
    }
    public void buildTables()
    {
        functions = new Dictionary<string, ShellCmd>();

        functions.Add("softcancelallmotions", softCancelAllMotions);
    }
    public void loadParams()
    {
        importBoneLengthsJSON();
        if (paramJSONName == "")
            return;

              //  set lengths
        leftRear.loadParams(boneLengths["leftrear.x"], boneLengths["leftrear.balradius"]); //leftRear.loadParams(0.318f, 0.37f);
        //  set lengths
        rightRear.loadParams(boneLengths["rightrear.x"], boneLengths["rightrear.balradius"]); //rightRear.loadParams(0.318f, 0.37f);
        
        
        wheelRadius = boneLengths["rightrear.balradius"];
        frameRadius = (boneHeads["cap04wheelfront"].y - boneHeads["rightrear.balradius"].y);


        segs.loadParams(boneLengths["l1"], 0f, 0f);


        //  component placement
        lrPos = boneHeads["root"] - boneHeads["leftrear.x"]; 
        rrPos = boneHeads["root"] - boneHeads["rightrear.x"];

       
        lrRot = new Vector3(0.0f, 0.0f, 0.0f);
        rrRot = new Vector3(0.0f, 0.0f, 0.0f);


        machineName = "BalTrike";
        leftRear.compName = "leftRear";
        rightRear.compName = "rightRear";
        segs.compName = "segs";

        lrFk = leftRear.valFk();
        rrFk = rightRear.valFk();


        //populate these with something
        lrIk = leftRear.doLeftIk(0f,0f,0f);
        rrIk = rightRear.doRightIk(0f, 0f, 0f);

        segsIk = segs.doIk(0f, 0f); //ik's sent to motors!
        segsFk = segs.valFk();


    }
    public void importBoneLengthsJSON()
    {
        if (paramJSONName == "")
            return;

        StreamReader sReader;
        string path = "c://sites//" + paramJSONName;//  BLSphere8Quadsv2.json"; //Application.persistentDataPath + "/trajectory/" + trajectoryName + ".json";
        sReader = new StreamReader(path);
        string fileString = sReader.ReadToEnd();

        paramJSON = JsonUtility.FromJson<BLBoneLengthList>(fileString);


        sReader.Close();

        boneLengths = new Dictionary<string, float>();
        boneHeads = new Dictionary<string, Vector3>();
        for (int i = 0; i < paramJSON.bonenames.Count; i++)
        {
            boneLengths.Add(paramJSON.bonenames[i], paramJSON.bonelengths[i]);
            boneHeads.Add(paramJSON.bonenames[i], paramJSON.boneheads[i]);
        }


        /*
        Debug.Log("printing bone lengths...");
        foreach (string k in boneLengths.Keys)
        {
            Debug.Log(k + " : " + boneLengths[k]);
        }
        */


    }
    public Vector3 dormantLRIk()
    {
        Vector3 ret = leftRear.dormantFk();
        // ret has component space fk, need to translate to machine_space
        // do rotation side of component_space -> machine_space
        ret = Geometry.rotateZXY(lrRot, ret);
        ret += lrPos;

        return ret;
    }
    public Vector3 dormantRRIk()
    {
        Vector3 ret = rightRear.dormantFk();
        // ret has component space fk, need to translate to machine_space
        // do rotation side of component_space -> machine_space
        ret = Geometry.rotateZXY(rrRot, ret);
        ret += rrPos;

        return ret;
    }
    public Vector3 getlrStance() { return lrStance; }
    public Vector3 getrrStance() { return rrStance; }
    public Vector3 lrToMachine(Vector3 lr)
    {

        // component space fk, need to translate to machine_space space
        // do rotation side of component_space -> machine_space
        Vector3 ret = Geometry.rotateZXY(lrRot, lr);
        ret += lrPos;

        return ret;
    }
    public Vector3 rrToMachine(Vector3 rr)
    {
        // component space fk, need to translate to machine_space space
        // do rotation side of component_space -> machine_space
        Vector3 ret = Geometry.rotateZXY(rrRot, rr);
        ret += rrPos;

        return ret;
    }
    public Vector3 machineTolr(Vector3 machine)
    {
        // machine_space fk, need to translate to component_space space
        Vector3 ret = machine - lrPos;

        // do rotation side of machine_space -> component_space
        ret = Geometry.rotateYXZ(-lrRot, ret);

        return ret;
    }
    public Vector3 machineTorr(Vector3 machine)
    {
        // machine_space fk, need to translate to component_space space
        Vector3 ret = machine - rrPos;

        // do rotation side of machine_space -> component_space
        ret = Geometry.rotateYXZ(-rrRot, ret);

        return ret;
    }
    void processDiff()
    {
          

        if (balanceMode == BALANCEMODE.BALANCING)
        {
            processWSADBalanced();
            balance();
        }
        
        if (balanceMode == BALANCEMODE.NOTBALANCING)
        {
            processWSADUnbalanced();
            
        }

        lrIk.vector.z = leftRear.valIk().z;
        rrIk.vector.z = rightRear.valIk().z;
 

    }
    void processPoseInput()
    {

        if (rsPoseController.isTracking)
        {
            posePos = rsPoseController.subject.position;
            poseRot = rsPoseController.subject.rotation.eulerAngles * Mathf.Deg2Rad;
        }
        else
        {
            //keep pose from pose control
            if (msPoseController.isTrackingCompleted)
            {
                updateStance();
                msPoseController.resetTracking();
            }
            posePos = poseRot = Vector3.zero;
        }

        if (msPoseController.isTracking)
        {
            poseSegs = msPoseController.track;
        }
        else
        {
            poseSegs = Vector3.zero;
        }


        //if the deck is playing, grab the playback inputs
        if (inputDeck.deckState == InputDeck.DECKSTATE.PLAY)
        {
            poseSegs = inputDeck.currentMousePlayback;
        }


        // anytime you send a trajectory to a component you have to update the stance.
        // the stance is basically saved fk info, and if you update the trajectory at the component level, you need a new stance.
        // here pose input is just like a temporary adjustment to the trajectory, which doesn't update the stance
        // but "jump" switches the current trajectory, needing an updated stance


        //calculates machine space ik
        if (isStanceValid == false)
        {
            lrIk = leftRear.doLeftIk(lrFk.x, lrFk.y, lrFk.z); // this is a component space fk
            rrIk = rightRear.doRightIk(rrFk.x, rrFk.y, rrFk.z); // this is a component space fk
            updateStance();
        }

        pose(0f, -poseSegs.y, -poseSegs.x, -posePos.x, -posePos.y, posePos.z, -poseRot.x, poseRot.y, poseRot.z);

    }
    void processMotionInput()
    {
        //lMotionInput
        //rMotionInput

        lMotionInput.x = motionController.lMotionInput.x;
        lMotionInput.y = motionController.lMotionInput.y;
    }
    void broadcastMessage()
    {
        if (phys != null)
        {

            // iks are in lf_ik, etc...
            //phys.processTripleMessage("1", lrIk.vector.x, lrIk.vector.y, 0f);
            //phys.processTripleMessage("2", rrIk.vector.x, rrIk.vector.y, 0f);
            phys.processTripleMessage("3", 0f, 0f, lrIk.vector.z);
            phys.processTripleMessage("4", 0f, 0f, rrIk.vector.z);
            phys.processTripleMessage("5", segsIk.vector.x, segsIk.vector.y, segsIk.vector.z);
            phys.processTripleMessage("6", cwIk.vector.x, cwIk.vector.y, cwIk.vector.z);
        }

        sendPhysString = lrIk.vector.z.ToString("F5");
        sendPhysString = sendPhysString + ";" + rrIk.vector.z.ToString("F5");
        sendPhysString = sendPhysString + ";" + segsIk.vector.x.ToString("F5");
        sendPhysString = sendPhysString + ";" + segsIk.vector.y.ToString("F5");
        sendPhysString = sendPhysString + ";" + segsIk.vector.z.ToString("F5");

        //send command string to remote phys
        if (remotePhys != null)
        {
            if(remotePhys.runNetwork)
            {
                //TODO
                // 1 build actuation string
                // 2 broadcast actuation string
                // 3 recieve sensor string
                // 4 parse sensor string
                // 5 push sensor string tokens into sensor variables

                remotePhys.setOutMessage(sendPhysString);
            }
        }

    }

    void getRemotePhysMessages()
    {

        while (remotePhys.hasMessages())
        {
            recievePhysString = remotePhys.getNextMessage();

            string[] lines = recievePhysString.Split(';');

            // data is in Yaw (Z) Pitch (Y) Roll (X) order, So if you get
            // yaw, vertical axis
            // pitch, axis is left to right
            // roll, axis is front to back

            float z = float.Parse(lines[0]);
            float y = float.Parse(lines[1]);
            float x = float.Parse(lines[2]);

            //this order is odd, but it's from the arduinno source
            float qw = float.Parse(lines[3]);
            float qx = float.Parse(lines[4]);
            float qy = float.Parse(lines[5]);
            float qz = float.Parse(lines[6]);

            remoteBalanceRotation = new Vector3(x, z, y); //swapping z and y
            remoteTransform.rotation = Quaternion.Euler(remoteBalanceRotation);

        }

    }

    void pose(float sx, float sy, float sz, float tx, float ty, float tz, float rx, float ry, float rz)
    {
        if (poseMode == POSEMODE.ROTATE)
            rotateStance(rx, ry, rz);
        if (poseMode == POSEMODE.SEGMENTSBLIND)
            poseSegmentsBlind(sx, sy, sz);
        if (poseMode == POSEMODE.SEGMENTSINFLUENCED)
            poseSegmentsWithInfluence(sx, sy, sz);
        if (poseMode == POSEMODE.TANDRSEGMENTSINFLUENCED)
            poseSegmentsTAndRStance(sx, sy, sz, tx, ty, tz, rx, ry, rz);

        //translateStance(-posePos.x, -posePos.y, posePos.z);
        //translateAndRotateStance(-posePos.x, -posePos.y, posePos.z, -poseRot.x, poseRot.y, poseRot.z);

    }
    public void drawTestTrajectoryKeyframes(string trajName)
    {

        if (trajName == "cleartest")
        {
            for (int i = 0; i < testMarkers.Count; i++)
            {
                Destroy(testMarkers[i].gameObject);
            }
            testMarkers.Clear();
            return;
        }
        if (motionMode == MOTIONMODE.TRAJTEST)
        {
            //testMarkers.Clear();


            if (planner.trajectories.ContainsKey(trajName + "lr"))
            {
                lrTrajectory = planner.trajectories[trajName + "lr"];
                for (int i = 0; i < lrTrajectory.keyFrames.Count; i++)
                {
                    Transform another = Instantiate<Transform>(testMarker);
                    another.position = lrTrajectory.keyFrames[i];
                    another.parent = testMarker.parent;
                    testMarkers.Add(another);
                }
            }

            

            if (planner.trajectories.ContainsKey(trajName + "rr"))
            {
                rrTrajectory = planner.trajectories[trajName + "rr"];
                for (int i = 0; i < rrTrajectory.keyFrames.Count; i++)
                {
                    Transform another = Instantiate<Transform>(testMarker);
                    another.position = rrTrajectory.keyFrames[i];
                    another.parent = testMarker.parent;
                    testMarkers.Add(another);
                }
            }
        }
    }
    void processTrajectory()
    {
        if (spTrajectory != null)
        {
            if (!spTrajectory.isComplete)
            {
                if (!spTrajectory.isAPause())
                {
                    Vector3 sp = spTrajectory.getNextFrame(); // get machine space fks from Motions    
                    balanceOppositeGoal = sp.x;
                }
                else
                {
                    //ignore the frame, this is a pause traj
                    spTrajectory.getNextFrame();
                }
            }
        }


        if (lbTrajectory != null)
        {
            if (!lbTrajectory.isComplete)
            {
                if (!lbTrajectory.isAPause())
                {
                    Vector3 lb = lbTrajectory.getNextFrame(); // get machine space fks from Motions               
                    lrIk.vector.z = leftRear.doBal(lb.x).vector.z; // send those requests to components       
                }
                else
                {
                    //ignore the frame, this is a pause traj
                    lbTrajectory.getNextFrame();
                }
            }
        }
      

        if (rbTrajectory != null)
        {
            if (!rbTrajectory.isComplete)
            {
                if (!rbTrajectory.isAPause())
                {
                    Vector3 rb = rbTrajectory.getNextFrame(); // get machine space fks from Motions
                    rrIk.vector.z = rightRear.doBal(rb.x).vector.z; // send those requests to components 
                }
                else
                {
                    //ignore the frame, this is a pause traj
                    rbTrajectory.getNextFrame();
                }
            }
        }

        if (lrTrajectory != null)
        {
            if (!lrTrajectory.isComplete)
            {
                if (!lrTrajectory.isAPause())
                {
                    Vector3 lr = lrTrajectory.getNextFrame(); // get machine space fks from Motions
                    lr = lr - lrPos;  // tmp has machine space fk, need to translate to component space
                    lr = Geometry.rotateYXZ(-lrRot, lr); // do rotation side of machine_space -> component_space
                    lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z); // send those requests to components 
                }
                else
                {
                    //ignore the frame, this is a pause traj
                    lrTrajectory.getNextFrame();
                }
            }
        }



        if (rrTrajectory != null)
        {
            if (!rrTrajectory.isComplete)
            {
                if (!rrTrajectory.isAPause())
                {
                    Vector3 rr = rrTrajectory.getNextFrame(); // get machine space fks from Motions
                    rr = rr - rrPos;  // tmp has machine space fk, need to translate to component space
                    rr = Geometry.rotateYXZ(-rrRot, rr); // do rotation side of machine_space -> component_space
                    rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z); // send those requests to components 
                }
                else
                {
                    //ignore the frame, this is a pause traj
                    rrTrajectory.getNextFrame();
                }
            }
        }

        if (segsTrajectory != null)
        {
            if (!segsTrajectory.isComplete)
            {
                if (!segsTrajectory.isAPause())
                {
                    Vector3 ss = segsTrajectory.getNextFrame(); // get machine space fks from Motions            
                    segsIk = segs.doIk(ss.x, ss.z); // send those requests to components 
                }
                else
                {
                    //ignore the frame, this is a pause traj
                    segsTrajectory.getNextFrame();
                }
            }
        }



        //update stance
        updateStance();


    }
    public void runTrajectoryOnAllChannels(string trajName)
    {
        if (motionMode != MOTIONMODE.TRAJECTORY)
        {
            return;
        }
        Debug.Log("run traj: " + trajName);
        //set point trajectory
        if (planner.trajectories.ContainsKey(trajName + ".sp"))
        {
            spTrajectory = planner.trajectories[trajName + ".sp"];

            spTrajectory.resetTrajectory();
        }
        else
        {
            spTrajectory = null;
        }



        if (planner.trajectories.ContainsKey(trajName + ".lb"))
        {
            lbTrajectory = planner.trajectories[trajName + ".lb"];
            lbTrajectory.resetTrajectory();
        }
        else
        {
            lbTrajectory = null;
        }

            

        if (planner.trajectories.ContainsKey(trajName + ".rb"))
        {
            rbTrajectory = planner.trajectories[trajName + ".rb"];
            rbTrajectory.resetTrajectory();
        }
        else
        {
            rbTrajectory = null;
        }

        if (planner.trajectories.ContainsKey(trajName + ".lr"))
        {
            lrTrajectory = planner.trajectories[trajName + ".lr"];
            lrTrajectory.resetTrajectory();
        }
        else
        {
            lrTrajectory = null;
        }



        if (planner.trajectories.ContainsKey(trajName + ".rr"))
        {
            rrTrajectory = planner.trajectories[trajName + ".rr"];
            rrTrajectory.resetTrajectory();
        }
        else
        {
            rrTrajectory = null;
        }


        if (planner.trajectories.ContainsKey(trajName + ".segs"))
        {
            segsTrajectory = planner.trajectories[trajName + ".segs"];
            segsTrajectory.resetTrajectory();
        }
        else
        {
            lrTrajectory = null;
        }


    }

    void processMotions(float userInput)
    {
        if (!spMotion.currentMotionIsOver)
        {
            if (!spMotion.isPlayingPauseTraj())
            {
                Vector3 sp = spMotion.getCurrentFrame(userInput); // get machine space fks from Motions
                balanceOppositeGoal = sp.x;
            }
            else
            {
                spMotion.getCurrentFrame(userInput);
            }
        }


        if (!lbMotion.currentMotionIsOver)
        {
            if (!lbMotion.isPlayingPauseTraj())
            {
                Vector3 lb = lbMotion.getCurrentFrame(userInput); // get machine space fks from Motions
                lrIk.vector.z = leftRear.doBal(lb.x).vector.z; // send those requests to components  
            }
            else
            {
                lbMotion.getCurrentFrame(userInput);
            }
        }


        if (!rbMotion.currentMotionIsOver)
        {
            if (!rbMotion.isPlayingPauseTraj())
            {
                Vector3 rb = rbMotion.getCurrentFrame(userInput); // get machine space fks from Motions
                rrIk.vector.z = leftRear.doBal(rb.x).vector.z; // send those requests to components  
            }
            else
            {
                rbMotion.getCurrentFrame(userInput);
            }
        }

        if (!lrMotion.currentMotionIsOver)
        {
            if (!lrMotion.isPlayingPauseTraj())
            {
                Vector3 lr = lrMotion.getCurrentFrame(userInput); // get machine space fks from Motions
                lr = lr - lrPos; // tmp has machine space fk, need to translate to component space
                lr = Geometry.rotateYXZ(-lrRot, lr); // do rotation side of machine_space -> component_space
                lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z); // 
                lrStance = lrPos + Geometry.rotateZXY(lrRot, leftRear.valFk()); // update stance
            }
            else
            {
                lrMotion.getCurrentFrame(userInput);
            }
        }
        
        if (!rrMotion.currentMotionIsOver)
        {
            if (!rrMotion.isPlayingPauseTraj())
            {
                Vector3 rr = rrMotion.getCurrentFrame(userInput); // get machine space fks from Motions
                rr = rr - rrPos; // tmp has machine space fk, need to translate to component space
                rr = Geometry.rotateYXZ(-rrRot, rr); // do rotation side of machine_space -> component_space
                rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z); // 
                rrStance = rrPos + Geometry.rotateZXY(rrRot, rightRear.valFk()); // update stance
            }
            else
            {
                rrMotion.getCurrentFrame(userInput);
            }
        }

        //this is a blind motion, doesn't do anything to other channels
        //0f, -poseSegs.y, poseSegs.x; from pose call
        //segsIk = segs.doIk(y, z); in pose call
        if (!segsMotion.currentMotionIsOver)
        {
            if (!segsMotion.isPlayingPauseTraj())
            {
                Vector3 s = segsMotion.getCurrentFrame(userInput); // get machine space fks from Motions
                segsIk = segs.doIk(-s.y, s.x); //ik's sent to motors!
                segsFk = segs.valFk();

            }
            else
            {
                segsMotion.getCurrentFrame(userInput);
            }
        }

    }

    public void softCancelAllMotions()
    {
        spMotion.setInterupt(Motion.MOTIONINTERUPT.SOFTCANCEL);
        lbMotion.setInterupt(Motion.MOTIONINTERUPT.SOFTCANCEL);
        rbMotion.setInterupt(Motion.MOTIONINTERUPT.SOFTCANCEL);

        lrMotion.setInterupt(Motion.MOTIONINTERUPT.SOFTCANCEL);
        rrMotion.setInterupt(Motion.MOTIONINTERUPT.SOFTCANCEL);
        segsMotion.setInterupt(Motion.MOTIONINTERUPT.SOFTCANCEL);
    }

    public void runMotionOnAllChannels(string motionName)
    {

        isRunning = true;

        if (motionMode == MOTIONMODE.MOTION)
        {
            spMotion.startMotion(motionName + ".sp");

            lbMotion.startMotion(motionName + ".lb");
            rbMotion.startMotion(motionName + ".rb");

            lrMotion.startMotion(motionName + ".lr");
            rrMotion.startMotion(motionName + ".rr");
            segsMotion.startMotion(motionName + ".segs");
        }

    }

    void poseSegmentsTAndRStance(float sx, float sy, float sz, float tx, float ty, float tz, float rx, float ry, float rz)
    {

        //if I'm playing segs motion, don't do anything
        if (!segsMotion.currentMotionIsOver)
            return;

        //segs
        segsIk = segs.doIk(sy, sz); //ik's sent to motors!
        segsFk = segs.valFk();

        //influencing the stanceFks doesn't seem quite right.
        //I think I just keep te fks, and use getLowerInfluece to
        //calculate the current component position and rotation
        //then calculate the new component space fks, with the influenced component transforms


        lrInfluecedPosition = segs.getLowerInfluencePosition(lrPos);
        rrInfluecedPosition = segs.getLowerInfluencePosition(rrPos);

        lrInfluecedRotation = segs.getLowerInfluenceRotation(lrRot);
        rrInfluecedRotation = segs.getLowerInfluenceRotation(rrRot);


        // translate self.stance valuees
        Vector3 lr = lrStance + new Vector3(tx, ty, tz);
        Vector3 rr = rrStance + new Vector3(tx, ty, tz);


        //rotate the previous result with input rotate pose values
        lr = Geometry.rotateZXY(new Vector3(rx, ry, rz), lr);
        rr = Geometry.rotateZXY(new Vector3(rx, ry, rz), rr);


        // tmp has machine space ik, need to translate to component space
        //using the influence position for the rear  
        lr = lr - lrInfluecedPosition;
        rr = rr - rrInfluecedPosition;



        // do rotation side of machine_space -> component_space
        //using the influence rotation for rear
        lr = Geometry.rotateYXZ(-lrInfluecedRotation, lr);
        rr = Geometry.rotateYXZ(-rrInfluecedRotation, rr);



        // send those requests to components 
        lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z);
        rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z);

    }
    public void translateAndRotateStance(float x, float y, float z, float rx, float ry, float rz)
    {

        // translate self.stance valuees
        Vector3 lr = lrStance + new Vector3(x, y, z);
        Vector3 rr = rrStance + new Vector3(x, y, z);

        //rotate the previous result with input rotate pose values
        lr = Geometry.rotateZXY(new Vector3(rx, ry, rz), lr);
        rr = Geometry.rotateZXY(new Vector3(rx, ry, rz), rr);


        // tmp has machine space ik, need to translate to component space
        lr = lr - lrPos;
        rr = rr - rrPos;


        // do rotation side of machine_space -> component_space
        lr = Geometry.rotateYXZ(-lrRot, lr);
        rr = Geometry.rotateYXZ(-rrRot, rr);


        // send those requests to components 
        lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z);
        rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z);


    }
    // sends new fks to components, 4 channel equivelant of do_ik
    // this is like a temp update, doesn't actual change the state of the machine
    public void translateStance(float x, float y, float z)
    {

        // translate self.stance valuees
        Vector3 lr = lrStance + new Vector3(x, y, z);
        Vector3 rr = rrStance + new Vector3(x, y, z);


        // tmp has machine space ik, need to translate to component space
        lr = lr - lrPos;
        rr = rr - rrPos;


        // do rotation side of machine_space -> component_space
        lr = Geometry.rotateYXZ(-lrRot, lr);
        rr = Geometry.rotateYXZ(-rrRot, rr);

        // send those requests to components 
        lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z);
        rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z);

    }

    // R = 
    // |cos a   -sin a |
    // |sin a    cos a |

    // v' = Rv
    // matrix mult is rows x columns
    // sends new fks to components, 4 channel equivelant of calk_ik

    // stance contains machine space fks. here we rotate them, then convert them to component space, before sending them to the components
    public void rotateStance(float x, float y, float z)
    {


        // rotate the stance fks on user pose control
        Vector3 lr = Geometry.rotateZXY(new Vector3(x, y, z), lrStance);
        Vector3 rr = Geometry.rotateZXY(new Vector3(x, y, z), rrStance);


        // tmp has machine space fk, need to translate to component space
        lr = lr - lrPos;
        rr = rr - rrPos;


        // do rotation side of machine_space -> component_space
        lr = Geometry.rotateYXZ(-lrRot, lr);
        rr = Geometry.rotateYXZ(-rrRot, rr);


        // send those requests to components 
        lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z);
        rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z);

    }

    void poseSegmentsWithInfluence(float x, float y, float z)
    {
        //if I'm playing segs motion, don't do anything
        if (!segsMotion.currentMotionIsOver)
            return;

        //segs
        segsIk = segs.doIk(y, z); //ik's sent to motors!
        segsFk = segs.valFk();

        //influencing the stanceFks doesn't seem quite right.
        //I think I just keep te fks, and use getLowerInfluece to
        //calculate the current component position and rotation
        //then calculate the new component space fks, with the influenced component transforms


        lrInfluecedPosition = segs.getLowerInfluencePosition(lrPos);
        rrInfluecedPosition = segs.getLowerInfluencePosition(rrPos);

        lrInfluecedRotation = segs.getLowerInfluenceRotation(lrRot);
        rrInfluecedRotation = segs.getLowerInfluenceRotation(rrRot);



        // translate self.stance valuees  
        Vector3 lr = lrStance;
        Vector3 rr = rrStance;


        // tmp has machine space ik, need to translate to component space 
        //using the influence position
        lr = lr - lrInfluecedPosition;
        rr = rr - rrInfluecedPosition;


        // do rotation side of machine_space -> component_space
        //using the influence position
        lr = Geometry.rotateYXZ(-lrInfluecedRotation, lr);
        rr = Geometry.rotateYXZ(-rrInfluecedRotation, rr);



        // send those requests to components 
        lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z);
        rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z);


    }

    void poseSegmentsWithInfluenceTake1(float x, float y, float z)
    {
        //if I'm playing segs motion, don't do anything
        if (!segsMotion.currentMotionIsOver)
            return;


        //segs
        segsIk = segs.doIk(y, z); //ik's sent to motors!
        segsFk = segs.valFk();

        //influencing the stanceFks doesn't seem quite right.
        //I think I just keep te fks, and use getLowerInfluece to
        //calculate the current component position and rotation


        lrInfluecedPosition = segs.getLowerInfluencePosition(lrStance);
        rrInfluecedPosition = segs.getLowerInfluencePosition(rrStance);

        lrDeltaInflueced = lrInfluecedPosition - lrStance;
        rrDeltaInflueced = rrInfluecedPosition - rrStance;

        lrInflueceInverse = lrStance - lrDeltaInflueced;
        rrInflueceInverse = rrStance - rrDeltaInflueced;


        // translate self.stance valuees  
        Vector3 lr = lrInflueceInverse;
        Vector3 rr = rrInflueceInverse;


        // tmp has machine space ik, need to translate to component space 
        lr = lr - lrPos;
        rr = rr - rrPos;


        // do rotation side of machine_space -> component_space
        lr = Geometry.rotateYXZ(-lrRot, lr);
        rr = Geometry.rotateYXZ(-rrRot, rr);



        // send those requests to components 
        lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z);
        rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z);


    }

    void poseSegmentsBlind(float x, float y, float z)
    {

        //if I'm playing segs motion, don't do anything
        if (!segsMotion.currentMotionIsOver)
            return;

        //segs
        segsIk = segs.doIk(y, z); //ik's sent to motors!
        segsFk = segs.valFk();


    }


    // this takes component space fks, and converts to machine space
    // so the machine can make machine level decisions
    // 
    // when machine to component new_fk = component.rotation_inverse(old_fk) // rotate_yxz(-ax,-ay,-az,px,py,pz)
    // when component to machine new_fk = component.rotaion(old_fk)          // rotate_zxy(ax,ay,az,px,py,pz)
    void updateStance()
    {

        lrStance = lrPos + Geometry.rotateZXY(lrRot, leftRear.valFk());
        rrStance = rrPos + Geometry.rotateZXY(rrRot, rightRear.valFk());

        isStanceValid = true;

    }
    void updateSegs()
    {
        segsFk = segs.valFk();
        isDeckValid = true;
    }

}
