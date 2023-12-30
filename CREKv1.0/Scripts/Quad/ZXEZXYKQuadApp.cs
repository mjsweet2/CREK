/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZXEZXYKQuadApp : MonoBehaviour
{

    public Canvas inputCanvas;
    public int canvasIndex;
    public int canvasCount;

    public InputField inputIF;


    public GameObject stand;

    public ZXEZXYKQuadShell zxezxykQuadShell;
    public Machine2ZXE2ZXYK machine2ZXE2ZXYK;
    public Articulated2ZXE2ZXYK articulated2ZXE2ZXYK;
    public CRMotionDBController db;

    public Transform[] spawnPoints;
    public int spawnIndex;


    public Motion testMotion;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void testButton()
    {
        string ret;
        ret = db.getInputStringByMotion("forback03rf").ToString();
        Debug.Log("test: " + ret);
    }
    public void hideStand()
    {
        stand.SetActive(false);

    }

    //runtime positioning doesn't work on physics objects
    public void toggleSpawn()
    {
        machine2ZXE2ZXYK.gameObject.SetActive(false);
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
    public void connectPoseController()
    {
        machine2ZXE2ZXYK.rsPoseController.connectUDPClient();
    }
    public void runCmd()
    {

        zxezxykQuadShell.runCmd(inputIF.text);
        inputIF.text = "";



        //gives focus back to input field for uninterupted use
        //doesn't work quite right?
        //inputIF.Select();
        //inputIF.ActivateInputField();


    }
}
