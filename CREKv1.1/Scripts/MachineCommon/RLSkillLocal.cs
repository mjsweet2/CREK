/* Copyright (C) 2025 Michael Sweet - All Rights Reserved
 * mjcsweet2@outlook.com
 * You may use, distribute and modify this code under the terms of the MIT License.
 * You should have received a copy of the MIT License with this file.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using MathNet.Numerics.Distributions;


using Unity.Barracuda;

/*  This local version works, but I'm trying to use RL models non-locally,
 *  through network connection or mqtt
 * 
 * 
 */

// https://medium.com/@a.abelhopereira/how-to-use-pytorch-models-in-unity-aa1e964d3374
// https://docs.unity3d.com/Packages/com.unity.barracuda@2.0/manual/ModelOutput.html

public class RLSkillLocal : MonoBehaviour
{
    public NNModel modelSource;

    public Model model;
    public IWorker worker;

    public Tensor inputTensor;
    public Tensor outputY;
    public Tensor output27;

    public int actionCount;
    public double[] outActions;
    public float actionLow; // for mapping normalized output to action space
    public float actionHigh; // for mapping normalized output to action space


    // Start is called before the first frame update
    void Start()
    {
        model = ModelLoader.Load(modelSource);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);

        outActions = new double[actionCount];

    }

    // Update is called once per frame
    void Update()
    {
        // I want to call this from machine, I'll pass in the input
        
        //doInference();
    }
    public void doInference(float [] i, ref double[] o)
    {
        //In the C# code, you will need to load the model, create a worker to execute the forward pass
        //and feed it a Barracuda input tensor by running the Execute method. PeekOutput() can then be used
        //to get your output. Don’t forget to dispose all tensors and workers
        //as Unity’s garbage collector does not do it automatically.
        
        inputTensor = new Tensor(1, 12, i);
        worker.Execute(inputTensor);

        outputY = worker.PeekOutput("Y");
        output27 = worker.PeekOutput("27");

        // Debug.Log("outputY: " + outputY.ToString());
        // Debug.Log("output27: " + output27.ToString());
        // outputY.shape.channels; //this should match actionCount

        // sample = MathNet.Numerics.Distributions.Normal.Samples(mean, sd).Take(n);

        Normal normal = new Normal(0f,1f);
        
        double mean = 0d, std = 0d, z = 0d;
        for (int index = 0; index < actionCount; index++)
        {
            // generate the actions from the inference output
            mean = outputY[index];
            std = Mathf.Exp(output27[index]);

            z = normal.Sample();
            outActions[index] = System.Math.Tanh(mean + (z * std));

            // action = self.action_low + (action + 1.0) * 0.5 * (self.action_high - self.action_low)
            // action = np.clip(action, self.action_low, self.action_high)
            outActions[index] = actionLow + (outActions[index] + 1.0) * .5 * (actionHigh - actionLow);
            outActions[index] = Mathf.Clamp((float)outActions[index], actionLow, actionHigh);
            o[index] = outActions[index];

        }

        //Destroy these each frame?
        inputTensor.Dispose();


    }
    void doInference()
    {
    //In the C# code, you will need to load the model, create a worker to execute the forward pass
    //and feed it a Barracuda input tensor by running the Execute method. PeekOutput() can then be used
    //to get your output. Don’t forget to dispose all tensors and workers
    //as Unity’s garbage collector does not do it automatically.
        

        float[] floatArray = { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
        inputTensor = new Tensor(1, 12, floatArray);
        worker.Execute(inputTensor);

        output27 = worker.PeekOutput("27");
        outputY = worker.PeekOutput("Y");
        Debug.Log("output27: " + output27.ToString());
        Debug.Log("outputY: " + outputY.ToString());


        //Destroy these each frame?
        inputTensor.Dispose();
        output27.Dispose();
        outputY.Dispose();
        

    }
    void OnDestroy()
    {

        output27.Dispose();
        outputY.Dispose();

        worker.Dispose();
        //Debug.Log("Destroying Skill");

    }
}
