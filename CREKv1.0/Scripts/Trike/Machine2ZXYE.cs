/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

[Serializable]
public class BLBoneLengthList
{
    public string armaturename;
    public List<string> bonenames;
    public List<Vector3> boneheads;
    public List<float> bonelengths;

    public BLBoneLengthList() { armaturename = ""; bonenames = new List<string>(); boneheads = new List<Vector3>(); bonelengths = new List<float>(); }

}
public class Machine2ZXYE : MonoBehaviour
{

    public Articulated2ZXYE phys;
    public RSPoseController rsPoseController;
    public MSPoseController msPoseController;
    public InputDeck inputDeck;
    public MotionController motionController; //input into motion fsm


    public Machine2ZXYETask machine2ZXYETask;
    public Machine2ZXYEPlanner planner;
    public Machine2ZXYEShell shell;

    public TaskMessageSubscriber taskMessageSubscriber;


    public string paramJSONName;
    public BLBoneLengthList paramJSON;
    public Dictionary<string, float> boneLengths;
    public Dictionary<string, Vector3> boneHeads;

    public Transform testMarker;
    public List<Transform> testMarkers;

    public string machineName; // .name = "Trike"

    public enum MOTIONMODE { TRAJECTORY, MOTION, TASK, TRAJTEST };
    public MOTIONMODE motionMode;
    public enum POSEMODE
    {
        TRANSLATE, ROTATE, SEGMENTSBLIND, SEGMENTSINFLUENCED, TANDRSEGMENETSBLIND, TANDRSEGMENTSINFLUENCED,
        FRONT,
        REAR,
        FRONTSEGMENTS,
        LEFTFRONTRIGHTFRONT,
        LEFTREARRIGHTREAR,
        TASKPOSE
    }
    public POSEMODE poseMode;

    public enum POSEEXITMODE { SPRINGBACKPOSEEXIT, STICKPOSEEXIT }; //when implemented pose mode, so I stay or spring back to stance
    public POSEEXITMODE poseExitMode;
    public enum MOTIONCHANNELTYPE { FK, DIRECTIONU, DIRECTIONL, POSEPOSITION, POSEROTATE };


    public ComponentZXYE leftFront;// self.left_front = ComponentTypeXYE()
    public ComponentZXYE rightFront;// self.right_front = ComponentTypeXYE()
    public CompSegs segs;

    public Vector3 lfPos;// self.lf_pos = []    # component positions
    public Vector3 rfPos;// self.rf_pos = []
    public Vector3 lrPos;// self.lr_pos = []
    public Vector3 rrPos;// self.rr_pos = []

    public Vector3 lfRot;// self.lf_rot = []    # component rotations
    public Vector3 rfRot;// self.rf_rot = []


    public Vector3 lfStance;// self.stance_lf = [0.0,0.0,0.0]
    public Vector3 rfStance;// self.stance_rf = [0.0,0.0,0.0]
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
    public Vector3 lfFk;
    public Vector3 rfFk;
    public Vector3 lrFk;
    public Vector3 rrFk;
    public Vector3 segsFk;

    //component iks
    Geometry.GeoPacket lfIk;
    Geometry.GeoPacket rfIk;
    Geometry.GeoPacket lrIk;
    Geometry.GeoPacket rrIk;
    Geometry.GeoPacket segsIk;



    public bool isRunning;

    public Trajectory lfTrajectory;
    public Trajectory rfTrajectory;
    public Trajectory segsTrajectory;

    
    public string motionCompletedToken; //when motion is completed, if this is not "", then it contains the topic to post a message to.
    public List<string> motionChannels;
    public List<Machine2ZXYEMotion> motions;
    public List<MOTIONCHANNELTYPE> motionChannelTypes; //MOTIONCHANNELTYPE  { FK, DIRECTIONU, DIRECTIONL, POSEPOSITION, POSEROTATE };





    public delegate void ShellCmd();
    public Dictionary<string, ShellCmd> functions;

    // Start is called before the first frame update
    void Start()
    {
        buildTables();
        loadParams();
        planner.buildTables();

        taskMessageSubscriber = GetComponent<TaskMessageSubscriber>();

    }

    // Update is called once per frame
    void Update()
    {

        processMotionInput();

        if (motionMode == MOTIONMODE.TRAJECTORY)
        {
            processTrajectory();
        }

        if (motionMode == MOTIONMODE.MOTION | motionMode == MOTIONMODE.TASK)
        {
            processMotions(motionController.controlValue);
        }


        processPoseInput();


        broadcastMessage();


        processTaskMessages();


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

        //  set lengths
        leftFront.loadParams(boneLengths["leftfront.x"], boneLengths["leftfront.e"]); //leftFront.loadParams(0.28f, 0.29f);
        //  set lengths
        rightFront.loadParams(boneLengths["rightfront.x"], boneLengths["rightfront.e"]); //rightFront.loadParams(0.28f, 0.29f);



        //segs.loadParams(.03f, -.0325f, 0f);
        float topBlock = (boneHeads["l1"] - boneHeads["machineorigin"]).magnitude;
        segs.loadParams(boneLengths["l1"], topBlock, 0f);



        //  component placement
        lfPos = boneHeads["leftfront.x"] - boneHeads["machineorigin"]; // new Vector3(-.152f, 0f, .2675f); //  unity space
        rfPos = boneHeads["rightfront.x"] - boneHeads["machineorigin"]; // new Vector3(.152f, 0f, .2675f);
        lrPos = boneHeads["leftrear.base"] - boneHeads["machineorigin"]; // new Vector3(-.075f, 0f, -.2675f);
        rrPos = boneHeads["rightrear.base"] - boneHeads["machineorigin"]; // new Vector3(.075f, 0f, -.2675f);


        lfRot = new Vector3(0.0f, 0.0f, 0.0f); //  unity space, radians //  Z, X ,Y  order.  // 30 deg = (2f * Mathf.PI) / 12f
        rfRot = new Vector3(0.0f, 0.0f, 0.0f);



        leftFront.compName = "leftFront";
        rightFront.compName = "rightFront";


        lfFk = leftFront.valFk();
        rfFk = rightFront.valFk();

        Vector3 startLeft = dormantLFIk();
        Vector3 startRight = dormantRFIk();

        //ini the comps
        lfIk = leftFront.doLeftIk(startLeft.x, startLeft.y, startLeft.z);
        rfIk = rightFront.doRightIk(startRight.x, startRight.y, startRight.z);
        segsIk = segs.doIk(0f, 0f);


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

    public Vector3 dormantLFIk()
    {
        Vector3 ret = leftFront.dormantFk();
        // ret has component space fk, need to translate to machine_space
        // do rotation side of component_space -> machine_space
        ret = Geometry.rotateZXY(lfRot, ret);
        ret += lfPos;

        return ret;
    }
    public Vector3 dormantLRIk()
    {
        return lrPos;
    }
    public Vector3 dormantRFIk()
    {
        Vector3 ret = rightFront.dormantFk();

        // ret has component space fk, need to translate to machine_space
        // do rotation side of component_space -> machine_space
        ret = Geometry.rotateZXY(rfRot, ret);
        ret += rfPos;

        return ret;
    }
    public Vector3 dormantRRIk()
    {
        return rrPos;
    }

    public Vector3 getlfStance() { return lfStance; }
    public Vector3 getlrStance() { return lrStance; }
    public Vector3 getrfStance() { return rfStance; }
    public Vector3 getrrStance() { return rrStance; }

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
        return lrPos;
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
        return rrPos;
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
        if (poseMode == POSEMODE.TASKPOSE)
        {
            ;//pose input already set in processTaskMessages()

        }

        else
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
        if (phys == null)
            return;

        // iks are in lf_ik, etc...
        phys.processTripleMessage("1", lfIk.vector.x, lfIk.vector.y, lfIk.vector.z);
        phys.processTripleMessage("2", rfIk.vector.x, rfIk.vector.y, rfIk.vector.z);
        phys.processTripleMessage("5", segsIk.vector.x, segsIk.vector.y, segsIk.vector.z);

        //Y actuation of front components
        phys.processTripleMessage("8", lfIk.vector.w, 0f, 0f);
        phys.processTripleMessage("9", rfIk.vector.w, 0f, 0f);

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
    void processTrajectory()
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
        if (motionMode == MOTIONMODE.TRAJECTORY)
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

    void processTaskMessages()
    {
        string messageString = "";
        
        if (taskMessageSubscriber == null)
        {
            return;
        }

        while (taskMessageSubscriber.hasMessages())
        {

            messageString = taskMessageSubscriber.getNextMessage();
            TopicMessageJSON incoming = JsonUtility.FromJson<TopicMessageJSON>(messageString);

            if (incoming.topic == "MOTIONSTARTED")
            {
                motionCompletedToken = incoming.messagestring;
            }
            if(incoming.topic == "CR/Trike/Motion/poseSegs")
            {
                poseSegs = Vector3.zero;
                string[] pieces = incoming.messagestring.Split(',');

                if (pieces.Length == 3)
                {
                    pieces[0] = pieces[0].Trim();
                    pieces[1] = pieces[1].Trim();
                    pieces[2] = pieces[2].Trim();

                    if (float.TryParse(pieces[0], out poseSegs.x))
                    {
                        if (float.TryParse(pieces[1], out poseSegs.y))
                        {
                            float.TryParse(pieces[2], out poseSegs.z);
                        }

                    }
                }

            }

            // when Task visit a MotionTI Node, Task posts a message to outgoing topic 
            // with the token = [return topic];taskname;nodename;COMPLETED 
            // post this token to the [outgoing topic], so I don't process my own message
            // when Task gets the message back, and if the taskName and currentNode name matchs,
            // set currentFlowNodeComplete = true;

        }

    }
    void processMotions(float userInput)
    {
        bool allCompleted = true;
        //inc'ing is seperate from consuming
        for (int i = 0; i < motions.Count;i++)
        {
            motions[i].tick(userInput);
            allCompleted = allCompleted & motions[i].currentMotionIsOver;
        }

        //return my MOTIONCOMPLETED token
        if(allCompleted & motionCompletedToken != "")
        { 
            string[] pieces = motionCompletedToken.Split(';');
            if (pieces.Length < 4)
                return;

            pieces[0] = pieces[0].Trim();
            pieces[1] = pieces[1].Trim();
            pieces[2] = pieces[2].Trim();
            pieces[3] = pieces[3].Trim();
            TopicMessageJSON outgoing = new TopicMessageJSON();
            outgoing.topic = pieces[0];
            outgoing.messagestring = motionCompletedToken;

            taskMessageSubscriber.postMessage(JsonUtility.ToJson(outgoing));
            motionCompletedToken = "";
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

        // 3 is lflMotion, uses direction trajectories, no position translation in space conversion
        if (!motions[3].currentMotionIsOver)
        {
            if (!motions[3].isPlayingPauseTraj())
            {
                Vector3 lfl = motions[3].getCurrentFrame(userInput); // get machine space fks from Motions 
                lfl = Geometry.rotateYXZ(-lfRot, lfl); // do rotation side of machine_space -> component_space
                lfIk = leftFront.doLeftLowerDir(lfl); // 
                lfStance = lfPos + Geometry.rotateZXY(lfRot, leftFront.valFk()); // update stance
            }

            else
            {
                motions[3].getCurrentFrame(userInput);
            }
        }
        // 4 is lfuMotion, uses direction trajectories, no position translation in space conversion
        if (!motions[4].currentMotionIsOver)
        {
            if (!motions[4].isPlayingPauseTraj())
            {
                Vector3 lfu = motions[4].getCurrentFrame(userInput); // get machine space fks from Motions 
                lfu = Geometry.rotateYXZ(-lfRot, lfu); // do rotation side of machine_space -> component_space
                lfIk = leftFront.doLeftUpperDir(lfu); // 
                lfStance = lfPos + Geometry.rotateZXY(lfRot, leftFront.valFk()); // update stance
            }

            else
            {
                motions[4].getCurrentFrame(userInput);
            }
        }
        // 5 is rflMotion, uses direction trajectories, no position translation in space conversion
        if (!motions[5].currentMotionIsOver)
        {
            if (!motions[5].isPlayingPauseTraj())
            {
                Vector3 rfl = motions[5].getCurrentFrame(userInput); // get machine space fks from Motions 
                rfl = Geometry.rotateYXZ(-rfRot, rfl); // do rotation side of machine_space -> component_space
                rfIk = rightFront.doRightLowerDir(rfl); // 
                rfStance = rfPos + Geometry.rotateZXY(rfRot, rightFront.valFk()); // update stance
            }

            else
            {
                motions[5].getCurrentFrame(userInput);
            }
        }
        // 6 is rfuMotion, uses direction trajectories, no position translation in space conversion
        if (!motions[6].currentMotionIsOver)
        {
            if (!motions[6].isPlayingPauseTraj())
            {
                Vector3 rfu = motions[6].getCurrentFrame(userInput); // get machine space fks from Motions 
                rfu = Geometry.rotateYXZ(-rfRot, rfu); // do rotation side of machine_space -> component_space
                rfIk = rightFront.doRightUpperDir(rfu); // 
                rfStance = rfPos + Geometry.rotateZXY(rfRot, rightFront.valFk()); // update stance
            }

            else
            {
                motions[6].getCurrentFrame(userInput);
            }
        }

    }

    public void softCancelAllMotions()
    {

        for(int i = 0; i < motions.Count; i++)
        {
            motions[i].setInterupt(Machine2ZXYEMotion.MOTIONINTERUPT.SOFTCANCEL);
        }    
        
    }

    public void runMotionOnAllChannels(string motionName)
    {
        isRunning = true;

        if (motionMode == MOTIONMODE.MOTION | motionMode == MOTIONMODE.TASK)
        {
            for (int i = 0; i < motions.Count; i++)
            {
                motions[i].startMotion(motionName + "." + motionChannels[i]);
            }
        }
    }

    void poseSegmentsTAndRStance(float sx, float sy, float sz, float tx, float ty, float tz, float rx, float ry, float rz)
    {

        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
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

        lrInfluecedRotation = segs.getLowerInfluenceRotation(Vector3.zero);
        rrInfluecedRotation = segs.getLowerInfluenceRotation(Vector3.zero);


        // translate self.stance valuees
        Vector3 lf = lfStance + new Vector3(tx, ty, tz);
        Vector3 rf = rfStance + new Vector3(tx, ty, tz);
        Vector3 lr = lrStance + new Vector3(tx, ty, tz);
        Vector3 rr = rrStance + new Vector3(tx, ty, tz);



        //rotate the previous result with input rotate pose values
        lf = Geometry.rotateZXY(new Vector3(rx, ry, rz), lf);
        rf = Geometry.rotateZXY(new Vector3(rx, ry, rz), rf);
        lr = Geometry.rotateZXY(new Vector3(rx, ry, rz), lr);
        rr = Geometry.rotateZXY(new Vector3(rx, ry, rz), rr);


        // tmp has machine space ik, need to translate to component space
        //using the influence position for the rear
        lf = lf - lfPos;
        rf = rf - rfPos;
        lr = lr - lrInfluecedPosition;
        rr = rr - rrInfluecedPosition;



        // do rotation side of machine_space -> component_space
        //using the influence rotation for rear
        lf = Geometry.rotateYXZ(-lfRot, lf);
        rf = Geometry.rotateYXZ(-rfRot, rf);
        lr = Geometry.rotateYXZ(-lrInfluecedRotation, lr);
        rr = Geometry.rotateYXZ(-rrInfluecedRotation, rr);



        // send those requests to components 
        lfIk = leftFront.doLeftIk(lf.x, lf.y, lf.z);
        rfIk = rightFront.doRightIk(rf.x, rf.y, rf.z);




    }
    public void translateAndRotateStance(float x, float y, float z, float rx, float ry, float rz)
    {

        // translate self.stance valuees
        Vector3 lf = lfStance + new Vector3(x, y, z);
        Vector3 rf = rfStance + new Vector3(x, y, z);
        Vector3 lr = lrStance + new Vector3(x, y, z);
        Vector3 rr = rrStance + new Vector3(x, y, z);

        //rotate the previous result with input rotate pose values
        lf = Geometry.rotateZXY(new Vector3(rx, ry, rz), lf);
        rf = Geometry.rotateZXY(new Vector3(rx, ry, rz), rf);
        lr = Geometry.rotateZXY(new Vector3(rx, ry, rz), lr);
        rr = Geometry.rotateZXY(new Vector3(rx, ry, rz), rr);


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

        //segs
        segsIk = segs.doIk(y, z); //ik's sent to motors!
        segsFk = segs.valFk();

        //influencing the stanceFks doesn't seem quite right.
        //I think I just keep te fks, and use getLowerInfluece to
        //calculate the current component position and rotation
        //then calculate the new component space fks, with the influenced component transforms


        lrInfluecedPosition = segs.getLowerInfluencePosition(lrPos);
        rrInfluecedPosition = segs.getLowerInfluencePosition(rrPos);

        lrInfluecedRotation = segs.getLowerInfluenceRotation(Vector3.zero);
        rrInfluecedRotation = segs.getLowerInfluenceRotation(Vector3.zero);



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




    }


    void poseSegmentsBlind(float x, float y, float z)
    {

        //if I'm playing segs motion, don't do anything
        if (!motions[2].currentMotionIsOver)
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

        lfStance = lfPos + Geometry.rotateZXY(lfRot, leftFront.valFk());
        rfStance = rfPos + Geometry.rotateZXY(rfRot, rightFront.valFk());
        lrStance = lrPos;
        rrStance = rrPos;

        isStanceValid = true;

    }
    void updateSegs()
    {
        segsFk = segs.valFk();
        isDeckValid = true;

    }

}
