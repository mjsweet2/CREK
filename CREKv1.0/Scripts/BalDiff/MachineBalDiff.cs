/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineBalDiff : MonoBehaviour
{

    public ArticulatedBalDiff phys;
    public RSPoseController rsPoseController;
    public MSPoseController msPoseController;
    public InputDeck inputDeck;
    public MotionController motionController; //input into motion fsm
    public MachineBalDiffPlanner planner;
    public MachineBalDiffShell shell;

    public TaskMessageSubscriber taskMessageSubscriber;


    public string paramJSONName;
    public BLBoneLengthList paramJSON;
    public Dictionary<string, float> boneLengths;
    public Dictionary<string, Vector3> boneHeads;


    public Transform testMarker;
    public List<Transform> testMarkers;

    public string machineName;

    public enum MOTIONMODE { TRAJECTORY, MOTION, TASK, TRAJTEST };
    public MOTIONMODE motionMode;
    public enum POSEMODE { TRANSLATE, ROTATE, TASKPOSE }
    public POSEMODE poseMode;

    public enum BALANCEMODE { NOTBALANCING, BALANCING };
    public BALANCEMODE balanceMode;

    public enum POSEEXITMODE { SPRINGBACKPOSEEXIT, STICKPOSEEXIT }; //when implemented pose mode, so I stay or spring back to stance
    public POSEEXITMODE poseExitMode;

    //for balance
    public PID pid;



    public Transform balanceSensor;
    public Vector3 balanceRotation;
    public float balanceThreshold;
    public float balanceOpposite;
    public float balanceOppositeGoal;
    public float balanceForward;
    public float balanceBackward;
    public float balanceNeutral;
    public float frameRadius;
    public float wheelRadius;


    public float balRotOffset;

    public float lTestInput, rTestInput;
    public ComponentWheel leftWheel;
    public ComponentWheel rightWheel;


    public Vector3 lwPos;// self.lr_pos = []
    public Vector3 rwPos;// self.rr_pos = []


    public Vector3 lwRot;// self.lr_rot = []
    public Vector3 rwRot;// self.rr_rot = []


    public Vector3 lwStance;// self.stance_lr = [0.0,0.0,0.0]
    public Vector3 rwStance;// self.stance_rr = [0.0,0.0,0.0] 


    public bool isStanceValid;// self.is_stance_valid = False


    // realtime input for pose, usually from mouse and 6 dof controller
    public Vector3 posePos; // poseLeft;
    public Vector3 poseRot; // poseRight;

    //realtime input for motion input, usually from dual analog controller
    public Vector2 lMotionInput;
    public Vector2 rMotionInput;



    //component iks
    Geometry.GeoPacket lwIk;
    Geometry.GeoPacket rwIk;


    public bool isRunning;

    
    public Trajectory lwTrajectory;
    public Trajectory rwTrajectory;
    public Trajectory spTrajectory; // set point trajectory


    public enum MOTIONCHANNELTYPE { FK, VELOCITY, RADIAN, RADIANVELOCITY, SETPOINT };

    public string motionCompletedToken; //when motion is completed, if this is not "", then it contains the topic to post a message to.
    public List<string> motionChannels;
    public List<MachineBalDiffMotion> motions;
    public List<MOTIONCHANNELTYPE> motionChannelTypes; //MOTIONCHANNELTYPE  { FK, DIRECTIONU, DIRECTIONL, POSEPOSITION, POSEROTATE };


    public delegate void ShellCmd();
    public Dictionary<string, ShellCmd> functions;

    // Start is called before the first frame update
    void Start()
    {

        

        buildTables();
        loadParams();
        planner.buildTables();

        pid = new PID(0.2f, 2.1f, 0f);


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

        
        if (motionMode == MOTIONMODE.MOTION)
        {
            processMotions(motionController.controlValue);
        }

        processPoseInput();


        broadcastMessage();


        processTaskMessages();
    }
   
   
  
    void balance()
    {

        //I need to use the x value of this
        balanceRotation = balanceSensor.rotation.eulerAngles;

        


        balanceRotation.x -= balRotOffset;

        if (balanceRotation.x < -balanceThreshold || balanceRotation.x > balanceThreshold)
        {
            balanceMode = BALANCEMODE.NOTBALANCING;
            return;
        }


        balanceOpposite = Mathf.Sin(balanceRotation.x * Mathf.Deg2Rad) * frameRadius;

        float wheelCirc = (Mathf.PI * 2 * wheelRadius);

        float pidControlled = pid.Update(balanceOppositeGoal, balanceOpposite, 1 / 60f);

        leftWheel.lastValidIk.z -= ((pidControlled / wheelCirc) * 1f);
        rightWheel.lastValidIk.z -= ((pidControlled / wheelCirc) * 1f);


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

        //until I create a BoneLengthsJSON, I can initialize here
        lwPos = new Vector3(-0.3f, 0.0f, 0.0f);
        rwPos = new Vector3(0.3f, 0.0f, 0.0f);
        lwRot = new Vector3(0.0f, 0.0f, 0.0f);
        rwRot = new Vector3(0.0f, 0.0f, 0.0f);


        lwIk = leftWheel.doPosition(0f);
        rwIk = rightWheel.doPosition(0f);

        updateStance();

        importBoneLengthsJSON();
        if (paramJSONName == "")
            return;

        /*    
       //  set lengths
       lWheel.loadParams(boneLengths["leftrear.x"], boneLengths["leftrear.balradius"]); //leftRear.loadParams(0.318f, 0.37f);
       //  set lengths
       rWheel.loadParams(boneLengths["rightrear.x"], boneLengths["rightrear.balradius"]); //rightRear.loadParams(0.318f, 0.37f);


       wheelRadius = boneLengths["rightrear.balradius"];
       frameRadius = (boneHeads["cap04wheelfront"].y - boneHeads["rightrear.balradius"].y);



       //  component placement
       lwPos = boneHeads["root"] - boneHeads["leftrear.x"];
       rwPos = boneHeads["root"] - boneHeads["rightrear.x"];

       */

       


        machineName = "BalDiff";
        leftWheel.compName = "leftWheel";
        rightWheel.compName = "rightWheel";
     
       

        //populate these with something

        lwIk = leftWheel.doPosition(0f);
        rwIk = rightWheel.doPosition(0f);


    }
    public void importBoneLengthsJSON()
    {
        if (paramJSONName == "")
            return;

        /*
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



        */


    }
    public Vector3 dormantLWFk()
    {
        Vector3 ret = Vector3.zero;
       
        ret += lwPos;

        return ret;
    }
    public Vector3 dormantRWFk()
    {
        Vector3 ret = Vector3.zero;
     
        ret += rwPos;

        return ret;
    }
    public Vector3 getlwStance() { return lwStance; }
    public Vector3 getrwStance() { return rwStance; }
    public Vector3 lwToMachine(Vector3 lw)
    {

        // component space fk, need to translate to machine_space space
        // do rotation side of component_space -> machine_space
        Vector3 ret = Geometry.rotateZXY(lwRot, lw);
        ret += lwPos;

        return ret;
    }
    public Vector3 rwToMachine(Vector3 rw)
    {
        // component space fk, need to translate to machine_space space
        // do rotation side of component_space -> machine_space
        Vector3 ret = Geometry.rotateZXY(rwRot, rw);
        ret += rwPos;

        return ret;
    }
    public Vector3 machineTolw(Vector3 machine)
    {
        // machine_space fk, need to translate to component_space space
        Vector3 ret = machine - lwPos;

        // do rotation side of machine_space -> component_space
        ret = Geometry.rotateYXZ(-lwRot, ret);

        return ret;
    }
    public Vector3 machineTorw(Vector3 machine)
    {
        // machine_space fk, need to translate to component_space space
        Vector3 ret = machine - rwPos;

        // do rotation side of machine_space -> component_space
        ret = Geometry.rotateYXZ(-rwRot, ret);

        return ret;
    }
   
    void processPoseInput()
    {

        


        //ignore mouse and rs, get pose info from taskMessageService;
        if(poseMode == POSEMODE.TASKPOSE)
        {
            ;//pose input already set in processTaskMessages()

        }

        else
        {
            if (msPoseController.isTracking)
            {
                posePos = msPoseController.track;
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

        }

        // anytime you send a trajectory to a component you have to update the stance.
        // the stance is basically saved fk info, and if you update the trajectory at the component level, you need a new stance.
        // here pose input is just like a temporary adjustment to the trajectory, which doesn't update the stance
        // but "jump" switches the current trajectory, needing an updated stance


        //calculates machine space ik
        if (isStanceValid == false)
        {
            //lwIk = leftWheel.doPosition(0f); // this is a component space fk
            //rwIk = rightWheel.doPosition(0f); // this is a component space fk
            updateStance();
        }

        pose(posePos.x, posePos.y, posePos.z, posePos.x, posePos.y, posePos.z);

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
            phys.processTripleMessage("1", lwIk.vector.x, 0f, 0f);
            phys.processTripleMessage("2", rwIk.vector.x, 0f, 0f);
        }

    }

    void pose(float tx, float ty, float tz, float rx, float ry, float rz)
    {

        if(poseMode == POSEMODE.TASKPOSE)
        {
            translateStance(tx, ty, tz);
        }

        if (poseMode == POSEMODE.TRANSLATE)
            translateStance(tx, ty, tz);
        if (poseMode == POSEMODE.ROTATE)
            rotateStance(rx, ry, rz);

        //rotateStance(-rx, -ry, rz);
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
                lwTrajectory = planner.trajectories[trajName + "lr"];
                for (int i = 0; i < lwTrajectory.keyFrames.Count; i++)
                {
                    Transform another = Instantiate<Transform>(testMarker);
                    another.position = lwTrajectory.keyFrames[i];
                    another.parent = testMarker.parent;
                    testMarkers.Add(another);
                }
            }



            if (planner.trajectories.ContainsKey(trajName + "rr"))
            {
                rwTrajectory = planner.trajectories[trajName + "rr"];
                for (int i = 0; i < rwTrajectory.keyFrames.Count; i++)
                {
                    Transform another = Instantiate<Transform>(testMarker);
                    another.position = rwTrajectory.keyFrames[i];
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



        if (lwTrajectory != null)
        {
            if (!lwTrajectory.isComplete)
            {
                if (!lwTrajectory.isAPause())
                {
                    Vector3 lw = lwTrajectory.getNextFrame(); // get machine space fks from Motions
                    Debug.Log("processing Traj lw: " + lw.ToString("F6"));
                    //lw = lw - lwPos;  // tmp has machine space fk, need to translate to component space
                    //lw = Geometry.rotateYXZ(-lwRot, lw); // do rotation side of machine_space -> component_space
                    lwIk = leftWheel.doPosition(lw.z); // send those requests to components 
                }
                else
                {
                    //ignore the frame, this is a pause traj
                    lwTrajectory.getNextFrame();
                }
            }
        }



        if (rwTrajectory != null)
        {
            if (!rwTrajectory.isComplete)
            {
                if (!rwTrajectory.isAPause())
                {
                    Vector3 rw = rwTrajectory.getNextFrame(); // get machine space fks from Motions
                    Debug.Log("processing Traj rw: " + rw.ToString("F6"));
                    //rw = rw - rwPos;  // tmp has machine space fk, need to translate to component space
                    //rw = Geometry.rotateYXZ(-rwRot, rw); // do rotation side of machine_space -> component_space
                    rwIk = rightWheel.doPosition(rw.z); // send those requests to components 
                }
                else
                {
                    //ignore the frame, this is a pause traj
                    rwTrajectory.getNextFrame();
                }
            }
        }

     
        //update stance
        updateStance();


    }
    public void runTrajectoryOnChannel(string trajName)
    {
        if (motionMode != MOTIONMODE.TRAJECTORY)
        {
            return;
        }
        Debug.Log("run traj: " + trajName);
       
        string[] pieces = trajName.Split('.');
        if (pieces.Length == 2)
        {
            pieces[0] = pieces[0].Trim();
            pieces[1] = pieces[1].Trim();

            if (pieces[1] == "sp")
            {
                if (planner.trajectories.ContainsKey(trajName))
                {
                    spTrajectory = planner.trajectories[trajName];

                    spTrajectory.resetTrajectory();
                }
                else
                {
                    spTrajectory = null;
                }
            }
            if (pieces[1] == "lw")
            {
                if (planner.trajectories.ContainsKey(trajName))
                {
                    lwTrajectory = planner.trajectories[trajName];

                    lwTrajectory.resetTrajectory();
                }
                else
                {
                    lwTrajectory = null;
                }
            }
            if (pieces[1] == "rw")
            {
                if (planner.trajectories.ContainsKey(trajName))
                {
                    rwTrajectory = planner.trajectories[trajName];

                    rwTrajectory.resetTrajectory();
                }
                else
                {
                    rwTrajectory = null;
                }
            }
        }

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



        if (planner.trajectories.ContainsKey(trajName + ".lw"))
        {
            lwTrajectory = planner.trajectories[trajName + ".lw"];
            lwTrajectory.resetTrajectory();
        }
        else
        {
            lwTrajectory = null;
        }



        if (planner.trajectories.ContainsKey(trajName + ".rw"))
        {
            rwTrajectory = planner.trajectories[trajName + ".rw"];
            rwTrajectory.resetTrajectory();
        }
        else
        {
            rwTrajectory = null;
        }


    }

    void processTaskMessages()
    {
        string messageString = "";
        while (taskMessageSubscriber.hasMessages())
        {

            messageString = taskMessageSubscriber.getNextMessage();
            TopicMessageJSON incoming = JsonUtility.FromJson<TopicMessageJSON>(messageString);
            if (incoming.topic == "MOTIONSTARTED")
            {
                motionCompletedToken = incoming.messagestring;
            }
            
            if (incoming.topic == "CR/BalDiff/Motion/pose.tr")//pose translate
            {
                Float3JSON float3JSON = JsonUtility.FromJson<Float3JSON>(incoming.messagestring);
                posePos.x = float3JSON.x; posePos.y = float3JSON.y; posePos.z = float3JSON.z;

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
        for (int i = 0; i < motions.Count; i++)
        {
            motions[i].tick(userInput);
            allCompleted = allCompleted & motions[i].currentMotionIsOver;
        }

        //return my MOTIONCOMPLETED token
        if (allCompleted & motionCompletedToken != "")
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

        // 0 is lwMotion
        if (!motions[0].currentMotionIsOver)
        {
            if (!motions[0].isPlayingPauseTraj())
            {
                Vector3 lw = motions[0].getCurrentFrame(userInput); // get machine space fks from Motions
                Debug.Log("motion.lw machine space: " + lw.ToString("F6"));
                lw = lw - lwPos; // tmp has machine space fk, need to translate to component space
                lw = Geometry.rotateYXZ(-lwRot, lw); // do rotation side of machine_space -> component_space
                lwIk = leftWheel.doPosition(lw.z); // 
                Debug.Log("motion.lw comp space: " + lw.ToString("F6"));
                lwStance = lwPos + Geometry.rotateZXY(lwRot, leftWheel.valFk()); // update stance

            }

            else
            {
                motions[0].getCurrentFrame(userInput);
            }
        }

        // 1 is rwMotion
        if (!motions[1].currentMotionIsOver)
        {
            if (!motions[1].isPlayingPauseTraj())
            {
                Vector3 rw = motions[1].getCurrentFrame(userInput); // get machine space fks from Motions
                rw = rw - rwPos; // tmp has machine space fk, need to translate to component space
                rw = Geometry.rotateYXZ(-rwRot, rw); // do rotation side of machine_space -> component_space
                rwIk = rightWheel.doPosition(rw.z); // 
                rwStance = rwPos + Geometry.rotateZXY(rwRot, rightWheel.valFk()); // update stance
            }
            else
            {
                motions[1].getCurrentFrame(userInput);
            }
        }
        

    }

    public void softCancelAllMotions()
    {
        for (int i = 0; i < motions.Count; i++)
        {
            motions[i].setInterupt(MachineBalDiffMotion.MOTIONINTERUPT.SOFTCANCEL);
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


    public void translateAndRotateStance(float x, float y, float z, float rx, float ry, float rz)
    {

        // translate self.stance valuees
        Vector3 lw = lwStance + new Vector3(x, y, z);
        Vector3 rw = rwStance + new Vector3(x, y, z);

        //rotate the previous result with input rotate pose values
        lw = Geometry.rotateZXY(new Vector3(rx, ry, rz), lw);
        rw = Geometry.rotateZXY(new Vector3(rx, ry, rz), rw);


        // tmp has machine space ik, need to translate to component space
        lw = lw - lwPos;
        rw = rw - rwPos;


        // do rotation side of machine_space -> component_space
        lw = Geometry.rotateYXZ(-lwRot, lw);
        rw = Geometry.rotateYXZ(-rwRot, rw);


        // send those requests to components 
        lwIk = leftWheel.doPosition(lw.x);
        rwIk = rightWheel.doPosition(rw.x);


    }
    // sends new fks to components, 4 channel equivelant of do_ik
    // this is like a temp update, doesn't actual change the state of the machine
    public void translateStance(float x, float y, float z)
    {

        //take the length of the pose input, and add it to the z(forward/backward) axis
        float mag = new Vector3(x, y, z).magnitude;
        if (y < 0)
            mag = -mag;
        // translate self.stance valuees
        Vector3 lw = lwStance + new Vector3(0f,0f, mag);
        Vector3 rw = rwStance + new Vector3(0f, 0f, mag);


        // tmp has machine space ik, need to translate to component space
        lw = lw - lwPos;
        rw = rw - rwPos;


        // do rotation side of machine_space -> component_space
        lw = Geometry.rotateYXZ(-lwRot, lw);
        rw = Geometry.rotateYXZ(-rwRot, rw);

        // send those requests to components 
        lwIk = leftWheel.doPosition(lw.z);
        rwIk = rightWheel.doPosition(rw.z);

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


        //take the length of the pose input, and add it to the z(forward/backward) axis
        float mag = new Vector3(x, y, z).magnitude;
        if (y < 0)
            mag = -mag;
        // rotate by making one, forward, and on one backward
        Vector3 lw = lwStance + new Vector3(0f, 0f, mag);
        Vector3 rw = rwStance - new Vector3(0f, 0f, mag);


        // tmp has machine space ik, need to translate to component space
        lw = lw - lwPos;
        rw = rw - rwPos;


        // do rotation side of machine_space -> component_space
        lw = Geometry.rotateYXZ(-lwRot, lw);
        rw = Geometry.rotateYXZ(-rwRot, rw);

        // send those requests to components 
        lwIk = leftWheel.doPosition(lw.z);
        rwIk = rightWheel.doPosition(rw.z);

    }

   
   



    // this takes component space fks, and converts to machine space
    // so the machine can make machine level decisions
    // 
    // when machine to component new_fk = component.rotation_inverse(old_fk) // rotate_yxz(-ax,-ay,-az,px,py,pz)
    // when component to machine new_fk = component.rotaion(old_fk)          // rotate_zxy(ax,ay,az,px,py,pz)
    void updateStance()
    {

        lwStance = lwPos + Geometry.rotateZXY(lwRot, leftWheel.valFk());
        rwStance = rwPos + Geometry.rotateZXY(rwRot, rightWheel.valFk());

        isStanceValid = true;

    }


}
