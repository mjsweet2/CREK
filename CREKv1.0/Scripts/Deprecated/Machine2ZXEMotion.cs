using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machine2ZXEMotion : MonoBehaviour
{
	public CRMotionDBController nsdb;
	public Machine2ZXEPlanner planner;

	public Trajectory currTraj;

	public string currentNode;
	public string currentMotionName;
	public bool currentMotionIsOver;
	public string stateInput;
	public float stateInputFloat;
	public int stateInputInt;
	public Vector3 currentFrame;
	public bool isActive;
	public bool currentIsAPauseTraj;

	public int motionSpeed;//experimenting with setting speed at interpolation time

	public enum MOTIONINTERUPT { NOINTERUPT, SOFTCANCEL, HARDCANCEL };
	public MOTIONINTERUPT motionInterupt;


	public Vector3 watchRTVector3;
	public string watchRTTrajName;


	void Start()
	{
		currentMotionIsOver = true;
	}


	// Update is called once per frame
	void Update()
	{
		if (isActive)
			loop();
	}
	void loop()
	{
		tick();

		stateInput = "0";
		if (Input.GetKey(KeyCode.H))
			stateInput = "1";

		processInput(stateInput);
	}

	void tick()
	{
		if (!currentMotionIsOver)
			visitCurrentNode();
	}

	public void setInterupt(MOTIONINTERUPT newInterupt)
	{
		motionInterupt = newInterupt;
	}
	void processInput(string i)
	{
		if (i == "reset")
			resetMotion();
		else
			//stateInputFloat = float.Parse(i);
			processEdges();
	}

	public void startMotion(string motionName)
	{
		//if I'm already running a motion, don't do anything
		if (!currentMotionIsOver)
			return;

		//do nothing if I don't have a motion by that name, don't set currentNode to nothing
		string retNode = nsdb.getFirstNodeByMotionName(motionName);

		Debug.Log("in motion.startMotion(): " + motionName + " + returns: " + retNode);

		if (retNode == "")
		{
			currentMotionName = "";
			currentNode = "";
			return;
		}


		currentMotionName = motionName;
		currentNode = retNode;
		currentMotionIsOver = false;
		Debug.Log("traj name: " + nsdb.getTrajectoryByNode(currentNode));

		currTraj = resolveTrajByNodeName(currentNode);
		currTraj.resetTrajectory();
		currentFrame = currTraj.getFirstFrame();
		currentIsAPauseTraj = currTraj.isAPause();

	}
	public bool isPlayingPauseTraj() { return currentIsAPauseTraj; }
	public Vector3 getCurrentFrame(float userInput)
	{
		stateInputFloat = userInput;
		return currentFrame;
	}
	void visitCurrentNode()
	{
		visitNode(currentNode);
	}
	void visitNode(string n)
	{
		if (n != "nonode")
		{

			if (currTraj == null)
				return;
			if (!currTraj.isComplete)
			{
				//this line needs to be here for some reason
				//putting this in processedges breaks things
				currentIsAPauseTraj = currTraj.isAPause();
				currentFrame = currTraj.getNextFrame();
			}
			else
			{
				currentMotionIsOver = nsdb.isLeafNode(n);
			}
		}
	}
	Trajectory resolveTrajByNodeName(string nodeName)
	{
		string trajName = nsdb.getTrajectoryByNode(nodeName);

		string trajType = planner.getTrajectoryType(trajName);

		Debug.Log("traj name, type: " + trajName + ", " + trajType);

		if (trajType == "NAMEDTRAJECTORY")
		{
			return getTrajectoryByName(trajName);
		}
		else if (trajType == "STEPTRAJECTORY")
		{
			//this is a generated traj node
			//get the inputnodes from this trajectory node
			//and resolve, then use them to create trajectory points generateTrajectory(a,b);
			Trajectory retTraj = Instantiate<Trajectory>(planner.template);
			string nodeInputString = nsdb.getInputStringByNode(nodeName);
			Debug.Log("inputString: " + nodeInputString);
			CRInputsJSON inputsJSON = JsonUtility.FromJson<CRInputsJSON>(nodeInputString);

			//following is in development, needs more testing
			Vector3 aa = resolveVectorInput(inputsJSON.inputs[0]);
			Vector3 bb = resolveVectorInput(inputsJSON.inputs[1]);
			int rr = resolveIntInput(inputsJSON.inputs[2]);
			//<end dev stuff>

			Debug.Log("aa, bb, rr: " + aa.ToString() + ", " + bb.ToString() + ", " + rr.ToString());

			TrajJSON trajJSON = new TrajJSON();
			trajJSON.resolution = rr;// res;

			// changed this from 9 to 5, my trajectory doesn't hand let resolution than keyframes well
			Vector3[] points = new Vector3[5];
			planner.generateStepPoints(ref points, aa, bb);// a, b);
			for (int i = 0; i < points.Length; i++)
			{
				trajJSON.keyframes.Add(new KeyFrameJSON("raw", (points[i])));

			}

			retTraj.initFromJSONString(trajJSON);
			retTraj.buildMotionTable();

			return retTraj;
		}
		else if (trajType == "PAUSETRAJECTORY")
		{
			//this is a generated traj node
			//get the inputnodes from this trajectory node
			//and resolve, then use them to create trajectory points generateTrajectory(a,b);
			Trajectory retTraj = Instantiate<Trajectory>(planner.template);
			string nodeInputString = nsdb.getInputStringByNode(nodeName);
			Debug.Log("inputString: " + nodeInputString);
			CRInputsJSON inputsJSON = JsonUtility.FromJson<CRInputsJSON>(nodeInputString);

			//following is in development, needs more testing
			Vector3 aa = resolveVectorInput(inputsJSON.inputs[0]);
			Vector3 bb = resolveVectorInput(inputsJSON.inputs[1]);
			int rr = resolveIntInput(inputsJSON.inputs[2]);
			//<end dev stuff>

			Debug.Log("pause: aa, bb, rr: " + aa.ToString() + ", " + bb.ToString() + ", " + rr.ToString());

			TrajJSON trajJSON = new TrajJSON();
			trajJSON.resolution = rr;// res;

			retTraj.initFromJSONString(trajJSON);
			retTraj.buildMotionTable();

			return retTraj;
		}
		else //STRAIGHTTRAJECTORY, just 2 keyframes
		{

			//this is a generated traj node
			//get the inputnodes from this trajectory node
			//and resolve, then use them to create trajectory points generateTrajectory(a,b);
			Trajectory retTraj = Instantiate<Trajectory>(planner.template);
			string nodeInputString = nsdb.getInputStringByNode(nodeName);
			Debug.Log("inputString: " + nodeInputString);
			CRInputsJSON inputsJSON = JsonUtility.FromJson<CRInputsJSON>(nodeInputString);

			//following is in development, needs more testing
			Vector3 aa = resolveVectorInput(inputsJSON.inputs[0]);
			Vector3 bb = resolveVectorInput(inputsJSON.inputs[1]);
			int rr = resolveIntInput(inputsJSON.inputs[2]);
			//<end dev stuff>

			Debug.Log("aa, bb, rr: " + aa.ToString() + ", " + bb.ToString() + ", " + rr.ToString());

			TrajJSON trajJSON = new TrajJSON();
			trajJSON.resolution = rr;
			trajJSON.keyframes.Add(new KeyFrameJSON("raw", aa));
			trajJSON.keyframes.Add(new KeyFrameJSON("raw", bb));

			retTraj.initFromJSONString(trajJSON);
			retTraj.buildMotionTable();

			return retTraj;
		}
	}

	//these functions follow each input node in a depth firstsearch
	Vector3 resolveVectorInput(CRInputJSON nsInputJSON)
	{
		if (nsInputJSON.nodename == "")
			return Vector3.negativeInfinity;

		//IntFunction, VectorFunction, Float, Vector, Math, VectorMath
		string nodeType = nsdb.getTypeByNode(nsInputJSON.nodename);

		if (nodeType == "VectorFunction")
		{
			string funcName = nsdb.getFunctionByFunctionNode(nsInputJSON.nodename);
			return planner.getVectorValue(funcName);
		}
		else if (nodeType == "Vector")
		{
			return nsdb.getVectorFromVectorNode(nsInputJSON.nodename);
		}
		else if (nodeType == "VectorMath")
		{
			string vMathInputs = nsdb.getInputStringFromVectorMathNode(nsInputJSON.nodename);
			string ops = nsdb.getOperationFromVectorMathNode(nsInputJSON.nodename);
			CRInputsJSON vMathInputsJSON = JsonUtility.FromJson<CRInputsJSON>(vMathInputs);

			Vector3 a = resolveVectorInput(vMathInputsJSON.inputs[0]);
			Vector3 vb = resolveVectorInput(vMathInputsJSON.inputs[1]);
			float fb = resolveFloatInput(vMathInputsJSON.inputs[2]);
			Vector3 c = Vector3.negativeInfinity;
			if (ops == "ADD") { c = a + vb; }
			else if (ops == "SUB") { c = a - vb; }
			else if (ops == "MUL") { c = a * fb; }
			else if (ops == "DIV") { if (fb != 0) { c = a / fb; } }

			return c;
		}
		else if (planner.sessionVector3Exists(nsInputJSON.nodename)) //is this a session variable
		{
			return planner.getVectorValue(nsInputJSON.nodename);
		}
		else
			return Vector3.negativeInfinity;
	}

	int resolveIntInput(CRInputJSON nsInputJSON)
	{
		if (nsInputJSON.nodename == "")
			return 0;
		//IntFunction, VectorFunction, Float, Vector, Math, VectorMath
		string nodeType = nsdb.getTypeByNode(nsInputJSON.nodename);

		if (nodeType == "Int")
		{
			return nsdb.getValueFromIntNode(nsInputJSON.nodename);
		}
		else if (nodeType == "IntFunction")
		{
			string funcName = nsdb.getFunctionByFunctionNode(nsInputJSON.nodename);
			return planner.getIntValue(funcName);
		}
		if (planner.doesIntExist(nsInputJSON.nodename))
		{
			return planner.getIntValue(nsInputJSON.nodename);
		}
		else if (nodeType == "Math")
		{
			string vMathInputs = nsdb.getInputStringFromMathNode(nsInputJSON.nodename);
			string ops = nsdb.getOperationFromMathNode(nsInputJSON.nodename);
			CRInputsJSON vMathInputsJSON = JsonUtility.FromJson<CRInputsJSON>(vMathInputs);

			int a = resolveIntInput(vMathInputsJSON.inputs[0]);
			int b = resolveIntInput(vMathInputsJSON.inputs[1]);

			int c = 0;
			if (ops == "ADD") { c = a + b; }
			else if (ops == "SUB") { c = a - b; }
			else if (ops == "MUL") { c = a * b; }
			else if (ops == "DIV") { if (b != 0) { c = a / b; } }
			return c;
		}
		else
			return 0;
	}
	float resolveFloatInput(CRInputJSON nsInputJSON)
	{
		if (nsInputJSON.nodename == "")
			return 0;
		//IntFunction, VectorFunction, Float, Vector, Math, VectorMath
		string nodeType = nsdb.getTypeByNode(nsInputJSON.nodename);

		if (nodeType == "Float")
		{
			return nsdb.getValueFromFloatNode(nsInputJSON.nodename);
		}
		else if (nodeType == "FloatFunction")
		{
			string funcName = nsdb.getFunctionByFunctionNode(nsInputJSON.nodename);
			return planner.getFloatValue(funcName);
		}
		else if (nodeType == "Math")
		{
			string vMathInputs = nsdb.getInputStringFromMathNode(nsInputJSON.nodename);
			string ops = nsdb.getOperationFromMathNode(nsInputJSON.nodename);
			CRInputsJSON vMathInputsJSON = JsonUtility.FromJson<CRInputsJSON>(vMathInputs);

			float a = resolveFloatInput(vMathInputsJSON.inputs[0]);
			float b = resolveFloatInput(vMathInputsJSON.inputs[1]);

			float c = float.NegativeInfinity;
			if (ops == "ADD") { c = a + b; }
			else if (ops == "SUB") { c = a - b; }
			else if (ops == "MUL") { c = a * b; }
			else if (ops == "DIV") { if (b != 0) { c = a / b; } }
			return c;
		}
		else
			return float.NegativeInfinity;
	}

	//uses the Trajectories in the Planner
	Trajectory getTrajectoryByName(string n)
	{
		if (planner.trajectories.ContainsKey(n))
			return planner.trajectories[n];
		else
			return null;
	}
	public void resetMotion()
	{
		currentNode = nsdb.getFirstNodeByMotionName(currentMotionName);
		currentMotionIsOver = false;
		currTraj = getTrajectoryByName(nsdb.getTrajectoryByNode(currentNode));
		currTraj.resetTrajectory(motionSpeed);
	}
	void processEdges()
	{

		if (currTraj == null)
			return;

		if (!currTraj.isComplete)
		{
			return;
		}
		else
		{
			if (motionInterupt != MOTIONINTERUPT.NOINTERUPT)
			{
				currentMotionIsOver = true;
				motionInterupt = MOTIONINTERUPT.NOINTERUPT;
			}
		}
		// 0th entry is automatic ie. no testing, 1st is equal userInput == 0, 2nd is userInput < 0, 3rd is userInput > 0
		if (currentMotionIsOver)
		{
			return;
		}
		else
		{
			if (!(nsdb.getEdgeAByNode(currentNode) == "noedge"))
			{

				currentNode = nsdb.getNextNodeByEdge(nsdb.getEdgeAByNode(currentNode));
				currTraj = resolveTrajByNodeName(currentNode);
				currTraj.resetTrajectory();

				return;
			}
			if (!(nsdb.getEdgeBByNode(currentNode) == "noedge") && stateInputFloat == 0)
			{

				currentNode = nsdb.getNextNodeByEdge(nsdb.getEdgeBByNode(currentNode));
				currTraj = resolveTrajByNodeName(currentNode);
				currTraj.resetTrajectory();

				return;
			}
			if (!(nsdb.getEdgeCByNode(currentNode) == "noedge") && stateInputFloat < 0)
			{

				currentNode = nsdb.getNextNodeByEdge(nsdb.getEdgeCByNode(currentNode));
				currTraj = resolveTrajByNodeName(currentNode);
				currTraj.resetTrajectory();

				return;
			}
			if (!(nsdb.getEdgeDByNode(currentNode) == "noedge") && stateInputFloat > 0)
			{

				currentNode = nsdb.getNextNodeByEdge(nsdb.getEdgeDByNode(currentNode));
				currTraj = resolveTrajByNodeName(currentNode);
				currTraj.resetTrajectory();

				return;
			}
		}
	}


}
