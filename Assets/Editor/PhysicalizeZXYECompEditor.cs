using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhysicalizeZXYEComp))]

public class PhysicalizeZXYECompEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PhysicalizeZXYEComp myScript = (PhysicalizeZXYEComp)target;

        if (GUILayout.Button("Setup Char"))
        {
            myScript.doPhysicalize();
        }



    }
}
