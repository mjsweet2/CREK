/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the MIT License.
 * You should have received a copy of the MIT License with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


[Serializable]
public class SkillRigDescriptorJSON
{
    public enum SKILLSPACEMODE
    {
        NONE,
		ACTUATORPOSITIONSKILL, // outputs position of actuators
		ACTUATORFORCESKILL, // outputs position and force of actuators
		FKPOSITIONSKILL, // outputs position of end effectors
		FKFORCESKILL // outputs position and force vector of end effectors
    };
    public SKILLSPACEMODE skillSpaceMode;

    // # 12, fdlt state: lf state, rf state, offcenter, armatureframezaxis.z, lf foot switch, rf foot switch
    // # 13, fdfw state: lf state, rf state, remaining.x, remaining.y, armatureframezaxis.z, lf foot switch, rf foot switch
    // # 19, rolltofacedown: lf state, rf state, segs state, framezaxis, frameyaxis, framexaxis
    public enum STATEMODE {
                                NONE,
                                ROLLING, 
                                FWBW, 
                                LTRT, 
                                CUSTOM
    };

    public STATEMODE stateMode;
    public int stateCount;

    public List<int> stateMap;      //list of indices of from list of 6-10 sensors

    public enum ACTIONMODE {
                                NONE,
                                ZXYEZXYE,
                                ZXYEZXYESEGS,
                                ZXYEZXYEZXZX,
                                ZXYEZXYEZXZXSEGS,
                                CUSTOM
    };
    public ACTIONMODE actionMode;

    public int actionCount;

    public List<int> actionMap;     //list of indices from list of all actuators

    public float goalDeltaRadians;
    public Vector3 goalDeltaPos;

    public string lfModelName;
    public string rfModelName;
    public string lrModelName;
    public string rrModelName;
    public string segsModelName;

    public bool hasChildren;

    public List<string> customModelList;
    public List<string> childModelList;

    public SkillRigDescriptorJSON() {  }

}


[Serializable]
public class ActionsJSON
{
    public List<double> actions;

    public ActionsJSON() { actions = new List<double>(); }
    public ActionsJSON(int c) {
        actions = new List<double>();
        for (int i = 0; i < c; i++)
        {
            actions.Add(0.0);
        }

    }
}
[Serializable]
public class StatesJSON
{
    public List<float> states;

    public StatesJSON() { states = new List<float>(); }
    public StatesJSON(int c) {
        states = new List<float>();
        for (int i = 0; i < c; i++)
        {
            states.Add(0.0f);
        }
    }
}

[Serializable]
public class SkillServiceJSON
{
    public string command;
    public string param1;
    public string param2;
    public SkillServiceJSON() { command = ""; param1 = ""; param2 = ""; }
    public SkillServiceJSON(string c, string p1, string p2) { command = c; param1 = p1; param2 = p2; }

}

public class RLSkillRig : MonoBehaviour
{
    public Machine2ZXYE2ZX machine;
    public M2MqttCRClient m2MqttCRClient;

    public string publishTopic;
    public string subscribeTopic;
    
    public string requestServiceTopic;
    public string responseServiceTopic;


    public ActionsJSON actionsJSON;
    public StatesJSON statesJSON;
    public SkillServiceJSON skillServiceJSON;
    //public TopicMessageJSON outer; am I using this, commented out 11/6/25
    public SkillRigDescriptorJSON skillRigDescriptorJSON;

    public string currentSkill;

    public float mujocoCompGear;
    public float mujocoSegsGear;

    public Transform frameZA, frameZB;
    public Transform frameYA, frameYB;
    public Transform frameXA, frameXB;

    public Vector3 frameZ;
    public Vector3 frameY;
    public Vector3 frameX;
    public Vector3 frameZMujoco;
    public Vector3 frameYMujoco;
    public Vector3 frameXMujoco;

    public Vector3 machinePos;

    public float goalDeltaRadians; // self.goal_delta_radians = np.radians(-30) # 30 is lt
    public Vector3 startYFrame; // self.start_yframe = np.array([0.0, 0.0, 0.0])
    public Vector3 goalYFrame; // self.goal_yframe = np.array([0.0, 0.0, 0.0])
    public Vector3 yFrameFrame; // self.yframe_frame = np.array([0.0, 0.0, 0.0])
    public float offCenter;

    public Vector3 goalDeltaPos; // self.goal_delta_pos = np.array([0.0, 0.1, 0.0]) # meters
    public Vector3 startPos; // self.start_pos = np.array([0.0, 0.0, 0.0]) # to calculate the delta pos for this skill
    public float goalMag; // self.goal_mag = np.linalg.norm(self.goal_delta_pos)
    public Vector3 posFrame; // self.pos_frame = np.array([0.0, 0.0, 0.0])
    public Vector3 goalPos; // self.goal_pos = np.array([0.0, 0.0, 0.0])
    public Vector3 remaining; // self.remaining = np.array([0.0, 0.0, 0.0]) # meters
    public float remainingMag; // self.remaining_mag = 0.0
    public float lfFootValue;
    public float rfFootValue;


    public enum RUNNINGSTATE { NONE, STOPPED, RUNNING, COMPLETE };
    public RUNNINGSTATE runningState;

    public int aps; //actions per second
    public int fpa; //frames per action
    public int actionFrameIndex; // for linear interpolation of action across all fpas

    // Start is called before the first frame update
    void Start()
    {
        skillRigDescriptorJSON = new SkillRigDescriptorJSON();
        actionsJSON = new ActionsJSON(10);
        statesJSON = new StatesJSON(19);
        skillServiceJSON = new SkillServiceJSON();
        //outer = new TopicMessageJSON(); //am I using this, commented out 11/6/25
        

        m2MqttCRClient = GetComponent<M2MqttCRClient>();


        aps = 10;
        fpa = 60 / aps;
        actionFrameIndex = 0;
       

    }
    void updateAllDesc()
    {

        //temporary function to refresh jsons.


        importFromJSON("rolltofaceup");
        exportToJSON("rolltofaceup");
        importFromJSON("rolltofacedown");
        exportToJSON("rolltofacedown");
        importFromJSON("fdfw");
        exportToJSON("fdfw");
        importFromJSON("fdbw");
        exportToJSON("fdbw");
        importFromJSON("fdlt");
        exportToJSON("fdlt");
        importFromJSON("fdrt");
        exportToJSON("fdrt");

    }

    // Update is called once per frame
    void Update()
    {


        updateSensors();

        processMqttMessages();
        

    }
    void updateSensors()
    {

        //public float goalDeltaRadians; // self.goal_delta_radians = np.radians(-30) # 30 is lt
        //public Vector3 startYFrame; // self.start_yframe = np.array([0.0, 0.0, 0.0])
        //public Vector3 goalYFrame; // self.goal_yframe = np.array([0.0, 0.0, 0.0])
        //public Vector3 yFrameFrame; // self.yframe_frame = np.array([0.0, 0.0, 0.0])
        //public float offCenter;

        //public Vector3 goalDeltaPos; // self.goal_delta_pos = np.array([0.0, 0.1, 0.0]) # meters
        //public Vector3 startPos; // self.start_pos = np.array([0.0, 0.0, 0.0]) # to calculate the delta pos for this skill
        //public float goalMag; // self.goal_mag = np.linalg.norm(self.goal_delta_pos)
        //public Vector3 posFrame; // self.pos_frame = np.array([0.0, 0.0, 0.0])
        //public Vector3 goalPos; // self.goal_pos = np.array([0.0, 0.0, 0.0])
        //public Vector3 remaining; // self.remaining = np.array([0.0, 0.0, 0.0]) # meters
        //public float remainingMag; // self.remaining_mag = 0.0
        //public float lfFootValue;
        //public float rfFootValue;

        string[] parts = machine.sensorString.Split(';');

        lfFootValue = float.Parse(parts[0].Trim());
        rfFootValue = float.Parse(parts[1].Trim());
        machinePos.x = float.Parse(parts[2].Trim());
        machinePos.y = float.Parse(parts[3].Trim());
        machinePos.z = float.Parse(parts[4].Trim());

        frameZ = frameZB.position - frameZA.position;
        frameZ.Normalize();
        frameZMujoco = unityToMujocoSpace(frameZ);

        frameY = frameYB.position - frameYA.position;
        frameY.Normalize();
        frameYMujoco = unityToMujocoSpace(frameY);

        frameX = frameXB.position - frameXA.position;
        frameX.Normalize();
        frameXMujoco = unityToMujocoSpace(frameX);


        if(lfFootValue + rfFootValue < 1.9f)
        {
            startYFrame = frameYMujoco;             //self.start_yframe = np.array([self.d.sensor(self.sensors[2]).data[0], self.d.sensor(self.sensors[2]).data[1], self.d.sensor(self.sensors[2]).data[2]])
            goalYFrame = Geometry.rotateAroundZAxis(goalDeltaRadians, startYFrame);  //self.goal_yframe = geo.rotate_z_axis(self.goal_delta_radians, self.start_yframe)

            startPos = unityToMujocoSpace(machinePos);//self.start_pos = np.array([self.d.sensor(self.sensors[0]).data[0], self.d.sensor(self.sensors[0]).data[1], self.d.sensor(self.sensors[0]).data[2]])

            //self.start_yframe = np.array([self.d.sensor(self.sensors[2]).data[0], self.d.sensor(self.sensors[2]).data[1], self.d.sensor(self.sensors[2]).data[2]])
            

            goalPos = startPos + (goalMag * startYFrame);//self.goal_pos = self.start_pos + (self.goal_mag * self.start_yframe)

        }

    //block for turning

        yFrameFrame = frameYMujoco; // self.yframe_frame = np.array([self.d.sensor(self.sensors[2]).data[0], self.d.sensor(self.sensors[2]).data[1], self.d.sensor(self.sensors[2]).data[2]])
        //# flatten the z axis
        goalYFrame.z = yFrameFrame.z = 0f;//self.goal_yframe[2] = self.yframe_frame[2] = 0.0
        
        //    self.off_center = 0.0
        if((goalYFrame.z == yFrameFrame.z) & (goalYFrame.y == yFrameFrame.y) & (goalYFrame.x == yFrameFrame.x))//if ((self.goal_yframe[0] == self.yframe_frame[0]) and(self.goal_yframe[1] == self.yframe_frame[1]) and(self.goal_yframe[2] == self.yframe_frame[2])):
        {
            offCenter = 0f;//    self.off_center = 0.0
        }
        else //else:
        {
            offCenter = Geometry.angleBetweenVectors(goalYFrame, yFrameFrame);//    self.off_center = geo.angle_between_vectors(self.goal_yframe, self.yframe_frame)
        }
    //end block for turning


    //block for translating

        posFrame = unityToMujocoSpace(machinePos);//self.pos_frame = np.array([self.d.sensor(self.sensors[0]).data[0], self.d.sensor(self.sensors[0]).data[1], self.d.sensor(self.sensors[0]).data[2]])
        //# flatten the z axis
        goalPos.z = posFrame.z = 0f;//self.goal_pos[2] = self.pos_frame[2] = 0.0
        remaining = goalPos - posFrame;//self.remaining = self.goal_pos - self.pos_frame
        remainingMag = remaining.magnitude;//self.remaining_mag = np.linalg.norm(self.remaining)


    //end block for tranlating

    }

    public void tickSkill()
    {
        if(runningState != RLSkillRig.RUNNINGSTATE.RUNNING)
        {
            return;
        }


        switch (skillRigDescriptorJSON.skillSpaceMode)
        {
            case SkillRigDescriptorJSON.SKILLSPACEMODE.ACTUATORPOSITIONSKILL:
                tickPositionSkill();
                break;

            default:
                break;
        }

    }
    void tickPositionSkill()
    {   // 0, 1, 2, 3, 4, 5, (6)
        if (actionFrameIndex >= fpa)
        {
            updatePositionState();
            publishState();

            actionFrameIndex = 0;
        }
            
        pushPositionActionsToMachine();
        actionFrameIndex++;
    }
    void tickForceSkill() { }
    void tickFkPositionSkill() { }
    void tickFkForceSkill() { }

    public void updatePositionState()
    {

        
        switch(skillRigDescriptorJSON.stateMode) 
        {
            case SkillRigDescriptorJSON.STATEMODE.ROLLING:
                updateRollingState();
                break;
            case SkillRigDescriptorJSON.STATEMODE.FWBW:
                updateTranslatingState();
            break;
            case SkillRigDescriptorJSON.STATEMODE.LTRT:
                updateTurningState();
                break;
            default:
            break;
        }


    }

    void updateRollingState()
    {
        // # 19, rolltofacedown: lf state, rf state, segs state, framezaxis, frameyaxis, framexaxis



        statesJSON.states[0] = machine.leftFront.lastValidIk.x * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[1] = machine.leftFront.lastValidIk.y * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[2] = machine.leftFront.lastValidIk.w * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[3] = machine.leftFront.lastValidIk.z * (2 * Mathf.PI) * mujocoCompGear;

        statesJSON.states[4] = machine.rightFront.lastValidIk.x * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[5] = machine.rightFront.lastValidIk.y * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[6] = machine.rightFront.lastValidIk.w * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[7] = machine.rightFront.lastValidIk.z * (2 * Mathf.PI) * mujocoCompGear;



        // seg units are degrees, not revolutions!
        statesJSON.states[8] = -(machine.segs.inputX / 360f) * (2 * Mathf.PI) * mujocoSegsGear; // reverse the direction
        statesJSON.states[9] = (machine.segs.inputZ / 360f) * (2 * Mathf.PI) * mujocoSegsGear; // verified

        
        statesJSON.states[10] = frameZMujoco.x;
        statesJSON.states[11] = frameZMujoco.y;
        statesJSON.states[12] = frameZMujoco.z; 

        statesJSON.states[13] = frameYMujoco.x;
        statesJSON.states[14] = frameYMujoco.y;
        statesJSON.states[15] = frameYMujoco.z; 

        statesJSON.states[16] = frameXMujoco.x;
        statesJSON.states[17] = frameXMujoco.y;
        statesJSON.states[18] = frameXMujoco.z; 

    }
    void updateTranslatingState()
    {
        // # 13, fdfw state: lf state, rf state, remaining.x, remaining.y, armatureframezaxis.z, lf foot switch, rf foot switch



        statesJSON.states[0] = machine.leftFront.lastValidIk.x * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[1] = machine.leftFront.lastValidIk.y * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[2] = machine.leftFront.lastValidIk.w * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[3] = machine.leftFront.lastValidIk.z * (2 * Mathf.PI) * mujocoCompGear;

        statesJSON.states[4] = machine.rightFront.lastValidIk.x * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[5] = machine.rightFront.lastValidIk.y * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[6] = machine.rightFront.lastValidIk.w * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[7] = machine.rightFront.lastValidIk.z * (2 * Mathf.PI) * mujocoCompGear;



        statesJSON.states[8] = remaining.x; // remaining x
        statesJSON.states[9] = remaining.y; // remaining y
        statesJSON.states[10] = frameZMujoco.z;


        statesJSON.states[11] = lfFootValue; // lf foot
        statesJSON.states[12] = rfFootValue; // rf foot

    }
    void updateTurningState()
    {
        // # 12, fdlt state: lf state, rf state, offcenter, armatureframezaxis.z, lf foot switch, rf foot switch



        statesJSON.states[0] = machine.leftFront.lastValidIk.x * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[1] = machine.leftFront.lastValidIk.y * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[2] = machine.leftFront.lastValidIk.w * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[3] = machine.leftFront.lastValidIk.z * (2 * Mathf.PI) * mujocoCompGear;

        statesJSON.states[4] = machine.rightFront.lastValidIk.x * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[5] = machine.rightFront.lastValidIk.y * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[6] = machine.rightFront.lastValidIk.w * (2 * Mathf.PI) * mujocoCompGear;
        statesJSON.states[7] = machine.rightFront.lastValidIk.z * (2 * Mathf.PI) * mujocoCompGear;



        statesJSON.states[8] = offCenter; // off center
        statesJSON.states[9] = frameZMujoco.z;


        statesJSON.states[10] = lfFootValue; // lf foot
        statesJSON.states[11] = rfFootValue; // rf foot


    }



    public void preProcessActions()
    {
        switch (skillRigDescriptorJSON.actionMode)
        {
            case SkillRigDescriptorJSON.ACTIONMODE.ZXYEZXYE:
                preProcessActionsZXYEZXYE();
                break;
            case SkillRigDescriptorJSON.ACTIONMODE.ZXYEZXYESEGS:
                preProcessActionsZXYEZXYESEGS();
                break;

            default:
                break;
        }
    }
    public void preProcessActionsZXYEZXYE()
    {

        actionsJSON.actions[0] = actionsJSON.actions[0] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[1] = actionsJSON.actions[1] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[2] = actionsJSON.actions[2] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[3] = actionsJSON.actions[3] / ((2 * Mathf.PI) * mujocoCompGear);

        actionsJSON.actions[4] = actionsJSON.actions[4] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[5] = actionsJSON.actions[5] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[6] = actionsJSON.actions[6] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[7] = actionsJSON.actions[7] / ((2 * Mathf.PI) * mujocoCompGear);



        //shrink the actions to interpolate across the entire range of actionFrames

        actionsJSON.actions[0] = actionsJSON.actions[0] / fpa;
        actionsJSON.actions[1] = actionsJSON.actions[1] / fpa;
        actionsJSON.actions[2] = actionsJSON.actions[2] / fpa;
        actionsJSON.actions[3] = actionsJSON.actions[3] / fpa;

        actionsJSON.actions[4] = actionsJSON.actions[4] / fpa;
        actionsJSON.actions[5] = actionsJSON.actions[5] / fpa;
        actionsJSON.actions[6] = actionsJSON.actions[6] / fpa;
        actionsJSON.actions[7] = actionsJSON.actions[7] / fpa;



    }
    public void preProcessActionsZXYEZXYESEGS()
    {

        actionsJSON.actions[0] = actionsJSON.actions[0] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[1] = actionsJSON.actions[1] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[2] = actionsJSON.actions[2] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[3] = actionsJSON.actions[3] / ((2 * Mathf.PI) * mujocoCompGear);

        actionsJSON.actions[4] = actionsJSON.actions[4] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[5] = actionsJSON.actions[5] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[6] = actionsJSON.actions[6] / ((2 * Mathf.PI) * mujocoCompGear);
        actionsJSON.actions[7] = actionsJSON.actions[7] / ((2 * Mathf.PI) * mujocoCompGear);


        // seg units are degrees, not revolutions!
        actionsJSON.actions[8] = (-actionsJSON.actions[8] / ((2 * Mathf.PI) * mujocoSegsGear)) * 360; // reverse the direction
        actionsJSON.actions[9] = (actionsJSON.actions[9] / ((2 * Mathf.PI) * mujocoSegsGear)) * 360;
        


        //shrink the actions to interpolate across the entire range of actionFrames

        actionsJSON.actions[0] = actionsJSON.actions[0] / fpa;
        actionsJSON.actions[1] = actionsJSON.actions[1] / fpa;
        actionsJSON.actions[2] = actionsJSON.actions[2] / fpa;
        actionsJSON.actions[3] = actionsJSON.actions[3] / fpa;

        actionsJSON.actions[4] = actionsJSON.actions[4] / fpa;
        actionsJSON.actions[5] = actionsJSON.actions[5] / fpa;
        actionsJSON.actions[6] = actionsJSON.actions[6] / fpa;
        actionsJSON.actions[7] = actionsJSON.actions[7] / fpa;


        // segs
        actionsJSON.actions[8] = actionsJSON.actions[8] / fpa;
        actionsJSON.actions[9] = actionsJSON.actions[9] / fpa;


    }
    void pushPositionActionsToMachine()
    {
        switch (skillRigDescriptorJSON.actionMode)
        {
            case SkillRigDescriptorJSON.ACTIONMODE.ZXYEZXYE:
                pushPositionActionsToMachineZXYEZXYE();
                break;
            case SkillRigDescriptorJSON.ACTIONMODE.ZXYEZXYESEGS:
                pushPositionActionsToMachineZXYEZXYESEGS();
                break;

            default:
                break;
        }
    }

    void pushPositionActionsToMachineZXYEZXYE()
    {
        //RL is additive, these function don't set, it adds

        //order is x, y, w, z -> 0, 1, 3, 2
        //4, 5, 7, 6
        machine.lfIk = machine.leftFront.doDirectLeft(
                            (float)actionsJSON.actions[0],
                            (float)actionsJSON.actions[1],
                            (float)actionsJSON.actions[3],
                            (float)actionsJSON.actions[2]);

        machine.lfFk = machine.leftFront.valFk();

        machine.rfIk = machine.rightFront.doDirectRight(
                            (float)actionsJSON.actions[4],
                            (float)actionsJSON.actions[5],
                            (float)actionsJSON.actions[7],
                            (float)actionsJSON.actions[6]);

        machine.rfFk = machine.rightFront.valFk();

        
        //do I need to call updatestance?
        machine.updateStance();

    }
    void pushPositionActionsToMachineZXYEZXYESEGS()
    {
        //RL is additive, these function don't set, it adds

        //order is x, y, w, z -> 0, 1, 3, 2
        //4, 5, 7, 6
        machine.lfIk = machine.leftFront.doDirectLeft(
                            (float)actionsJSON.actions[0],
                            (float)actionsJSON.actions[1],
                            (float)actionsJSON.actions[3],
                            (float)actionsJSON.actions[2]);

        machine.lfFk = machine.leftFront.valFk();

        machine.rfIk = machine.rightFront.doDirectRight(
                            (float)actionsJSON.actions[4],
                            (float)actionsJSON.actions[5],
                            (float)actionsJSON.actions[7],
                            (float)actionsJSON.actions[6]);

        machine.rfFk = machine.rightFront.valFk();


        machine.segsIk = machine.segs.doAdditiveIk((float)actionsJSON.actions[8], (float)actionsJSON.actions[9]);
        machine.segsFk = machine.segs.valFk();


        //do I need to call updatestance?
        machine.updateStance();

    }
    public bool isCurrentSkillComplete()
    {
        return (runningState == RUNNINGSTATE.COMPLETE);
    }
    public bool isCurrentSkillStopped()
    {
        return (runningState == RUNNINGSTATE.STOPPED);
    }
    public bool isCurrentSkillCompleteOrStopped()
    {
        return ((runningState == RUNNINGSTATE.COMPLETE) | (runningState == RUNNINGSTATE.STOPPED));
    }
    public void stopCurrentSkill()
    {
        runningState = RUNNINGSTATE.STOPPED;
    }
    public void processMqttMessages()
    {
        while (m2MqttCRClient.hasMessages())
        {
            TopicMessageJSON m = m2MqttCRClient.getNextMessage();
            if (m.topic == subscribeTopic)
            {
                actionsJSON = JsonUtility.FromJson<ActionsJSON>(m.messagestring);
                preProcessActions();
            }
            else if (m.topic == responseServiceTopic)
            {
                skillServiceJSON = JsonUtility.FromJson<SkillServiceJSON>(m.messagestring);

                if (skillServiceJSON.command == "skillcomplete")
                {
                    if (skillServiceJSON.param1 == currentSkill)
                    {
                        Debug.Log("skill completed:");
                        runningState = RUNNINGSTATE.COMPLETE;
                    }
                }

            }

        }
    }

    public void publishState()
    {

        m2MqttCRClient.publish(publishTopic, JsonUtility.ToJson(statesJSON));

    }
    public void setupSkill(string name)
    {
        //by the time this is called, the descriptor is already loaded?
        //but I have to recreate the state and action lists to the correct size

        statesJSON.states.Clear();
        actionsJSON.actions.Clear();

        for (int i = 0; i < skillRigDescriptorJSON.stateCount; i++)
        {
            statesJSON.states.Add(0.0f);
        }
        for (int i = 0; i < skillRigDescriptorJSON.actionCount; i++)
        {
            actionsJSON.actions.Add(0.0);
        }


        requestLoadFromService(name);
        runningState = RLSkillRig.RUNNINGSTATE.RUNNING;

    }
    void requestLoadFromService(string name)
    {

        m2MqttCRClient.publish(requestServiceTopic, JsonUtility.ToJson(new SkillServiceJSON("loadskillrig", name, "")));

    }


    Vector3 unityToMujocoSpace(Vector3 u)
    {
        Vector3 ret = u;
        //swap z and y
        ret.y = u.z;
        ret.z = u.y;

        return ret;
    }

    public bool importFromJSON(string name)
    {
        StreamReader sReader;
        bool success = true;

        string fullPath = "c:\\crek\\db\\skill\\" + name + ".skillrigdescriptor.json";
        try
        {
            sReader = new StreamReader(fullPath);
            string fileString = sReader.ReadToEnd();
            skillRigDescriptorJSON = JsonUtility.FromJson<SkillRigDescriptorJSON>(fileString);
            sReader.Close();
            success = true;

            currentSkill = name;
            goalDeltaPos = skillRigDescriptorJSON.goalDeltaPos;
            goalDeltaRadians = skillRigDescriptorJSON.goalDeltaRadians;
            goalMag = goalDeltaPos.magnitude;
            if (goalDeltaPos.y < 0)
            {
                goalMag = -goalMag;
            }

            runningState = RLSkillRig.RUNNINGSTATE.STOPPED;
        }
        catch(FileNotFoundException e)
        {
            Debug.Log(name + ": not found");
            success = false;
        }
        finally
        {
            ;
        }
        


        return success;
    }
    
    public bool exportToJSON(string name)
    {


        //second parameter is to print prettey
        string jsonString = JsonUtility.ToJson(skillRigDescriptorJSON, true);

        StreamWriter sWriter;
        string fullPath = "c:\\crek\\db\\skill\\" + name + ".skillrigdescriptor.json";
        sWriter = new StreamWriter(fullPath);
        sWriter.Write(jsonString);
        sWriter.Close();

        return true;

    }
}
