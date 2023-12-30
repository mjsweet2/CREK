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



[Serializable]
public class Float3RecordJSON
{
    public string name;
    public Vector3 value;
    public Float3RecordJSON() { value = new Vector3(0f, 0f, 0f); }
}


[Serializable]
public class FloatRecordJSON
{
    public string name;
    public float value;
    public FloatRecordJSON(float f = 0f) { value = f; }
}
[Serializable]
public class IntRecordJSON
{
    public string name;
    public int value;
    public IntRecordJSON(int i = 0) { value = i; }
}




[Serializable]
public class SessionValuesOldJSON
{
    public List<string> float3names;
    public List<Vector3> float3s;

    public List<string> floatnames;
    public List<float> floats;

    public List<string> intnames;
    public List<int> ints;
    public SessionValuesOldJSON()
    {
        float3names = new List<string>(); float3s = new List<Vector3>();
        floatnames = new List<string>(); floats = new List<float>();
        intnames = new List<string>(); ints = new List<int>();
    }

}
[Serializable]
public class SessionValuesJSON
{
    public List<Float3RecordJSON> float3Records;
    public List<FloatRecordJSON> floatRecords;
    public List<IntRecordJSON> intRecords;

    public SessionValuesJSON()
    {
        float3Records = new List<Float3RecordJSON>();
        floatRecords = new List<FloatRecordJSON>();
        intRecords = new List<IntRecordJSON>();
    }

}


public class Machine2ZXYEPlanner : MonoBehaviour
{


    public Machine2ZXYE machine2ZXYE;
    public List<string> trajectoryNames;//game players lines
    public List<string> trajectoryTypes;
    public Dictionary<string, Trajectory> trajectories;
    public Trajectory template;


    public Vector3 Crouch01lf;
    public Vector3 Crouch01rf;


    public Vector3 Crouch02lf;
    public Vector3 Crouch02rf;


    public Vector3 currCrouchlf;
    public Vector3 currCrouchrf;


    public float stepRadius;
    public float sideStepRadius;
    public int halfStepResolution;
    public int fullStepResolution;
    public int crouchResolution;
    public int pause03Resolution;

    public float jumpLength;
    public int jumpOpenResolution;
    public int jumpResolution;

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
        trajectoryTypes.Add("STEPTRAJECTORY");
        trajectoryTypes.Add("STRAIGHTTRAJECTORY");
        trajectoryTypes.Add("CIRCLEDIRTRAJECTORY");
        trajectoryTypes.Add("PAUSETRAJECTORY");

        Crouch01lf = new Vector3(-.25f, -.36f, .15f);
        Crouch01rf = new Vector3(.25f, -.36f, .15f);

        Crouch02lf = new Vector3(-.10f, -.32f, .40f);
        Crouch02rf = new Vector3(.10f, -.32f, .40f);

        currCrouchlf = Crouch01lf;
        currCrouchrf = Crouch01rf;



        trajectories = new Dictionary<string, Trajectory>();

        createCrouchTrajectories();
        createStepTrajectories();
        createSegsTrajectories();

        functions = new Dictionary<string, PlannerCmd>();
        //functions["exportsessionvalues"] = exportsessionvalues;
        //functions["importsessionvalues"] = importsessionvalues;


        intFunctions = new Dictionary<string, IntCmd>();

        intFunctions["getHalfStepResolution"] = getHalfStepResolution;
        intFunctions["getFullStepResolution"] = getFullStepResolution;
        intFunctions["getCrouchResolution"] = getCrouchResolution;

        floatFunctions = new Dictionary<string, FloatCmd>();


        vectorFunctions = new Dictionary<string, VectorCmd>();


        vectorFunctions["getStanceLF"] = getStanceLF;
        vectorFunctions["getStanceLR"] = getStanceLR;
        vectorFunctions["getStanceRF"] = getStanceRF;
        vectorFunctions["getStanceRR"] = getStanceRR;

        vectorFunctions["getLowerLF"] = getLowerLF;
        vectorFunctions["getUpperLF"] = getUpperLF;
        vectorFunctions["getLowerRF"] = getLowerRF;
        vectorFunctions["getUpperRF"] = getUpperRF;

        vectorFunctions["getCurrCrouchLF"] = getCurrCrouchLF;
        vectorFunctions["getCurrCrouchRF"] = getCurrCrouchRF;

        vectorFunctions["getCurrFrontwardLF"] = getCurrFrontwardLF;
        vectorFunctions["getCurrFrontwardRF"] = getCurrFrontwardRF;

        vectorFunctions["getCurrRearwardLF"] = getCurrRearwardLF;

        vectorFunctions["getCurrRearwardRF"] = getCurrRearwardRF;

        vectorFunctions["getCurrLeftwardLF"] = getCurrLeftwardLF;
        vectorFunctions["getCurrLeftwardRF"] = getCurrLeftwardRF;


        vectorFunctions["getCurrRightwardLF"] = getCurrRightwardLF;
        vectorFunctions["getCurrRightwardRF"] = getCurrRightwardRF;

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
        string path = "c://crek//db//" + a;
        sWriter = new StreamWriter(path);
        sWriter.Write(jsonString);
        sWriter.Close();


    }
    public void importsessionvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        StreamReader sReader;
        string path = "c://crek//db//" + a;//  BLSphere8Quadsv2.json"; //Application.persistentDataPath + "/trajectory/" + trajectoryName + ".json";
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

    public void exportsessionOLDvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        // List<string> float3names;
        // List<Vector3> float3s;

        // List<string> floatnames;
        // List<float> floats;

        // List<string> intnames;
        // List<int> ints;
        SessionValuesOldJSON sessionValuesOldJSON = new SessionValuesOldJSON();

        foreach (string n in sessionVector3s.Keys)
        {
            sessionValuesOldJSON.float3names.Add(n);
            sessionValuesOldJSON.float3s.Add(sessionVector3s[n]);
        }
        foreach (string n in sessionFloats.Keys)
        {
            sessionValuesOldJSON.floatnames.Add(n);
            sessionValuesOldJSON.floats.Add(sessionFloats[n]);
        }
        foreach (string n in sessionInts.Keys)
        {
            sessionValuesOldJSON.intnames.Add(n);
            sessionValuesOldJSON.ints.Add(sessionInts[n]);
        }
        string jsonString = JsonUtility.ToJson(sessionValuesOldJSON, true);

        StreamWriter sWriter;
        string path = "c://crek//db//" + a;
        sWriter = new StreamWriter(path);
        sWriter.Write(jsonString);
        sWriter.Close();


    }
    public void importsessionOLDvalues(string a = "", string b = "", string c = "", string d = "", string e = "")
    {
        StreamReader sReader;
        string path = "c://crek//db//" + a;//  BLSphere8Quadsv2.json"; //Application.persistentDataPath + "/trajectory/" + trajectoryName + ".json";
        sReader = new StreamReader(path);
        string fileString = sReader.ReadToEnd();

        SessionValuesOldJSON sessionValuesOldJSON = JsonUtility.FromJson<SessionValuesOldJSON>(fileString);

        for (int i = 0; i < sessionValuesOldJSON.float3s.Count; i++)
        {
            setSessionVector3(sessionValuesOldJSON.float3names[i], sessionValuesOldJSON.float3s[i]);
        }

        for (int i = 0; i < sessionValuesOldJSON.floats.Count; i++)
        {
            setSessionFloat(sessionValuesOldJSON.floatnames[i], sessionValuesOldJSON.floats[i]);
        }
        for (int i = 0; i < sessionValuesOldJSON.ints.Count; i++)
        {
            setSessionInt(sessionValuesOldJSON.intnames[i], sessionValuesOldJSON.ints[i]);
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
    public void setcurrentcrouch()
    {
        currCrouchlf = machine2ZXYE.getlfStance();
        currCrouchrf = machine2ZXYE.getrfStance();
    }
    public string getTrajectoryType(string t)
    {
        if (trajectoryTypes.Contains(t))
            return t;
        else
            return trajectoryTypes[0];
    }

    int getHalfStepResolution() { return halfStepResolution; }
    int getFullStepResolution() { return fullStepResolution; }
    int getCrouchResolution() { return crouchResolution; }

    Vector3 getStanceLF() { return machine2ZXYE.getlfStance(); }
    Vector3 getStanceLR() { return machine2ZXYE.getlrStance(); }
    Vector3 getStanceRF() { return machine2ZXYE.getrfStance(); }
    Vector3 getStanceRR() { return machine2ZXYE.getrrStance(); }

    Vector3 getLowerLF() { return machine2ZXYE.getlfLower(); }
    Vector3 getUpperLF() { return machine2ZXYE.getlfUpper(); }

    Vector3 getLowerRF() { return machine2ZXYE.getrfLower(); }
    Vector3 getUpperRF() { return machine2ZXYE.getrfUpper(); }
    Vector3 getCurrCrouchLF() { return currCrouchlf; }
    Vector3 getCurrCrouchRF() { return currCrouchrf; }

    Vector3 getCurrFrontwardLF() { return currCrouchlf + new Vector3(0f, 0f, stepRadius); }
    Vector3 getCurrFrontwardRF() { return currCrouchrf + new Vector3(0f, 0f, stepRadius); }

    Vector3 getCurrRearwardLF() { return currCrouchlf - new Vector3(0f, 0f, stepRadius); }
    Vector3 getCurrRearwardRF() { return currCrouchrf - new Vector3(0f, 0f, stepRadius); }

    Vector3 getCurrLeftwardLF() { return currCrouchlf + new Vector3(sideStepRadius, 0f, 0f); }
    Vector3 getCurrLeftwardRF() { return currCrouchrf + new Vector3(sideStepRadius, 0f, 0f); }

    Vector3 getCurrRightwardLF() { return currCrouchlf - new Vector3(sideStepRadius, 0f, 0f); }
    Vector3 getCurrRightwardRF() { return currCrouchrf - new Vector3(sideStepRadius, 0f, 0f); }


    void loadTrajectoryJSONs()
    {
        //string path = Application.persistentDataPath + "/trajectory/";

        string path = "c:\\crek\\db\\trajectory\\";
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
        string path = "c:\\crek\\db\\trajectory\\";

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

    void createSegsTrajectories()
    {



        TrajJSON trajJSON;


        //circlerearwardccwsegs, for rolling, not turning
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = secondsToFrames(15);
            trajJSON.keyframes.Clear();

            //+y moves backward, -y moves forward, +x moves right, -x moves left
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, -5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(-18f, -5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(-18f, 18f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(18f, 18f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(18f, 5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0f)));

            createTraj("circlerearwardccwsegs", ref trajJSON);

        }

        //medcirclerearwardccwsegs, for turning, not rolling
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = secondsToFrames(10);
            trajJSON.keyframes.Clear();

            //+y moves backward, -y moves forward, +x moves right, -x moves left
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, -5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(-9f, -5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(-9f, 10f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(9f, 10f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(9f, -5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, -5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0f)));

            createTraj("medcirclerearwardccwsegs", ref trajJSON);

        }

        //circleforwardcwsegs, for rolling, not turning
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = secondsToFrames(15);
            trajJSON.keyframes.Clear();

            //+y moves backward, -y moves forward, +x moves right, -x moves left
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(-18f, 5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(-18f, -18f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(18f, -18f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(18f, -5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, -5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0f)));

            createTraj("circleforwardcwsegs", ref trajJSON);

        }


        //medcircleforwardcwsegs  for turning, not rolling
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = secondsToFrames(10);
            trajJSON.keyframes.Clear();

            //+y moves backward, -y moves forward, +x moves right, -x moves left
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(-9f, 5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(-9f, -10f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(9f, -10f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(9f, 5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 5f, 0f)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", new Vector3(0f, 0f, 0f)));

            createTraj("medcircleforwardcwsegs", ref trajJSON);

        }





    }
    void createStepTrajectories()
    {
        createPauseTrajectories();
        createForwardHalfStepTrajectories();
        createBackwardHalfStepTrajectories();
        createFullStepTrajectories();
        createGlideStepTrajectories();

        createSidewardStepTrajectories();

    }
    void createPauseTrajectories()
    {

        TrajJSON trajJSON;


        trajJSON = new TrajJSON();
        trajJSON.resolution = halfStepResolution;
        trajJSON.keyframes.Clear();

        createTraj("halfpauself", ref trajJSON);
        createTraj("halfpauselr", ref trajJSON);
        createTraj("halfpauserf", ref trajJSON);
        createTraj("halfpauserr", ref trajJSON);

        trajJSON.resolution = fullStepResolution;

        createTraj("fullpauself", ref trajJSON);
        createTraj("fullpauselr", ref trajJSON);
        createTraj("fullpauserf", ref trajJSON);
        createTraj("fullpauserr", ref trajJSON);

        trajJSON.resolution = pause03Resolution;

        createTraj("pause03lf", ref trajJSON);
        createTraj("pause03lr", ref trajJSON);
        createTraj("pause03rf", ref trajJSON);
        createTraj("pause03rr", ref trajJSON);

    }
    void createJumpTrajectories()
    {

        TrajJSON trajJSONlf;
        TrajJSON trajJSONrf;


        Vector3 leftW01 = new Vector3(-jumpLength, 0f, 0f);
        Vector3 rightW01 = new Vector3(jumpLength, 0f, 0f);

        Vector3 leftW02 = new Vector3(-1.5f * jumpLength, -jumpLength, 0f);
        Vector3 rightW02 = new Vector3(1.5f * jumpLength, -jumpLength, 0f);


        Vector3 leftW03 = new Vector3(0f, jumpLength, 0f);
        Vector3 rightW03 = new Vector3(0f, jumpLength, 0f);


        //leftwardjumpoglide
        //rightwardjumpoglide
        {
            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();

            trajJSONlf.resolution = jumpOpenResolution;
            trajJSONrf.resolution = jumpOpenResolution;

            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();


            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf + rightW01)));


            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf + rightW01)));




            createTraj("leftwardjumpoglidelf", ref trajJSONlf);
            createTraj("leftwardjumpogliderf", ref trajJSONrf);


            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();

            trajJSONlf.resolution = jumpOpenResolution;
            trajJSONrf.resolution = jumpOpenResolution;

            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();


            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf + leftW01)));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf + leftW01)));



            createTraj("rightwardjumpoglidelf", ref trajJSONlf);
            createTraj("rightwardjumpogliderf", ref trajJSONrf);

        }

        //leftward jump
        {
            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();

            trajJSONlf.resolution = jumpResolution;
            trajJSONrf.resolution = jumpResolution;

            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();


            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf + rightW01)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf + rightW02)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf + leftW03)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf)));


            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf + rightW01)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf + rightW02)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf + leftW03)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf)));


            createTraj("leftwardjumplf", ref trajJSONlf);
            createTraj("leftwardjumprf", ref trajJSONrf);


        }

        //rightward jump
        {
            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();

            trajJSONlf.resolution = jumpResolution;
            trajJSONrf.resolution = jumpResolution;

            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();


            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf + leftW01)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf + leftW02)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf + rightW03)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02lf)));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf + leftW01)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf + leftW02)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf + rightW03)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch02rf)));

            createTraj("rightwardjumplf", ref trajJSONlf);
            createTraj("rightwardjumprf", ref trajJSONrf);


        }


    }
    void createSidewardStepTrajectories()
    {
        Vector3[] theLeftwardArc = new Vector3[9];
        Vector3[] theRightwardArc = new Vector3[9];

        Vector3[] theLeftwardOHalfArc = new Vector3[9];
        Vector3[] theLeftwardCHalfArc = new Vector3[9];

        Vector3[] theRightwardOHalfArc = new Vector3[9];
        Vector3[] theRightwardCHalfArc = new Vector3[9];


        //for glide calc
        Vector3 leftW = new Vector3(-sideStepRadius, 0f, 0f);
        Vector3 rightW = new Vector3(sideStepRadius, 0f, 0f);

        TrajJSON trajJSONlf;
        TrajJSON trajJSONrf;

        //create leftward points

        Geometry.generatePointCircleTop(sideStepRadius, ref theLeftwardArc);
        Geometry.generatePointCircleTop(sideStepRadius / 2, ref theLeftwardOHalfArc);
        Geometry.generatePointCircleTop(sideStepRadius / 2, ref theLeftwardCHalfArc);

        reversePoints(ref theLeftwardArc);
        reversePoints(ref theLeftwardOHalfArc);
        reversePoints(ref theLeftwardCHalfArc);

        rotatePoints(ref theLeftwardArc, new Vector3(0f, 90f * Mathf.Deg2Rad, 0f));
        rotatePoints(ref theLeftwardOHalfArc, new Vector3(0f, 90f * Mathf.Deg2Rad, 0f));
        rotatePoints(ref theLeftwardCHalfArc, new Vector3(0f, 90f * Mathf.Deg2Rad, 0f));


        scalePoints(ref theLeftwardArc, new Vector3(1f, 2f, 1f));
        scalePoints(ref theLeftwardOHalfArc, new Vector3(1f, 3f, 1f));
        scalePoints(ref theLeftwardCHalfArc, new Vector3(1f, 3f, 1f));

        translatePoints(ref theLeftwardOHalfArc, new Vector3(-sideStepRadius / 2, 0f, 0f));
        translatePoints(ref theLeftwardCHalfArc, new Vector3(sideStepRadius / 2, 0f, 0f));



        //create rightward points

        Geometry.generatePointCircleTop(sideStepRadius, ref theRightwardArc);
        Geometry.generatePointCircleTop(sideStepRadius / 2, ref theRightwardOHalfArc);
        Geometry.generatePointCircleTop(sideStepRadius / 2, ref theRightwardCHalfArc);

        rotatePoints(ref theRightwardArc, new Vector3(0f, 90f * Mathf.Deg2Rad, 0f));
        rotatePoints(ref theRightwardOHalfArc, new Vector3(0f, 90f * Mathf.Deg2Rad, 0f));
        rotatePoints(ref theRightwardCHalfArc, new Vector3(0f, 90f * Mathf.Deg2Rad, 0f));


        scalePoints(ref theRightwardArc, new Vector3(1f, 2f, 1f));
        scalePoints(ref theRightwardOHalfArc, new Vector3(1f, 2f, 1f));
        scalePoints(ref theRightwardCHalfArc, new Vector3(1f, 2f, 1f));

        translatePoints(ref theRightwardOHalfArc, new Vector3(sideStepRadius / 2, 0f, 0f));
        translatePoints(ref theRightwardCHalfArc, new Vector3(-sideStepRadius / 2, 0f, 0f));



        // leftwardstep

        {
            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();

            trajJSONlf.resolution = fullStepResolution;
            trajJSONrf.resolution = fullStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            for (int i = 0; i < theLeftwardArc.Length; i++)
            {
                trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theLeftwardArc[i])));
                trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theLeftwardArc[i])));
            }
            createTraj("leftwardfullsteplf", ref trajJSONlf);
            createTraj("leftwardfullsteprf", ref trajJSONrf);



            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = halfStepResolution;
            trajJSONrf.resolution = halfStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            for (int i = 0; i < theLeftwardOHalfArc.Length; i++)
            {
                trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theLeftwardOHalfArc[i])));
                trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theLeftwardOHalfArc[i])));
            }
            createTraj("leftwardohalfsteplf", ref trajJSONlf);
            createTraj("leftwardohalfsteprf", ref trajJSONrf);


            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = halfStepResolution;
            trajJSONrf.resolution = halfStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            for (int i = 0; i < theLeftwardCHalfArc.Length; i++)
            {
                trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theLeftwardCHalfArc[i])));
                trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theLeftwardCHalfArc[i])));
            }
            createTraj("leftwardchalfsteplf", ref trajJSONlf);
            createTraj("leftwardchalfsteprf", ref trajJSONrf);

        }

        // rightwardstep

        {
            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = fullStepResolution;
            trajJSONrf.resolution = fullStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            for (int i = 0; i < theRightwardArc.Length; i++)
            {
                trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theRightwardArc[i])));
                trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theRightwardArc[i])));
            }
            createTraj("rightwardfullsteplf", ref trajJSONlf);
            createTraj("rightwardfullsteprf", ref trajJSONrf);


            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = halfStepResolution;
            trajJSONrf.resolution = halfStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            for (int i = 0; i < theRightwardOHalfArc.Length; i++)
            {
                trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theRightwardOHalfArc[i])));
                trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theRightwardOHalfArc[i])));
            }
            createTraj("rightwardohalfsteplf", ref trajJSONlf);
            createTraj("rightwardohalfsteprf", ref trajJSONrf);


            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = halfStepResolution;
            trajJSONrf.resolution = halfStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            for (int i = 0; i < theRightwardCHalfArc.Length; i++)
            {
                trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theRightwardCHalfArc[i])));
                trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theRightwardCHalfArc[i])));
            }
            createTraj("rightwardchalfsteplf", ref trajJSONlf);
            createTraj("rightwardchalfsteprf", ref trajJSONrf);

        }

        // rightwardglides
        {

            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = fullStepResolution;
            trajJSONrf.resolution = fullStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + rightW)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + leftW)));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + rightW)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + leftW)));

            createTraj("rightwardfullglidelf", ref trajJSONlf);
            createTraj("rightwardfullgliderf", ref trajJSONrf);


            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = halfStepResolution;
            trajJSONrf.resolution = halfStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();


            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + leftW)));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + leftW)));

            createTraj("rightwardohalfglidelf", ref trajJSONlf);
            createTraj("rightwardohalfgliderf", ref trajJSONrf);

            //

            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = halfStepResolution;
            trajJSONrf.resolution = halfStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + rightW)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf)));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + rightW)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf)));

            createTraj("rightwardchalfglidelf", ref trajJSONlf);
            createTraj("rightwardchalfgliderf", ref trajJSONrf);


        }

        // leftwardglides
        {

            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = fullStepResolution;
            trajJSONrf.resolution = fullStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();


            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + leftW)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + rightW)));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + leftW)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + rightW)));

            createTraj("leftwardfullglidelf", ref trajJSONlf);
            createTraj("leftwardfullgliderf", ref trajJSONrf);


            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = halfStepResolution;
            trajJSONrf.resolution = halfStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + rightW)));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + rightW)));

            createTraj("leftwardohalfglidelf", ref trajJSONlf);
            createTraj("leftwardohalfgliderf", ref trajJSONrf);


            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();
            trajJSONlf.resolution = halfStepResolution;
            trajJSONrf.resolution = halfStepResolution;
            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + leftW)));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf)));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + leftW)));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf)));

            createTraj("leftwardchalfglidelf", ref trajJSONlf);
            createTraj("leftwardchalfgliderf", ref trajJSONrf);

        }


    }
    void createGlideStepTrajectories()
    {

        TrajJSON trajJSON;

        Vector3 fW = new Vector3(0f, 0f, stepRadius);
        Vector3 rW = new Vector3(0f, 0f, -stepRadius);

        //backwardohalfglide
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = halfStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + fW)));

            createTraj("backwardohalfglidelf", ref trajJSON);




            trajJSON = new TrajJSON();
            trajJSON.resolution = halfStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + fW)));

            createTraj("backwardohalfgliderf", ref trajJSON);


        }

        //backwardchalfglide
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = halfStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + rW)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf)));

            createTraj("backwardchalfglidelf", ref trajJSON);



            trajJSON = new TrajJSON();
            trajJSON.resolution = halfStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + rW)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf)));

            createTraj("backwardchalfgliderf", ref trajJSON);


        }

        //forwardohalfglide
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = halfStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + rW)));

            createTraj("forwardohalfglidelf", ref trajJSON);



            trajJSON = new TrajJSON();
            trajJSON.resolution = halfStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + rW)));

            createTraj("forwardohalfgliderf", ref trajJSON);


        }

        //forwardchalfglide
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = halfStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + fW)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf)));

            createTraj("forwardchalfglidelf", ref trajJSON);



            trajJSON = new TrajJSON();
            trajJSON.resolution = halfStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + fW)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf)));

            createTraj("forwardchalfgliderf", ref trajJSON);


        }

        //backwardfullglide
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = fullStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + rW)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + fW)));

            createTraj("backwardfullglidelf", ref trajJSON);






            trajJSON = new TrajJSON();
            trajJSON.resolution = fullStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + rW)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + fW)));

            createTraj("backwardfullgliderf", ref trajJSON);



        }

        //forwardfullglide
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = fullStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + fW)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + rW)));

            createTraj("forwardfullglidelf", ref trajJSON);






            trajJSON = new TrajJSON();
            trajJSON.resolution = fullStepResolution;
            trajJSON.keyframes.Clear();


            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + fW)));
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + rW)));

            createTraj("forwardfullgliderf", ref trajJSON);


        }


    }
    void createFullStepTrajectories()
    {
        Vector3[] theForwardArc = new Vector3[9];
        Vector3[] theBackwardArc = new Vector3[9];

        Geometry.generatePointCircleTop(stepRadius, ref theForwardArc);
        reversePoints(ref theForwardArc);
        scalePoints(ref theForwardArc, new Vector3(1f, 2f, 1f));



        Geometry.generatePointCircleTop(stepRadius, ref theBackwardArc);
        scalePoints(ref theBackwardArc, new Vector3(1f, 2f, 1f));


        TrajJSON trajJSON;


        //forwardfullstep
        {
            trajJSON = new TrajJSON();
            trajJSON.resolution = fullStepResolution;
            trajJSON.keyframes.Clear();

            for (int i = 0; i < theForwardArc.Length; i++)
            {
                trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theForwardArc[i])));
            }
            createTraj("forwardfullsteplf", ref trajJSON);


            trajJSON = new TrajJSON();
            trajJSON.resolution = fullStepResolution;
            trajJSON.keyframes.Clear();

            for (int i = 0; i < theForwardArc.Length; i++)
            {
                trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theForwardArc[i])));
            }
            createTraj("forwardfullsteprf", ref trajJSON);



        }


        //backwardfullstep
        {

            trajJSON = new TrajJSON();
            trajJSON.resolution = fullStepResolution;
            trajJSON.keyframes.Clear();

            for (int i = 0; i < theForwardArc.Length; i++)
            {
                trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theBackwardArc[i])));
            }
            createTraj("backwardfullsteplf", ref trajJSON);


            trajJSON = new TrajJSON();
            trajJSON.resolution = fullStepResolution;
            trajJSON.keyframes.Clear();

            for (int i = 0; i < theForwardArc.Length; i++)
            {
                trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theBackwardArc[i])));
            }
            createTraj("backwardfullsteprf", ref trajJSON);



        }


    }
    void createBackwardHalfStepTrajectories()
    {

        Vector3[] theArc = new Vector3[9];
        Vector3[] theArcC = new Vector3[9];

        Geometry.generatePointCircleTop(stepRadius / 2, ref theArc);

        translatePoints(ref theArc, new Vector3(0f, 0f, -stepRadius / 2));
        scalePoints(ref theArc, new Vector3(1f, 3f, 1f));

        Geometry.generatePointCircleTop(stepRadius / 2, ref theArcC);

        translatePoints(ref theArcC, new Vector3(0f, 0f, stepRadius / 2));
        scalePoints(ref theArcC, new Vector3(1f, 3f, 1f));



        TrajJSON trajJSON;


        trajJSON = new TrajJSON();
        trajJSON.resolution = halfStepResolution;
        trajJSON.keyframes.Clear();

        for (int i = 0; i < theArc.Length; i++)
        {
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theArc[i])));
        }
        createTraj("backwardohalfsteplf", ref trajJSON);



        trajJSON = new TrajJSON();
        trajJSON.resolution = halfStepResolution;
        trajJSON.keyframes.Clear();

        for (int i = 0; i < theArc.Length; i++)
        {
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theArc[i])));
        }
        createTraj("backwardohalfsteprf", ref trajJSON);




        /*
         * 
         * 
         * 
         * 
         * */

        trajJSON = new TrajJSON();
        trajJSON.resolution = halfStepResolution;
        trajJSON.keyframes.Clear();

        for (int i = 0; i < theArc.Length; i++)
        {
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theArcC[i])));
        }
        createTraj("backwardchalfsteplf", ref trajJSON);




        trajJSON = new TrajJSON();
        trajJSON.resolution = halfStepResolution;
        trajJSON.keyframes.Clear();

        for (int i = 0; i < theArc.Length; i++)
        {
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theArcC[i])));
        }
        createTraj("backwardchalfsteprf", ref trajJSON);




    }
    void createForwardHalfStepTrajectories()
    {

        Vector3[] theArc = new Vector3[9];
        Vector3[] theArcC = new Vector3[9];

        Geometry.generatePointCircleTop(stepRadius / 2, ref theArc);
        reversePoints(ref theArc);
        translatePoints(ref theArc, new Vector3(0f, 0f, stepRadius / 2));
        scalePoints(ref theArc, new Vector3(1f, 3f, 1f));

        Geometry.generatePointCircleTop(stepRadius / 2, ref theArcC);
        reversePoints(ref theArcC);
        translatePoints(ref theArcC, new Vector3(0f, 0f, -stepRadius / 2));
        scalePoints(ref theArcC, new Vector3(1f, 3f, 1f));



        TrajJSON trajJSON;


        trajJSON = new TrajJSON();
        trajJSON.resolution = halfStepResolution;
        trajJSON.keyframes.Clear();

        for (int i = 0; i < theArc.Length; i++)
        {
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theArc[i])));
        }
        createTraj("forwardohalfsteplf", ref trajJSON);




        trajJSON = new TrajJSON();
        trajJSON.resolution = halfStepResolution;
        trajJSON.keyframes.Clear();

        for (int i = 0; i < theArc.Length; i++)
        {
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theArc[i])));
        }
        createTraj("forwardohalfsteprf", ref trajJSON);




        /*
         * 
         * 
         * 
         * 
         * */

        trajJSON = new TrajJSON();
        trajJSON.resolution = halfStepResolution;
        trajJSON.keyframes.Clear();

        for (int i = 0; i < theArc.Length; i++)
        {
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01lf + theArcC[i])));
        }
        createTraj("forwardchalfsteplf", ref trajJSON);




        trajJSON = new TrajJSON();
        trajJSON.resolution = halfStepResolution;
        trajJSON.keyframes.Clear();

        for (int i = 0; i < theArc.Length; i++)
        {
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", (Crouch01rf + theArcC[i])));
        }
        createTraj("forwardchalfsteprf", ref trajJSON);




    }
    void createCircleTrajectories()
    {
        Vector3[] theCircle = new Vector3[16];

        Geometry.generatePointCircle(stepRadius, ref theCircle);




        TrajJSON trajJSON;


        trajJSON = new TrajJSON();
        trajJSON.resolution = fullStepResolution;
        trajJSON.keyframes.Clear();

        trajJSON.keyframes.Add(new KeyFrameJSON("raw", machine2ZXYE.lfToMachine(Crouch01lf)));
        for (int i = 0; i < theCircle.Length; i++)
        {
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", machine2ZXYE.lfToMachine(Crouch01lf + theCircle[i])));
        }
        trajJSON.keyframes.Add(new KeyFrameJSON("raw", machine2ZXYE.lfToMachine(Crouch01lf)));
        createTraj("Circle01lf", ref trajJSON);







        trajJSON = new TrajJSON();
        trajJSON.resolution = fullStepResolution;
        trajJSON.keyframes.Clear();

        trajJSON.keyframes.Add(new KeyFrameJSON("raw", machine2ZXYE.rfToMachine(Crouch01rf)));
        for (int i = 0; i < theCircle.Length; i++)
        {
            trajJSON.keyframes.Add(new KeyFrameJSON("raw", machine2ZXYE.rfToMachine(Crouch01rf + theCircle[i])));
        }
        trajJSON.keyframes.Add(new KeyFrameJSON("raw", machine2ZXYE.rfToMachine(Crouch01rf)));
        createTraj("Circle01rf", ref trajJSON);






    }
    void createCrouchTrajectories()
    {

        TrajJSON trajJSONlf;
        TrajJSON trajJSONrf;

        // Crouch01
        {

            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();

            trajJSONlf.resolution = crouchResolution;
            trajJSONrf.resolution = crouchResolution;

            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();


            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", machine2ZXYE.dormantLFIk()));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", Crouch01lf));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", machine2ZXYE.dormantRFIk()));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", Crouch01rf));


            createTraj("Crouch01lf", ref trajJSONlf);
            createTraj("Crouch01rf", ref trajJSONrf);
        }


        // down
        {

            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();

            trajJSONlf.resolution = crouchResolution;
            trajJSONrf.resolution = crouchResolution;

            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", Crouch01lf));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", Crouch02lf));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", Crouch01rf));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", Crouch02rf));


            createTraj("downlf", ref trajJSONlf);
            createTraj("downrf", ref trajJSONrf);
        }


        // up
        {

            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();

            trajJSONlf.resolution = crouchResolution;
            trajJSONrf.resolution = crouchResolution;

            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", Crouch02lf));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", Crouch01lf));


            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", Crouch02rf));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", Crouch01rf));

            createTraj("uplf", ref trajJSONlf);
            createTraj("uprf", ref trajJSONrf);

        }

        // downup
        {
            trajJSONlf = new TrajJSON();
            trajJSONrf = new TrajJSON();

            trajJSONlf.resolution = crouchResolution;
            trajJSONrf.resolution = crouchResolution;

            trajJSONlf.keyframes.Clear();
            trajJSONrf.keyframes.Clear();

            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", Crouch02lf));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", Crouch01lf));
            trajJSONlf.keyframes.Add(new KeyFrameJSON("raw", Crouch02lf));

            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", Crouch02rf));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", Crouch01rf));
            trajJSONrf.keyframes.Add(new KeyFrameJSON("raw", Crouch02rf));

            createTraj("updownlf", ref trajJSONlf);
            createTraj("updownrf", ref trajJSONrf);

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

    //for stepping mostly vertical
    public void generateClimbPoints(ref Vector3[] points, Vector3 a, Vector3 b)
    {

        //the back of a circle, straight up and downs to steps

        Vector3 flatA = new Vector3(a.x, 0f, a.z);
        Vector3 flatB = new Vector3(b.x, 0f, b.z);
        Vector3 flatDiff = flatB - flatA;
        Vector3 fullDiff = b - a; //the 3d step located at the origin
        float radius = (b - a).magnitude / 2f;

        Vector3[] arc = new Vector3[points.Length - 2];

        Geometry.generatePointCircleBack(radius, ref arc);
        if (a.y > b.y)
        {
            translatePoints(ref arc, new Vector3(0f, -radius, 0f)); //move up so bottom is at b
        }
        else
        {
            reversePoints(ref arc);
            translatePoints(ref arc, new Vector3(0f, radius, 0f)); //move up so bottom is at a
        }

        translatePoints(ref arc, new Vector3(0f, radius / 2f, 0f)); //move up for step clearance

        points[0] = Vector3.zero;
        points[points.Length - 1] = (b - a);

        for (int i = 0; i < arc.Length; i++)
        {
            points[i + 1] = arc[i];
        }

        float radians = Geometry.angleBetweenVectors(new Vector2(0f, 1f), new Vector2(flatDiff.x, flatDiff.z));

        Vector3 oneWay = Geometry.rotateAroundYAxis(radians, new Vector3(0f, 0f, radius));
        Vector3 otherWay = Geometry.rotateAroundYAxis(-radians, new Vector3(0f, 0f, radius));

        /*
        if ((fullDiff - oneWay).magnitude < (fullDiff - otherWay).magnitude)
        {
            rotatePoints(ref points, new Vector3(0f, radians, 0f));
        }
        else
        {
            rotatePoints(ref points, new Vector3(0f, -radians, 0f));
        }
        */

        //last step, position at point a
        translatePoints(ref points, a);
    }
    public void generatePointCircle(float r, ref Vector3 [] points )
    {

        Geometry.generatePointCircle(r, ref points);

    }
    public void generateStepPoints(ref Vector3[] points, Vector3 a, Vector3 b)
    {

        Vector3 flatA = new Vector3(a.x, 0f, a.z);
        Vector3 flatB = new Vector3(b.x, 0f, b.z);
        Vector3 flatDiff = flatB - flatA;
        Vector3 fullDiff = b - a; //the 3d step located at the origin
        float radius = (flatB - flatA).magnitude / 2f;

        Geometry.generatePointCircleTop(radius, ref points);

        reversePoints(ref points);
        scalePoints(ref points, new Vector3(1f, 2f, 1f)); //stretch the height
        translatePoints(ref points, new Vector3(0f, 0f, radius)); //move so 1st point is on origin

        shearPointsAboutXAlongY(ref points, (b.y - a.y) / (2f * radius)); //deal with delta y

        float radians = Geometry.angleBetweenVectors(new Vector2(0f, 1f), new Vector2(flatDiff.x, flatDiff.z));

        Vector3 oneWay = Geometry.rotateAroundYAxis(radians, new Vector3(0f, 0f, radius));
        Vector3 otherWay = Geometry.rotateAroundYAxis(-radians, new Vector3(0f, 0f, radius));


        if ((fullDiff - oneWay).magnitude < (fullDiff - otherWay).magnitude)
        {
            rotatePoints(ref points, new Vector3(0f, radians, 0f));
        }
        else
        {
            rotatePoints(ref points, new Vector3(0f, -radians, 0f));
        }

        //last step, position at point a
        translatePoints(ref points, a);
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
}
