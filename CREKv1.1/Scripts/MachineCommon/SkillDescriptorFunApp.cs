/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SkillDescriptorFunApp : MonoBehaviour
{
    public InputField nameIF;
    public RLSkillRig rlSkillRig;
    public string descName;
    // Start is called before the first frame update
    void Start()
    {


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void importJSON()
    {
        descName = nameIF.text;

        if (descName == "") 
            return;
        rlSkillRig.importFromJSON(descName);
    }

    public void exportJSON()
    {
        descName = nameIF.text;

        if (descName == "")
            return;
        rlSkillRig.exportToJSON(descName);
    }
}
