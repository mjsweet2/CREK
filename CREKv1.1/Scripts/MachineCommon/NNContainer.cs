/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the MIT License.
 * You should have received a copy of the MIT License with this file.
 */
using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

public class NNContainer : MonoBehaviour
{
    public NNModel modelSource;

    // Start is called before the first frame update
    void Start()
    {
        var model = ModelLoader.Load(modelSource);
        var worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);

        var inputTensor = new Tensor(1, 2, new float[2] { 0, 0 });
        worker.Execute(inputTensor);

        var output = worker.PeekOutput();
        print("This is the output: " + (output[0] < 0.5 ? 0 : 1));

        inputTensor.Dispose();
        output.Dispose();
        worker.Dispose();
    }
}
