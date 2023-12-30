using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Machine2ZXEShell : MonoBehaviour
{

    public Machine2ZXE machine2ZXE;
    public Machine2ZXEPlanner planner;
    public CRMotionDBController db;
    public Articulated2ZXE articulated2ZXE;

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
        functions.Add("importsessionOLDvalues", importsessionOLDvalues);
        functions.Add("exportsessionOLDvalues", exportsessionOLDvalues);
        functions.Add("importsessionvalues", importsessionvalues);
        functions.Add("exportsessionvalues", exportsessionvalues);
        functions.Add("setcurrentcrouch", setcurrentcrouch);
        functions.Add("getvaluetype", getvaluetype);
    }

    public void parameterlisttest(string a, string b, string c, string d, string e)
    {
        Debug.Log("param abc: " + a + ":" + b + ":" + c);
    }
    public void getvaluetype(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        Debug.Log(planner.getValueType(a));
    }
    public void importsessionvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        if (a == "")
            return;
        planner.importsessionvalues(a);

    }
    public void exportsessionvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        if (a == "")
            return;
        planner.exportsessionvalues(a);
    }
    public void importsessionOLDvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        if (a == "")
            return;
        planner.importsessionOLDvalues(a);

    }
    public void exportsessionOLDvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        if (a == "")
            return;
        planner.exportsessionOLDvalues(a);
    }
    public void setsessionint(string name, int value)
    {
        planner.setSessionInt(name, value);
    }
    public void setsessionvector(string name, Vector3 value)
    {
        planner.setSessionVector3(name, value);
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
    public void setcurrentcrouch(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        planner.setcurrentcrouch();
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


            //is this a motion call
            string firstNodeNamelf = db.getFirstNodeByMotionName(iTokens[0] + ".lf");
            string firstNodeNamelr = db.getFirstNodeByMotionName(iTokens[0] + ".lr");
            string firstNodeNamerf = db.getFirstNodeByMotionName(iTokens[0] + ".rf");
            string firstNodeNamerr = db.getFirstNodeByMotionName(iTokens[0] + ".rr");
            string firstNodeNamesegs = db.getFirstNodeByMotionName(iTokens[0] + ".segs");


            Debug.Log("firstNodeName: " + firstNodeNamelf + firstNodeNamelr + firstNodeNamerf + firstNodeNamerr + firstNodeNamesegs);

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
            else if (machine2ZXE.cmdExists(iTokens[0]))
            {
                machine2ZXE.runCmd(iTokens[0]);
            }
            else if ((firstNodeNamelf + firstNodeNamelr + firstNodeNamerf + firstNodeNamerr + firstNodeNamesegs) != "")// this is a motion call
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
                bool paramsLoaded = loadMotionInputs(ref iTokens);
                if (!paramsLoaded)
                    Debug.Log("param loaded: failed");

                if (paramsLoaded)
                {
                    if (machine2ZXE.motionMode == Machine2ZXE.MOTIONMODE.MOTION)
                    {
                        machine2ZXE.runMotionOnAllChannels(iTokens[0]);
                    }
                    else if (machine2ZXE.motionMode == Machine2ZXE.MOTIONMODE.TRAJECTORY)
                    {
                        machine2ZXE.runTrajectoryOnAllChannels(iTokens[0]);
                    }
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

        string retStringlf = db.getInputStringByMotion(tokens[0] + ".lf");
        string retStringlr = db.getInputStringByMotion(tokens[0] + ".lr");
        string retStringrf = db.getInputStringByMotion(tokens[0] + ".rf");
        string retStringrr = db.getInputStringByMotion(tokens[0] + ".rr");
        string retStringsegs = db.getInputStringByMotion(tokens[0] + ".segs");

        //if the motion node doesn't exist, set the string to an empty container
        //so my testing works
        if (retStringlf == "") retStringlf = JsonUtility.ToJson(new CRInputsJSON());
        if (retStringlr == "") retStringlr = JsonUtility.ToJson(new CRInputsJSON());
        if (retStringrf == "") retStringrf = JsonUtility.ToJson(new CRInputsJSON());
        if (retStringrr == "") retStringrr = JsonUtility.ToJson(new CRInputsJSON());
        if (retStringsegs == "") retStringsegs = JsonUtility.ToJson(new CRInputsJSON());


        CRInputsJSON lfJSON = JsonUtility.FromJson<CRInputsJSON>(retStringlf);
        CRInputsJSON lrJSON = JsonUtility.FromJson<CRInputsJSON>(retStringlr);
        CRInputsJSON rfJSON = JsonUtility.FromJson<CRInputsJSON>(retStringrf);
        CRInputsJSON rrJSON = JsonUtility.FromJson<CRInputsJSON>(retStringrr);
        CRInputsJSON segsJSON = JsonUtility.FromJson<CRInputsJSON>(retStringsegs);

        //type checking   
        if (lfJSON.inputs.Count > 0)
        {
            Debug.Log("lfJSON.input.Count > 0");
            if (lfJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("lfJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (planner.getValueType(tokens[i]) != lfJSON.inputs[i - 1].returntype)
                    {
                        typesMatch = false;
                        Debug.Log("lfJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + planner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + lfJSON.inputs[i - 1].returntype);
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
                    if (planner.getValueType(tokens[i]) != lrJSON.inputs[i - 1].returntype)
                    {
                        typesMatch = false;
                        Debug.Log("lrJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + planner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + lrJSON.inputs[i - 1].returntype);
                    }
                }
            }
        }
        if (rfJSON.inputs.Count > 0)
        {
            Debug.Log("rfJSON.input.Count > 0");
            if (rfJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("rfJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (planner.getValueType(tokens[i]) != rfJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("rfJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + planner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + rrJSON.inputs[i - 1].returntype);
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
                    if (planner.getValueType(tokens[i]) != rrJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("rrJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + planner.getValueType(tokens[i]));
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
                    if (planner.getValueType(tokens[i]) != segsJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("segsJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + planner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + segsJSON.inputs[i - 1].returntype);
                        typesMatch = false;
                    }
                }
            }
        }


        if (typesMatch)//set session variables for each channel
        {
            Debug.Log("setting session variables");
            for (int i = 0; i < lfJSON.inputs.Count; i++)
            {
                if (lfJSON.inputs[i].returntype == "int")
                {
                    planner.setSessionInt(lfJSON.inputs[i].nodename, planner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session lf int");
                }
                else //vector
                {
                    planner.setSessionVector3(lfJSON.inputs[i].nodename, planner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session lf vector");
                }
            }
            for (int i = 0; i < lrJSON.inputs.Count; i++)
            {
                if (lrJSON.inputs[i].returntype == "int")
                {
                    planner.setSessionInt(lrJSON.inputs[i].nodename, planner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session lr int");
                }
                else //vector
                {
                    planner.setSessionVector3(lrJSON.inputs[i].nodename, planner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session lr vector");

                }
            }
            for (int i = 0; i < rfJSON.inputs.Count; i++)
            {
                if (rfJSON.inputs[i].returntype == "int")
                {
                    planner.setSessionInt(rfJSON.inputs[i].nodename, planner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session rf int");
                }
                else //vector
                {
                    planner.setSessionVector3(rfJSON.inputs[i].nodename, planner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session rf vector");
                }
            }
            for (int i = 0; i < rrJSON.inputs.Count; i++)
            {
                if (rrJSON.inputs[i].returntype == "int")
                {
                    planner.setSessionInt(rrJSON.inputs[i].nodename, planner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session rr int");
                }
                else //vector
                {
                    planner.setSessionVector3(rrJSON.inputs[i].nodename, planner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session rr vector");
                }
            }
            for (int i = 0; i < segsJSON.inputs.Count; i++)
            {
                if (segsJSON.inputs[i].returntype == "int")
                {
                    planner.setSessionInt(segsJSON.inputs[i].nodename, planner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session segs int");
                }
                else //vector
                {
                    planner.setSessionVector3(segsJSON.inputs[i].nodename, planner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session segs vector");
                }
            }

        }


        return typesMatch;
    }





}
