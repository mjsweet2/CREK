/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FPSController : MonoBehaviour
{

    public int targetFPS;
    public float actualFPS;
    public InputField fpsIF;
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = targetFPS;
    }

    // Update is called once per frame
    void Update()
    {
        actualFPS = (1f / Time.deltaTime);
        if (fpsIF != null)
            fpsIF.text = actualFPS.ToString();
        
    }
}
