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
public class TopicMessageJSON
{
    public string topic;
    public string messagestring;
    public TopicMessageJSON(string t = "", string m = "") { topic = t; messagestring = m; }

}
[Serializable]
public class PRFrameJSON
{
    public Vector3 position;
    public Quaternion rotation;
    public PRFrameJSON() { position = Vector3.zero; rotation = Quaternion.identity; }
    public PRFrameJSON(Vector3 p, Quaternion r) { position = p; rotation = r; }

}

[Serializable]
public class PRFramesJSON
{
    public List<PRFrameJSON> frames;
    
    public PRFramesJSON() { frames = new List<PRFrameJSON>(); }

}


[Serializable]
public class GenericMessageJSON
{
    public string commandA;
    public string commandB;

    public float float00;
    public float float01;
    public float float02;
    public float float03;

    public float float04;
    public float float05;
    public float float06;
    public float float07;


    public GenericMessageJSON(string a = "", string b = "",
                        float f00 = 0f, float f01 = 0f, float f02 = 0f, float f03 = 0f,
                        float f04 = 0f, float f05 = 0f, float f06 = 0f, float f07 = 0f)
        {
            commandA = a; commandB = b;
            float00 = f00; float01 = f01; float02 = f02; float03 = f03;
            float04 = f04; float05 = f05; float06 = f06; float07 = f07;
        }

}

[Serializable]
public class IntPairJSON
{
    public int int1;
    public int int2;
    public IntPairJSON(int i = 0, int ii = 0) { int1 = i; int2 = ii; }

}
