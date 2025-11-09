/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class MachineZXYEDemiQuadShell : MonoBehaviour
{

    public CmdLexer cmdLexer;
    public CmdParser cmdParser;
    
    public Machine2ZXYE2ZX machine;
    public Machine2ZXYE2ZXPlanner planner;
    public CRTaskDBController crTaskdb;
    public Machine2ZXYE2ZXTask task;
    public RLSkillRig rlSkillRig;
    public ArticulatedZXYESegsDemiQuad articulated;

    public M2MqttCRClient m2MqttCRClient;


    public delegate Vector3 ShellVectorCmd();
    public Dictionary<string, ShellVectorCmd> vectorFunctions;

    public delegate int ShellIntCmd();
    public Dictionary<string, ShellIntCmd> intFunctions;

    public delegate void ShellCmd(string a = "", string b = "", string c = "", string d = "", string e = "");
    public Dictionary<string, ShellCmd> functions;


    // Start is called before the first frame update
    void Start()
    {
        m2MqttCRClient = GetComponent<M2MqttCRClient>();

        cmdLexer = GetComponent<CmdLexer>();
        cmdParser = GetComponent<CmdParser>();


        buildTables();

    }

    // Update is called once per frame
    void Update()
    {
        processMqttMessages();

    }

    void processMqttMessages()
    {
        while (m2MqttCRClient.hasMessages())
        {
            TopicMessageJSON m = m2MqttCRClient.getNextMessage();
            //Debug.Log(m.topic);
            if (m.topic == "demiquad/cmdpanel")
            {
                runCmd(m.messagestring);
            }

        }
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
        if ((cmdLexer != null) & (cmdParser != null))
        {
            cmdLexer.lexCmd(inputCmd);
            if (cmdLexer.errors.Count == 0)
            {
                cmdParser.parseCmd();
                if (cmdParser.errors.Count == 0)
                {
                    foreach (string s in cmdParser.sessionInts.Keys)
                    {
                        setsessionint(s, cmdParser.sessionInts[s]);
                    }
                    foreach (string s in cmdParser.sessionFloats.Keys)
                    {
                        ;// setsessionFloat(s,cmdParser.sessionFloats[s]);   
                    }
                    foreach (string s in cmdParser.sessionVector3s.Keys)
                    {
                        setsessionvector(s, cmdParser.sessionVector3s[s]);
                    }
                    inputCmd = cmdParser.outputCmd;
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }

        }


        //TODO
        //parse tokens
        //check to see if cmd is a shell cmd...
        //machine cmd
        //db or 
        //planner cmd
        //skill
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
            string firstNodeNamelf = crTaskdb.getFirstNodeByMotionName(iTokens[0] + ".lf");
            string firstNodeNamelr = crTaskdb.getFirstNodeByMotionName(iTokens[0] + ".lr");

            string firstNodeNamesegs = crTaskdb.getFirstNodeByMotionName(iTokens[0] + ".segs");

            string firstNodeNamelfl = crTaskdb.getFirstNodeByMotionName(iTokens[0] + ".lfl");
            string firstNodeNamelfu = crTaskdb.getFirstNodeByMotionName(iTokens[0] + ".lfu");
            string firstNodeNamerfl = crTaskdb.getFirstNodeByMotionName(iTokens[0] + ".rfl");
            string firstNodeNamerfu = crTaskdb.getFirstNodeByMotionName(iTokens[0] + ".rfu");

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
            else if (machine.cmdExists(iTokens[0]))
            {
                machine.runCmd(iTokens[0]);
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
                    if (machine.behaviorMode == Machine2ZXYE2ZX.BEHAVIORMODE.MOTION)
                    {
                        machine.runMotionOnAllChannels(iTokens[0]);
                    }
                    else if (machine.behaviorMode == Machine2ZXYE2ZX.BEHAVIORMODE.TRAJECTORY)
                    {
                        machine.runTrajectoryOnAllChannels(iTokens[0]);
                    }
                }

            }
            else if (rlSkillRig.importFromJSON(iTokens[0])) // this is a skill
            {
                rlSkillRig.setupSkill(iTokens[0]);
                
                //Debug.Log(iTokens[0] + " loaded");
            }
            else if (firstTaskNodeName != "")
            {
                if (machine.behaviorMode == Machine2ZXYE2ZX.BEHAVIORMODE.TASK)
                {
                    task.startTask(iTokens[0]);
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
        for (int i = 0; i < machine.motionChannels.Count; i++)
        {
            motionInputStr.Add(crTaskdb.getInputStringByMotion(tokens[0] + "." + machine.motionChannels[i]));
            if (motionInputStr[i] == "") motionInputStr[i] = JsonUtility.ToJson(new CRInputsJSON());
            jsons.Add(JsonUtility.FromJson<CRInputsJSON>(motionInputStr[i]));


            //type checking   
            if (jsons[i].inputs.Count > 0)
            {
                string channelName = machine.motionChannels[i];
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
            for (int i = 0; i < machine.motionChannels.Count; i++)
            {
                string channelName = machine.motionChannels[i];
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

   


        return typesMatch;
    }





}
