using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhysicalizeComp))]
public class PhysicalizeCompEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PhysicalizeComp myScript = (PhysicalizeComp)target;

        if (GUILayout.Button("Setup Char"))
        {
            myScript.doPhysicalize();
        }



    }
}
