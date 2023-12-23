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
public class MouseInput
{
    public List<Vector3> poses;
    public MouseInput() { poses = new List<Vector3>(); }
}

[Serializable]
public class RSInput
{
    public List<Vector3> poses;
    public List<Quaternion> rots;
    public RSInput() { poses = new List<Vector3>(); rots = new List<Quaternion>(); }
}

public class InputDeck : MonoBehaviour
{

    public MSPoseController msPoseController;
    public RSPoseController rsPoseController;


    public MouseInput mouseInput;
    public RSInput rsInput;

    //for loading and playing jsons
    public string inputName;
    public int playbackIndex;
    public Vector3 currentMousePlayback;
    public Vector3 currentRSPositionPlayback;
    public Quaternion currentRSRotationPlayback;

    public enum DECKSTATE { STOP, RECORD, PLAY };
    public DECKSTATE deckState;


    // Start is called before the first frame update
    void Start()
    {
        mouseInput = new MouseInput();
        rsInput = new RSInput();
    }

    // Update is called once per frame
    void Update()
    {
        if (deckState == DECKSTATE.RECORD)
        {
            mouseInput.poses.Add(msPoseController.track);
            rsInput.poses.Add(rsPoseController.subject.position);
            rsInput.rots.Add(rsPoseController.subject.rotation);
        }


        if (deckState == DECKSTATE.PLAY)
        {
            
            if(playbackIndex >= mouseInput.poses.Count)
            {
                playbackIndex = 0;
            }

            currentMousePlayback = mouseInput.poses[playbackIndex];
            currentRSPositionPlayback = rsInput.poses[playbackIndex];
            currentRSRotationPlayback = rsInput.rots[playbackIndex];

            playbackIndex++;

        }


        processKeyboard();

    }
    public void processKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (deckState == DECKSTATE.STOP)
            {
                startRecord();
            }
            else
            {
                stopRecord();
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (deckState == DECKSTATE.STOP)
            {
                startPlay();
            }
            else
            {
                stopPlay();
            }
        }

    }
    public void startPlay()
    {
        if (deckState != DECKSTATE.STOP)
            return;

        if (mouseInput.poses.Count <= 0)
            return;


        deckState = DECKSTATE.PLAY;
        playbackIndex = 0;

    }
    public void stopPlay()
    {
        if (deckState != DECKSTATE.PLAY)
            return;

        deckState = DECKSTATE.STOP;

    }

    public void startRecord()
    {
        if (deckState != DECKSTATE.STOP)
            return;

        deckState = DECKSTATE.RECORD;

        mouseInput.poses.Clear();
        rsInput.poses.Clear();
        rsInput.rots.Clear();
    }
    public void stopRecord()
    {
        if (deckState != DECKSTATE.RECORD)
            return;

        deckState = DECKSTATE.STOP;

        DateTime rightNow = System.DateTime.Now;
        string seperator = ".";
        string timeString = rightNow.Hour.ToString() + seperator + rightNow.Minute.ToString() + seperator + rightNow.Second.ToString();
        string path = "c:\\sites\\inputrecord\\";

        System.Object theM = mouseInput;
        System.Object theRS = rsInput;

        exportJSON(path + "mouseinput." + timeString + ".json", ref theM);
        exportJSON(path + "rsinput." + timeString + ".json", ref theRS);

    }

    void exportJSON(string fullpath, ref System.Object theObject)
    {
        StreamWriter writer = new StreamWriter(fullpath);

        string filestring = JsonUtility.ToJson(theObject, true);
        writer.Write(filestring);
        writer.Close();
    }
    void exportMouseJSON(string fullpath, ref MouseInput theJSON)
    {
        //blShapeList = JsonUtility.FromJson<BLShapeList>(fileString);
        //filestring = JsonUtility.ToJson<TransformTrack>(obj);

        StreamWriter writer = new StreamWriter(fullpath);

        string filestring = JsonUtility.ToJson(theJSON, true);
        writer.Write(filestring);
        writer.Close();

    }
    void exportRSJSON(string fullpath, ref RSInput theJSON)
    {
        //blShapeList = JsonUtility.FromJson<BLShapeList>(fileString);
        //filestring = JsonUtility.ToJson<TransformTrack>(obj);

        StreamWriter writer = new StreamWriter(fullpath);

        string filestring = JsonUtility.ToJson(theJSON, true);
        writer.Write(filestring);
        writer.Close();

    }


}
