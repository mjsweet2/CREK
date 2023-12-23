using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PhysicalizeZXYKComp))]

public class PhysicalizeZXYKCompEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PhysicalizeZXYKComp myScript = (PhysicalizeZXYKComp)target;

        if (GUILayout.Button("Setup Char"))
        {
            myScript.doPhysicalize();
        }



    }
}
