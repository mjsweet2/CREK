using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class BalTrikeApp : MonoBehaviour
{

    public Canvas inputCanvas;
    public int canvasIndex;
    public int canvasCount;

    public InputField inputIF;


    public GameObject stand;

    public BalTrikeShell balTrikeShell;
    public MachineBalTrike machineBalTrike;
    public ArticulatedBalTrike articulatedBalTrike;
    public CRMotionDBController db;

    public Transform[] spawnPoints;
    public int spawnIndex;

    public UDPSocketClient remotePhys;

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
        machineBalTrike.gameObject.SetActive(false);
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
        machineBalTrike.rsPoseController.connectUDPClient();
    }
    public void runCmd()
    {

        balTrikeShell.runCmd(inputIF.text);
        inputIF.text = "";



        //gives focus back to input field for uninterupted use
        //doesn't work quite right?
        //inputIF.Select();
        //inputIF.ActivateInputField();


    }

    public void connectRemotePhysUDPClient()
    {

        remotePhys.startNetwork();

    }
}
