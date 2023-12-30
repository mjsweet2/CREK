using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class BalTrikeShell : MonoBehaviour
{

    public MachineBalTrike machineBalTrike;
    public BalTrikePlanner balTrikePlanner;
    public CRMotionDBController db;
    public ArticulatedBalTrike articulatedBalTrike;

    public UDPSocketServer udpSocketServer;


    public delegate Vector3 ShellVectorCmd();
    public Dictionary<string, ShellVectorCmd> vectorFunctions;

    public delegate int ShellIntCmd();
    public Dictionary<string, ShellIntCmd> intFunctions;

    public delegate void ShellCmd(string a = "", string b = "", string c = "", string d = "", string e = "");
    public Dictionary<string, ShellCmd> functions;


    // Start is called before the first frame update
    void Start()
    {
        buildTables();

    }

    // Update is called once per frame
    void Update()
    {
        while (udpSocketServer.hasMessages())
        {
            string m = udpSocketServer.getNextMessage();
            runCmd(m);
        }

    }

    public void connectNetwork()
    {
        udpSocketServer.startNetwork();

    }

    public void setupsessionvaluesfromfile()
    {
        string path = "c:\\sites\\notstop\\setupsessionvalues.txt";
        StreamReader reader = new StreamReader(path);
        string fileLine = "";
        fileLine = reader.ReadLine();
        while (!reader.EndOfStream)
        {
            if (fileLine != "")
            {
                Debug.Log(fileLine);
                runCmd(fileLine);

            }
            fileLine = reader.ReadLine();
        }

    }
    void buildTables()
    {

        vectorFunctions = new Dictionary<string, ShellVectorCmd>();
        intFunctions = new Dictionary<string, ShellIntCmd>();
        functions = new Dictionary<string, ShellCmd>();

        functions.Add("parameterlisttest", parameterlisttest);
        functions.Add("setsessionint", setsessionint);
        functions.Add("setsessionvector", setsessionvector);
        functions.Add("importsessionvalues", importsessionvalues);
        functions.Add("exportsessionvalues", exportsessionvalues);
        functions.Add("getvaluetype", getvaluetype);
    }

    public void parameterlisttest(string a, string b, string c, string d, string e)
    {
        Debug.Log("param abc: " + a + ":" + b + ":" + c);
    }
    public void getvaluetype(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        Debug.Log(balTrikePlanner.getValueType(a));
    }
    public void importsessionvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        if (a == "")
            return;
        balTrikePlanner.importsessionvalues(a);

    }
    public void exportsessionvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        if (a == "")
            return;
        balTrikePlanner.exportsessionvalues(a);
    }

    public void setsessionint(string name, int value)
    {
        balTrikePlanner.setSessionInt(name, value);
    }
    public void setsessionvector(string name, Vector3 value)
    {
        balTrikePlanner.setSessionVector3(name, value);
    }
    public void setsessionint(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        int intB;
        if (int.TryParse(b, out intB))
        {
            setsessionint(a, intB);
            Debug.Log("sessionint set: " + a);
        }
    }

    public void setsessionvector(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        float floatX;
        float floatY;
        float floatZ;

        if (float.TryParse(b, out floatX) && float.TryParse(c, out floatY) && float.TryParse(d, out floatZ))
        {
            setsessionvector(a, new Vector3(floatX, floatY, floatZ));
            Debug.Log("sessionvector set: " + (new Vector3(floatX, floatY, floatZ)).ToString());
        }

    }
   
    public void runCmd(string inputCmd)
    {

        //TODO
        //parse tokens
        //check to see if cmd is a shell cmd...
        //machine cmd
        //db or 
        //planner cmd
        //articulated cmd

        string[] iTokens = inputCmd.Trim().Split(' ');
        for (int i = 0; i < iTokens.Length; i++)
        {
            iTokens[i] = iTokens[i].Trim();
            Debug.Log("input token " + i.ToString() + ": <" + iTokens[i] + ">");
        }


        if (iTokens.Length > 0)
        {


            //is this a motion call?
            string firstNodeNamesp = db.getFirstNodeByMotionName(iTokens[0] + ".sp");
            string firstNodeNamelb = db.getFirstNodeByMotionName(iTokens[0] + ".lb");
            string firstNodeNamerb = db.getFirstNodeByMotionName(iTokens[0] + ".rb");
            string firstNodeNamelr = db.getFirstNodeByMotionName(iTokens[0] + ".lr");
            string firstNodeNamerr = db.getFirstNodeByMotionName(iTokens[0] + ".rr");
            string firstNodeNamesegs = db.getFirstNodeByMotionName(iTokens[0] + ".segs");

            //is this a trajectory call?
            // trajsp || trajlb || trajrb || trajlr || trajrr || trajsegs
            bool trajsp = balTrikePlanner.doesTrajectoryExist(iTokens[0] + ".sp");
            bool trajlb = balTrikePlanner.doesTrajectoryExist(iTokens[0] + ".lb");
            bool trajrb = balTrikePlanner.doesTrajectoryExist(iTokens[0] + ".rb");
            bool trajlr = balTrikePlanner.doesTrajectoryExist(iTokens[0] + ".lr");
            bool trajrr = balTrikePlanner.doesTrajectoryExist(iTokens[0] + ".rr");
            bool trajsegs = balTrikePlanner.doesTrajectoryExist(iTokens[0] + ".segs");


            Debug.Log("firstNodeName: " + firstNodeNamesp + firstNodeNamelb + firstNodeNamerb + firstNodeNamelr + firstNodeNamerr + firstNodeNamesegs);

            //is this a shell function
            if (vectorFunctions.ContainsKey(iTokens[0]))
            {
                vectorFunctions[iTokens[0]]();
            }
            else if (intFunctions.ContainsKey(iTokens[0]))
            {
                intFunctions[iTokens[0]]();
            }
            else if (functions.ContainsKey(iTokens[0]))
            {
                Debug.Log("shell cmd: " + iTokens[0]);
                if (iTokens.Length == 1)
                {
                    functions[iTokens[0]]();
                }
                else if (iTokens.Length == 2)
                {
                    functions[iTokens[0]](iTokens[1]);
                }
                else if (iTokens.Length == 3)
                {
                    functions[iTokens[0]](iTokens[1], iTokens[2]);
                }
                else if (iTokens.Length == 4)
                {
                    functions[iTokens[0]](iTokens[1], iTokens[2], iTokens[3]);
                }
                else if (iTokens.Length == 5)
                {
                    functions[iTokens[0]](iTokens[1], iTokens[2], iTokens[3], iTokens[4]);
                }
                else if (iTokens.Length == 6)
                {
                    functions[iTokens[0]](iTokens[1], iTokens[2], iTokens[3], iTokens[4], iTokens[5]);
                }


            }
            else if (machineBalTrike.cmdExists(iTokens[0]))
            {
                machineBalTrike.runCmd(iTokens[0]);
            }
            else if ((firstNodeNamesp + firstNodeNamelb + firstNodeNamerb + firstNodeNamelr + firstNodeNamerr + firstNodeNamesegs) != "")// this is a motion call
            {
                //TODO
                //resolve parameter requirements from the motion db, all channels
                //parameter requirements are the same for all channels
                //all of the passed in types should exist in planner as session variables
                //I will never go into the database to get these named parameter values


                //verify types of all passed in tokens are the same as the required parameters
                //set the session variables in the planner
                //forward the motion ( ie. machine.startMotion(inputTokens[0]) )


                //typechecking+set session variables in planner
                //returns true if no parameters are required.
                //returns true if no motion exists
                bool paramsLoaded = loadMotionInputs(ref iTokens);
                if (!paramsLoaded)
                    Debug.Log("param loaded: failed");

                if (paramsLoaded)
                {
                    if (machineBalTrike.motionMode == MachineBalTrike.MOTIONMODE.MOTION)
                    {
                        machineBalTrike.runMotionOnAllChannels(iTokens[0]);
                    }
                }

            }
            else if(trajsp || trajlb || trajrb || trajlr || trajrr || trajsegs) //this is a trajectory call
            {
                if (machineBalTrike.motionMode == MachineBalTrike.MOTIONMODE.TRAJECTORY)
                {
                    machineBalTrike.runTrajectoryOnAllChannels(iTokens[0]);
                }
            }
            else
            {
                //do nothing
            }

        }

    }
    bool loadMotionInputs(ref string[] tokens)
    {
        //called by mistake
        if (tokens.Length < 1)
            return false;

        bool typesMatch = true;

        string retStringsp = db.getInputStringByMotion(tokens[0] + ".sp");
        string retStringlb = db.getInputStringByMotion(tokens[0] + ".lb");
        string retStringrb = db.getInputStringByMotion(tokens[0] + ".rb");
        string retStringlr = db.getInputStringByMotion(tokens[0] + ".lr");
        string retStringrr = db.getInputStringByMotion(tokens[0] + ".rr");
        string retStringsegs = db.getInputStringByMotion(tokens[0] + ".segs");

        //if the motion node doesn't exist, set the string to an empty container
        //so my testing works
        if (retStringsp == "") retStringsp = JsonUtility.ToJson(new CRInputsJSON());

        if (retStringlb == "") retStringlb = JsonUtility.ToJson(new CRInputsJSON());
        if (retStringrb == "") retStringrb = JsonUtility.ToJson(new CRInputsJSON());

        if (retStringlr == "") retStringlr = JsonUtility.ToJson(new CRInputsJSON());
        if (retStringrr == "") retStringrr = JsonUtility.ToJson(new CRInputsJSON());

        if (retStringsegs == "") retStringsegs = JsonUtility.ToJson(new CRInputsJSON());

        CRInputsJSON spJSON = JsonUtility.FromJson<CRInputsJSON>(retStringsp);

        CRInputsJSON lbJSON = JsonUtility.FromJson<CRInputsJSON>(retStringlb);
        CRInputsJSON rbJSON = JsonUtility.FromJson<CRInputsJSON>(retStringrb);

        CRInputsJSON lrJSON = JsonUtility.FromJson<CRInputsJSON>(retStringlr);
        CRInputsJSON rrJSON = JsonUtility.FromJson<CRInputsJSON>(retStringrr);

        CRInputsJSON segsJSON = JsonUtility.FromJson<CRInputsJSON>(retStringsegs);

        //type checking   
        if (spJSON.inputs.Count > 0)
        {
            Debug.Log("spJSON.input.Count > 0");
            if (spJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("spJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (balTrikePlanner.getValueType(tokens[i]) != spJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("spJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + balTrikePlanner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + spJSON.inputs[i - 1].returntype);
                        typesMatch = false;
                    }
                }
            }
        }

        if (lbJSON.inputs.Count > 0)
        {
            Debug.Log("lfJSON.input.Count > 0");
            if (lbJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("lbJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (balTrikePlanner.getValueType(tokens[i]) != lbJSON.inputs[i - 1].returntype)
                    {
                        typesMatch = false;
                        Debug.Log("lbJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + balTrikePlanner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + lbJSON.inputs[i - 1].returntype);
                    }
                }

            }
        }

        if (lrJSON.inputs.Count > 0)
        {
            Debug.Log("lrJSON.input.Count > 0");
            if (lrJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("lrJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (balTrikePlanner.getValueType(tokens[i]) != lrJSON.inputs[i - 1].returntype)
                    {
                        typesMatch = false;
                        Debug.Log("lrJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + balTrikePlanner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + lrJSON.inputs[i - 1].returntype);
                    }
                }
            }
        }
        if (rbJSON.inputs.Count > 0)
        {
            Debug.Log("rbJSON.input.Count > 0");
            if (rbJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("rbJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (balTrikePlanner.getValueType(tokens[i]) != rbJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("rbJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + balTrikePlanner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + rbJSON.inputs[i - 1].returntype);
                        typesMatch = false;
                    }
                }
            }
        }
        if (rrJSON.inputs.Count > 0)
        {
            Debug.Log("rrJSON.input.Count > 0");
            if (rrJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("rrJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (balTrikePlanner.getValueType(tokens[i]) != rrJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("rrJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + balTrikePlanner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + rrJSON.inputs[i - 1].returntype);
                        typesMatch = false;
                    }
                }
            }
        }
        if (segsJSON.inputs.Count > 0)
        {
            Debug.Log("segsJSON.input.Count > 0");
            if (segsJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("segsJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (balTrikePlanner.getValueType(tokens[i]) != segsJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("segsJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + balTrikePlanner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + segsJSON.inputs[i - 1].returntype);
                        typesMatch = false;
                    }
                }
            }
        }

        if (typesMatch)//set session variables for each channel
        {
            Debug.Log("setting session variables");
            for (int i = 0; i < spJSON.inputs.Count; i++)
            {
                if (spJSON.inputs[i].returntype == "int")
                {
                    balTrikePlanner.setSessionInt(spJSON.inputs[i].nodename, balTrikePlanner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session sp int");
                }
                else //vector
                {
                    balTrikePlanner.setSessionVector3(spJSON.inputs[i].nodename, balTrikePlanner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session sp vector");
                }
            }

            for (int i = 0; i < lbJSON.inputs.Count; i++)
            {
                if (lbJSON.inputs[i].returntype == "int")
                {
                    balTrikePlanner.setSessionInt(lbJSON.inputs[i].nodename, balTrikePlanner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session lb int");
                }
                else //vector
                {
                    balTrikePlanner.setSessionVector3(lbJSON.inputs[i].nodename, balTrikePlanner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session lb vector");
                }
            }
            for (int i = 0; i < lrJSON.inputs.Count; i++)
            {
                if (lrJSON.inputs[i].returntype == "int")
                {
                    balTrikePlanner.setSessionInt(lrJSON.inputs[i].nodename, balTrikePlanner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session lr int");
                }
                else //vector
                {
                    balTrikePlanner.setSessionVector3(lrJSON.inputs[i].nodename, balTrikePlanner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session lr vector");

                }
            }
            for (int i = 0; i < rbJSON.inputs.Count; i++)
            {
                if (rbJSON.inputs[i].returntype == "int")
                {
                    balTrikePlanner.setSessionInt(rbJSON.inputs[i].nodename, balTrikePlanner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session rb int");
                }
                else //vector
                {
                    balTrikePlanner.setSessionVector3(rbJSON.inputs[i].nodename, balTrikePlanner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session rb vector");
                }
            }
            for (int i = 0; i < rrJSON.inputs.Count; i++)
            {
                if (rrJSON.inputs[i].returntype == "int")
                {
                    balTrikePlanner.setSessionInt(rrJSON.inputs[i].nodename, balTrikePlanner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session rr int");
                }
                else //vector
                {
                    balTrikePlanner.setSessionVector3(rrJSON.inputs[i].nodename, balTrikePlanner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session rr vector");
                }
            }
            for (int i = 0; i < segsJSON.inputs.Count; i++)
            {
                if (segsJSON.inputs[i].returntype == "int")
                {
                    balTrikePlanner.setSessionInt(segsJSON.inputs[i].nodename, balTrikePlanner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session segs int");
                }
                else //vector
                {
                    balTrikePlanner.setSessionVector3(segsJSON.inputs[i].nodename, balTrikePlanner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session segs vector");
                }
            }

        }


        return typesMatch;
    }





}
