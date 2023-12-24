using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MoveArticulated))]
public class MoveArticulatedEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MoveArticulated myScript = (MoveArticulated)target;

        if (GUILayout.Button("setA"))
        {
            myScript.setTargetA();
        }
        if (GUILayout.Button("setB"))
        {
            myScript.setTargetB();
        }
        if (GUILayout.Button("setC"))
        {
            myScript.setTargetC();
        }
    }
}
