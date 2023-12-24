using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArticulizeBalTrike))]

public class ArticulizeBalTrikeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Start is called before the first frame update
        DrawDefaultInspector();

        ArticulizeBalTrike myScript = (ArticulizeBalTrike)target;

        if (GUILayout.Button("Setup Char"))
        {
            myScript.doArticulize();
        }

    }
}
