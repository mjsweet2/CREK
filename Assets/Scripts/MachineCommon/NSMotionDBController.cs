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
public class NSInputJSON
{
    public string nodename;
    public string returntype;
    public NSInputJSON() { nodename = ""; returntype = ""; }

}

[Serializable]
public class NSInputsJSON
{
    public List<NSInputJSON> inputs;
    public NSInputsJSON() { inputs = new List<NSInputJSON>(); }
}

public class NSMotionDBController : MonoBehaviour
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

    public void createBlankNSMotionDB()
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

        //inputString is JSON list of TrajInputsNodes names and input types (float, Vector3), no values here
        //input JSON {"type":["float" | "float3"], "inputnodename":"" }
        //inputs JSON { "inputs":[{"type":["float" | "vector3"], "inputnodename":"" },{"type":["float" | "float3"], "inputnodename":"" }]}
        string createTrajNodesTableQuery = "CREATE TABLE TrajectoryNodes(";
        createTrajNodesTableQuery = createTrajNodesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createTrajNodesTableQuery = createTrajNodesTableQuery + "trajectory TEXT,";
        createTrajNodesTableQuery = createTrajNodesTableQuery + "inputString TEXT,"; //upgrading NotStop to support gametime trajs
        createTrajNodesTableQuery = createTrajNodesTableQuery + "edgeA TEXT,";
        createTrajNodesTableQuery = createTrajNodesTableQuery + "edgeB TEXT,";
        createTrajNodesTableQuery = createTrajNodesTableQuery + "edgeC TEXT,";
        createTrajNodesTableQuery = createTrajNodesTableQuery + "edgeD TEXT)";



        string createEdgesTableQuery = "CREATE TABLE TrajectoryEdges(";
        createEdgesTableQuery = createEdgesTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createEdgesTableQuery = createEdgesTableQuery + "nextNode TEXT)";



        string createMotionsTableQuery = "CREATE TABLE MotionNodes(";
        createMotionsTableQuery = createMotionsTableQuery + "name TEXT PRIMARY KEY NOT NULL,";
        createMotionsTableQuery = createMotionsTableQuery + "firstNode TEXT NOT NULL,";
        createMotionsTableQuery = createMotionsTableQuery + "inputString TEXT,"; //upgrading NotStop to support gametime trajs
        createMotionsTableQuery = createMotionsTableQuery + "edgeA TEXT,"; //not used 1/1/23
        createMotionsTableQuery = createMotionsTableQuery + "edgeB TEXT,"; //not used 1/1/23
        createMotionsTableQuery = createMotionsTableQuery + "edgeC TEXT,"; //not used 1/1/23
        createMotionsTableQuery = createMotionsTableQuery + "edgeD TEXT)"; //not used 1/1/23




        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;


        cmd.CommandText = createAllNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createFunctionNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createVectorNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createFloatNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createIntNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createVectorMathNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createMathNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createTrajNodesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createEdgesTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

        cmd.CommandText = createMotionsTableQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed




    }
    public void insertMotion(string motionName, string firstNode)
    { 
        string insertQuery = "INSERT INTO MotionNodes VALUES ('" + motionName + "', '" + firstNode + "', ";
        insertQuery = insertQuery + "'noinputs','noedge', 'noedge', 'noedge', 'noedge')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }

    public void insertTrajectoryNode(string trajNameKey, string trajName)
    {
        string insertQuery = "INSERT INTO TrajectoryNodes VALUES ('" + trajNameKey + "', '" + trajName + "', ";
        insertQuery = insertQuery + "'noinputs','noedge', 'noedge', 'noedge', 'noedge')";

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
    public void insertAllNodesNameType(string nodeNameKey, string nodeType)
    {
        string insertQuery = "INSERT INTO AllNodes VALUES ('" + nodeNameKey + "', '" + nodeType + "')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void insertEdgeTrajNextNode(string edgeName, string nextNode)
    {
        string insertQuery = "INSERT INTO TrajectoryEdges VALUES ('" + edgeName + "', '" + nextNode + "')";

        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
    }

    public void updateMotionFirstNode(string motionName, string trajNodeName)
    {
        string updateQuery = "UPDATE MotionNodes SET firstNode = '" + trajNodeName + "' WHERE name = '" + motionName + "'";

        Debug.Log(updateQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = updateQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void UpdateInputsOnTrajectoryNode(string trajNodeName, string inputString)
    {
        string updateQuery = "UPDATE TrajectoryNodes SET inputString = '" + inputString + "' WHERE name = '" + trajNodeName + "'";

        Debug.Log(updateQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = updateQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }
    public void UpdateInputsOnMotionNode(string motionNodeName, string inputString)
    {
        string updateQuery = "UPDATE MotionNodes SET inputString = '" + inputString + "' WHERE name = '" + motionNodeName + "'";

        Debug.Log(updateQuery);
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;

        cmd.CommandText = updateQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed

    }

    public void updateNodeEdgeA(string name, string edgeA)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE TrajectoryNodes SET edgeA = \'" + edgeA + "\' WHERE name = \'" + name + "\'";

        Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        Debug.Log("insert node: " + retValue.ToString());

    }
    public void updateNodeEdgeB(string name, string edgeB)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE TrajectoryNodes SET edgeB = \'" + edgeB + "\' WHERE name = \'" + name + "\'";

        //Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        //Debug.Log("insert node: " + retValue.ToString());

    }

    public void updateNodeEdgeC(string name, string edgeC)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE TrajectoryNodes SET edgeC = \'" + edgeC + "\' WHERE name = \'" + name + "\'";

        //Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        //Debug.Log("insert node: " + retValue.ToString());

    }
    public void updateNodeEdgeD(string name, string edgeD)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE TrajectoryNodes SET edgeD = \'" + edgeD + "\' WHERE name = \'" + name + "\'";

        //Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        //Debug.Log("insert node: " + retValue.ToString());

    }

    public void updateMotionEdgeA(string name, string edgeA)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE MotionNodes SET edgeA = \'" + edgeA + "\' WHERE name = \'" + name + "\'";

        Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        Debug.Log("insert node: " + retValue.ToString());

    }
    public void updateMotionEdgeB(string name, string edgeB)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE MotionNodes SET edgeB = \'" + edgeB + "\' WHERE name = \'" + name + "\'";

        //Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        //Debug.Log("insert node: " + retValue.ToString());

    }

    public void updateMotionEdgeC(string name, string edgeC)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE MotionNodes SET edgeC = \'" + edgeC + "\' WHERE name = \'" + name + "\'";

        //Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        //Debug.Log("insert node: " + retValue.ToString());

    }
    public void updateMotionEdgeD(string name, string edgeD)
    {
        IDbCommand cmd = dbcon.CreateCommand();
        int retValue;
        string insertQuery = "UPDATE MotionNodes SET edgeD = \'" + edgeD + "\' WHERE name = \'" + name + "\'";

        //Debug.Log(insertQuery);
        cmd.CommandText = insertQuery;
        retValue = cmd.ExecuteNonQuery(); // returns rows changed
        //Debug.Log("insert node: " + retValue.ToString());

    }




    public string getFirstNode()
	{
		IDbCommand cmnd_read = dbcon.CreateCommand();
		IDataReader reader;
		string queryString = "SELECT firstNode FROM Motions";

		cmnd_read.CommandText = queryString;
		reader = cmnd_read.ExecuteReader();
		Debug.Log(reader[0].ToString());
		return reader[0].ToString();
	}
	public string getFirstNodeByMotionName(string motionName)
	{
		IDbCommand cmnd_read = dbcon.CreateCommand();
		IDataReader reader;
		string queryString = "SELECT firstNode FROM MotionNodes WHERE name = \'" + motionName + "\'";

		cmnd_read.CommandText = queryString;
		reader = cmnd_read.ExecuteReader();

		return reader[0].ToString();

	}

    public string getInputStringByNode(string node)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT inputString FROM TrajectoryNodes WHERE name = \'" + node + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        return reader[0].ToString();
    }
    public string getInputStringByMotion(string mNode)
    {
        IDbCommand cmnd_read = dbcon.CreateCommand();
        IDataReader reader;
        string queryString = "SELECT inputString FROM MotionNodes WHERE name = \'" + mNode + "\'";

        cmnd_read.CommandText = queryString;
        reader = cmnd_read.ExecuteReader();

        string retString = "";

        //Field count is how many columns, not result count

        //this sets the string to "" if nothing returned
        retString = reader["inputString"].ToString();
        return retString;
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

        return new Vector3(x,y,z);
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

        NSInputJSON a = JsonUtility.FromJson<NSInputJSON>(reader[0].ToString());
        NSInputJSON b = JsonUtility.FromJson<NSInputJSON>(reader[1].ToString());
        NSInputsJSON nsInputsJSON = new NSInputsJSON();
        nsInputsJSON.inputs.Add(a);
        nsInputsJSON.inputs.Add(b);
        string retString = JsonUtility.ToJson(nsInputsJSON);

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

        NSInputJSON a = JsonUtility.FromJson<NSInputJSON>(reader[0].ToString());
        NSInputJSON b = JsonUtility.FromJson<NSInputJSON>(reader[1].ToString());
        NSInputJSON c = JsonUtility.FromJson<NSInputJSON>(reader[2].ToString());
        NSInputsJSON nsInputsJSON = new NSInputsJSON();
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
	public bool isLeafNode(string node)
	{

		//"""SELECT edgeA, edgeB, edgeC, edgeD FROM TrajectoryNodes WHERE name = '""" + node + """'"""

		IDbCommand cmnd_read = dbcon.CreateCommand();
		IDataReader reader;
		string queryString = "SELECT edgeA, edgeB, edgeC, edgeD FROM TrajectoryNodes WHERE name = \'" + node + "\'";

		cmnd_read.CommandText = queryString;
		reader = cmnd_read.ExecuteReader();

		//should be 4 on the return;

		return ((reader[0].ToString() == "noedge") && (reader[1].ToString() == "noedge") && (reader[2].ToString() == "noedge") && (reader[3].ToString() == "noedge"));

	}

	public string getTrajectoryByNode(string node)
	{
		//"""SELECT trajectory FROM TrajectoryNodes WHERE name = '""" + node + """'"""


		IDbCommand cmnd_read = dbcon.CreateCommand();
		IDataReader reader;
		string queryString = "SELECT trajectory FROM TrajectoryNodes WHERE name = \'" + node + "\'";
		
		cmnd_read.CommandText = queryString;
		reader = cmnd_read.ExecuteReader();

		return reader[0].ToString();

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
		string queryString = "SELECT nextNode FROM TrajectoryEdges WHERE name = \'" + edge + "\'";

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

    // Update is called once per frame
    public void createDB()
	{
		// Create database
		// https://www.connectionstrings.com/sqlite/
		//string connection = "URI=file:" + Application.persistentDataPath + "/" + "My_Database.db";
		//string connection = "Data Source = " + Application.persistentDataPath + "/" + dbName;
        string connection = "URI=file:" + "c:\\crek\\notstop\\" + dbName;
        Debug.Log(connection);

        Debug.Log(connection);
		// Open connection
		dbcon = new SqliteConnection(connection);
		dbcon.Open();

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
