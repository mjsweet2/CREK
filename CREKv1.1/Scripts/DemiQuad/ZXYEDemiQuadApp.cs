/* Copyright (C) 2024 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZXYEDemiQuadApp : MonoBehaviour
{

    public Canvas inputCanvas;
    public int canvasIndex;
    public int canvasCount;

    public Canvas shellOutputCanvas;
    public Text shellOutputText;

    public InputField inputIF;


    public GameObject stand;
    public GameObject stand02;

    public MachineZXYEDemiQuadShell shell;
    public Machine2ZXYE2ZX machine;
    public ArticulatedZXYESegsDemiQuad articulated;

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
        if( stand != null)
        {
            stand.SetActive(false);
        }
        

        if (stand02 != null)
        {
            stand02.SetActive(false);
        }

    }

    //runtime positioning doesn't work on physics objects
    public void toggleSpawn()
    {
        machine.gameObject.SetActive(false);
        //articulated4ZXE.gameObject.transform.position = spawnPoints[spawnIndex].position;
        //machine4ZXE.gameObject.SetActive(true);

        spawnIndex++;
        if (spawnIndex >= spawnPoints.Length)
            spawnIndex = 0;
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

    public void runCmdOnEnter()
    {
        


            runCmd();
        
    }
    public void runCmd()
    {

        shell.runCmd(inputIF.text);
        inputIF.text = "";

        string line = "";
        shellOutputText.text = "";


        for (int i = 0; i < shell.cmdLexer.errors.Count; i++)
        {
            line = shell.cmdLexer.errors[i] + '\n';
            shellOutputText.text += line;
        }
        for (int i = 0; i < shell.cmdParser.errors.Count; i++)
        {
            line = "parse: " + shell.cmdParser.errors[i] + '\n';
            shellOutputText.text += line;
        }



        //gives focus back to input field for uninterupted use
        //doesn't work quite right?
        //inputIF.Select();
        inputIF.ActivateInputField();


    }
}
