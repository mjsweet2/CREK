using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CustomEditor(typeof(Articulize4ZXYE))]
public class ArticulizeZXYEQuadEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Articulize4ZXYE myScript = (Articulize4ZXYE)target;

        if (GUILayout.Button("Setup Char"))
        {
            myScript.doArticulize();
        }




    }

}
