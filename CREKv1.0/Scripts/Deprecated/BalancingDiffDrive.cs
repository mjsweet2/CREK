using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalancingDiffDrive : MonoBehaviour
{

    public enum MOVESTATE { STILL, FORWARD, BACKWARD, LEFTWARD, RIGHTWARD };
    public MOVESTATE moveState;

    public enum BALANCEMODE { NOTBALANCING, BALANCING };
    public BALANCEMODE balanceMode;

    public ArticulationBody leftWheel;
    public ArticulationBody rightWheel;
    public ArticulationDrive leftAD;
    public ArticulationDrive rightAD;



    public float currLeftTarget;
    public float currRightTarget;

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
    public float turnRate;

    public float balRotOffset;

    public int trajIndex;

    //trajectories is implemented as velocity, not position
    public List<float> velTrajLeft;
    public List<float> velTrajRight;
    public List<float> secondaryPosTraj;
    public string cmd01, cmd02;


   

    public float toggleBalance;
    public int toggleIndex;
    public int toggleIndexMax;
    public float toggleRange;

    // Start is called before the first frame update
    void Start()
    {
        leftAD = leftWheel.xDrive;
        rightAD = rightWheel.xDrive;
       

        velTrajLeft = new List<float>();
        velTrajRight = new List<float>();

        //balance0, pid = new PID(0.2f,2.1f,0f);
        pid = new PID(0.2f,2.1f,0f);

      

    }

    // Update is called once per frame
    void Update()
    {
        processWSAD();

        
       
        tick(); //increment trajectories

        pushTargets();

        balance();
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

    

    void balance()
    {

        if(balanceMode == BALANCEMODE.NOTBALANCING)
        {
            return;
        }

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
        if(moveState != MOVESTATE.STILL)
        {
            return;
        }

        balanceOppositeGoal = balanceNeutral;
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
            currLeftTarget += 360*(turnRate * (1 / 60f));
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


        leftWheel.xDrive = leftAD;
        rightWheel.xDrive = rightAD;

       
        
    }

}
