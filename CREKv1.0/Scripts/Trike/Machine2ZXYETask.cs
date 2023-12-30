/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Machine2ZXYETask : MonoBehaviour
{

	public CRTaskDBController nsTaskDB;
	public CRMotionDBController nsMotionDB;
	public Machine2ZXYE machine2ZXYE;
	public TaskMessageSubscriber taskMessageSubscriber;
	public M2MqttCRClient m2MqttNSCClient;

	public Machine2ZXYEPlanner planner;

	public MotionController motionController; //controller input


	public string taskName;
	public bool taskIsOver;
	public bool tickIsOver; // not implemented, if I'm in a looping Task, I want to set this at the Alias Node to indicate
							// the Task completed 1 tick unit of work


	//input nodes are traversed synchronously.
	//these apply to flow nodes ( ie. MotionTaskItem, BoolFlow and Alias Flow) as opposed to input nodes
	public string currentNodeName;
	public string currentNodeType;
	public bool currentFlowNodeComplete;
	public bool currentFlowNodeRunning; // might move this to enum, this is for nodes I'm waiting to here back from
	


	public int mockMotionTimer;//for testing
	

	public Vector3 currentFrame;
	public bool isActive;

	public enum TASKINTERUPT { NOINTERUPT, SOFTCANCEL, HARDCANCEL };
	public TASKINTERUPT taskInterupt;




	void Start()
	{
		taskMessageSubscriber = GetComponent<TaskMessageSubscriber>();
		taskIsOver = true;
	}
	// Update is called once per frame
	void Update()
	{

		tick();
	}
	public void tick()
	{
		if (!isActive)
			return;

		if (!taskIsOver)
			visitCurrentNode();

		processEdges();


		//I"m not just a Task, I'm a Member!
		processTaskMessages();

	}

	public void setInterupt(TASKINTERUPT newInterupt)
	{
		taskInterupt = newInterupt;
	}

	void processTaskMessages()
	{
		string messageString = "";
		while (taskMessageSubscriber.hasMessages())
		{

			messageString = taskMessageSubscriber.getNextMessage();
			TopicMessageJSON incoming = JsonUtility.FromJson<TopicMessageJSON>(messageString);
			// actual processing of these messages
			// when I visit the MotionTI Node, I"ll post a message to my outgoing topic 
			// with the token = [return topic];taskname;nodename;COMPLETED 
			// post this token to the [outgoing topic], so I don't process my own message
			// when I get the message back, I look up the task, and if the taskName and currentNode name matchs,
			// set currentFlowNodeComplete = true;
			string [] pieces = incoming.messagestring.Split(';');

			pieces[0] = pieces[0].Trim();
			pieces[1] = pieces[1].Trim();
			pieces[2] = pieces[2].Trim();
			pieces[3] = pieces[3].Trim();
			if(pieces[1] != taskName)
            {
				return;
            }

			if (pieces[2] == currentNodeName)
			{
				currentFlowNodeComplete = true;
				currentFlowNodeRunning = false;
			}
			else
            {
				return;
            }
			

		}

	}

	public void startTask(string taskReq)
	{
		//if I'm already running a task, don't do anything
		if (!taskIsOver)
			return;

		//do nothing if I don't have a motion by that name, don't set currentNode to nothing
		string retNode = nsTaskDB.getFirstNodeByTaskName(taskReq);

		Debug.Log("in task.startTask(): " + taskReq + " + returns: " + retNode);

		if (retNode == "")
		{
			currentNodeName = "";
			return;
		}
		taskName = taskReq;
		currentNodeName = retNode;
		currentNodeType = nsTaskDB.getTypeByNode(currentNodeName);
		taskIsOver = false;

		//this is like processEdges, it's as if we're processing incoming edge to this task
		currentFlowNodeComplete = false;
		if(currentNodeType == "NSMotionTI")
        {
			if (!currentFlowNodeRunning)
			{
				// start motion asynchronously
				string motionName = nsTaskDB.getMotionByNode(currentNodeName);
				if (motionName != "")
				{

					//resolve MotionTI inputs, and cross reference to motionnode inputString
					//typechecking+set session variables in planner
					//returns true if no parameters are required.
					if (loadMotionInputs(currentNodeName, motionName))
					{
						//JsonUtility.FromJson<TopicMessageJSON>(outerMessage);
						TopicMessageJSON outgoingMessage = new TopicMessageJSON();
						outgoingMessage.topic = "MOTIONSTARTED";
						outgoingMessage.messagestring = "MOTIONCOMPLETED" + ";" + taskName + ";" + currentNodeName + ";" + "COMPLETED";
						taskMessageSubscriber.postMessage(JsonUtility.ToJson(outgoingMessage));
						machine2ZXYE.runMotionOnAllChannels(motionName);
						currentFlowNodeRunning = true;
					}
				}
			}
			else
			{

			}


		}
		else if (currentNodeType == "TopicMessageTI")
		{

			Debug.Log("visitin TopicMessageTI");
			string topic = nsTaskDB.getTopicFromTopicMessageTINode(currentNodeName);
			string message = nsTaskDB.getMessageFromTopicMessageTINode(currentNodeName);
			if (message == "")
			{
				string inputs = nsTaskDB.getInputStringFromTopicMessageTINode(currentNodeName);
				CRInputsJSON inputsJSON = JsonUtility.FromJson<CRInputsJSON>(inputs);
				Vector3 messageVector3 = resolveVectorInput(inputsJSON.inputs[0]);
				Float3JSON float3JSON = new Float3JSON();
				float3JSON.x = messageVector3.x; float3JSON.y = messageVector3.y; float3JSON.z = messageVector3.z;
				if (float3JSON.x == float.NegativeInfinity) float3JSON.x = 0f;
				if (float3JSON.y == float.NegativeInfinity) float3JSON.y = 0f;
				if (float3JSON.z == float.NegativeInfinity) float3JSON.z = 0f;
				message = JsonUtility.ToJson(float3JSON);

			}
			if (topic != "")
			{
				m2MqttNSCClient.publish(topic, message);
			}
			//this node doesn't wait for anything, force edge process
			currentFlowNodeComplete = true;


		}
		else if (currentNodeType == "BoolFlow")
		{
			//this node doesn't wait for anything, force edge process
			currentFlowNodeComplete = true;
		}
		else if (currentNodeType == "AliasFlow")
		{
			//this node doesn't wait for anything, force edge process
			currentFlowNodeComplete = true;
		}

	}
	
	public Vector3 getCurrentFrame()
	{
		return currentFrame;
	}
	void visitCurrentNode()
	{
		visitNode(currentNodeName);
	}
	void visitNode(string n)
	{
		if (n == "nonode" | n == "")
		{
			Debug.Log("no node name n");
			return;
		}


		Debug.Log("visiting n:" + n);

		if (!currentFlowNodeComplete)
		{
			
			if (currentNodeType == "NSMotionTI")
			{

				if (!currentFlowNodeRunning)
				{
					// start motion asynchronously
					string motionName = nsTaskDB.getMotionByNode(currentNodeName);
					if (motionName != "")
					{

						//resolve MotionTI inputs, and cross reference to motionnode inputString
						//typechecking+set session variables in planner
						//returns true if no parameters are required.
						if (loadMotionInputs(currentNodeName, motionName))
						{ 
							//JsonUtility.FromJson<TopicMessageJSON>(outerMessage);
							TopicMessageJSON outgoingMessage = new TopicMessageJSON();
							outgoingMessage.topic = "MOTIONSTARTED";
							outgoingMessage.messagestring = "MOTIONCOMPLETED" + ";" + taskName + ";" + currentNodeName + ";" + "COMPLETED";
							taskMessageSubscriber.postMessage(JsonUtility.ToJson(outgoingMessage));
							machine2ZXYE.runMotionOnAllChannels(motionName);
							currentFlowNodeRunning = true;
						}

					}
				}
				else
				{

				}

			}
			else if (currentNodeType == "TopicMessageTI")
			{

				Debug.Log("visitin TopicMessageTI");
				string topic = nsTaskDB.getTopicFromTopicMessageTINode(currentNodeName);
				string message = nsTaskDB.getMessageFromTopicMessageTINode(currentNodeName);
				if(message == "")
                {
					string inputs = nsTaskDB.getInputStringFromTopicMessageTINode(currentNodeName);
					CRInputsJSON inputsJSON = JsonUtility.FromJson< CRInputsJSON>(inputs);
					Vector3 messageVector3 = resolveVectorInput(inputsJSON.inputs[0]);
					Float3JSON float3JSON = new Float3JSON();
					float3JSON.x = messageVector3.x; float3JSON.y = messageVector3.y; float3JSON.z = messageVector3.z;
					if (float3JSON.x == float.NegativeInfinity) float3JSON.x = 0f;
					if (float3JSON.y == float.NegativeInfinity) float3JSON.y = 0f;
					if (float3JSON.z == float.NegativeInfinity) float3JSON.z = 0f;
					message = JsonUtility.ToJson(float3JSON);

				}
				if (topic != "")
				{
					m2MqttNSCClient.publish(topic, message);
				}
				//this node doesn't wait for anything, force edge process
				currentFlowNodeComplete = true;


			}
			else if (currentNodeType == "BoolFlow")
			{
				//this node doesn't wait for anything, force edge process
				currentFlowNodeComplete = true;
			}
			else if (currentNodeType == "AliasFlow")
			{
				//this node doesn't wait for anything, force edge process
				currentFlowNodeComplete = true;
			}
		}
		else
		{
			if (currentNodeType == "NSMotionTI")
			{
				taskIsOver = nsTaskDB.isMotionTILeafNode(n);
			}
			else if (currentNodeType == "BoolFlow")
			{
				taskIsOver = nsTaskDB.isBoolFlowLeafNode(n);
			}	
			else if (currentNodeType == "AliasFlow")
			{
				//do nothing, AliasFlow is a loop point
			}
		}
		
	}

	bool loadMotionInputs(string motionTINodeName, string motionName)
	{

		bool typesMatch = true;

		string motionTIInputString = nsTaskDB.getInputStringFromMotionTINode(motionTINodeName);
		CRInputsJSON motionTIInputs = JsonUtility.FromJson<CRInputsJSON>(motionTIInputString);
		Debug.Log("motionTI inputString: " + motionTIInputString);

		List<string> motionInputStrings = new List<string>();
		List<CRInputsJSON> motionChannelInputs = new List<CRInputsJSON>();

		for (int i = 0; i < machine2ZXYE.motionChannels.Count; i++)
		{
			motionInputStrings.Add(nsMotionDB.getInputStringByMotion(motionName + "." + machine2ZXYE.motionChannels[i]));
			if (motionInputStrings[i] == "") motionInputStrings[i] = JsonUtility.ToJson(new CRInputsJSON());
			motionChannelInputs.Add(JsonUtility.FromJson<CRInputsJSON>(motionInputStrings[i]));
			Debug.Log(motionName + "." + machine2ZXYE.motionChannels[i] + ": "+ motionInputStrings[i]);

			//type checking   
			if (motionChannelInputs[i].inputs.Count > 0)
			{
				string channelName = machine2ZXYE.motionChannels[i];
				Debug.Log(channelName + ".input.Count > 0");
				if (motionChannelInputs[i].inputs.Count != (motionTIInputs.inputs.Count))
				{
					typesMatch = false;
					Debug.Log(channelName + " input count didn't match");
				}
				else
				{
					for (int j = 0; j < motionChannelInputs[i].inputs.Count; j++)
					{
						if (motionChannelInputs[i].inputs[j].returntype != motionTIInputs.inputs[j].returntype)
						{
							typesMatch = false;
							Debug.Log(channelName + " type mismatch index: " + j.ToString());
							Debug.Log("from motion: " + motionChannelInputs[i].inputs[j].returntype);
							Debug.Log("from motionTIInputs: " + motionTIInputs.inputs[j].returntype);
						}
					}

				}
			}
			

		}//end of channel, test types loop

		if (typesMatch)//set session variables for each channel
		{
			Debug.Log("setting session variables");

			for (int i = 0; i < motionTIInputs.inputs.Count; i++)
			{
				int intValue = 0;
				Vector3 vectorValue = Vector3.zero;

				if (motionTIInputs.inputs[i].returntype == "int")
				{
					intValue = resolveIntInput(motionTIInputs.inputs[i]);
					Debug.Log("resolving session int" + motionTIInputs.inputs[i].nodename + intValue.ToString());
					for (int j = 0; j < machine2ZXYE.motionChannels.Count; j++)
					{
						if (motionChannelInputs[j].inputs.Count > 0)//not all channels have a motion defined
						{
							planner.setSessionInt(motionChannelInputs[j].inputs[i].nodename, intValue);
							Debug.Log("setting session " + motionChannelInputs[j].inputs[i].nodename + " int");
						}
					}
				}
				else //vector
				{
					vectorValue = resolveVectorInput(motionTIInputs.inputs[i]);
					Debug.Log("resolving session vector" + motionTIInputs.inputs[i].nodename + vectorValue.ToString());
					for (int j = 0; j < machine2ZXYE.motionChannels.Count; j++)
					{
						if (motionChannelInputs[j].inputs.Count > 0)//not all channels have a motion defined
						{
							planner.setSessionVector3(motionChannelInputs[j].inputs[i].nodename, vectorValue);
							Debug.Log("setting session " + motionChannelInputs[j].inputs[i].nodename + " vector");
						}
					}
				}
				
			}
		} //end of setting session variables loop


		return typesMatch;

	}

	Trajectory resolveTrajByNodeName(string nodeName) {return Instantiate<Trajectory>(planner.template); }
	//these functions follow each input node in a depth first search
	Vector3 resolveVectorInput(CRInputJSON nsInputJSON)
	{
		if (nsInputJSON.nodename == "")
			return Vector3.negativeInfinity;

		//IntFunction, VectorFunction, Float, Vector, Math, VectorMath
		string nodeType = nsTaskDB.getTypeByNode(nsInputJSON.nodename);

		if (nodeType == "VectorFunction")
		{
			string funcName = nsTaskDB.getFunctionByFunctionNode(nsInputJSON.nodename);
			return planner.getVectorValue(funcName);
		}
		else if (nodeType == "Vector")
		{
			return nsTaskDB.getVectorFromVectorNode(nsInputJSON.nodename);
		}
		else if (nodeType == "VectorMath")
		{
			string vMathInputs = nsTaskDB.getInputStringFromVectorMathNode(nsInputJSON.nodename);
			string ops = nsTaskDB.getOperationFromVectorMathNode(nsInputJSON.nodename);
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
		else if (nodeType == "FloatsToFloat3")
        {
			string fInputs = nsTaskDB.getInputStringFromFloatsToFloat3Node(nsInputJSON.nodename);
			CRInputsJSON fInputsJSON = JsonUtility.FromJson<CRInputsJSON>(fInputs);
			float a = resolveFloatInput(fInputsJSON.inputs[0]);
			float b = resolveFloatInput(fInputsJSON.inputs[1]);
			float c = resolveFloatInput(fInputsJSON.inputs[2]);

			return new Vector3(a, b, c);

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
		string nodeType = nsTaskDB.getTypeByNode(nsInputJSON.nodename);

		if (nodeType == "Int")
		{
			return nsTaskDB.getValueFromIntNode(nsInputJSON.nodename);
		}
		else if (nodeType == "IntFunction")
		{
			string funcName = nsTaskDB.getFunctionByFunctionNode(nsInputJSON.nodename);
			return planner.getIntValue(funcName);
		}
		if (planner.doesIntExist(nsInputJSON.nodename))
		{
			return planner.getIntValue(nsInputJSON.nodename);
		}
		else if (nodeType == "Math")
		{
			string vMathInputs = nsTaskDB.getInputStringFromMathNode(nsInputJSON.nodename);
			string ops = nsTaskDB.getOperationFromMathNode(nsInputJSON.nodename);
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

		//check to see if nodename is dotted (ie nodename.outputname)
		string[] pieces = nsInputJSON.nodename.Split('.');
		string outputName = "";


		if(pieces.Length > 1)
        {
			pieces[0] = pieces[0].Trim();
			pieces[1] = pieces[1].Trim();
			nsInputJSON.nodename = pieces[0];
			outputName = pieces[1];
		}

		//IntFunction, VectorFunction, Float, Vector, Math, VectorMath
		string nodeType = nsTaskDB.getTypeByNode(nsInputJSON.nodename);

		if (nodeType == "Float")
		{
			return nsTaskDB.getValueFromFloatNode(nsInputJSON.nodename);
		}
		else if (nodeType == "FloatFunction")
		{
			string funcName = nsTaskDB.getFunctionByFunctionNode(nsInputJSON.nodename);
			return planner.getFloatValue(funcName);
		}
		else if (nodeType == "Math")
		{
			string vMathInputs = nsTaskDB.getInputStringFromMathNode(nsInputJSON.nodename);
			string ops = nsTaskDB.getOperationFromMathNode(nsInputJSON.nodename);
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
		else if (nodeType == "ControllerInput")
		{
			float controllerInput = float.NegativeInfinity;
			if(outputName == "leftX")
            {
				controllerInput = motionController.lMotionInput.x;
			}
			else if(outputName == "leftY")
            {
				controllerInput = motionController.lMotionInput.y;
			}
			else if (outputName == "rightX")
			{
				controllerInput = motionController.rMotionInput.x;
			}
			else if (outputName == "rightY")
			{
				controllerInput = motionController.rMotionInput.y;
			}
			return controllerInput;
		}
		else
			return float.NegativeInfinity;
	}
	bool resolveBoolInput(CRInputJSON nsInputJSON)
    {
		bool result = false;

		if (nsInputJSON.nodename == "")
			return false;

		//Float Bool
		string nodeType = nsTaskDB.getTypeByNode(nsInputJSON.nodename);
		

		if (nodeType == "FloatBool")
		{
			string floatBoolInputs = nsTaskDB.getInputStringFromFloatBoolNode(nsInputJSON.nodename);

			string ops = nsTaskDB.getOperationFromFloatBoolNode(nsInputJSON.nodename);
			CRInputsJSON floatBoolInputsJSON = JsonUtility.FromJson<CRInputsJSON>(floatBoolInputs);

			float a = resolveFloatInput(floatBoolInputsJSON.inputs[0]);
			float b = resolveFloatInput(floatBoolInputsJSON.inputs[1]);

			if (ops == "EQL") { result = (a==b); }
			else if (ops == "NOTEQL") { result = (a != b); }
			else if (ops == "LT") { result = (a < b); }
			else if (ops == "GT") { result = (a > b); }
			else if (ops == "LTEQL") { result = (a <= b); }
			else if (ops == "GTEQL") { result = (a >= b); }
			return result;
		}


		//default
		return false;

		
    }
	//uses the Trajectories in the Planner

	public void resetTask()
	{
		currentNodeName = nsTaskDB.getFirstNodeByTaskName(taskName);
		currentNodeType = nsTaskDB.getTypeByNode(currentNodeName);
		taskIsOver = false;
		currentFlowNodeComplete = false;
	}
	void processEdges()
	{

		if (currentNodeName == "")
		{
			return;
		}

		if (!currentFlowNodeComplete)
		{
			return;
		}

		if (taskInterupt != TASKINTERUPT.NOINTERUPT)
		{
			taskIsOver = true;
			taskInterupt = TASKINTERUPT.NOINTERUPT;
		}
		
		if (taskIsOver)
		{
			return;
		}
		else
		{

			if (currentNodeType == "NSMotionTI")
			{

				currentNodeName = nsTaskDB.getNextNodeByEdge(nsTaskDB.getExitByMotionTINode(currentNodeName));
				currentNodeType = nsTaskDB.getTypeByNode(currentNodeName);
				currentFlowNodeComplete = false;
				return;
			}
			else if (currentNodeType == "TopicMessageTI")
			{

				currentNodeName = nsTaskDB.getNextNodeByEdge(nsTaskDB.getExitByTopicMessageTINode(currentNodeName));
				currentNodeType = nsTaskDB.getTypeByNode(currentNodeName);
				currentFlowNodeComplete = false;
				return;
			}
			else if (currentNodeType == "BoolFlow")
			{
				string ret = nsTaskDB.getInputStringFromBoolFlowNode(currentNodeName);
				CRInputsJSON inputs = JsonUtility.FromJson<CRInputsJSON>(ret);
				bool exitValue = resolveBoolInput(inputs.inputs[0]); //this is a list, send a single

				if(exitValue)
                {			
					currentNodeName = nsTaskDB.getNextNodeByEdge(nsTaskDB.getExitTrueFromBoolFlowNode(currentNodeName));
					currentNodeType = nsTaskDB.getTypeByNode(currentNodeName);
				}
				else
                {
					currentNodeName = nsTaskDB.getNextNodeByEdge(nsTaskDB.getExitFalseFromBoolFlowNode(currentNodeName));
					currentNodeType = nsTaskDB.getTypeByNode(currentNodeName);
				}
				currentFlowNodeComplete = false;
				return;
			}
			else if (currentNodeType == "AliasFlow")
			{
				currentNodeName = nsTaskDB.getNextNodeByAliasNodeName(currentNodeName);
				currentNodeType = nsTaskDB.getTypeByNode(currentNodeName);
				currentFlowNodeComplete = false;
				return;
			}

		}
	}


}
