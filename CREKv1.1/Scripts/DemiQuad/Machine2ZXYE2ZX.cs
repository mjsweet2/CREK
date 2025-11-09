/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Machine2ZXYE2ZX : MonoBehaviour
{

    public ArticulatedZXYESegsDemiQuad phys;
    public string sensorString; //internal copy of sensorString sent from phys
    public SixDofPoseController sixDofPoseControllerA;
    public SixDofPoseController sixDofPoseControllerB;

    public InputDeck inputDeck;
    public MotionController motionController; //input into motion fsm


    public Machine2ZXYE2ZXTask task;
    public Machine2ZXYE2ZXPlanner planner;
    public MachineZXYEDemiQuadShell shell;

   
    public string paramJSONName;
    public BLBoneLengthList paramJSON;
    public Dictionary<string, float> boneLengths;
    public Dictionary<string, Vector3> boneHeads;

    public Transform testMarker;
    public List<Transform> testMarkers;

    public string machineName; // .name = "Trike"

    public enum BEHAVIORMODE { TRAJECTORY, MOTION, TASK, TRAJTEST, SKILL };
    public BEHAVIORMODE behaviorMode;

    //actions: whip, quadrilateral, bopping quadrilateral
    public enum POSEMODE
    {
        FRONTTANDRBLIND,
        FRONTROTATESTATICSEGS,
        SEGSBLIND, // single 2 or 3 dof
        SEGSUPPERBLIND, 
        SEGSPAIRBLIND,
        SEGSINFLUENCED,// 
        SEGSINFLUENCED2,// for action 3, during x axis seg acts tries to maintain the same front height with x rotation
        SEGSINFLUENCED3, // for action 2, during y axis seg acts tries to mainain the same angle using y axis rotations
        REARBLIND,
        REARXWITHSEGS,
        REARZWITHSEGS,
        REARXANDREARZWITHSEGS, //poseRearXAndRearZWithSegs

        TESTROTATEFRONTSHOULDERS,
        TESTSHOULDERSSEGS,
        TESTFORWARDTOBALDIFF,
        FRONTROTATEFORWARDTOBALDIFF,
        SEGSINFLUENCEDFORWARDTOBALDIFF,
        TESTFULLFORWARDTOBALDIFF
    }
    public POSEMODE poseMode;
    public enum POSEINPUTMODE
    {
        NONE,
        CONTROLLER,
        DECK,
        TASK
    }
    public POSEINPUTMODE poseInputMode;
    public enum POSEEXITMODE { SPRINGBACKPOSEEXIT, STICKPOSEEXIT }; //when implemented pose mode, so I stay or spring back to stance
    public POSEEXITMODE poseExitMode;
    public enum MOTIONCHANNELTYPE { FK, DIRECTIONU, DIRECTIONL, POSEPOSITION, POSEROTATE };


    public ComponentZXYE leftFront;// self.left_front = ComponentTypeXYE()
    public ComponentZXYE rightFront;// self.right_front = ComponentTypeXYE()
    public ComponentZX leftRear;
    public ComponentZX rightRear;
    public CompSegs segs;
    public Quadrilateral rearQuadrilateral;

    public Vector3 lfPos;// self.lf_pos = []    # component positions
    public Vector3 rfPos;// self.rf_pos = []
    public Vector3 lrPos;
    public Vector3 rrPos;

    public Vector3 segsPos;
    public Vector3 segsBasePos; // position of base end effector on single point base in relationship to bottom of segs
                                // mostly for a purely Trike implementation



    public Vector3 lfRot;// self.lf_rot = []    # component rotations
    public Vector3 rfRot;// self.rf_rot = []
    public Vector3 segsRot;
    public Vector3 lrRot;// self.lf_rot = []    # component rotations
    public Vector3 rrRot;// self.rf_rot = []


    public Vector3 lfStance;// 
    public Vector3 rfStance;// 
    public Vector3 lrStance;// 
    public Vector3 rrStance;// 
    public Vector3 segsStance;  
    public Vector3 segsInputStance; //this is the requested input Stance



    public Vector3 segsInfluencedPosition;
    public Vector3 lrInfluecedPosition;
    public Vector3 rrInfluecedPosition;

    public Vector3 segsInfluencedRotation;
    public Vector3 lrInfluecedRotation;
    public Vector3 rrInfluecedRotation;

    public Vector3 segsDeltaInfluenced;
    public Vector3 lrDeltaInflueced;
    public Vector3 rrDeltaInflueced;

    public Vector3 segsInfluenceInverse;
    public Vector3 lrInfluenceInverse;
    public Vector3 rrInfluenceInverse;

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
    public Vector3 lfFk;
    public Vector3 rfFk;
    public Vector3 lrFk;
    public Vector3 rrFk;
    public Vector3 segsFk; // this is computed from both sets of segs

    //component iks
    public Geometry.GeoPacket lfIk;
    public Geometry.GeoPacket rfIk;
    public Geometry.GeoPacket lrIk;
    public Geometry.GeoPacket rrIk;
    public Geometry.GeoPacket segsIk;
    

    public bool isRunning;

    public Trajectory lfTrajectory;
    public Trajectory rfTrajectory;
    public Trajectory lrTrajectory;
    public Trajectory rrTrajectory;
    public Trajectory segsTrajectory;


    
    public List<string> motionChannels;
    public List<Machine2ZXYE2ZXMotion> motions;
    public List<MOTIONCHANNELTYPE> motionChannelTypes; //MOTIONCHANNELTYPE  { FK, DIRECTIONU, DIRECTIONL, POSEPOSITION, POSEROTATE };

    public RLSkillRig rlSkillRig;

    public delegate void ShellCmd();
    public Dictionary<string, ShellCmd> functions;



    //troubleshooting marker
    public Transform dormantFKMarker;
    public Transform segsInfluenceMarker;
    public Vector3 segsInfluencePosition;

    // Start is called before the first frame update
    void Start()
    {
        buildTables();
        loadParams();
        planner.buildTables();


    }

    // Update is called once per frame
    void Update()
    {

        processMotionInput();

        if (behaviorMode == BEHAVIORMODE.TRAJECTORY)
        {
            tickTrajectories();
        }

        if (behaviorMode == BEHAVIORMODE.MOTION )
        {
            tickMotionChannels(motionController.controlValue);
        }

        if (behaviorMode == BEHAVIORMODE.SKILL)
        {
            tickSkill();
        }

        if (behaviorMode == BEHAVIORMODE.TASK)
        {
            if (task.isRunningMotionNode())
            {
                tickMotionChannels(motionController.controlValue);
            }

            if (task.isRunningSkillNode())
            {
                tickSkill();
            }

        }

        // no real time posing while I"m running an RL Skill
        // it messes up the actuator values
        if (behaviorMode != BEHAVIORMODE.SKILL)
        {
            processPoseInput();
        }


        broadcastMessage();
        getPhysicalSensors();

        

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
        functions.Add("stopskill", stopSkill);

        //behavior modes
        functions.Add("settotrajectorymode", setToTrajectoryMode);
        functions.Add("settomotionmode", setToMotionMode);
        functions.Add("settoskillmode", setToSkillMode);
        functions.Add("settotaskmode", setToTaskMode);


        //pose modes
        functions.Add("settosegsblindmode", setToSegsBlindMode);



    }
    public void loadParams()
    {
        importBoneLengthsJSON();

        //  set lengths
        leftFront.loadParams(boneLengths["leftfront.x"], boneLengths["leftfront.y"], boneLengths["leftfront.e"]); //leftFront.loadParams(0.28f, 0.29f);
        //  set lengths
        rightFront.loadParams(boneLengths["rightfront.x"], boneLengths["rightfront.y"], boneLengths["rightfront.e"]); //rightFront.loadParams(0.28f, 0.29f);

        leftRear.loadParams(boneLengths["leftrear.x"]);
        rightRear.loadParams(boneLengths["rightrear.x"]);



        //segs.loadParams(.03f, -.0325f, 0f);
        float topBlock = (boneHeads["u4"] - boneHeads["machineorigin"]).magnitude;
        float middleblock = boneLengths["u5"];
        segs.loadParams(boneLengths["l1"], topBlock, middleblock, 0f);
        segsBasePos = new Vector3(0f, -.18f, -.25f);



        //  component placement
        lfPos = boneHeads["leftfront.x"] - boneHeads["machineorigin"]; // new Vector3(-.152f, 0f, .2675f); //  unity space
        rfPos = boneHeads["rightfront.x"] - boneHeads["machineorigin"]; // new Vector3(.152f, 0f, .2675f);
        lrPos = boneHeads["leftrear.base"] - boneHeads["machineorigin"]; // new Vector3(-.075f, 0f, -.2675f);
        rrPos = boneHeads["rightrear.base"] - boneHeads["machineorigin"]; // new Vector3(.075f, 0f, -.2675f);


        lfRot = new Vector3(0.0f, 0.0f, 0.0f); //  unity space, radians //  Z, X ,Y  order.  // 30 deg = (2f * Mathf.PI) / 12f
        rfRot = new Vector3(0.0f, 0.0f, 0.0f);
        lrRot = new Vector3(0.0f, 0.0f, 0.0f); 
        rrRot = new Vector3(0.0f, 0.0f, 0.0f);



        leftFront.compName = "leftFront";
        rightFront.compName = "rightFront";
        segs.compName = "segs";
        leftRear.compName = "leftRear";
        rightRear.compName = "rightRear";



        //ini the comps
        Vector3 startLeftFront = dormantLFFk();
        Vector3 startRightFront = dormantRFFk();

        Vector3 startLeftRear = dormantLRFk();
        Vector3 startRightRear = dormantRRFk();
        
        lfIk = leftFront.doLeftIk(startLeftFront.x, startLeftFront.y, startLeftFront.z);
        rfIk = rightFront.doRightIk(startRightFront.x, startRightFront.y, startRightFront.z);
        segsIk = segs.doIk(0f, 0f);
        lrIk = leftRear.doLeftIk(startLeftRear.x, startLeftRear.y, startLeftRear.z);
        rrIk = rightRear.doRightIk(startRightRear.x, startRightRear.y, startRightRear.z);

        

        lfFk = leftFront.valFk();
        rfFk = rightFront.valFk();
        segsFk = segs.valFk();
        lrFk = leftRear.valFk();
        rrFk = rightRear.valFk();


        //put the rear legs in direction mode
        leftRear.ikMode = ComponentZX.IKMODE.DIRECTION;
        rightRear.ikMode = ComponentZX.IKMODE.DIRECTION;


    }

    public void importBoneLengthsJSON()
    {
        StreamReader sReader;
        string path = "c://crek//" + paramJSONName;//  BLSphere8Quadsv2.json"; //Application.persistentDataPath + "/trajectory/" + trajectoryName + ".json";
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

    //should I move this to SkillRig
    public void stopSkill() { rlSkillRig.runningState = RLSkillRig.RUNNINGSTATE.STOPPED; }

    public void setToTrajectoryMode() { behaviorMode = BEHAVIORMODE.TRAJECTORY; }
    public void setToMotionMode() { behaviorMode = BEHAVIORMODE.MOTION; }
    public void setToSkillMode() { behaviorMode = BEHAVIORMODE.SKILL; }
    public void setToTaskMode() { behaviorMode = BEHAVIORMODE.TASK; }



    public void setToSegsBlindMode() { poseMode = POSEMODE.SEGSBLIND; }


    public Vector3 dormantLFFk()
    {
        Vector3 ret = leftFront.dormantFk();
        // ret has component space fk, need to translate to machine_space
        // do rotation side of component_space -> machine_space
        ret = Geometry.rotateZXY(lfRot, ret);
        ret += lfPos;

        return ret;
    }
    public Vector3 dormantLRFk()
    {
        return lrPos;
    }
    public Vector3 dormantRFFk()
    {
        Vector3 ret = rightFront.dormantFk();

        // ret has component space fk, need to translate to machine_space
        // do rotation side of component_space -> machine_space
        ret = Geometry.rotateZXY(rfRot, ret);
        ret += rfPos;

        return ret;
    }
    public Vector3 dormantRRFk()
    {
        return rrPos;
    }

    public Vector3 dormantSegsFk()
    {
        Vector3 ret = segs.dormantFk + segsBasePos;
        // ret has component space fk, need to translate to machine_space
        // do rotation side of component_space -> machine_space
        ret = Geometry.rotateZXY(segsRot, ret);
        ret += segsPos;

        return ret;

    }

    public Vector3 getlfStance() { return lfStance; }
    public Vector3 getlrStance() { return lrStance; }
    public Vector3 getrfStance() { return rfStance; }
    public Vector3 getrrStance() { return rrStance; }
    public Vector3 getSegsStance() { return segsStance; }

    public Vector3 getlfLower()
    {
        Vector3 ret = Geometry.rotateZXY(lfRot, leftFront.lDirVector());
        return ret;
    }
    public Vector3 getlfUpper()
    {
        Vector3 ret = Geometry.rotateZXY(lfRot, leftFront.uDirVector());
        return ret;
    }
    public Vector3 getrfLower()
    {
        Vector3 ret = Geometry.rotateZXY(rfRot, rightFront.lDirVector());
        return ret;
    }
    public Vector3 getrfUpper()
    {

        Vector3 ret = Geometry.rotateZXY(rfRot, rightFront.uDirVector());
        return ret;
    }

    public Vector3 lfToMachine(Vector3 lf)
    {

        // component space fk, need to translate to machine_space space
        // do rotation side of component_space -> machine_space
        Vector3 ret = Geometry.rotateZXY(lfRot, lf);
        ret += lfPos;

        return ret;
    }
    public Vector3 lrToMachine(Vector3 lr)
    {
        // component space fk, need to translate to machine_space space
        // do rotation side of component_space -> machine_space
        Vector3 ret = Geometry.rotateZXY(lrRot, lr);
        ret += lrPos;

        return ret;
    }
    public Vector3 rfToMachine(Vector3 rf)
    {
        // component space fk, need to translate to machine_space space
        // do rotation side of component_space -> machine_space
        Vector3 ret = Geometry.rotateZXY(rfRot, rf);
        ret += rfPos;

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
    public Vector3 machineTolf(Vector3 machine)
    {
        // machine_space fk, need to translate to component_space space
        Vector3 ret = machine - lfPos;

        // do rotation side of machine_space -> component_space
        ret = Geometry.rotateYXZ(-lfRot, ret);

        return ret;
    }
    public Vector3 machineTolr(Vector3 machine)
    {
        // machine_space fk, need to translate to component_space space
        Vector3 ret = machine - lrPos;

        return ret;
    }
    public Vector3 machineTorf(Vector3 machine)
    {
        // machine_space fk, need to translate to component_space space
        Vector3 ret = machine - rfPos;

        // do rotation side of machine_space -> component_space
        ret = Geometry.rotateYXZ(-rfRot, ret);

        return ret;
    }
    public Vector3 machineTorr(Vector3 machine)
    {
        // machine_space fk, need to translate to component_space space
        Vector3 ret = machine - rrPos;

        return ret;
    }
    void processPoseInput()
    {

        //ignore mouse and rs, get pose info from taskMessageService;
        if (poseInputMode == POSEINPUTMODE.TASK)
        {
            ;//pose input already set in processTaskMessages()

        }
        else
        {
            if (sixDofPoseControllerA.isTracking()) //this is started/stopped at mouse click
            {
                posePos = sixDofPoseControllerA.subjectPos;
                poseRot = sixDofPoseControllerA.subjectRot.eulerAngles * Mathf.Deg2Rad;
            }
            else
            {
                //keep pose from pose control
                if (sixDofPoseControllerA.isTrackingComplete())
                {
                    sixDofPoseControllerA.resetTracking();
                    updateStance();
                }


                posePos = poseRot = Vector3.zero;

            }

            if (sixDofPoseControllerB.isTracking()) //this is started/stopped at mouse click
            {
                poseSegs = sixDofPoseControllerB.subjectPos;
            }
            else
            {

                if (sixDofPoseControllerB.isTrackingComplete())
                {
                    sixDofPoseControllerB.resetTracking();
                    updateStance();
                }

                poseSegs = Vector3.zero;

            }

        }

        // anytime you send a trajectory to a component you have to update the stance.
        // the stance is basically saved fk info, and if you update the trajectory at the component level, you need a new stance.
        // here pose input is just like a temporary adjustment to the trajectory, which doesn't update the stance
        // but "jump" switches the current trajectory, needing an updated stance


        //calculates machine space ik
        if (isStanceValid == false)
        {
            lfIk = leftFront.doLeftIk(lfFk.x, lfFk.y, lfFk.z); // this is a component space fk
            rfIk = rightFront.doRightIk(rfFk.x, rfFk.y, rfFk.z); // this is a component space fk
            updateStance();
        }

        pose(poseSegs.x, poseSegs.y, poseSegs.z, -posePos.x, -posePos.y, posePos.z, -poseRot.x, poseRot.y, poseRot.z);

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
        if (phys == null)
            return;

        // iks are in lf_ik, etc...
        phys.processTripleMessage("1", lfIk.vector.x, lfIk.vector.y, lfIk.vector.z);
        phys.processTripleMessage("2", rfIk.vector.x, rfIk.vector.y, rfIk.vector.z);

        phys.processTripleMessage("5", segsIk.vector.x, segsIk.vector.y, segsIk.vector.z);
        phys.processTripleMessage("6", segsIk.vector.w, 0f, 0f);

        phys.processTripleMessage("3", lrIk.vector.x, lrIk.vector.y, lrIk.vector.z);
        phys.processTripleMessage("4", rrIk.vector.x, rrIk.vector.y, rrIk.vector.z);

        //Y actuation of front components
        phys.processTripleMessage("8", lfIk.vector.w, 0f, 0f);
        phys.processTripleMessage("9", rfIk.vector.w, 0f, 0f);


    }
    void getPhysicalSensors()
    {
        sensorString = phys.sensorString;
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
        if (behaviorMode == BEHAVIORMODE.TRAJTEST)
        {
            //testMarkers.Clear();
            if (planner.trajectories.ContainsKey(trajName + "lf"))
            {
                lfTrajectory = planner.trajectories[trajName + "lf"];
                for (int i = 0; i < lfTrajectory.keyFrames.Count; i++)
                {
                    Transform another = Instantiate<Transform>(testMarker);
                    another.position = lfTrajectory.keyFrames[i];
                    another.parent = testMarker.parent;
                    testMarkers.Add(another);
                }
            }



            if (planner.trajectories.ContainsKey(trajName + "rf"))
            {
                rfTrajectory = planner.trajectories[trajName + "rf"];
                for (int i = 0; i < rfTrajectory.keyFrames.Count; i++)
                {
                    Transform another = Instantiate<Transform>(testMarker);
                    another.position = rfTrajectory.keyFrames[i];
                    another.parent = testMarker.parent;
                    testMarkers.Add(another);
                }
            }

        }
    }
    void tickTrajectories()
    {

        if (lfTrajectory != null)
        {
            if (!lfTrajectory.isComplete)
            {
                if (!lfTrajectory.isAPause())
                {
                    Vector3 lf = lfTrajectory.getNextFrame(); // get machine space fks from Motions
                    lf = lf - lfPos;  // tmp has machine space fk, need to translate to component space
                    lf = Geometry.rotateYXZ(-lfRot, lf); // do rotation side of machine_space -> component_space
                    lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z); // send those requests to components 

                }
                else
                {
                    //ignore the frame, this is a pause traj
                    lfTrajectory.getNextFrame();

                }
            }
        }



        if (rfTrajectory != null)
        {
            if (!rfTrajectory.isComplete)
            {
                if (!rfTrajectory.isAPause())
                {
                    Vector3 rf = rfTrajectory.getNextFrame(); // get machine space fks from Motions
                    rf = rf - rfPos;  // tmp has machine space fk, need to translate to component space
                    rf = Geometry.rotateYXZ(-rfRot, rf); // do rotation side of machine_space -> component_space
                    rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z); // send those requests to components 
                }
                else
                {
                    //ignore the frame, this is a pause traj
                    rfTrajectory.getNextFrame();
                }
            }
        }



        //update stance
        updateStance();


    }
    public void runTrajectoryOnAllChannels(string trajName)
    {
        if (behaviorMode == BEHAVIORMODE.TRAJECTORY)
        {
            if (planner.trajectories.ContainsKey(trajName + "lf"))
            {
                lfTrajectory = planner.trajectories[trajName + "lf"];
                lfTrajectory.resetTrajectory();
            }
            else
            {
                lfTrajectory = null;
            }


            if (planner.trajectories.ContainsKey(trajName + "rf"))
            {

                rfTrajectory = planner.trajectories[trajName + "rf"];
                rfTrajectory.resetTrajectory();
            }
            else
            {
                rfTrajectory = null;
            }


        }
    }

    
    void tickSkill()
    {
        if (rlSkillRig.runningState == RLSkillRig.RUNNINGSTATE.RUNNING)
        {
            rlSkillRig.tickSkill();
        }
    }




    // This function is used by Task to see if all motions are completed
    // as an alternative to the asynchronous TaskMessageService which I think might be to complex
    public bool allMotionsCompleted()
    {
        bool allCompleted = true;
        //inc'ing is seperate from consuming
        for (int i = 0; i < motions.Count; i++)
        {
            allCompleted = allCompleted & motions[i].currentMotionIsOver;
        }
        return allCompleted;
    }

    void tickMotionChannels(float userInput)
    {
        
        //inc'ing motions is seperate from consuming frame
        for (int i = 0; i < motions.Count; i++)
        {
            motions[i].tick(userInput);
        }


        // 0 is lfMotion
        if (!motions[0].currentMotionIsOver)
        {
            if (!motions[0].isPlayingPauseTraj())
            {
                Vector3 lf = motions[0].getCurrentFrame(userInput); // get machine space fks from Motions
                lf = lf - lfPos; // tmp has machine space fk, need to translate to component space
                lf = Geometry.rotateYXZ(-lfRot, lf); // do rotation side of machine_space -> component_space
                lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z); // 
                lfStance = lfPos + Geometry.rotateZXY(lfRot, leftFront.valFk()); // update stance

            }

            else
            {
                motions[0].getCurrentFrame(userInput);
            }
        }

        // 1 is rfMotion
        if (!motions[1].currentMotionIsOver)
        {
            if (!motions[1].isPlayingPauseTraj())
            {
                Vector3 rf = motions[1].getCurrentFrame(userInput); // get machine space fks from Motions
                rf = rf - rfPos; // tmp has machine space fk, need to translate to component space
                rf = Geometry.rotateYXZ(-rfRot, rf); // do rotation side of machine_space -> component_space
                rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z); // 
                rfStance = rfPos + Geometry.rotateZXY(rfRot, rightFront.valFk()); // update stance
            }
            else
            {
                motions[1].getCurrentFrame(userInput);
            }
        }


        //this is a blind motion, doesn't do anything to other channels
        //0f, -poseSegs.y, poseSegs.x; from pose call
        //segsIk = segs.doIk(y, z); in pose call
        // 2 is segsMotion
        if (!motions[2].currentMotionIsOver)
        {
            if (!motions[2].isPlayingPauseTraj())
            {
                Vector3 s = motions[2].getCurrentFrame(userInput); // get machine space fks from Motions
                segsIk = segs.doIk(-s.y, s.x); //ik's sent to motors!
                segsFk = segs.valFk();

            }
            else
            {
                motions[2].getCurrentFrame(userInput);
            }
        }


        // 3 is lrMotion
        if (!motions[3].currentMotionIsOver)
        {
            if (!motions[3].isPlayingPauseTraj())
            {
                Vector3 lr = motions[3].getCurrentFrame(userInput); // get machine space fks from Motions
                lr = lr - lrPos; // tmp has machine space fk, need to translate to component space
                lr = Geometry.rotateYXZ(-lrRot, lr); // do rotation side of machine_space -> component_space
                lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z); // 
                lrStance = lrPos + Geometry.rotateZXY(lrRot, leftRear.valFk()); // update stance

            }

            else
            {
                motions[3].getCurrentFrame(userInput);
            }
        }

        // 4 is rrMotion
        if (!motions[4].currentMotionIsOver)
        {
            if (!motions[4].isPlayingPauseTraj())
            {
                Vector3 rr = motions[4].getCurrentFrame(userInput); // get machine space fks from Motions
                rr = rr - rrPos; // tmp has machine space fk, need to translate to component space
                rr = Geometry.rotateYXZ(-rrRot, rr); // do rotation side of machine_space -> component_space
                rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z); // 
                rrStance = rrPos + Geometry.rotateZXY(rrRot, rightRear.valFk()); // update stance
            }
            else
            {
                motions[4].getCurrentFrame(userInput);
            }
        }







        // 5 is lflMotion, uses direction trajectories, no position translation in space conversion
        if (!motions[5].currentMotionIsOver)
        {
            if (!motions[5].isPlayingPauseTraj())
            {
                Vector3 lfl = motions[5].getCurrentFrame(userInput); // get machine space fks from Motions 
                lfl = Geometry.rotateYXZ(-lfRot, lfl); // do rotation side of machine_space -> component_space
                lfIk = leftFront.doLeftLowerDir(lfl); // 
                lfStance = lfPos + Geometry.rotateZXY(lfRot, leftFront.valFk()); // update stance
            }

            else
            {
                motions[5].getCurrentFrame(userInput);
            }
        }
        // 6 is lfuMotion, uses direction trajectories, no position translation in space conversion
        if (!motions[6].currentMotionIsOver)
        {
            if (!motions[6].isPlayingPauseTraj())
            {
                Vector3 lfu = motions[6].getCurrentFrame(userInput); // get machine space fks from Motions 
                lfu = Geometry.rotateYXZ(-lfRot, lfu); // do rotation side of machine_space -> component_space
                lfIk = leftFront.doLeftUpperDir(lfu); // 
                lfStance = lfPos + Geometry.rotateZXY(lfRot, leftFront.valFk()); // update stance
            }

            else
            {
                motions[6].getCurrentFrame(userInput);
            }
        }
        // 7 is rflMotion, uses direction trajectories, no position translation in space conversion
        if (!motions[7].currentMotionIsOver)
        {
            if (!motions[7].isPlayingPauseTraj())
            {
                Vector3 rfl = motions[7].getCurrentFrame(userInput); // get machine space fks from Motions 
                rfl = Geometry.rotateYXZ(-rfRot, rfl); // do rotation side of machine_space -> component_space
                rfIk = rightFront.doRightLowerDir(rfl); // 
                rfStance = rfPos + Geometry.rotateZXY(rfRot, rightFront.valFk()); // update stance
            }

            else
            {
                motions[7].getCurrentFrame(userInput);
            }
        }
        // 8 is rfuMotion, uses direction trajectories, no position translation in space conversion
        if (!motions[8].currentMotionIsOver)
        {
            if (!motions[8].isPlayingPauseTraj())
            {
                Vector3 rfu = motions[8].getCurrentFrame(userInput); // get machine space fks from Motions 
                rfu = Geometry.rotateYXZ(-rfRot, rfu); // do rotation side of machine_space -> component_space
                rfIk = rightFront.doRightUpperDir(rfu); // 
                rfStance = rfPos + Geometry.rotateZXY(rfRot, rightFront.valFk()); // update stance
            }

            else
            {
                motions[8].getCurrentFrame(userInput);
            }
        }

    }

    public void softCancelAllMotions()
    {

        for (int i = 0; i < motions.Count; i++)
        {
            motions[i].setInterupt(Machine2ZXYE2ZXMotion.MOTIONINTERUPT.SOFTCANCEL);
        }

    }

    public void runMotionOnAllChannels(string motionName)
    {
        isRunning = true;

        if (behaviorMode == BEHAVIORMODE.MOTION | behaviorMode == BEHAVIORMODE.TASK)
        {
            for (int i = 0; i < motions.Count; i++)
            {
                motions[i].startMotion(motionName + "." + motionChannels[i]);
            }
        }
    }



    void pose(float sx, float sy, float sz, float tx, float ty, float tz, float rx, float ry, float rz)
    {

     

        if (poseMode == POSEMODE.FRONTTANDRBLIND) //take 1, needs to be tested
            translateAndRotateFrontBlind(tx, ty, tz, rx, ry, rz);
        if (poseMode == POSEMODE.FRONTROTATESTATICSEGS) //works 
            rotateFrontStaticSegs(rx, ry, rz);



        if (poseMode == POSEMODE.SEGSBLIND) 
            poseSegmentsBlind(sx, sy, sz);
        if (poseMode == POSEMODE.SEGSUPPERBLIND) 
            poseSegmentsUpperBlind(sx, sy, sz);
        if (poseMode == POSEMODE.SEGSPAIRBLIND) 
            poseSegmentsPairBlind(sx, sy, sz);


        if (poseMode == POSEMODE.SEGSINFLUENCED) //works
            poseSegmentsWithInfluence(sx, sy, sz);
        if (poseMode == POSEMODE.SEGSINFLUENCED2)
            poseSegmentsWithInfluence2(sx, sy, sz);
        if (poseMode == POSEMODE.SEGSINFLUENCED3)
            poseSegmentsWithInfluence3(sx, sy, sz);



        if (poseMode == POSEMODE.REARBLIND)
            poseRearBlindUprightStance(sx, sy, sz);
        if (poseMode == POSEMODE.REARXWITHSEGS)
            poseRearXWithSegs(sx, sy, sz);
        if (poseMode == POSEMODE.REARZWITHSEGS)
            poseRearZWithSegs(sx, sy, sz);
        if (poseMode == POSEMODE.REARXANDREARZWITHSEGS)
            poseRearXAndRearZWithSegs(sx, sy, sz);






        if (poseMode == POSEMODE.TESTROTATEFRONTSHOULDERS)
        {
            testPoseRotateFrontShoulders(rx, ry, rz);
        }
        if (poseMode == POSEMODE.TESTSHOULDERSSEGS)
        {
            testPoseShouldersSegs(sx, sy, sz, rx, ry, rz);
        }
        if (poseMode == POSEMODE.TESTFORWARDTOBALDIFF)
        {
            testPoseForwardToBalDiff(tx, ty, tz, rx, ry, rz);
        }
        if (poseMode == POSEMODE.FRONTROTATEFORWARDTOBALDIFF)
        {
            poseFrontForwardToBalDiff(sx, sy, sz, rx, ry, rz);
        }
        if (poseMode == POSEMODE.SEGSINFLUENCEDFORWARDTOBALDIFF)
        {
            poseSegsInfluencedForwardToBalDiff(sx, sy, sz, rx, ry, rz);
        }
        if (poseMode == POSEMODE.TESTFULLFORWARDTOBALDIFF)
        {
            poseFrontSegsInfluencedForwardToBD(sx, sy, sz, rx, ry, rz);
        }

    }
    void poseRearXWithSegs(float sx, float sy, float sz)
    {
        //as of 2/9/25
        //this pose is a guestimate for experimations


        //segs


        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
            return;

        float tempX = segsInputStance.x + sy;
        float tempZ = segsInputStance.z; // + sx; 0 for now, since this pose mode is x only
        float tempUpperX = segsInputStance.y + 2 * sz;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ, tempUpperX); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector




        //rear components

        Vector3 lr = lrStance;
        Vector3 rr = rrStance;

        lr = segs.getLowerDeltaPositionInverse(lr);
        rr = segs.getLowerDeltaPositionInverse(rr);

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

    void poseRearZWithSegs(float sx, float sy, float sz)
    {
        //as of 2/9/25
        //this pose is a guestimate for experimations


        //segs


        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
            return;

        float tempX = segsInputStance.x; // + sy; // since this pose mode is x only
        float tempZ = segsInputStance.z + sx;
        float tempUpperX = segsInputStance.y; // + 2 * sz;  // since this pose mode is x only

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ, tempUpperX); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector




        //rear components

        Vector3 lr = lrStance;
        Vector3 rr = rrStance;

        lr = segs.getLowerDeltaPositionInverse(lr);
        rr = segs.getLowerDeltaPositionInverse(rr);

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
    void poseRearXAndRearZWithSegs(float sx, float sy, float sz)
    {
        //as of 2/9/25
        //this pose is a guestimate for experimations


        //rear components

        Vector3 lr = lrStance;
        Vector3 rr = rrStance;

        // tmp has machine space ik, need to translate to component space
        lr = lr - lrPos;
        rr = rr - rrPos;


        // do rotation side of machine space -> component space
        lr = Geometry.rotateYXZ(-lrRot, lr);
        rr = Geometry.rotateYXZ(-rrRot, rr);


        lr = Geometry.rotateAroundXAxis(sy * .08f, lr);
        rr = Geometry.rotateAroundXAxis(sy * .08f, rr);



        // send those requests to components 
        // lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z);
        // rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z);

        //segs





        float tempX = segsInputStance.x; // + sy; // since this pose mode is x only
        float tempZ = segsInputStance.z + sx;
        float tempUpperX = segsInputStance.y; // + 2 * sz;  // since this pose mode is x only

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ, tempUpperX); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector



        // do rotation side of component space -> machine space
        lr = Geometry.rotateZXY(lrRot, lr);
        rr = Geometry.rotateZXY(rrRot, rr);

        // component space ik, need to translate to machine space
        lr = lr + lrPos;
        rr = rr + rrPos;

        lr = segs.getLowerDeltaPositionInverse(lr);
        rr = segs.getLowerDeltaPositionInverse(rr);

        // tmp has machine space ik, need to translate to component space
        lr = lr - lrPos;
        rr = rr - rrPos;


        // do rotation side of machine space -> component space
        lr = Geometry.rotateYXZ(-lrRot, lr);
        rr = Geometry.rotateYXZ(-rrRot, rr);


        // send those requests to components 
        lrIk = leftRear.doLeftIk(lr.x, lr.y, lr.z);
        rrIk = rightRear.doRightIk(rr.x, rr.y, rr.z);
        




    }
    void poseRearBlindUprightStance(float rx, float ry, float rz)
    {
        //the most useful case for this is when standing,
        //so for now I'm going to assume that rear components are
        //symmetrical from left to right, and they are turned 45 degrees downward, or rearward


        //TODO, work with these values as if they rear components are 45 degrees down/rear ward

        Vector3 lr = lrStance;
        Vector3 rr = rrStance;

        //flatten for quadrilateral
        lr.z -= lrPos.z;
        rr.z -= rrPos.z;
        lr.y -= lrPos.y;
        rr.y -= rrPos.y;



        rearQuadrilateral.upperLength = rrPos.x - lrPos.x;




        rearQuadrilateral.setDormantPositions(lr, rr);
        rearQuadrilateral.setStance((lr - rr).magnitude);

        rearQuadrilateral.inputRotationX = ry * .01f;
        rearQuadrilateral.inputRotationZ = rz * .01f;



        rearQuadrilateral.doRotations();


        lr = rearQuadrilateral.lowerLeftPosition;
        rr = rearQuadrilateral.lowerRightPosition;


        //I flattened to to quadrilateral to do calcs, now put it back
        lr.z += lrPos.z;
        rr.z += rrPos.z;
        lr.y += lrPos.y;
        rr.y += rrPos.y;






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

    void poseRearBlindDormantStance(float rx, float ry, float rz)
    {
        

        Vector3 lr = lrStance;
        Vector3 rr = rrStance;

        //flatten for quadrilateral
        lr.z -= lrPos.z;
        rr.z -= rrPos.z;
        lr.y -= lrPos.y;
        rr.y -= rrPos.y;



        rearQuadrilateral.upperLength = rrPos.x - lrPos.x;

        
        rearQuadrilateral.setDormantPositions(lr, rr);
        rearQuadrilateral.setStance((lr - rr).magnitude);

        rearQuadrilateral.inputRotationX = ry * .01f;
        rearQuadrilateral.inputRotationZ = rz * .01f;


        
        rearQuadrilateral.doRotations();

        
        lr = rearQuadrilateral.lowerLeftPosition;
        rr = rearQuadrilateral.lowerRightPosition;


        //I flattened to to quadrilateral to do calcs, now put it back
        lr.z += lrPos.z;
        rr.z += rrPos.z;
        lr.y += lrPos.y;
        rr.y += rrPos.y;



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
    void poseRotateFrontSegsInfluenced(float sx, float sy, float sz, float tx, float ty, float tz, float rx, float ry, float rz)
    {
        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
            return;

        float tempX = segsInputStance.x + sy;
        float tempZ = segsInputStance.z + sz;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector


        //need to move lf and rf to to keep the same triangular stance
        //do I just need to calc the difference and add it to lf and rf?

        Vector3 currentSegsStance = segsPos + Geometry.rotateZXY(segsRot, segs.getLowerDeltaPositionInverse(segs.dormantFk + segsBasePos));

        Vector3 segsDiff = currentSegsStance - segsStance;

        Vector3 lf = lfStance + segsDiff;
        Vector3 rf = rfStance + segsDiff;



        ///////////////////////////////////////////////////////////////////////////////////////
        ///TODO, the front/back segs actuation works, can I make the side/side actuation work?




        /////////////////////////////////////////////////////////////////////////////
        //segs posed, now rotate with lf and rf


        // subtract the base stance to work about the base
        // translate self.stance values
        lf = lf - segsStance;
        rf = rf - segsStance;

        // Z axis, the X axis, and the Y axis, in that order. 

        lf = Geometry.rotateAroundZAxis(rz, lf);
        lf = Geometry.rotateAroundXAxis(-rx, lf);
        lf = Geometry.rotateAroundYAxis(ry, lf);


        rf = Geometry.rotateAroundZAxis(rz, rf);
        rf = Geometry.rotateAroundXAxis(-rx, rf);
        rf = Geometry.rotateAroundYAxis(ry, rf);


        //convert to machine space
        lf = lf + segsStance;
        rf = rf + segsStance;



        /////////////////////////////////////////////////////////////////////////////
        //end of front rotate


        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;



        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);


        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);



    }

    void poseRotateFrontSegsInfluenced2(float sx, float sy, float sz, float tx, float ty, float tz, float rx, float ry, float rz)
    {
        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
            return;

        float tempX = segsInputStance.x + sy;
        float tempZ = segsInputStance.z + sz;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector


        //need to move lf and rf to to keep the same triangular stance
        //do I just need to calc the difference and add it to lf and rf?

        Vector3 currentSegsStance = segsPos + Geometry.rotateZXY(segsRot, segs.getLowerDeltaPositionInverse(segs.dormantFk + segsBasePos));

        Vector3 segsDiff = currentSegsStance - segsStance;

        Vector3 lf = lfStance + segsDiff;
        Vector3 rf = rfStance + segsDiff;

        ///////////////////////////////////////////////////////////////////////////////////////
        //rotate the stance to keep the front height the same-ish?

        Vector3 frontCenter = (lfStance + rfStance) / 2f;
        Vector3 currentFrontCenter = (lf + rf) / 2f;
        Vector3 diff = currentFrontCenter - frontCenter;
        float stanceLength = frontCenter.z - currentSegsStance.z;
        float radians = Mathf.Sin(diff.y / stanceLength);



        // subtract the base stance to work about the base
        // translate self.stance values
        lf = lf - currentSegsStance;
        rf = rf - currentSegsStance;


        //Unity rotation order: Z axis, the X axis, and the Y axis, in that order. 
        lf = Geometry.rotateAroundXAxis(radians, lf);
        rf = Geometry.rotateAroundXAxis(radians, rf);



        //convert to machine space
        lf = lf + currentSegsStance;
        rf = rf + currentSegsStance;


        ///////////////////////////////////////////////////////////////////////////////////////
        ///TODO, the front/back segs actuation works, can I make the side/side actuation work?




        /////////////////////////////////////////////////////////////////////////////
        //segs posed, now rotate with lf and rf


        // subtract the base stance to work about the base
        // translate self.stance values
        lf = lf - currentSegsStance;
        rf = rf - currentSegsStance;

        // Z axis, the X axis, and the Y axis, in that order. 

        lf = Geometry.rotateAroundZAxis(rz, lf);
        lf = Geometry.rotateAroundXAxis(-rx, lf);
        lf = Geometry.rotateAroundYAxis(ry, lf);


        rf = Geometry.rotateAroundZAxis(rz, rf);
        rf = Geometry.rotateAroundXAxis(-rx, rf);
        rf = Geometry.rotateAroundYAxis(ry, rf);


        //convert to machine space
        lf = lf + currentSegsStance;
        rf = rf + currentSegsStance;



        /////////////////////////////////////////////////////////////////////////////
        //end of front rotate


        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;



        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);


        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);



    }
    public void translateAndRotateFrontBlind(float x, float y, float z, float rx, float ry, float rz)
    {

        // translate self.stance valuees
        Vector3 lf = lfStance + new Vector3(x, y, z);
        Vector3 rf = rfStance + new Vector3(x, y, z);


        //rotate the previous result with input rotate pose values
        lf = Geometry.rotateZXY(new Vector3(rx, ry, rz), lf);
        rf = Geometry.rotateZXY(new Vector3(rx, ry, rz), rf);



        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;



        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);



        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);


    }
    public void testPoseRotateFrontShoulders(float rx, float ry, float rz)
    {

        // I want to rotate at the shoulders or component origins, don't do component space rotations
        Vector3 lf = lfStance - lfPos;
        Vector3 rf = rfStance - rfPos;


        //rotate the previous result with input rotate pose values
        lf = Geometry.rotateZXY(new Vector3(rx, ry, rz), lf);
        rf = Geometry.rotateZXY(new Vector3(rx, ry, rz), rf);


        //skip, I'm already at the component origin
        // tmp has machine space ik, need to translate to component space
        //lf = lf - lfPos;
        //rf = rf - rfPos;



        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);



        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);


    }
    public void testPoseShouldersSegs(float sx, float sy, float sz, float rx, float ry, float rz)
    {

        // I want to rotate at the shoulders or component origins, don't do component space rotations
        Vector3 lf = lfStance - lfPos;
        Vector3 rf = rfStance - rfPos;


        //rotate the previous result with input rotate pose values
        lf = Geometry.rotateZXY(new Vector3(rx, ry, rz), lf);
        rf = Geometry.rotateZXY(new Vector3(rx, ry, rz), rf);


        //skip, I'm already at the component origin
        // tmp has machine space ik, need to translate to component space
        //lf = lf - lfPos;
        //rf = rf - rfPos;



        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);



        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);


        float tempX = segsInputStance.x + sy;
        float tempZ = segsInputStance.z + sz;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ); //ik's sent to motors!



        segsFk = segs.valFk(); //the location of the end effector


    }
    public void testPoseForwardToBalDiff(float tx, float ty, float tz, float rx, float ry, float rz)
    {
        TopicMessageJSON tm = new TopicMessageJSON();
        PRFrameJSON frame = new PRFrameJSON(new Vector3(tx, ty, tz), Quaternion.Euler(rx, ry, rz));
        tm.topic = "";
        tm.messagestring = JsonUtility.ToJson(frame);
        string outoing = JsonUtility.ToJson(tm);
        
    }
    public void poseFrontForwardToBalDiff(float sx, float sy, float sz, float rx, float ry, float rz)
    {

        float stanceYDiff = ((lfStance.y + rfStance.y) / 2f) - segsStance.y;
        float currentYDiff;

        // subtract the base stance to work about the base
        // translate self.stance values
        Vector3 lf = lfStance - segsStance;
        Vector3 rf = rfStance - segsStance;

        // Z axis, the X axis, and the Y axis, in that order. 

        lf = Geometry.rotateAroundZAxis(rz, lf);
        lf = Geometry.rotateAroundXAxis(-rx, lf);
        lf = Geometry.rotateAroundYAxis(ry, lf);


        rf = Geometry.rotateAroundZAxis(rz, rf);
        rf = Geometry.rotateAroundXAxis(-rx, rf);
        rf = Geometry.rotateAroundYAxis(ry, rf);


        //convert to machine space
        lf = lf + segsStance;
        rf = rf + segsStance;


        currentYDiff = ((lf.y + rf.y) / 2f) - segsStance.y;

        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;


        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);



        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);

        //calculate change in shoulder.y ?

        float moveVehicleZ = currentYDiff - stanceYDiff;


        TopicMessageJSON tm = new TopicMessageJSON();
        PRFrameJSON frame = new PRFrameJSON(new Vector3(0f, 0f, moveVehicleZ), Quaternion.identity);
        tm.topic = "";
        tm.messagestring = JsonUtility.ToJson(frame);
        string outoing = JsonUtility.ToJson(tm);
        
        //TODO, I need to forward this from the task using an mqtt message
    }
    public void poseSegsInfluencedForwardToBalDiff(float sx, float sy, float sz, float rx, float ry, float rz)
    {

        float tempX = segsInputStance.x + sy;
        float tempZ = segsInputStance.z + sz;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector




        //need to move lf and rf to to keep the same triangular stance
        //do I just need to calc the difference and add it to lf and rf?

        Vector3 currentSegsStance = segsPos + Geometry.rotateZXY(segsRot, segs.getLowerDeltaPositionInverse(segs.dormantFk + segsBasePos));

        Vector3 segsDiff = currentSegsStance - segsStance;

        Vector3 lf = lfStance + segsDiff;
        Vector3 rf = rfStance + segsDiff;



        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;



        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);


        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);

        //calculate change in shoulder.y ?

        float moveVehicleZ = segsDiff.y;


        TopicMessageJSON tm = new TopicMessageJSON();
        PRFrameJSON frame = new PRFrameJSON(new Vector3(0f, 0f, moveVehicleZ), Quaternion.identity);
        tm.topic = "";
        tm.messagestring = JsonUtility.ToJson(frame);
        string outoing = JsonUtility.ToJson(tm);
        //TODO, I need to forward this from the task using an mqtt message
    }
    public void poseFrontSegsInfluencedForwardToBD(float sx, float sy, float sz, float rx, float ry, float rz)
    {

        //start of pose segs


        float tempX = segsInputStance.x + sy;
        float tempZ = segsInputStance.z + sz;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector




        //need to move lf and rf to to keep the same triangular stance
        //do I just need to calc the difference and add it to lf and rf?

        Vector3 currentSegsStance = segsPos + Geometry.rotateZXY(segsRot, segs.getLowerDeltaPositionInverse(segs.dormantFk + segsBasePos));

        Vector3 segsDiff = currentSegsStance - segsStance;

        Vector3 lf = lfStance + segsDiff;
        Vector3 rf = rfStance + segsDiff;



        // tmp has machine space ik, need to translate to component space
        //lf = lf - lfPos;
        //rf = rf - rfPos;



        // do rotation side of machine_space -> component_space
        //lf = Geometry.rotateYXZ(-lfRot, lf);
        //rf = Geometry.rotateYXZ(-rfRot, rf);


        // send those requests to components 
        //lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        //rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);


        //end of pose segs


        //start of rotate front

        //uses newly posed segs stance
        float stanceYDiff = ((lf.y + rf.y) / 2f) - currentSegsStance.y;
        float currentYDiff;

        // subtract the base stance to work about the base
        // translate self.stance values
        lf = lf - currentSegsStance;
        rf = rf - currentSegsStance;

        // Z axis, the X axis, and the Y axis, in that order. 

        lf = Geometry.rotateAroundZAxis(rz, lf);
        lf = Geometry.rotateAroundXAxis(rx, lf);
        lf = Geometry.rotateAroundYAxis(ry, lf);


        rf = Geometry.rotateAroundZAxis(rz, rf);
        rf = Geometry.rotateAroundXAxis(rx, rf);
        rf = Geometry.rotateAroundYAxis(ry, rf);


        //convert to machine space
        lf = lf + currentSegsStance;
        rf = rf + currentSegsStance;


        currentYDiff = ((lf.y + rf.y) / 2f) - currentSegsStance.y;

        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;


        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);



        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);


        //end of rotate front

        //calculate change in shoulder.y ?
        //segsInfluenced: float moveVehicleZ = segsDiff.y;
        //front: float moveVehicleZ = currentYDiff - stanceYDiff;
        float moveVehicleZ = (currentYDiff - stanceYDiff) + segsDiff.y;


        TopicMessageJSON tm = new TopicMessageJSON();
        PRFrameJSON frame = new PRFrameJSON(new Vector3(0f, 0f, moveVehicleZ), Quaternion.identity);
        tm.topic = "";
        tm.messagestring = JsonUtility.ToJson(frame);
        string outoing = JsonUtility.ToJson(tm);
        //TODO, I need to forward this from the task using an mqtt message
    }
    public void rotateFrontStaticSegs(float rx, float ry, float rz)
    {

        // subtract the base stance to work about the base
        // translate self.stance values
        Vector3 lf = lfStance - segsStance;
        Vector3 rf = rfStance - segsStance;

        // Z axis, the X axis, and the Y axis, in that order. 

        lf = Geometry.rotateAroundZAxis(rz, lf);
        lf = Geometry.rotateAroundXAxis(-rx, lf);
        lf = Geometry.rotateAroundYAxis(ry, lf);


        rf = Geometry.rotateAroundZAxis(rz, rf);
        rf = Geometry.rotateAroundXAxis(-rx, rf);
        rf = Geometry.rotateAroundYAxis(ry, rf);


        //convert to machine space
        lf = lf + segsStance;
        rf = rf + segsStance;

        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;


        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);



        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);


    }

    // sends new fks to components, 4 channel equivelant of do_ik
    // this is like a temp update, doesn't actual change the state of the machine
    public void translateStance(float x, float y, float z)
    {

        // translate self.stance valuees
        Vector3 lf = lfStance + new Vector3(x, y, z);
        Vector3 rf = rfStance + new Vector3(x, y, z);
        Vector3 lr = lrStance + new Vector3(x, y, z);
        Vector3 rr = rrStance + new Vector3(x, y, z);


        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;
        lr = lr - lrPos;
        rr = rr - rrPos;


        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);


        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);

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
        Vector3 lf = Geometry.rotateZXY(new Vector3(x, y, z), lfStance);
        Vector3 rf = Geometry.rotateZXY(new Vector3(x, y, z), rfStance);
        Vector3 lr = Geometry.rotateZXY(new Vector3(x, y, z), lrStance);
        Vector3 rr = Geometry.rotateZXY(new Vector3(x, y, z), rrStance);


        // tmp has machine space fk, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;
        lr = lr - lrPos;
        rr = rr - rrPos;


        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);


        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);


    }


    void poseSegmentsWithInfluence(float x, float y, float z)
    {

        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
            return;

        float tempX = segsInputStance.x + y;
        float tempZ = segsInputStance.z + z;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector




        //need to move lf and rf to to keep the same triangular stance
        //do I just need to calc the difference and add it to lf and rf?

        Vector3 currentSegsStance = segsPos + Geometry.rotateZXY(segsRot, segs.getLowerDeltaPositionInverse(segs.dormantFk + segsBasePos));

        Vector3 segsDiff = currentSegsStance - segsStance;

        Vector3 lf = lfStance + segsDiff;
        Vector3 rf = rfStance + segsDiff;



        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;



        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);


        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);


    }

    void poseSegmentsWithInfluence2(float x, float y, float z)
    {

        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
            return;

        float tempX = segsInputStance.x + y;
        float tempZ = segsInputStance.z + z;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector




        //need to move lf and rf to to keep the same triangular stance
        //do I just need to calc the difference and add it to lf and rf?

        Vector3 currentSegsStance = segsPos + Geometry.rotateZXY(segsRot, segs.getLowerDeltaPositionInverse(segs.dormantFk + segsBasePos));

        Vector3 segsDiff = currentSegsStance - segsStance;

        Vector3 lf = lfStance + segsDiff;
        Vector3 rf = rfStance + segsDiff;

        ///////////////////////////////////////////////////////////////////////////////////////
        //rotate the stance to keep the front height the same-ish?

        Vector3 frontCenter = (lfStance + rfStance) / 2f;
        Vector3 currentFrontCenter = (lf + rf) / 2f;
        Vector3 diff = currentFrontCenter - frontCenter;
        float stanceLength = frontCenter.z - currentSegsStance.z;
        float radians = Mathf.Sin(diff.y / stanceLength);



        // subtract the base stance to work about the base
        // translate self.stance values
        lf = lf - currentSegsStance;
        rf = rf - currentSegsStance;


        //Unity rotation order: Z axis, the X axis, and the Y axis, in that order. 
        lf = Geometry.rotateAroundXAxis(radians, lf);
        rf = Geometry.rotateAroundXAxis(radians, rf);



        //convert to machine space
        lf = lf + currentSegsStance;
        rf = rf + currentSegsStance;


        ///////////////////////////////////////////////////////////////////////////////////////
        ///TODO, the front/back segs actuation works, can I make the side/side actuation work?




        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;



        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);


        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);


    }
    void poseSegmentsWithInfluence3(float x, float y, float z)
    {
        // for action 2, during y axis seg acts tries to maintain the same angle using y axis rotations
        //rotate in the y axis, about a point ((lf+rf)/2)

        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
            return;

        float tempX = segsInputStance.x + y;
        float tempZ = segsInputStance.z + z;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector




        //calc the difference in the segs
        Vector3 currentSegsStance = segsPos + Geometry.rotateZXY(segsRot, segs.getLowerDeltaPositionInverse(segs.dormantFk + segsBasePos));
        Vector3 segsDiff = currentSegsStance - segsStance;



        //can I rotate about a point between the front fks?
        Vector3 rotOrigin = ((lfStance + rfStance) / 2f);

        Vector3 lf = lfStance - rotOrigin;
        Vector3 rf = rfStance - rotOrigin;

        ///////////////////////////////////////////////////////////////////////////////////////
        //rotate the stance to keep the front height the same-ish?

        Vector3 startVector = segsStance - rotOrigin;
        Vector3 currentVector = currentSegsStance - rotOrigin;

        float radians = Geometry.angleBetweenVectors(new Vector2(startVector.x, startVector.z), new Vector2(currentVector.x, currentVector.z));



        //Unity rotation order: Z axis, the X axis, and the Y axis, in that order. 
        lf = Geometry.rotateAroundXAxis(radians, lf);
        rf = Geometry.rotateAroundXAxis(radians, rf);



        //convert to machine space
        lf = lf + rotOrigin;
        rf = rf + rotOrigin;



        // tmp has machine space ik, need to translate to component space
        lf = lf - lfPos;
        rf = rf - rfPos;



        // do rotation side of machine_space -> component_space
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);


        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);


    }

    void poseSegmentsPairBlind(float sx, float sy, float sz)
    {

        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
            return;

        float tempX = segsInputStance.x + sy;
        float tempZ = segsInputStance.z + sx;
        float tempUpperX = segsInputStance.y + 2 * sz;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ, tempUpperX); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector





        if(dormantFKMarker != null)
        {
            dormantFKMarker.localPosition = segs.dormantFk;
        }
        if(segsInfluenceMarker != null)
        {
            segsInfluenceMarker.localPosition = segs.getLowerDeltaPositionInverse(segs.dormantFk);
        }
        segsInfluencePosition = segsInfluenceMarker.localPosition;

    }
    void poseSegmentsBlind(float x, float y, float z)
    {

        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
            return;

        float tempX = segsInputStance.x + y;
        float tempZ = segsInputStance.z + x;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(tempX, tempZ); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector

    }

    void poseSegmentsUpperBlind(float x, float y, float z)
    {

        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
            return;

        float tempX = segsInputStance.x + y;
        float tempZ = segsInputStance.z + x;
        float tempUpperX = segsInputStance.y + 2 * z;

        //requested degrees of rotation of each segment
        // inputX = x;
        // inputZ = z;
        segsIk = segs.doIk(0f, 0f, tempUpperX); //ik's sent to motors!
        segsFk = segs.valFk(); //the location of the end effector

    }


    // this takes component space fks, and converts to machine space
    // so the machine can make machine level decisions
    // 
    // when machine to component new_fk = component.rotation_inverse(old_fk) // rotate_yxz(-ax,-ay,-az,px,py,pz)
    // when component to machine new_fk = component.rotaion(old_fk)          // rotate_zxy(ax,ay,az,px,py,pz)
    public void updateStance()
    {

        lfStance = lfPos + Geometry.rotateZXY(lfRot, leftFront.valFk());
        rfStance = rfPos + Geometry.rotateZXY(rfRot, rightFront.valFk());
        //segsStance = segsPos + Geometry.rotateZXY(segsRot, segs.valFk());
        segsStance = segsPos + Geometry.rotateZXY(segsRot, segs.getLowerDeltaPositionInverse(segs.dormantFk + segsBasePos));

        segsInputStance.x = segs.inputX;
        segsInputStance.z = segs.inputZ;
        segsInputStance.y = segs.inputUpperX;

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
