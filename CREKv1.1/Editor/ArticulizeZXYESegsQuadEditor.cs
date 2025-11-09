using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArticulizeZXYESegsQuad))]

public class ArticulizeZXYESegsQuadEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ArticulizeZXYESegsQuad myScript = (ArticulizeZXYESegsQuad)target;

        if (GUILayout.Button("Setup Char"))
        {
            myScript.doArticulize();
        }




    }

}
