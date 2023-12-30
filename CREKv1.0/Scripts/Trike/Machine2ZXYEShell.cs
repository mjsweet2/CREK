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

public class Machine2ZXYEShell : MonoBehaviour
{

    public Machine2ZXYE machine2ZXYE;
    public Machine2ZXYEPlanner planner;
    public CRMotionDBController crMotiondb;
    public CRTaskDBController crTaskdb;
    public Machine2ZXYETask machine2ZXYETask;
    public Articulated2ZXYE articulated2ZXYE;

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
        string path = "c:\\crek\\db\\setupsessionvalues.txt";
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
            string firstNodeNamelf = crMotiondb.getFirstNodeByMotionName(iTokens[0] + ".lf");
            string firstNodeNamelr = crMotiondb.getFirstNodeByMotionName(iTokens[0] + ".lr");

            string firstNodeNamesegs = crMotiondb.getFirstNodeByMotionName(iTokens[0] + ".segs");

            string firstNodeNamelfl = crMotiondb.getFirstNodeByMotionName(iTokens[0] + ".lfl");
            string firstNodeNamelfu = crMotiondb.getFirstNodeByMotionName(iTokens[0] + ".lfu");
            string firstNodeNamerfl = crMotiondb.getFirstNodeByMotionName(iTokens[0] + ".rfl");
            string firstNodeNamerfu = crMotiondb.getFirstNodeByMotionName(iTokens[0] + ".rfu");

            string combined = firstNodeNamelf + firstNodeNamelr + firstNodeNamesegs;
            combined = combined + firstNodeNamelfl + firstNodeNamelfu + firstNodeNamerfl + firstNodeNamerfu;


            //is this a task call
            string firstTaskNodeName = crTaskdb.getFirstNodeByTaskName(iTokens[0]);

            Debug.Log("Motion firstNodeName: " + combined);
            Debug.Log("Task firstNodeName: " + firstTaskNodeName);

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
            else if (machine2ZXYE.cmdExists(iTokens[0]))
            {
                machine2ZXYE.runCmd(iTokens[0]);
            }
            else if ((combined) != "")// this is a motion call
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
                    if (machine2ZXYE.motionMode == Machine2ZXYE.MOTIONMODE.MOTION)
                    {
                        machine2ZXYE.runMotionOnAllChannels(iTokens[0]);
                    }
                    else if (machine2ZXYE.motionMode == Machine2ZXYE.MOTIONMODE.TRAJECTORY)
                    {
                        machine2ZXYE.runTrajectoryOnAllChannels(iTokens[0]);
                    }
                }

            }
            else if (firstTaskNodeName != "")
            {
                if (machine2ZXYE.motionMode == Machine2ZXYE.MOTIONMODE.TASK)
                {
                    machine2ZXYETask.startTask(iTokens[0]);
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


        List<string> motionInputStr = new List<string>();
        List<CRInputsJSON> jsons = new List<CRInputsJSON>();
        for (int i = 0; i < machine2ZXYE.motionChannels.Count; i++)
        {
            motionInputStr.Add(crMotiondb.getInputStringByMotion(tokens[0] + "." + machine2ZXYE.motionChannels[i]));
            if (motionInputStr[i] == "") motionInputStr[i] = JsonUtility.ToJson(new CRInputsJSON());
            jsons.Add(JsonUtility.FromJson<CRInputsJSON>(motionInputStr[i]));


            //type checking   
            if (jsons[i].inputs.Count > 0)
            {
                string channelName = machine2ZXYE.motionChannels[i];
                Debug.Log(channelName + ".input.Count > 0");
                if (jsons[i].inputs.Count != (tokens.Length - 1)) //first token is motion name
                {
                    typesMatch = false;
                    Debug.Log(channelName + " input count didn't match");
                }
                else
                {
                    for (int j = 1; j < tokens.Length; j++)
                    {
                        if (planner.getValueType(tokens[j]) != jsons[i].inputs[j - 1].returntype)
                        {
                            typesMatch = false;
                            Debug.Log(channelName + " type mismatch index: " + j.ToString());
                            Debug.Log("from planner: " + planner.getValueType(tokens[j]));
                            Debug.Log("from inputString: " + jsons[i].inputs[j - 1].returntype);
                        }
                    }

                }
            }
        } //end of motion channel loop
        if (typesMatch)//set session variables for each channel
        {
            Debug.Log("setting session variables");
            for (int i = 0; i < machine2ZXYE.motionChannels.Count; i++)
            {
                string channelName = machine2ZXYE.motionChannels[i];
                for (int j = 0; j < jsons[i].inputs.Count; j++)
                {
                    if (jsons[i].inputs[j].returntype == "int")
                    {
                        planner.setSessionInt(jsons[i].inputs[j].nodename, planner.getIntValue(tokens[j + 1]));
                        Debug.Log("setting session " + channelName + " int");
                    }
                    else //vector
                    {
                        planner.setSessionVector3(jsons[i].inputs[j].nodename, planner.getVectorValue(tokens[j + 1]));
                        Debug.Log("setting session " + channelName + " vector");
                    }
                }
            }
        }

        //remove this block after testing, replaced 8/27/23
        /*
        string retStringlf = db.getInputStringByMotion(tokens[0] + ".lf");
        string retStringlr = db.getInputStringByMotion(tokens[0] + ".lr");
        string retStringsegs = db.getInputStringByMotion(tokens[0] + ".segs");
        string retStringlfl = db.getInputStringByMotion(tokens[0] + ".lfl");
        string retStringlfu = db.getInputStringByMotion(tokens[0] + ".lfu");
        string retStringrfl = db.getInputStringByMotion(tokens[0] + ".rfl");
        string retStringrfu = db.getInputStringByMotion(tokens[0] + ".rfu");

        //if the motion node doesn't exist, set the string to an empty container
        //so my testing works
        if (retStringlf == "") retStringlf = JsonUtility.ToJson(new NSInputsJSON());
        if (retStringlr == "") retStringlr = JsonUtility.ToJson(new NSInputsJSON());
        if (retStringsegs == "") retStringsegs = JsonUtility.ToJson(new NSInputsJSON());
        if (retStringlfl == "") retStringlfl = JsonUtility.ToJson(new NSInputsJSON());
        if (retStringlfu == "") retStringlfu = JsonUtility.ToJson(new NSInputsJSON());
        if (retStringrfl == "") retStringrfl = JsonUtility.ToJson(new NSInputsJSON());
        if (retStringrfu == "") retStringrfu = JsonUtility.ToJson(new NSInputsJSON());


        NSInputsJSON lfJSON = JsonUtility.FromJson<NSInputsJSON>(retStringlf);
        NSInputsJSON lrJSON = JsonUtility.FromJson<NSInputsJSON>(retStringlr);
        NSInputsJSON segsJSON = JsonUtility.FromJson<NSInputsJSON>(retStringsegs);
        NSInputsJSON lflJSON = JsonUtility.FromJson<NSInputsJSON>(retStringlfl);
        NSInputsJSON lfuJSON = JsonUtility.FromJson<NSInputsJSON>(retStringlfu);
        NSInputsJSON rflJSON = JsonUtility.FromJson<NSInputsJSON>(retStringrfl);
        NSInputsJSON rfuJSON = JsonUtility.FromJson<NSInputsJSON>(retStringrfu);

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
        if (lflJSON.inputs.Count > 0)
        {
            Debug.Log("lflJSON.input.Count > 0");
            if (lflJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("lflJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (planner.getValueType(tokens[i]) != lflJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("lflJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + planner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + lflJSON.inputs[i - 1].returntype);
                        typesMatch = false;
                    }
                }
            }
        }
        if (lfuJSON.inputs.Count > 0)
        {
            Debug.Log("lfuJSON.input.Count > 0");
            if (lfuJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("lfuJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (planner.getValueType(tokens[i]) != lfuJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("lfuJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + planner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + lfuJSON.inputs[i - 1].returntype);
                        typesMatch = false;
                    }
                }
            }
        }
        if (rflJSON.inputs.Count > 0)
        {
            Debug.Log("rflJSON.input.Count > 0");
            if (rflJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("rflJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (planner.getValueType(tokens[i]) != rflJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("rflJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + planner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + rflJSON.inputs[i - 1].returntype);
                        typesMatch = false;
                    }
                }
            }
        }
        if (rfuJSON.inputs.Count > 0)
        {
            Debug.Log("rfuJSON.input.Count > 0");
            if (rfuJSON.inputs.Count != (tokens.Length - 1)) //first token is motion name
            {
                typesMatch = false;
                Debug.Log("rfuJSON input count didn't match");
            }
            else
            {
                for (int i = 1; i < tokens.Length; i++)
                {
                    if (planner.getValueType(tokens[i]) != rfuJSON.inputs[i - 1].returntype)
                    {
                        Debug.Log("rfuJSON type mismatch index: " + i.ToString());
                        Debug.Log("from planner: " + planner.getValueType(tokens[i]));
                        Debug.Log("from inputString: " + rfuJSON.inputs[i - 1].returntype);
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
            for (int i = 0; i < lflJSON.inputs.Count; i++)
            {
                if (lflJSON.inputs[i].returntype == "int")
                {
                    planner.setSessionInt(lflJSON.inputs[i].nodename, planner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session lflJSON int");
                }
                else //vector
                {
                    planner.setSessionVector3(lflJSON.inputs[i].nodename, planner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session lflJSON vector");
                }
            }
            for (int i = 0; i < lfuJSON.inputs.Count; i++)
            {
                if (lfuJSON.inputs[i].returntype == "int")
                {
                    planner.setSessionInt(lfuJSON.inputs[i].nodename, planner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session lfuJSON int");
                }
                else //vector
                {
                    planner.setSessionVector3(lfuJSON.inputs[i].nodename, planner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session lfuJSON vector");
                }
            }
            for (int i = 0; i < rflJSON.inputs.Count; i++)
            {
                if (rflJSON.inputs[i].returntype == "int")
                {
                    planner.setSessionInt(rflJSON.inputs[i].nodename, planner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session rflJSON int");
                }
                else //vector
                {
                    planner.setSessionVector3(rflJSON.inputs[i].nodename, planner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session rflJSON vector");
                }
            }
            for (int i = 0; i < rfuJSON.inputs.Count; i++)
            {
                if (rfuJSON.inputs[i].returntype == "int")
                {
                    planner.setSessionInt(rfuJSON.inputs[i].nodename, planner.getIntValue(tokens[i + 1]));
                    Debug.Log("setting session rfuJSON int");
                }
                else //vector
                {
                    planner.setSessionVector3(rfuJSON.inputs[i].nodename, planner.getVectorValue(tokens[i + 1]));
                    Debug.Log("setting session rfuJSON vector");
                }
            }


        }
        */


        return typesMatch;
    }





}
