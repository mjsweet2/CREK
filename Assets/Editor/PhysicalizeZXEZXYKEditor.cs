using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhysicalizeZXEZXYKChar))]

public class PhysicalizeZXEZXYKEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PhysicalizeZXEZXYKChar myScript = (PhysicalizeZXEZXYKChar)target;

        if (GUILayout.Button("Setup Char"))
        {
            myScript.doPhysicalize();
        }




    }

}
