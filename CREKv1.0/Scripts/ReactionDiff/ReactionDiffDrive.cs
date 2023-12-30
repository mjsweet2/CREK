using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionDiffDrive : MonoBehaviour
{

    public enum MOVESTATE { STILL, FORWARD, BACKWARD, LEFTWARD, RIGHTWARD };
    public MOVESTATE moveState;

    public ArticulationBody leftWheel;
    public ArticulationBody rightWheel;
    public ArticulationDrive leftAD;
    public ArticulationDrive rightAD;

    public ArticulationBody reactionWheel;
    public ArticulationDrive reactionAD;

    public float currLeftTarget;
    public float currRightTarget;

    public PID pid;
    public float currPID;

    public Transform balanceSensor;
    public Vector3 balanceRotation;
    public Vector3 prevBalanceRotation;
    public float balanceOpposite;
    public float prevBalanceOpposite;
    public float balanceOppositeGoal;
    public float balanceForward;
    public float balanceBackward;
    public float balanceNeutral;
    public float frameRadius;
    public float wheelRadius;
    public float turnRate;
    public int trajIndex;

    
    public float reactionWheelRadius;
    public float reactionWheelMass;
    public float frameVelocity;
    public float prevFrameVelocity;
    public float frameAcceleration;
    public float frameTorque;
    public float loadMass;
    public float frameInertia;
    public float wheelInertia;
    public float currReactionTarget;
    public float reactionVelocity;
    public float prevReactionVelocity;
    public float reactionAcceleration;
    public float reactionTorque;
    public float deltaReactionRotation;
    public float totalReactionRotation;
    public float gravityAcceleration;
    public int zeroVelocityCount;
    public int frameCount;
    public float outputScaler;
   

    //trajectories is implemented as velocity, not position
    public List<float> velTrajLeft;
    public List<float> velTrajRight;


    // Start is called before the first frame update
    void Start()
    {
        leftAD = leftWheel.xDrive;
        rightAD = rightWheel.xDrive;
        reactionAD = reactionWheel.xDrive;

        velTrajLeft = new List<float>();
        velTrajRight = new List<float>();

        pid = new PID(0.4f, 0f, 0.05f);



    }

    // Update is called once per frame
    void Update()
    {
        processWSAD();

        tick();
        pushTargets();

        reactionBalance();

    }

    public void tick()
    {
        if (moveState == MOVESTATE.STILL)
        {
            return;
        }

        if (trajIndex >= velTrajLeft.Count)
        {
            moveState = MOVESTATE.STILL;
            trajIndex = 0;
            velTrajLeft.Clear(); 
            velTrajRight.Clear();
            balanceOppositeGoal = balanceNeutral;
            return;

        }

        currLeftTarget += (velTrajLeft[trajIndex] * (1 / 60f));
        currRightTarget += (velTrajRight[trajIndex] * (1 / 60f));

        trajIndex++;
    }
    void reactionBalance()
    {

        //I=mr^2
        //t=Fr = mar, t=I*(angular acceleration)
        frameInertia = (loadMass + reactionWheelMass) * frameRadius * frameRadius;
        wheelInertia = (reactionWheelMass) * reactionWheelRadius * reactionWheelRadius;  

        //I need to use the x value of this
        balanceRotation = balanceSensor.rotation.eulerAngles;

        balanceOpposite = Mathf.Sin(balanceRotation.x * Mathf.Deg2Rad) * -9.8f;
        currPID = pid.Update(balanceOppositeGoal, balanceOpposite, 1f / 60f);

        gravityAcceleration = currPID; //balanceOpposite

        frameTorque = gravityAcceleration * (loadMass + reactionWheelMass) * frameRadius;// fframeAcceleration * frameInertia;

        //do I just set the reaction torque equal to the frame torque?
        //reactionTorque = frameTorque
        //observation controller, accumulate the velocity and 
        //oscillate the setpoint in the opposite direction?

        reactionAcceleration = (frameTorque / wheelInertia);

        prevReactionVelocity = reactionVelocity;
        reactionVelocity += (reactionAcceleration * (1f / 60f));

        deltaReactionRotation = (360f / (reactionWheelRadius * 2 * Mathf.PI)) * -(reactionVelocity * (1f / 60f));
        totalReactionRotation += deltaReactionRotation;

        currReactionTarget += deltaReactionRotation;

    }
    void tradBalance()
    {

        //I need to use the x value of this
        balanceRotation = balanceSensor.rotation.eulerAngles;

        balanceOpposite = Mathf.Sin(balanceRotation.x * Mathf.Deg2Rad) * frameRadius;

        float wheelCirc = (Mathf.PI * 2f * wheelRadius);


        float pidControlled = pid.Update(balanceOppositeGoal, balanceOpposite, 1 / 60f);


        currLeftTarget += ((pidControlled / wheelCirc) * 360f);
        currRightTarget += ((pidControlled / wheelCirc) * 360f);

    }

    public void processCmd(string cmdString)
    {
        // support forward, backward, leftward, rightward + 2 float param
        // params are velocity, and frame count. 60 frames would be 1 s.
        //1. using wheelDiameter, convert velocity(m/s) to rotation speed (angles/s)
        //2. create list of velocities in velTrajLeft & velTrajRight, noting direction

        // when moving
        //3. get next angle/s, integrate over 1/60 to get delta pos.
        //4. when trajIndex = 0 traj.Count, clear the trajectories, and set my state to still

        if (moveState != MOVESTATE.STILL)
            return;

        Debug.Log(cmdString);

        string[] tokens = cmdString.Trim().Split(' ');

        if (tokens.Length != 3)
            return;

        float velParam;
        if (!float.TryParse(tokens[1], out velParam))
            return;

        int frameParam;
        if (!int.TryParse(tokens[2], out frameParam))
            return;

        //convert velocity to angular velocity of wheels;
        //- to reverse direction
        velParam = -(velParam / (Mathf.PI * 2 * wheelRadius)) * 360f;
        velTrajLeft.Clear();
        velTrajRight.Clear();
        switch (tokens[0])
        {
            case "forward":
                {
                    moveState = MOVESTATE.FORWARD;
                    balanceOppositeGoal = balanceForward;
                    for (int i = 0; i < frameParam; i++)
                    {
                        velTrajLeft.Add(velParam);
                        velTrajRight.Add(velParam);
                    }
                    break;
                }
            case "backward":
                {
                    moveState = MOVESTATE.BACKWARD;
                    balanceOppositeGoal = balanceBackward;
                    for (int i = 0; i < frameParam; i++)
                    {
                        velTrajLeft.Add(-velParam);
                        velTrajRight.Add(-velParam);
                    }
                    break;
                }
            case "leftward":
                {
                    moveState = MOVESTATE.LEFTWARD;
                    for (int i = 0; i < frameParam; i++)
                    {
                        velTrajLeft.Add(-velParam);
                        velTrajRight.Add(velParam);
                    }
                    break;
                }
            case "rightward":
                {
                    moveState = MOVESTATE.RIGHTWARD;
                    for (int i = 0; i < frameParam; i++)
                    {
                        velTrajLeft.Add(velParam);
                        velTrajRight.Add(-velParam);
                    }
                    break;
                }
        }

    }


    //positive values move backward
    public void processWSAD()
    {

        //balanceOppositeGoal
        //balanceForward
        //balanceBackward
        //balanceNeutral
        if (moveState != MOVESTATE.STILL)
        {
            return;
        }

        balanceOppositeGoal = balanceNeutral;
        if (Input.GetKey(KeyCode.W))
        {
            currLeftTarget += -360 * (turnRate * (1 / 60f));
            currRightTarget += -360 * (turnRate * (1 / 60f));
            //balanceOppositeGoal = balanceForward; //doesn't seem to affect
        }
        if (Input.GetKey(KeyCode.S))
        {
            currLeftTarget += 360 * (turnRate * (1 / 60f));
            currRightTarget += 360 * (turnRate * (1 / 60f));
            //balanceOppositeGoal = balanceBackward; //doesn't seem to affect
        }

        if (Input.GetKey(KeyCode.A))
        {
            currLeftTarget += 360 * (turnRate * (1 / 60f));
            currRightTarget += -360 * (turnRate * (1 / 60f));
        }
        if (Input.GetKey(KeyCode.D))
        {
            currLeftTarget += -360 * (turnRate * (1 / 60f));
            currRightTarget += 360 * (turnRate * (1 / 60f));
        }






    }
    public void pushTargets()
    {
        leftAD.target = currLeftTarget;
        rightAD.target = currRightTarget;
        reactionAD.target = currReactionTarget;


        leftWheel.xDrive = leftAD;
        rightWheel.xDrive = rightAD;
        reactionWheel.xDrive = reactionAD;
    }

}
