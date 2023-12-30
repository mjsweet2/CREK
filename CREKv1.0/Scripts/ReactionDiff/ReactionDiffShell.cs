using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionDiffShell : MonoBehaviour
{

    public Transform subject;
    public ReactionDiffDrive reactionDiffDrive;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void sendCmdToMachine(string cmdText)
    {

        reactionDiffDrive.processCmd(cmdText);
    }
}
