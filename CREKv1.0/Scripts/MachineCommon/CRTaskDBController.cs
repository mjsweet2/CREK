/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;
using System;

[Serializable]
public class Float3JSON
{
    public float x;
    public float y;
    public float z;
    public Float3JSON() { x = 0f; y = 0f; z = 0f; }

}

public class CRTaskDBController : MonoBehaviour
{

    public IDbConnection dbcon;
    public string dbName;

    public bool connectOnStart;


    // Start is called before the first frame update
    void Start()
    {
        if (connectOnStart)
        {
            createDB();
        }

    }

    // Update is called once per frame
    void Update()
    {


    }

    public void createBlankCRTaskDB()
    {

        //for maintaining node type for runtime instantiation
        //the type determines what table the node is kept in
        string createAllNodesTableQuery = "CREATE TABLE AllNodes(";
        createAllNodesTableQuery = createAllNodesTableQuery + "nodeName TEXT PRIMARY KEY NOT NULL,";
        createAllNodesTableQuery = createAllNodesTableQuery + "nodeType TEXT)";


        string createFunctionNodesTableQuery = "CREATE TABLE FunctionNodes(";
        createFunctionNodesTableQuery = createFunctionNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createFunctionNodesTableQuery = createFunctionNodesTableQuery + "functionName TEXT NOT NULL,";
        createFunctionNodesTableQuery = createFunctionNodesTableQuery + "returnType TEXT)";

        string createVectorNodesTableQuery = "CREATE TABLE VectorNodes(";
        createVectorNodesTableQuery = createVectorNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createVectorNodesTableQuery = createVectorNodesTableQuery + "x TEXT,";
        createVectorNodesTableQuery = createVectorNodesTableQuery + "y TEXT,";
        createVectorNodesTableQuery = createVectorNodesTableQuery + "z TEXT)";

        string createFloatNodesTableQuery = "CREATE TABLE FloatNodes(";
        createFloatNodesTableQuery = createFloatNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createFloatNodesTableQuery = createFloatNodesTableQuery + "value TEXT)";

        //ControllerInputNodes(name,value)
        string createControllerInputNodesTableQuery = "CREATE TABLE ControllerInputNodes(";
        createControllerInputNodesTableQuery = createControllerInputNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createControllerInputNodesTableQuery = createControllerInputNodesTableQuery + "inputString TEXT,"; //not implemented
        createControllerInputNodesTableQuery = createControllerInputNodesTableQuery + "value TEXT)";

        string createIntNodesTableQuery = "CREATE TABLE IntNodes(";
        createIntNodesTableQuery = createIntNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createIntNodesTableQuery = createIntNodesTableQuery + "value TEXT)";

        string createVectorMathNodesTableQuery = "CREATE TABLE VectorMathNodes(";
        createVectorMathNodesTableQuery = createVectorMathNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createVectorMathNodesTableQuery = createVectorMathNodesTableQuery + "operation TEXT,";
        createVectorMathNodesTableQuery = createVectorMathNodesTableQuery + "inputA TEXT,";
        createVectorMathNodesTableQuery = createVectorMathNodesTableQuery + "inputB TEXT,";
        createVectorMathNodesTableQuery = createVectorMathNodesTableQuery + "inputC TEXT)";

        string createMathNodesTableQuery = "CREATE TABLE MathNodes(";
        createMathNodesTableQuery = createMathNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createMathNodesTableQuery = createMathNodesTableQuery + "operation TEXT,";
        createMathNodesTableQuery = createMathNodesTableQuery + "inputA TEXT,";
        createMathNodesTableQuery = createMathNodesTableQuery + "inputB TEXT)";

        //FloatsToFloat3(name, inputA, inputB, inputC)
        string createFloatsToFloat3NodesTableQuery = "CREATE TABLE FloatsToFloat3Nodes(";
        createFloatsToFloat3NodesTableQuery = createFloatsToFloat3NodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createFloatsToFloat3NodesTableQuery = createFloatsToFloat3NodesTableQuery + "inputA TEXT,";
        createFloatsToFloat3NodesTableQuery = createFloatsToFloat3NodesTableQuery + "inputB TEXT,";
        createFloatsToFloat3NodesTableQuery = createFloatsToFloat3NodesTableQuery + "inputC TEXT)";

        //FloatBoolNodes(name, operation, inputA, inputB)
        string createFloatBoolNodesTableQuery = "CREATE TABLE FloatBoolNodes(";
        createFloatBoolNodesTableQuery = createFloatBoolNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createFloatBoolNodesTableQuery = createFloatBoolNodesTableQuery + "operation TEXT,";
        createFloatBoolNodesTableQuery = createFloatBoolNodesTableQuery + "inputA TEXT,";
        createFloatBoolNodesTableQuery = createFloatBoolNodesTableQuery + "inputB TEXT)";


        string createTaskItemEdgesTableQuery = "CREATE TABLE TaskItemEdges(";
        createTaskItemEdgesTableQuery = createTaskItemEdgesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createTaskItemEdgesTableQuery = createTaskItemEdgesTableQuery + "nextNode TEXT)";


        //MotionTaskItemNodes(name,motion,op,param1,param2,param3,param4,entry,inputString,exit)
        string createMotionTINodesTableQuery = "CREATE TABLE MotionTaskItemNodes(";
        createMotionTINodesTableQuery = createMotionTINodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createMotionTINodesTableQuery = createMotionTINodesTableQuery + "motion TEXT,";
        createMotionTINodesTableQuery = createMotionTINodesTableQuery + "op TEXT,";
        createMotionTINodesTableQuery = createMotionTINodesTableQuery + "param1 TEXT,";
        createMotionTINodesTableQuery = createMotionTINodesTableQuery + "param2 TEXT,";
        createMotionTINodesTableQuery = createMotionTINodesTableQuery + "param3 TEXT,";
        createMotionTINodesTableQuery = createMotionTINodesTableQuery + "param4 TEXT,";
        createMotionTINodesTableQuery = createMotionTINodesTableQuery + "entry TEXT,";
        createMotionTINodesTableQuery = createMotionTINodesTableQuery + "inputString TEXT,";
        createMotionTINodesTableQuery = createMotionTINodesTableQuery + "exit TEXT)";

        //TopicMessageTINodes(name,topic,message,inputString,entry,exit)
        string createTopicMessageTINodesTableQuery = "CREATE TABLE TopicMessageTINodes(";
        createTopicMessageTINodesTableQuery = createTopicMessageTINodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createTopicMessageTINodesTableQuery = createTopicMessageTINodesTableQuery + "topic TEXT,";
        createTopicMessageTINodesTableQuery = createTopicMessageTINodesTableQuery + "message TEXT,";
        createTopicMessageTINodesTableQuery = createTopicMessageTINodesTableQuery + "inputString TEXT,";
        createTopicMessageTINodesTableQuery = createTopicMessageTINodesTableQuery + "entry TEXT,";
        createTopicMessageTINodesTableQuery = createTopicMessageTINodesTableQuery + "exit TEXT)";


        //LocalTopicMessageTINodes(name,topic,message,inputA,entry,exit)
        string createLocalTopicMessageTINodesTableQuery = "CREATE TABLE LocalTopicMessageTINodes(";
        createLocalTopicMessageTINodesTableQuery = createLocalTopicMessageTINodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createLocalTopicMessageTINodesTableQuery = createLocalTopicMessageTINodesTableQuery + "topic TEXT,";
        createLocalTopicMessageTINodesTableQuery = createLocalTopicMessageTINodesTableQuery + "message TEXT,";
        createLocalTopicMessageTINodesTableQuery = createLocalTopicMessageTINodesTableQuery + "inputA TEXT,";
        createLocalTopicMessageTINodesTableQuery = createLocalTopicMessageTINodesTableQuery + "entry TEXT,";
        createLocalTopicMessageTINodesTableQuery = createLocalTopicMessageTINodesTableQuery + "exit TEXT)";


        //BoolFlowNodes(name, boolinput,entry,inputString,exitTrue,exitFalse)
        string createBoolFlowNodesTableQuery = "CREATE TABLE BoolFlowNodes(";
        createBoolFlowNodesTableQuery = createBoolFlowNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createBoolFlowNodesTableQuery = createBoolFlowNodesTableQuery + "boolInput TEXT,";
        createBoolFlowNodesTableQuery = createBoolFlowNodesTableQuery + "entry TEXT,";
        createBoolFlowNodesTableQuery = createBoolFlowNodesTableQuery + "inputString TEXT,";
        createBoolFlowNodesTableQuery = createBoolFlowNodesTableQuery + "exitTrue TEXT,";
        createBoolFlowNodesTableQuery = createBoolFlowNodesTableQuery + "exitFalse TEXT)";

        //AliasFlowNodes(name,entry,nextNodeName)
        string createAliasFlowNodesTableQuery = "CREATE TABLE AliasFlowNodes(";
        createAliasFlowNodesTableQuery = createAliasFlowNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createAliasFlowNodesTableQuery = createAliasFlowNodesTableQuery + "entry TEXT,";
        createAliasFlowNodesTableQuery = createAliasFlowNodesTableQuery + "nextNodeName TEXT)";


        string createTasksTableQuery = "CREATE TABLE TaskNodes(";
        createTasksTableQuery = createTasksTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createTasksTableQuery = createTasksTableQuery + "firstNode TEXT NOT NULL)";





        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;


        cmd.CommandText = createAllNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createFloatsToFloat3NodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createFunctionNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createVectorNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createFloatNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createControllerInputNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createIntNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createVectorMathNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createMathNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createFloatBoolNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed



        ////////////////////////////////////////////////////////////////////////
        //following are flow nodes tables


        cmd.CommandText = createLocalTopicMessageTINodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createTopicMessageTINodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createMotionTINodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createBoolFlowNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createAliasFlowNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed




        cmd.CommandText = createTaskItemEdgesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createTasksTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed



    }
    public void insertTask(string taskName, string firstNode)
    {
        string insertQuery = "INSERT INTO TaskNodes VALUES ('" + taskName + "', '" + firstNode + "')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }

    //MotionTaskItemNodes(name,motion,op,param1,param2,param3,param4,entry,inputString,exit)
    public void insertMotionTaskItemNode(string motionTINameKey, string motionName)
    {
        string insertQuery = "INSERT INTO MotionTaskItemNodes VALUES ('" + motionTINameKey + "', '" + motionName + "', ";
        insertQuery = insertQuery + "'noop','', '', '', '', 'noentry', 'noinputs', 'noexit')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    //TopicMessageTINodes(name,topic,message,inputA,entry,exit)
    public void insertTopicMessageTINode(string tiNameKey, string topic, string message)
    {
        string insertQuery = "INSERT INTO MotionTaskItemNodes VALUES ('" + tiNameKey + "', '" + topic + "', ";
        insertQuery = insertQuery + "'" + message + "', 'noinput', 'noentry', 'noexit')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    //LocalTopicMessageTINodes(name,topic,message,inputA,entry,exit)
    public void insertLocalTopicMessageTINode(string tiNameKey, string topic, string message)
    {
        string insertQuery = "INSERT INTO MotionTaskItemNodes VALUES ('" + tiNameKey + "', '" + topic + "', ";
        insertQuery = insertQuery + "'" + message + "', 'noinput', 'noentry', 'noexit')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }

    //BoolFlowNodes(name, boolinput,entry,inputString,exitTrue,exitFalse)
    public void insertBoolFlowNode(string boolFlowNameKey, string boolInput)
    {
        string insertQuery = "INSERT INTO BoolFlowNodes VALUES ('" + boolFlowNameKey + "', '" + boolInput + "', ";
        insertQuery = insertQuery + "'noentry', 'noinputs', 'noedge', 'noedge')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }

    public void insertAliasFlowNode(string aliasFlowNameKey, string entry, string nextNodeName)
    {
        //AliasFlowNodes(name,entry,nextNodeName)
        string insertQuery = "INSERT INTO AliasFlowNodes VALUES ('" + aliasFlowNameKey + "', '" + entry + "','" + nextNodeName + "')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }


    public void insertFunctionNode(string funcNameKey, string funcName, string retType)
    {
        string insertQuery = "INSERT INTO FunctionNodes VALUES ('" + funcNameKey + "', '" + funcName;
        insertQuery = insertQuery + "', '" + retType + "')";

        Debug.Log("insertFunctionNode: " + insertQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void insertIntNode(string intNameKey, string v)
    {
        string insertQuery = "INSERT INTO IntNodes VALUES ('" + intNameKey + "', '" + v;
        insertQuery = insertQuery + "')";

        Debug.Log("insertIntNode: " + insertQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void insertFloatNode(string floatNameKey, string v)
    {
        string insertQuery = "INSERT INTO FloatNodes VALUES ('" + floatNameKey + "', '" + v;
        insertQuery = insertQuery + "')";

        Debug.Log("insertFloatNode: " + insertQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    //ControllerInputNodes(name,value)
    public void insertControllerInputNode(string cNameKey, string v)
    {
        string insertQuery = "INSERT INTO ControllerInputNodes VALUES ('" + cNameKey + "', 'noinputs', '" + v + "')";
      
        Debug.Log("insertControllerInputNode: " + insertQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
    }
    public void insertVectorNode(string vectorNameKey, string x, string y, string z)
    {
        string insertQuery = "INSERT INTO VectorNodes VALUES ('" + vectorNameKey + "', '" + x;
        insertQuery = insertQuery + "', '" + y + "', '" + z + "')";

        Debug.Log("insertVectorNode: " + insertQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void insertVectorMathNode(string vMathNameKey, string operation, string inputA, string inputB, string inputC)
    {
        string insertQuery = "INSERT INTO VectorMathNodes VALUES ('" + vMathNameKey + "', '" + operation;
        insertQuery = insertQuery + "', '" + inputA + "', '" + inputB + "', '" + inputC + "')";

        Debug.Log("insertVectorMathNode: " + insertQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void insertMathNode(string mathNameKey, string operation, string inputA, string inputB)
    {
        string insertQuery = "INSERT INTO MathNodes VALUES ('" + mathNameKey + "', '" + operation;
        insertQuery = insertQuery + "', '" + inputA + "', '" + inputB + "')";

        Debug.Log("insertMathNode: " + insertQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }

    //FloatsToFloat3(name, inputA, inputB, inputC)
    public void insertFloatsToFloat3Node(string nodeNameKey, string inputA, string inputB, string inputC)
    {
        string insertQuery = "INSERT INTO FloatsToFloat3Nodes VALUES ('" + nodeNameKey + "', '" + inputA;
        insertQuery = insertQuery + "', '" + inputB + "', '" + inputC + "')";

        Debug.Log("insertFloatsToFloat3Node: " + insertQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void insertFloatBoolNode(string floatBoolNameKey, string operation, string inputA, string inputB)
    {
        string insertQuery = "INSERT INTO FloatBoolNodes VALUES ('" + floatBoolNameKey + "', '" + operation;
        insertQuery = insertQuery + "', '" + inputA + "', '" + inputB + "')";

        Debug.Log("insertFlowBoolNode: " + insertQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }

    public void insertAllNodesNameType(string nodeNameKey, string nodeType)
    {
        string insertQuery = "INSERT INTO AllNodes VALUES ('" + nodeNameKey + "', '" + nodeType + "')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void insertTaskItemEdge(string edgeName, string nextNode)
    {
        string insertQuery = "INSERT INTO TaskItemEdges VALUES ('" + edgeName + "', '" + nextNode + "')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
    }

    public void updateTaskFirstNode(string taskName, string flowNodeName)
    {
        string updateQuery = "UPDATE TaskNodes SET firstNode = '" + flowNodeName + "' WHERE name = '" + taskName + "'";

        Debug.Log(updateQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = updateQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void UpdateInputsOnMotionTINode(string motionTINodeName, string inputString)
    {
        string updateQuery = "UPDATE MotionTaskItemNodes SET inputString = '" + inputString + "' WHERE name = '" + motionTINodeName + "'";

        Debug.Log(updateQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = updateQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void UpdateInputsOnTopicMessageTINode(string tiNodeName, string inputString)
    {
        string updateQuery = "UPDATE TopicMessageTINodes SET inputString = '" + inputString + "' WHERE name = '" + tiNodeName + "'";

        Debug.Log(updateQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = updateQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void UpdateInputsOnLocalTopicMessageTINode(string tiNodeName, string inputString)
    {
        string updateQuery = "UPDATE LocalTopicMessageTINodes SET inputString = '" + inputString + "' WHERE name = '" + tiNodeName + "'";

        Debug.Log(updateQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = updateQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void UpdateInputsOnBoolFlowNode(string boolFlowNodeName, string inputString)
    {
        string updateQuery = "UPDATE BoolFlowNodes SET inputString = '" + inputString + "' WHERE name = '" + boolFlowNodeName + "'";

        Debug.Log(updateQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = updateQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }

    public void updateMotionTINodeExit(string nodeName, string exit)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE MotionTaskItemNodes SET exit = \'" + exit + "\' WHERE name = \'" + nodeName + "\'";

        Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        Debug.Log("insert node: " + retValue.ToString());

    }
    public void updateTopicMessageTINodeExit(string nodeName, string exit)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE TopicMessageTINodes SET exit = \'" + exit + "\' WHERE name = \'" + nodeName + "\'";

        Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        Debug.Log("insert node: " + retValue.ToString());

    }
    public void updateLocalTopicMessageTINodeExit(string nodeName, string exit)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE LocalTopicMessageTINodes SET exit = \'" + exit + "\' WHERE name = \'" + nodeName + "\'";

        Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        Debug.Log("insert node: " + retValue.ToString());

    }
    public void updateBoolFlowNodeExitTrue(string nodeName, string exitTrue)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE BoolFlowNodes SET exitTrue = \'" + exitTrue + "\' WHERE name = \'" + nodeName + "\'";

        Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        Debug.Log("insert node: " + retValue.ToString());

    }
    public void updateBoolFlowNodeExitFalse(string nodeName, string exitFalse)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE BoolFlowNodes SET exitFalse = \'" + exitFalse + "\' WHERE name = \'" + nodeName + "\'";

        Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        Debug.Log("insert node: " + retValue.ToString());

    }

    public string getMotionByNode(string nodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT motion FROM MotionTaskItemNodes WHERE name = \'" + nodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();

    }
    public string getTopicFromTopicMessageTINode(string nodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT topic FROM TopicMessageTINodes WHERE name = \'" + nodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();

    }
    public string getMessageFromTopicMessageTINode(string nodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT message FROM TopicMessageTINodes WHERE name = \'" + nodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();

    }

    public string getFirstNodeByTaskName(string taskName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT firstNode FROM TaskNodes WHERE name = \'" + taskName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();

    }

    public string getInputStringFromTopicMessageTINode(string flowNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT inputString FROM TopicMessageTINodes WHERE name = \'" + flowNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getInputStringFromMotionTINode(string flowNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT inputString FROM MotionTaskItemNodes WHERE name = \'" + flowNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getInputStringFromBoolFlowNode(string boolFlowNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT inputString FROM BoolFlowNodes WHERE name = \'" + boolFlowNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getExitTrueFromBoolFlowNode(string boolFlowNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT exitTrue FROM BoolFlowNodes WHERE name = \'" + boolFlowNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getExitFalseFromBoolFlowNode(string boolFlowNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT exitFalse FROM BoolFlowNodes WHERE name = \'" + boolFlowNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }

    //AliasFlowNodes(name,entry,nextNodeName)
    public string getNextNodeByAliasNodeName(string taskName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT nextNodeName FROM AliasFlowNodes WHERE name = \'" + taskName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();

    }
    public Vector3 getVectorFromVectorNode(string vNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT x, y, z FROM VectorNodes WHERE name = \'" + vNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();


        float x = float.Parse(reader[0].ToString());
        float y = float.Parse(reader[1].ToString());
        float z = float.Parse(reader[2].ToString());

        return new Vector3(x, y, z);
    }
    //value
    public float getValueFromFloatNode(string fNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT value FROM FloatNodes WHERE name = \'" + fNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        float v = float.Parse(reader[0].ToString());

        return v;
    }
    public int getValueFromIntNode(string iNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT value FROM IntNodes WHERE name = \'" + iNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        int v = int.Parse(reader[0].ToString());

        return v;
    }
    public string getInputStringFromMathNode(string mathNodeName)
    {

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT inputA, inputB FROM MathNodes WHERE name = \'" + mathNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        CRInputJSON a = JsonUtility.FromJson<CRInputJSON>(reader[0].ToString());
        CRInputJSON b = JsonUtility.FromJson<CRInputJSON>(reader[1].ToString());
        CRInputsJSON nsInputsJSON = new CRInputsJSON();
        nsInputsJSON.inputs.Add(a);
        nsInputsJSON.inputs.Add(b);
        string retString = JsonUtility.ToJson(nsInputsJSON);

        return retString;

    }
    public string getInputStringFromFloatBoolNode(string floatBoolNodeName)
    {

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT inputA, inputB FROM FloatBoolNodes WHERE name = \'" + floatBoolNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        CRInputJSON a = JsonUtility.FromJson<CRInputJSON>(reader[0].ToString());
        CRInputJSON b = JsonUtility.FromJson<CRInputJSON>(reader[1].ToString());
        CRInputsJSON nsInputsJSON = new CRInputsJSON();
        nsInputsJSON.inputs.Add(a);
        nsInputsJSON.inputs.Add(b);
        string retString = JsonUtility.ToJson(nsInputsJSON);

        return retString;

    }
    public string getInputStringFromFloatsToFloat3Node(string nodeName)
    {

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT inputA, inputB, inputC FROM FloatsToFloat3Nodes WHERE name = \'" + nodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        CRInputJSON a = JsonUtility.FromJson<CRInputJSON>(reader[0].ToString());
        CRInputJSON b = JsonUtility.FromJson<CRInputJSON>(reader[1].ToString());
        CRInputJSON c = JsonUtility.FromJson<CRInputJSON>(reader[2].ToString());
        CRInputsJSON nsInputsJSON = new CRInputsJSON();
        nsInputsJSON.inputs.Add(a);
        nsInputsJSON.inputs.Add(b);
        nsInputsJSON.inputs.Add(c);
        string retString = JsonUtility.ToJson(nsInputsJSON);

        return retString;

    }

    public string getInputStringFromControllerInputNode(string controllerInputNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT inputString FROM ControllerInputNodes WHERE name = \'" + controllerInputNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        string retString = "";

        //Field count is how many columns, not result count

        //this sets the string to "" if nothing returned
        retString = reader["inputString"].ToString();
        return retString;
    }
    public string getOperationFromMathNode(string mathNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT operation FROM MathNodes WHERE name = \'" + mathNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getOperationFromFloatBoolNode(string floatBoolNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT operation FROM FloatBoolNodes WHERE name = \'" + floatBoolNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getOperationFromVectorMathNode(string vMathNodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT operation FROM VectorMathNodes WHERE name = \'" + vMathNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getInputStringFromVectorMathNode(string vMathNodeName)
    {

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT inputA, inputB, inputC FROM VectorMathNodes WHERE name = \'" + vMathNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        CRInputJSON a = JsonUtility.FromJson<CRInputJSON>(reader[0].ToString());
        CRInputJSON b = JsonUtility.FromJson<CRInputJSON>(reader[1].ToString());
        CRInputJSON c = JsonUtility.FromJson<CRInputJSON>(reader[2].ToString());
        CRInputsJSON nsInputsJSON = new CRInputsJSON();
        nsInputsJSON.inputs.Add(a);
        nsInputsJSON.inputs.Add(b);
        nsInputsJSON.inputs.Add(c);
        string retString = JsonUtility.ToJson(nsInputsJSON);

        return retString;
    }
    
    public string getEdgeAByNode(string node)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT edgeA FROM TrajectoryNodes WHERE name = \'" + node + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getEdgeBByNode(string node)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT edgeB FROM TrajectoryNodes WHERE name = \'" + node + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getEdgeCByNode(string node)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT edgeC FROM TrajectoryNodes WHERE name = \'" + node + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getEdgeDByNode(string node)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT edgeD FROM TrajectoryNodes WHERE name = \'" + node + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getExitByMotionTINode(string nodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT exit FROM MotionTaskItemNodes WHERE name = \'" + nodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getExitByTopicMessageTINode(string nodeName)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT exit FROM TopicMessageTINodes WHERE name = \'" + nodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public bool isMotionTILeafNode(string motionTINodeName)
    {

        //"""SELECT exit FROM MotionTaskItemNodes WHERE name = '""" + node + """'"""

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT exit FROM MotionTaskItemNodes WHERE name = \'" + motionTINodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        //should be 4 on the return;

        return ((reader[0].ToString() == "noedge"));

    }
    public bool isBoolFlowLeafNode(string boolFlowNodeName)
    {

        //"""SELECT exit FROM BoolFlowNodes WHERE name = '""" + node + """'"""

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT exitTrue, exitFalse FROM BoolFlowNodes WHERE name = \'" + boolFlowNodeName + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        //should be 4 on the return;

        return ((reader[0].ToString() == "noedge") && (reader[1].ToString() == "noedge"));

    }

    public string getTypeByNode(string node)
    {
        //"""SELECT trajectory FROM TrajectoryNodes WHERE name = '""" + node + """'"""


        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT nodeType FROM AllNodes WHERE nodeName = \'" + node + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();

    }

    public string getNextNodeByEdge(string edge)
    {

        //"""SELECT nextNode FROM TrajectoryEdges WHERE name = '""" + edge + """'"""

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT nextNode FROM TaskItemEdges WHERE name = \'" + edge + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getFunctionByFunctionNode(string fNode)
    {

        //"""SELECT nextNode FROM TrajectoryEdges WHERE name = '""" + edge + """'"""

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT functionName FROM FunctionNodes WHERE name = \'" + fNode + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getReturnTypeByFunctionNode(string fNode)
    {

        //"""SELECT nextNode FROM TrajectoryEdges WHERE name = '""" + edge + """'"""

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT returnType FROM FunctionNodes WHERE name = \'" + fNode + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }

   


    public void createDB()
    {
        // Create database
        // https://www.connectionstrings.com/sqlite/
        //string connection = "URI=file:" + Application.persistentDataPath + "/" + "My_Database.db";
        //string connection = "Data Source = " + Application.persistentDataPath + "/" + dbName;
        string connection = "URI=file:" + "c:\\crek\\db\\" + dbName;
        Debug.Log(connection);

        Debug.Log(connection);
        // Open connection
        dbcon = new SqliteConnection(connection);
        dbcon.Open();

    }

    public void listTables(ref List<string> records)
    {
        //SELECT name FROM sqlite_master WHERE type = 'table'

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT name FROM sqlite_master WHERE type = 'table'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        while (reader.Read())
        {
            records.Add(reader[0].ToString());      
        }

    }
    public void listAllNodes(ref List<string> records)
    {


        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT * FROM AllNodes";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        while (reader.Read())
        {
            records.Add(reader[0].ToString() + ":" + reader[1].ToString()); 
        }

    }
    public void listTaskNodes(ref List<string> records)
    {
        

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT * FROM TaskNodes";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        //TaskNodes has 2 columns
        while (reader.Read())
        {
            records.Add(reader[0].ToString() + ":" + reader[1].ToString());
        }

    }
    public void listMotionTINodes(ref List<string> records)
    {

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT * FROM MotionTaskItemNodes";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        //MotionTaskItemNodes has 10 columns
        while (reader.Read())
        {
            string completeRow = reader[0].ToString() + ":" + reader[1].ToString() + ":";
            completeRow = completeRow + reader[2].ToString() + ":" + reader[3].ToString() + ":";
            completeRow = completeRow + reader[4].ToString() + ":" + reader[5].ToString() + ":";
            completeRow = completeRow + reader[6].ToString() + ":" + reader[7].ToString() + ":";
            completeRow = completeRow + reader[8].ToString() + ":" + reader[9].ToString() + ":";

            records.Add(completeRow);
        }

    }
    public void listBoolFlowNodes(ref List<string> records)
    {

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT * FROM BoolFlowNodes";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        //BoolFlowNodes has 6 columns
        while (reader.Read())
        {
            string completeRow = reader[0].ToString() + ":" + reader[1].ToString() + ":";
            completeRow = completeRow + reader[2].ToString() + ":" + reader[3].ToString() + ":";
            completeRow = completeRow + reader[4].ToString() + ":" + reader[5].ToString() + ":";

            records.Add(completeRow);
        }

    }
    public void listAliasFlowNodes(ref List<string> records)
    {

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT * FROM AliasFlowNodes";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        //AliasFlowNodes has 3 columns
        while (reader.Read())
        {
            string completeRow = reader[0].ToString() + ":" + reader[1].ToString() + ":";
            completeRow = completeRow + reader[2].ToString();
            records.Add(completeRow);
        }

    }
    public void listTaskItemEdges(ref List<string> records)
    {

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT * FROM TaskItemEdges";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        //TaskItemEdges has 2 columns
        while (reader.Read())
        {
            string completeRow = reader[0].ToString() + ":" + reader[1].ToString() + ":";
            records.Add(completeRow);
        }

    }
    public void listFloatBoolNodes(ref List<string> records)
    {

        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT * FROM FloatBoolNodes";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        //FloatBoolNodes has 4 columns
        while (reader.Read())
        {
            string completeRow = reader[0].ToString() + ":" + reader[1].ToString() + ":";
            completeRow = completeRow + reader[2].ToString() + ":" + reader[3].ToString() + ":";
            records.Add(completeRow);
        }

    }
    public void readFromDB()
    {
        // Read and print all values in table
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string query = "SELECT * FROM my_table";
        cmnd_read.CommandText = query;
        reader = cmnd_read.ExecuteReader();


        while (reader.Read())
        {
            Debug.Log("id: " + reader[0].ToString());
            Debug.Log("val: " + reader[1].ToString());
        }
    }

    public void closeDB()
    {
        // Close connection
        dbcon.Close();
    }


}
