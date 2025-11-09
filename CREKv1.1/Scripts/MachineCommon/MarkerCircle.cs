/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the MIT License.
 * You should have received a copy of the MIT License with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerCircle : MonoBehaviour
{

    public Vector3 center;
    public float radius;

    LineRenderer lineRenderer;
    public Material circleMaterial;


    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = circleMaterial;// new Material(Shader.Find("Particles/Additive"));


    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void drawCircle()
    {

        float theta_scale = 0.01f;        //Set lower to add more points
        int size; //Total number of points in circle
        float sizeValue = (2.0f * Mathf.PI) / theta_scale;
        size = (int)sizeValue + 1;

        lineRenderer.startWidth = .001f; //thickness of line
        lineRenderer.endWidth = .001f; //thickness of line
        lineRenderer.positionCount = size;


        Vector3 pos;
        float theta = 0f;
        for (int i = 0; i < size; i++)
        {
            theta += (2.0f * Mathf.PI * theta_scale);
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            x += gameObject.transform.position.x;
            y += gameObject.transform.position.y;
            pos = new Vector3(x, y, 0) + center;
            lineRenderer.SetPosition(i, pos);
        }


    }
}
