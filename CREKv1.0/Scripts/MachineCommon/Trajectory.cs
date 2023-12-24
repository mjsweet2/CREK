/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

[Serializable]
public class KeyFrameJSON
{
    public string name;
    public Vector3 point;
    public KeyFrameJSON(string n = "raw", Vector3 p = new Vector3()) { name = n; point = p; }

}

[Serializable]
public class TrajJSON
{
    public int resolution;
    public List<KeyFrameJSON> keyframes;
    public TrajJSON() { keyframes = new List<KeyFrameJSON>(); }
}

public class Trajectory : MonoBehaviour
{

    public string trajectoryName;
    public List<Transform> devKeyFrames;
    
    public List<Vector3> keyFrames;
    public List<string> namedKeyFrames; // points that are 3d and not "named" are named "raw"

    //build table from keyFrames to interpolate
    public List<float> distances;
    public List<float> percentages;

    public float totalDistance;

    public int prevKeyFrameIndex;

    public int interpolateIndex;
    public int interpolateResolution; // this is how many frames that this Traj takes. It directly effects speed

    public float motionWideMag;
    public float runningMag;
    public float insideMag; // how much of current keyframe to use
    public bool isComplete;
    public bool isLooping;
    

   

    public string customSubDirectory;

    // Start is called before the first frame update
    void Start()
    {
    
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void resetTrajectory(int resolution)
    {
        isComplete = false;
        interpolateIndex = 0;
        interpolateResolution = resolution;
        prevKeyFrameIndex = 0;
        runningMag = 0f;

    }
    public void resetTrajectory()
    {
        isComplete = false;
        interpolateIndex = 0;

        prevKeyFrameIndex = 0;
        runningMag = 0f;


    }

    //
    public void initFromJSONString(TrajJSON json)
    {
        interpolateResolution = json.resolution;
        if (keyFrames != null)
            keyFrames.Clear();
        else
            keyFrames = new List<Vector3>();

        for (int i = 0; i < json.keyframes.Count; i++)
        {
            keyFrames.Add(json.keyframes[i].point);
        }
    }
    public void importFromJSON()
    {
        StreamReader sReader;
        string path = Application.persistentDataPath + "/trajectory/" + trajectoryName + ".json";
        sReader = new StreamReader(path);
        string fileString = sReader.ReadToEnd();

        
        TrajJSON myTrajJSON = JsonUtility.FromJson<TrajJSON>(fileString);

        interpolateResolution = myTrajJSON.resolution;
        if (keyFrames != null)
            keyFrames.Clear();
        else
            keyFrames = new List<Vector3>();

        for(int i = 0; i < myTrajJSON.keyframes.Count; i++)
        {
            keyFrames.Add(myTrajJSON.keyframes[i].point);

        }

        sReader.Close();
    }
    public void importFromJSON(string fullPath)
    {
        StreamReader sReader;
        sReader = new StreamReader(fullPath);
        string fileString = sReader.ReadToEnd();


        TrajJSON myTrajJSON = JsonUtility.FromJson<TrajJSON>(fileString);

        interpolateResolution = myTrajJSON.resolution;
        if (keyFrames != null)
            keyFrames.Clear();
        else
            keyFrames = new List<Vector3>();

        for (int i = 0; i < myTrajJSON.keyframes.Count; i++)
        {
            keyFrames.Add(myTrajJSON.keyframes[i].point);

        }

        sReader.Close();
    }
    public void exportRetractToJSON()
    {
        Debug.Log(gameObject.name);

        TrajJSON thisTraj = new TrajJSON();
        thisTraj.resolution = interpolateResolution;
        for (int i = keyFrames.Count - 1; i >= 0; i--)
        {
            thisTraj.keyframes.Add(new KeyFrameJSON("raw",keyFrames[i]));
        }

        //second parameter is to print prettey
        string jsonString = JsonUtility.ToJson(thisTraj, true);

        Debug.Log(jsonString);

        //example
        //TrajJSON myObject = JsonUtility.FromJson<TrajJSON>(jsonString);


        StreamWriter sWriter;
        string path = Application.persistentDataPath + "/trajectory/" + gameObject.name + "Retract" + ".json";

        sWriter = new StreamWriter(path);
        sWriter.Write(jsonString);
        sWriter.Close();

    }
    public void exportToJSON()
    {

        Debug.Log(gameObject.name);

        TrajJSON thisTraj = new TrajJSON();
        thisTraj.resolution = interpolateResolution;
        for(int i = 0; i < keyFrames.Count; i++)
        {
            thisTraj.keyframes.Add(new KeyFrameJSON("raw", keyFrames[i]));
        }
        
        //second parameter is to print prettey
        string jsonString = JsonUtility.ToJson(thisTraj, true);

        Debug.Log(jsonString);

        //example
        //TrajJSON myObject = JsonUtility.FromJson<TrajJSON>(jsonString);


        StreamWriter sWriter;
        string path = Application.persistentDataPath + "/trajectory/" + gameObject.name + ".json";
        sWriter = new StreamWriter(path);
        sWriter.Write(jsonString);
        sWriter.Close();

    }
    public bool isAPause()
    {
        return (keyFrames.Count == 0);
    }
    public Vector3 getFirstFrame()
    {
        Vector3 ret = Vector3.zero;
        if (keyFrames.Count > 0)
            ret = keyFrames[0];
        return ret;
    }

    public Vector3 getNextFrame()
    {
        //this is a pause Traj, increment the counting variables, and send a throw out variable
        if(keyFrames.Count == 0)
        {
            if (interpolateIndex > interpolateResolution)
            {
                isComplete = true;
                return Vector3.zero;
                
            }

            interpolateIndex++;

            return Vector3.zero;
        }

        //increment through the entire motion
        if (interpolateIndex > interpolateResolution)
        {
            isComplete = true;
            //return last position
            return keyFrames[keyFrames.Count - 1];
        }

        motionWideMag = Geometry.getSinMag(interpolateIndex, interpolateResolution);

        interpolateIndex++;


        //calculate which keyframe based on what our current increment
        //TODO, does this account for when you have more keyframes than interpolateResolution?
        if (motionWideMag > runningMag + percentages[prevKeyFrameIndex])
        {
            runningMag += percentages[prevKeyFrameIndex];
            prevKeyFrameIndex++;

            if (prevKeyFrameIndex > keyFrames.Count - 2)
                prevKeyFrameIndex = keyFrames.Count - 2;

            if (runningMag >= 1f)
                runningMag = 1f - percentages[percentages.Count - 1];

        }

        //calculate what portion of our current keyframe in relation to entire magnitude
        insideMag = (motionWideMag - runningMag) / percentages[prevKeyFrameIndex];

           

        return Vector3.Lerp(keyFrames[prevKeyFrameIndex], keyFrames[prevKeyFrameIndex + 1], insideMag);

    }



    //There are n keyframes, and n-1 spaces between keyframes
    //if you want to create a closed loop, your list has to have the 
    //starting point in the first and last entry in the list
    //

    public void updateKeyframesFromDevKeyframes()
    {
        if (keyFrames == null)
            keyFrames = new List<Vector3>();
        else
            keyFrames.Clear();

        for( int i = 0; i < devKeyFrames.Count; i++)
        {
            keyFrames.Add(devKeyFrames[i].position);
        }


    }
    public void buildMotionTable()
    {
        //distances.Clear();
        //percentages.Clear();

        //There are n keyframes, and n-1 spaces between keyframes
        for (int i = 0; i < keyFrames.Count - 1; i++)
        {
            distances.Add((keyFrames[i] - keyFrames[i + 1]).magnitude);
        }



        for (int i = 0; i < distances.Count; i++)
        {
            totalDistance += distances[i];
        }

        for (int i = 0; i < distances.Count; i++)
        {
            percentages.Add(distances[i] / totalDistance);
        }

    }


}
