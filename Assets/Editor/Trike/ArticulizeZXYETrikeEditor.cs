using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArticulizeZXYETrike))]

public class ArticulizeZXYETrikeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Start is called before the first frame update
        DrawDefaultInspector();

        ArticulizeZXYETrike myScript = (ArticulizeZXYETrike)target;

        if (GUILayout.Button("Setup Char"))
        {
            myScript.doArticulize();
        }

    }
}
