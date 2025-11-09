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




public class InputDeck : MonoBehaviour
{

    public SixDofPoseController sixDofPoseControllerA;
    public SixDofPoseController sixDofPoseControllerB;

    public PRFrameJSON currentFrameA;
    public PRFrameJSON currentFrameB;

    public PRFramesJSON rsFramesA;
    public PRFramesJSON rsFramesB;

    //for loading and playing jsons
    public string inputName;
    public int playbackIndex;
    //public Vector3 currentPosAPlayBack;
    //public Vector3 currentPosBPlayBack;
    //public Quaternion currentRotAPlayBack;
    //public Quaternion currentRotBPlayBack;

    public enum DECKSTATE { STOP, RECORD, PLAY };
    public DECKSTATE deckState;


    // Start is called before the first frame update
    void Start()
    {
        currentFrameA = new PRFrameJSON();
        currentFrameB = new PRFrameJSON();

        rsFramesA = new PRFramesJSON();
        rsFramesB = new PRFramesJSON();
    }

    // Update is called once per frame
    void Update()
    {
        if (deckState == DECKSTATE.RECORD)
        {
            currentFrameA.position = sixDofPoseControllerA.subjectPos;
            currentFrameA.rotation = sixDofPoseControllerA.subjectRot;
            currentFrameB.position = sixDofPoseControllerB.subjectPos;
            currentFrameB.rotation = sixDofPoseControllerB.subjectRot;

            rsFramesA.frames.Add(new PRFrameJSON(sixDofPoseControllerA.subjectPos, sixDofPoseControllerA.subjectRot));
            rsFramesB.frames.Add(new PRFrameJSON(sixDofPoseControllerB.subjectPos, sixDofPoseControllerB.subjectRot));
        }


        if (deckState == DECKSTATE.PLAY)
        {
            
            if(playbackIndex >= rsFramesA.frames.Count)
            {
                playbackIndex = 0;
            }

            currentFrameA = rsFramesA.frames[playbackIndex];
            currentFrameB = rsFramesB.frames[playbackIndex];
           

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

        if (rsFramesA.frames.Count <= 0)
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

        rsFramesA.frames.Clear();
        rsFramesB.frames.Clear();
      
    }
    public void stopRecord()
    {
        if (deckState != DECKSTATE.RECORD)
            return;

        deckState = DECKSTATE.STOP;

        DateTime rightNow = System.DateTime.Now;
        string seperator = ".";
        string timeString = rightNow.Hour.ToString() + seperator + rightNow.Minute.ToString() + seperator + rightNow.Second.ToString();
        string path = "c:\\crek\\inputrecord\\";

        System.Object theA = rsFramesA;
        System.Object theB = rsFramesB;

        exportJSON(path + "rsframesa." + timeString + ".json", ref theA);
        exportJSON(path + "rsframesb." + timeString + ".json", ref theB);

    }

    void exportJSON(string fullpath, ref System.Object theObject)
    {
        StreamWriter writer = new StreamWriter(fullpath);

        string filestring = JsonUtility.ToJson(theObject, true);
        writer.Write(filestring);
        writer.Close();
    }

    void exportRSFramesJSON(string fullpath, ref PRFramesJSON theJSON)
    {
        //blShapeList = JsonUtility.FromJson<BLShapeList>(fileString);
        //filestring = JsonUtility.ToJson<TransformTrack>(obj);

        StreamWriter writer = new StreamWriter(fullpath);

        string filestring = JsonUtility.ToJson(theJSON, true);
        writer.Write(filestring);
        writer.Close();

    }


}
