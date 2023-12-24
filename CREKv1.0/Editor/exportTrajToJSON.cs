using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(Trajectory))]

public class exportTrajToJSON : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Trajectory myScript = (Trajectory)target;

        if (GUILayout.Button("Update Keys"))
        {
            myScript.updateKeyframesFromDevKeyframes();
        }
        if (GUILayout.Button("Export to JSON"))
        {
            myScript.exportToJSON();
        }
        if (GUILayout.Button("Export Retract to JSON"))
        {
            myScript.exportRetractToJSON();
        }
        if (GUILayout.Button("Import JSON"))
        {
            myScript.importFromJSON();
        }


    }

}

