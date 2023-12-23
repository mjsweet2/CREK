
/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentXZ : MonoBehaviour
{


    public string compName; // self.name = "base_class"     # for logging
    public Vector3 lastValidIk;   // self.last_valid_ik = [0.0,0.0,0.0] # same as current # The Component is storing state, so I need a seperate component for each
    public Vector3 lastValidFk;   // self.last_valid_fk =  [0.0,0.0,0.0]
    public bool isMotioning;     // self.is_motioning = False # is this component running a component trajectory 
    public float tolerance;         //this is the max a request can be off before I return true


    Geometry geo;
    public float upper;
    // Start is called before the first frame update
    void Start()
    {
        geo = new Geometry();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Vector3 valIk()
    {
        return lastValidIk;
    }
    public Vector3 valFk()
    {
        return lastValidFk;
    }
    public void loadParams(float u)
    {
        upper = u;
        lastValidFk = new Vector3(0, -upper, 0);
        tolerance = .01f;   // 1 cm.
    }
    public Geometry.GeoPacket doLeftIk(float x, float y, float z)
    {
        Vector3 ik = Vector3.zero;


        float dist_to_requested = Mathf.Sqrt((x * x) + (y * y) + (z * z));


        if (Mathf.Abs(upper - dist_to_requested) > tolerance) {
            return new Geometry.GeoPacket(false, valIk());
        }

        ik.z = 0.0f; // no z actuation on XZ component

        ik.y = Mathf.Atan(x / y) / (2 * Mathf.PI);

        ik.x = Mathf.Atan(z / y) / (2 * Mathf.PI);
        ik.x = -ik.x;

        // save to send in case of bad ik.
        lastValidIk = ik;
        lastValidFk = new Vector3(x, y, z);
        
        return new Geometry.GeoPacket(true, valIk());

    }

    public Geometry.GeoPacket doRightIk(float x, float y, float z)
    {
        Vector3 ik = Vector3.zero;


        float dist_to_requested = Mathf.Sqrt((x * x) + (y * y) + (z * z));


        if (Mathf.Abs(upper - dist_to_requested) > tolerance) {
            return new Geometry.GeoPacket(false, valIk());
        }

        ik.z = 0.0f;   // no z actuaion on XZ component

        ik.y = Mathf.Atan(x / y) / (2 * Mathf.PI);

        ik.x = Mathf.Atan(z / y) / (2 * Mathf.PI);


        // save to send in case of bad ik.
        lastValidIk = ik;
        lastValidFk = new Vector3(x, y, z);
        return new Geometry.GeoPacket(true, valIk());

    }

   


}
