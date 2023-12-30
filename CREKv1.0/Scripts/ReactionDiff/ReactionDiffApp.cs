using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ReactionDiffApp : MonoBehaviour
{

    public InputField inputIF;

    public ReactionDiffShell shell;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void sendCmdToShell()
    {

        shell.sendCmdToMachine(inputIF.text);

        inputIF.text = "";
        //gives focus back to input field for uninterupted use
        //inputIF.Select();
        //inputIF.ActivateInputField();

    }
}