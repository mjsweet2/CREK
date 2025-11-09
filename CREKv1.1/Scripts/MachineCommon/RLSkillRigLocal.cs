/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the MIT License.
 * You should have received a copy of the MIT License with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class is doing the space and gear conversion between mujoco and unity

public class RLSkillRigLocal : MonoBehaviour
{
    public RLSkillLocal lf;
    public RLSkillLocal rf;
    public RLSkillLocal segs;
    public double[] lfActions;
    public double[] rfActions;
    public double[] segsActions;
    public Machine2ZXYE machine2ZXYE;

    public float[] inputState;
    public int actionCount;
    public float mujocoCompGear;
    public float mujocoSegsGear;

    public Transform inputOrientationA, inputOrientationB;
    public Vector3 inputOrientation;

    public enum RUNNINGSTATE { NONE, STOPPED, RUNNING };
    public RUNNINGSTATE runningState;

    // Start is called before the first frame update
    void Start()
    {
        inputState = new float[12];

        lfActions = new double[lf.actionCount];
        rfActions = new double[rf.actionCount];
        segsActions = new double[segs.actionCount];

        actionCount = lf.actionCount + rf.actionCount + segs.actionCount;

    }

    // Update is called once per frame
    void Update()
    {
        //doInference();
    }

    public void doInference()
    {
 
        updateState();

        string inputString = inputState[0].ToString() + ":" + inputState[1].ToString() + ":" + inputState[2].ToString() + ":";
        inputString = inputString + inputState[3].ToString() + ":" + inputState[4].ToString() + ":" + inputState[5].ToString() + ":";
        inputString = inputString + inputState[6].ToString() + ":" + inputState[7].ToString() + ":" + inputState[8].ToString() + ":";
        inputString = inputString + inputState[9].ToString() + ":" + inputState[10].ToString() + ":" + inputState[11].ToString() + ":";
        //Debug.Log("state: " + inputString);
        lf.doInference(inputState, ref lfActions);
        rf.doInference(inputState, ref rfActions);
        segs.doInference(inputState, ref segsActions);

    }
    public void getActions(ref double[] actions)
    {
        if (actions.Length != actionCount)
            return;

        actions[0] = lfActions[0] / ((2 * Mathf.PI) * mujocoCompGear);
        actions[1] = lfActions[1] / ((2 * Mathf.PI) * mujocoCompGear);
        actions[2] = lfActions[2] / ((2 * Mathf.PI) * mujocoCompGear);
        actions[3] = lfActions[3] / ((2 * Mathf.PI) * mujocoCompGear); 

        actions[4] = rfActions[0] / ((2 * Mathf.PI) * mujocoCompGear);
        actions[5] = rfActions[1] / ((2 * Mathf.PI) * mujocoCompGear);
        actions[6] = rfActions[2] / ((2 * Mathf.PI) * mujocoCompGear);
        actions[7] = rfActions[3] / ((2 * Mathf.PI) * mujocoCompGear);

        // TODO
        // seg units are degrees, not revolutions!
        actions[8] = -segsActions[0] / ((2 * Mathf.PI) * mujocoSegsGear); // reverse the direction
        actions[9] = segsActions[1] / ((2 * Mathf.PI) * mujocoSegsGear);
        actions[8] *= 360f;
        actions[9] *= 360f;

    }

    public void updateState()
    {
        //lf, rf, segs, dir_z, dir_x

        inputOrientation = inputOrientationB.position - inputOrientationA.position;
        inputOrientation = unityToSkillSpace(inputOrientation);

        //I'm using rotation as a unit, covert to radians for skill calc
        //factor the gear from mujoco, 6 on lf, rf, 5 on segs
        inputState[0] = machine2ZXYE.leftFront.lastValidIk.x * (2 * Mathf.PI) * mujocoCompGear;
        inputState[1] = machine2ZXYE.leftFront.lastValidIk.y * (2 * Mathf.PI) * mujocoCompGear;
        inputState[2] = machine2ZXYE.leftFront.lastValidIk.w * (2 * Mathf.PI) * mujocoCompGear;
        inputState[3] = machine2ZXYE.leftFront.lastValidIk.z * (2 * Mathf.PI) * mujocoCompGear;

        inputState[4] = machine2ZXYE.rightFront.lastValidIk.x * (2 * Mathf.PI) * mujocoCompGear;
        inputState[5] = machine2ZXYE.rightFront.lastValidIk.y * (2 * Mathf.PI) * mujocoCompGear;
        inputState[6] = machine2ZXYE.rightFront.lastValidIk.w * (2 * Mathf.PI) * mujocoCompGear;
        inputState[7] = machine2ZXYE.rightFront.lastValidIk.z * (2 * Mathf.PI) * mujocoCompGear;

        

        // TODO
        // seg units are degrees, not revolutions!
        inputState[8] = -(machine2ZXYE.segs.inputX/360f) * (2 * Mathf.PI) * mujocoSegsGear; // reverse the direction
        inputState[9] = (machine2ZXYE.segs.inputZ/360f) * (2 * Mathf.PI) * mujocoSegsGear; // verified

        inputState[10] = inputOrientation.x;
        inputState[11] = inputOrientation.z; // verified

    }

    Vector3 unityToSkillSpace(Vector3 u)
    {
        Vector3 ret = u;
        //swap z and y
        ret.y = u.z;
        ret.z = u.y;

        return ret;
    }

}
