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

public class MachineBalDiffPlanner : MonoBehaviour
{

    public ArticulatedBalDiff articulatedBalDiff;
    public MachineBalDiff machineBalDiff;
    public List<string> trajectoryNames;
    public List<string> trajectoryTypes;
    public Dictionary<string, Trajectory> trajectories;
    public Trajectory template;


    public delegate Vector3 VectorCmd();
    public Dictionary<string, VectorCmd> vectorFunctions;

    public delegate float FloatCmd();
    public Dictionary<string, FloatCmd> floatFunctions;

    public delegate int IntCmd();
    public Dictionary<string, IntCmd> intFunctions;


    public delegate void PlannerCmd(string a = "", string b = "", string c = "", string d = "", string e = "");
    public Dictionary<string, PlannerCmd> functions;

    public Dictionary<string, Vector3> namedVector3s;
    public Dictionary<string, float> namedFloats;
    public Dictionary<string, int> namedInts;

    //I want to differentiate between named and session
    //their function is mostly the same. I expect later
    //the named containers will fill up from the database
    //session containers filled during game time
    public Dictionary<string, Vector3> sessionVector3s;
    public Dictionary<string, float> sessionFloats;
    public Dictionary<string, int> sessionInts;

    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {

    }
    public void buildTables()
    {
        namedVector3s = new Dictionary<string, Vector3>();
        namedFloats = new Dictionary<string, float>();
        namedInts = new Dictionary<string, int>();

        sessionVector3s = new Dictionary<string, Vector3>();
        sessionFloats = new Dictionary<string, float>();
        sessionInts = new Dictionary<string, int>();

        trajectoryTypes = new List<string>();

        trajectoryTypes.Add("NAMEDTRAJECTORY");
        trajectoryTypes.Add("STRAIGHTTRAJECTORY");
        trajectoryTypes.Add("PAUSETRAJECTORY");
        trajectoryTypes.Add("VELOCITYTRAJECTORY");//x value holds velocity, resolution == keyframes


        trajectories = new Dictionary<string, Trajectory>();

        
        createVelocityTrajectories();
        createSetPointTrajectories();
        createGlideTrajectories();//use these for diff driving


        functions = new Dictionary<string, PlannerCmd>();
        functions["exportsessionvalues"] = exportsessionvalues;
        functions["importsessionvalues"] = importsessionvalues;


        intFunctions = new Dictionary<string, IntCmd>();
        floatFunctions = new Dictionary<string, FloatCmd>();
        vectorFunctions = new Dictionary<string, VectorCmd>();


        vectorFunctions["getlwStance"] = getlwStance;
        vectorFunctions["getrwStance"] = getrwStance;


    }
    public void exportsessionvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        // List<string> float3names;
        // List<Vector3> float3s;

        // List<string> floatnames;
        // List<float> floats;

        // List<string> intnames;
        // List<int> ints;
        SessionValuesJSON sessionValuesJSON = new SessionValuesJSON();

        foreach (string n in sessionVector3s.Keys)
        {
            Float3RecordJSON another = new Float3RecordJSON();
            another.name = n;
            another.value = sessionVector3s[n];
            sessionValuesJSON.float3Records.Add(another);
        }
        foreach (string n in sessionFloats.Keys)
        {
            FloatRecordJSON another = new FloatRecordJSON();
            another.name = n;
            another.value = sessionFloats[n];
            sessionValuesJSON.floatRecords.Add(another);
        }
        foreach (string n in sessionInts.Keys)
        {
            IntRecordJSON another = new IntRecordJSON();
            another.name = n;
            another.value = sessionInts[n];
            sessionValuesJSON.intRecords.Add(another);
        }
        string jsonString = JsonUtility.ToJson(sessionValuesJSON, true);

        StreamWriter sWriter;
        string path = "c://sites//notstop//" + a;
        sWriter = new StreamWriter(path);
        sWriter.Write(jsonString);
        sWriter.Close();


    }
    public void importsessionvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        StreamReader sReader;
        string path = "c://sites//notstop//" + a;//  BLSphere8Quadsv2.json"; //Application.persistentDataPath + "/trajectory/" + trajectoryName + ".json";
        sReader = new StreamReader(path);
        string fileString = sReader.ReadToEnd();

        SessionValuesJSON sessionValuesJSON = JsonUtility.FromJson<SessionValuesJSON>(fileString);

        for (int i = 0; i < sessionValuesJSON.float3Records.Count; i++)
        {
            setSessionVector3(sessionValuesJSON.float3Records[i].name, sessionValuesJSON.float3Records[i].value);
        }
        for (int i = 0; i < sessionValuesJSON.floatRecords.Count; i++)
        {
            setSessionFloat(sessionValuesJSON.floatRecords[i].name, sessionValuesJSON.floatRecords[i].value);
        }
        for (int i = 0; i < sessionValuesJSON.intRecords.Count; i++)
        {
            setSessionInt(sessionValuesJSON.intRecords[i].name, sessionValuesJSON.intRecords[i].value);
        }

        sReader.Close();

    }

    public void setSessionVector3(string anotherName, Vector3 another)
    {
        Debug.Log("setsessionvector: " + anotherName + ":" + another.ToString());
        if (sessionVector3s.ContainsKey(anotherName))
        {
            sessionVector3s[anotherName] = another;
        }
        else
        {
            sessionVector3s.Add(anotherName, another);
        }
    }
    public void setSessionFloat(string anotherName, float another)
    {
        Debug.Log("setsessionfloat: " + anotherName + ":" + another.ToString());
        if (sessionFloats.ContainsKey(anotherName))
        {
            sessionFloats[anotherName] = another;
        }
        else
        {
            sessionFloats.Add(anotherName, another);
        }
    }
    public void setSessionInt(string anotherName, int another)
    {
        if (sessionInts.ContainsKey(anotherName))
        {
            sessionInts[anotherName] = another;
        }
        else
        {
            sessionInts.Add(anotherName, another);
        }
    }
    public bool sessionIntExists(string intName)
    {
        return sessionInts.ContainsKey(intName);
    }
    public bool sessionVector3Exists(string vName)
    {
        return sessionVector3s.ContainsKey(vName);
    }
    public string getValueType(string valueName)
    {
        string retString = "notfound";
        if (sessionVector3s.ContainsKey(valueName))
        {
            retString = "float3";
        }
        else if (vectorFunctions.ContainsKey(valueName))
        {
            retString = "float3";
        }
        else if (namedVector3s.ContainsKey(valueName))
        {
            retString = "float3";
        }
        else if (doesIntExist(valueName))
        {
            retString = "int";
        }

        return retString;

    }
    public Vector3 getVectorValue(string vName)
    {
        if (sessionVector3s.ContainsKey(vName))
        {

            return sessionVector3s[vName];
        }
        else if (vectorFunctions.ContainsKey(vName))
        {

            return vectorFunctions[vName]();
        }
        else if (namedVector3s.ContainsKey(vName))
        {

            return namedVector3s[vName];
        }
        else
        {

            return Vector3.negativeInfinity;
        }
    }
    public float getFloatValue(string fName)
    {
        if (sessionFloats.ContainsKey(fName))
        {
            return sessionFloats[fName];
        }
        else if (floatFunctions.ContainsKey(fName))
        {
            return floatFunctions[fName]();
        }
        else if (namedFloats.ContainsKey(fName))
        {
            return namedFloats[fName];
        }
        else
        {
            return float.NegativeInfinity;
        }

    }
    public bool doesIntExist(string iName)
    {

        if (sessionInts.ContainsKey(iName))
        {
            return true;
        }
        else if (intFunctions.ContainsKey(iName))
        {
            return true;
        }
        else if (namedInts.ContainsKey(iName))
        {
            return true;
        }
        return false;
    }
    public int getIntValue(string iName)
    {
        if (sessionInts.ContainsKey(iName))
        {
            return sessionInts[iName];
        }
        else if (intFunctions.ContainsKey(iName))
        {
            return intFunctions[iName]();
        }
        else if (namedInts.ContainsKey(iName))
        {
            return namedInts[iName];
        }
        else
        {
            return 0;
        }
    }

    public string getTrajectoryType(string t)
    {
        if (trajectoryTypes.Contains(t))
            return t;
        else
            return trajectoryTypes[0];
    }

    public bool doesTrajectoryExist(string n)
    {
        return trajectories.ContainsKey(n);
    }
    public Trajectory getTrajectoryByName(string n)
    {
        if (trajectories.ContainsKey(n))
            return trajectories[n];
        else
            return null;
    }
    void loadTrajectoryJSONs()
    {
        //string path = Application.persistentDataPath + "/trajectory/";

        string path = "c:\\sites\\notstop\\trajectory\\";
        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] info = dir.GetFiles("*.json");

        Trajectory traj = (Trajectory)Resources.Load("Prefabs/Trajectory", typeof(Trajectory));


        foreach (FileInfo f in info)
        {
            //Debug.Log("Found: " + f.Name.Substring(0, f.Name.Length - 5));
            Trajectory another = Instantiate<Trajectory>(traj);
            another.trajectoryName = (f.Name.Substring(0, f.Name.Length - 5));
            trajectoryNames.Add(another.trajectoryName);
            another.importFromJSON();
            another.buildMotionTable();
            trajectories.Add(another.trajectoryName, another);
        }
    }
    //this one uses the Trajectory assets loaded from the files
    Trajectory getTrajectoryByName0(string trajName)
    {
        string path = "c:\\sites\\notstop\\trajectory\\";

        Trajectory traj = (Trajectory)Resources.Load("Prefabs/Trajectory", typeof(Trajectory));

        Trajectory another = Instantiate<Trajectory>(traj);
        another.trajectoryName = trajName;
        another.importFromJSON(path + trajName + ".json");
        another.buildMotionTable();

        return another;
    }

    void createTraj(string name, ref TrajJSON trajJSON)
    {
        Trajectory another = Instantiate<Trajectory>(template);

        another.initFromJSONString(trajJSON);
        another.buildMotionTable();

        another.trajectoryName = name;
        trajectories.Add(name, another);

    }

    Vector3 getlwStance() { return machineBalDiff.getlwStance(); }
    Vector3 getrwStance() { return machineBalDiff.getrwStance(); }

    void createGlideTrajectories()
    {
        TrajJSON trajJSON;


        //glide010.lw, glide010.rw
        {
     
            trajJSON = new TrajJSON();
            trajJSON.resolution = 90;
            trajJSON.keyframes.Clear();

            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0.1f)));

            createTraj("glide010.lw", ref trajJSON);
            createTraj("glide010.rw", ref trajJSON);

        }
        //glide-010.lw, glide-010.rw
        {

            trajJSON = new TrajJSON();
            trajJSON.resolution = 90;
            trajJSON.keyframes.Clear();

            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, -0.1f)));

            createTraj("glide-010.lw", ref trajJSON);
            createTraj("glide-010.rw", ref trajJSON);

        }


    }
    void createVelocityTrajectories()
    {

        TrajJSON trajJSON;


        //forward60.lb, forward60.rb
        {
            Vector3[] v = new Vector3[60];
            createVelocityPoints(ref v, .05f, 10);

            trajJSON = new TrajJSON();
            trajJSON.resolution = secondsToFrames(1);
            trajJSON.keyframes.Clear();

            for (int i = 0; i < 60; i++)
            {

                trajJSON.keyframes.Add(new KeyFrameJSON("raw", v[i]));
            }

            createTraj("forward60.lb", ref trajJSON);
            createTraj("forward60.rb", ref trajJSON);
        }

        //backward60.lb, backward60.rb
        {
            Vector3[] v = new Vector3[60];
            createVelocityPoints(ref v, -.05f, 10);

            trajJSON = new TrajJSON();
            trajJSON.resolution = secondsToFrames(1);
            trajJSON.keyframes.Clear();

            for (int i = 0; i < 60; i++)
            {

                trajJSON.keyframes.Add(new KeyFrameJSON("raw", v[i]));
            }

            createTraj("backward60.lb", ref trajJSON);
            createTraj("backward60.rb", ref trajJSON);
        }

        //cycle120.lb, cycle120.rb
        {
            Vector3[] vf = new Vector3[60];
            Vector3[] vb = new Vector3[60];
            createVelocityPoints(ref vf, .05f, 30);
            createVelocityPoints(ref vb, -.05f, 30);

            trajJSON = new TrajJSON();
            trajJSON.resolution = secondsToFrames(2);
            trajJSON.keyframes.Clear();

            for (int i = 0; i < 60; i++)
            {

                trajJSON.keyframes.Add(new KeyFrameJSON("raw", vf[i]));
            }
            for (int i = 0; i < 60; i++)
            {

                trajJSON.keyframes.Add(new KeyFrameJSON("raw", vb[i]));
            }

            createTraj("cycle120.lb", ref trajJSON);
            createTraj("cycle120.rb", ref trajJSON);
        }

        //cycle240.lb, cycle240.rb
        {
            Vector3[] vf = new Vector3[120];
            Vector3[] vb = new Vector3[120];
            createVelocityPoints(ref vf, .025f, 60);
            createVelocityPoints(ref vb, -.025f, 60);

            trajJSON = new TrajJSON();
            trajJSON.resolution = secondsToFrames(4);
            trajJSON.keyframes.Clear();

            for (int i = 0; i < 120; i++)
            {

                trajJSON.keyframes.Add(new KeyFrameJSON("raw", vf[i]));
            }
            for (int i = 0; i < 120; i++)
            {

                trajJSON.keyframes.Add(new KeyFrameJSON("raw", vb[i]));
            }

            createTraj("cycle240.lb", ref trajJSON);
            createTraj("cycle240.rb", ref trajJSON);
        }


    }
    void createSetPointTrajectories()
    {
        TrajJSON trajJSON;


        //spcycle120.sp
        {
            Vector3[] vf = new Vector3[60];
            Vector3[] vb = new Vector3[60];
            createVelocityPoints(ref vf, .1f, 30);
            createVelocityPoints(ref vb, -.1f, 30);

            trajJSON = new TrajJSON();
            trajJSON.resolution = secondsToFrames(2);
            trajJSON.keyframes.Clear();

            for (int i = 0; i < 60; i++)
            {

                trajJSON.keyframes.Add(new KeyFrameJSON("raw", vf[i]));
            }
            for (int i = 0; i < 60; i++)
            {

                trajJSON.keyframes.Add(new KeyFrameJSON("raw", vb[i]));
            }

            createTraj("spcycle120.sp", ref trajJSON);

        }

        //spcycle60.sp
        {
            Vector3[] vf = new Vector3[30];
            Vector3[] vb = new Vector3[30];
            createVelocityPoints(ref vf, .1f, 15);
            createVelocityPoints(ref vb, -.1f, 15);

            trajJSON = new TrajJSON();
            trajJSON.resolution = secondsToFrames(1);
            trajJSON.keyframes.Clear();

            for (int i = 0; i < 30; i++)
            {

                trajJSON.keyframes.Add(new KeyFrameJSON("raw", vf[i]));
            }
            for (int i = 0; i < 30; i++)
            {

                trajJSON.keyframes.Add(new KeyFrameJSON("raw", vb[i]));
            }

            createTraj("spcycle60.sp", ref trajJSON);

        }

    }

    void printTrajectories()
    {
        foreach (string t in trajectories.Keys)
        {
            Debug.Log(t + ": " + trajectories[t].keyFrames.Count);
        }
    }

    public int secondsToFrames(int s)
    {
        return s * 60;
    }

    void createVelocityPoints(ref Vector3[] points, float v, int fadeFrameCount)
    {
        if ((2 * fadeFrameCount) > points.Length)
        {
            return;
        }

        for (int i = 0; i < points.Length; i++)
        {

            points[i].x = v;
            points[i].y = points[i].z = 0f;
        }

        {
            int i = 0; int j = points.Length - 1;
            for (; i < fadeFrameCount; i++, j--)
            {
                points[i] = points[i] * ((float)i / (float)fadeFrameCount);
                points[j] = points[j] * ((float)i / (float)fadeFrameCount);
                //Debug.Log("i,j: " +  i.ToString() + ":" + j.ToString() + ", " + points[i].ToString() + ":" + points[j].ToString());
            }
        }

        for (int i = 0; i < points.Length; i++)
        {
            Debug.Log(i.ToString() + ":" + points[i].ToString());
        }

    }

    

    void reversePoints(ref Vector3[] points)
    {
        int index0 = 0;
        int indexN = points.Length - 1;
        int max = points.Length / 2;
        Vector3 temp = Vector3.zero;

        for (; index0 < max; index0++, indexN--)
        {
            temp = points[index0];
            points[index0] = points[indexN];
            points[indexN] = temp;
        }
    }
    void translatePoints(ref Vector3[] points, Vector3 t)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i] += t;
        }
    }
    void rotatePoints(ref Vector3[] points, Vector3 r)
    {
        for (int i = 0; i < points.Length; i++)
        {   // Unity Rotation Order, Z, X, Y
            points[i] = Geometry.rotateZXY(r, points[i]);
        }
    }
    void scalePoints(ref Vector3[] points, Vector3 s)
    {
        for (int i = 0; i < points.Length; i++)
        {
            points[i].x *= s.x;
            points[i].y *= s.y;
            points[i].z *= s.z;
        }
    }
    void shearPointsAboutXAlongY(ref Vector3[] points, float a)
    {
        Matrix4x4 mat01 = new Matrix4x4();
        mat01.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
        mat01.m12 = a;

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = mat01.MultiplyPoint(points[i]);
        }
    }
    void shearPointsAboutXAlongZ(ref Vector3[] points, float a)
    {
        Matrix4x4 mat01 = new Matrix4x4();
        mat01.SetTRS(Vector3.zero, Quaternion.identity, Vector3.one);
        mat01.m21 = a;

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = mat01.MultiplyPoint(points[i]);
        }
    }
}
