/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class RobotChairApp : MonoBehaviour
{

    public Canvas inputCanvas;
    public int canvasIndex;
    public int canvasCount;

    public InputField inputIF;


    public GameObject stand;

    public MachineBalDiffShell shell;
    public MachineBalDiff machineBalDiff;
    public ArticulatedBalDiff articulatedBalDiff;

    public Transform[] spawnPoints;
    public int spawnIndex;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
   
    public void hideStand()
    {
        stand.SetActive(false);

    }

  

    public void toggleCanvas()
    {

        if (canvasIndex == 0)
        {
            inputCanvas.enabled = true;
        }
        else if (canvasIndex == 1)
        {
            inputCanvas.enabled = false;
        }

        canvasIndex++;
        if (canvasIndex > canvasCount)
            canvasIndex = 0;

    }
    public void connectPoseController()
    {
        machineBalDiff.rsPoseController.connectUDPClient();
    }
    public void runCmd()
    {

        shell.runCmd(inputIF.text);
        inputIF.text = "";



        //gives focus back to input field for uninterupted use
        //doesn't work quite right?
        //inputIF.Select();
        //inputIF.ActivateInputField();


    }
}

