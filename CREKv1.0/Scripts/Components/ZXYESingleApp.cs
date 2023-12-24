/* Copyright (C) 2023 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the GNU General Public License v3.0.
 * You should have received a copy of the GNU General Public License v3.0 license with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZXYESingleApp : MonoBehaviour
{

    public InputField outputIF;
    public InputField inputIF;


    public ArticulatedZXYE articulatedZXYELeft;
    public ArticulatedZXYE articulatedZXYERight;

    public ComponentZXYE componentZXYELeft;
    public ComponentZXYE componentZXYERight;


    public bool doLeft, doRight;
    public float scaler;

    public Transform test01, test02, test03;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (doLeft)
        {
            runDirCalcTestOnLeft();
        }
        else
        {
            if (doRight)
            {
                runDirCalcTestOnRight();
            }
        }

    }

    void runDirCalcTestOnLeft()
    {
        test01.position = componentZXYELeft.calcLeftLDirOnIks() * scaler;
        test02.position = componentZXYELeft.calcLeftLDirESpaceOnIks() * scaler;
        test03.position = componentZXYELeft.calcLeftUDirOnIks() * scaler;


    }
    void runDirCalcTestOnRight()
    {
        test01.position = componentZXYERight.calcRightLDirOnIks() * scaler;
        test02.position = componentZXYERight.calcRightLDirESpaceOnIks() * scaler;
        test03.position = componentZXYERight.calcRightUDirOnIks() * scaler;


    }







}