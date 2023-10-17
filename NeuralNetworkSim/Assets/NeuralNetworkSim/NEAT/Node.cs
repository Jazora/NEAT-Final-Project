using System;
using UnityEngine;

/*
 * ActivationFunctionType Enum
 * Description : The different type of activation functions
*/
[Serializable]
public enum ActivationFunctionType {
    rectlinear,
    step,
    sigmoid,
    linear,
    tanh,
    signum,
    abs
}

/*
 * Node Class
 * Description : Node that are contained within genomes and the neural network
*/
[Serializable]
public class Node : InnovationNumber
{
    //X position used within NEATGraph
    [SerializeField]
    float x;
    //Y position used within NEATGraph
    [SerializeField]
    float y;

    //Current activation function
    [SerializeField]
    ActivationFunctionType aFunction = ActivationFunctionType.linear;

    //Constructor
    public Node(int iNumber)
    {
        SetInnovationNumber(iNumber);
    }

    //Get X position
    public float GetX()
    {
        return x;
    }

    //Set X position
    public void SetX(float x)
    {
        this.x = x;
    }

    //Get Y position
    public float GetY()
    {
        return y;
    }

    //Set Y position
    public void SetY(float y)
    {
        this.y = y;
    }

    //Get Activation function type
    public ActivationFunctionType GetActivationFunction()
    {
        return aFunction;
    }

    //Set activation function type
    public void SetActivationFunction(ActivationFunctionType aFunctionType)
    {
        aFunction = aFunctionType;
    }

    //Get the output of the current node
    public float GetOutput(float value)
    {
        //Depending on the activation function produce a alter the output
        switch (aFunction)
        {
            case ActivationFunctionType.tanh:
                value = MathF.Tanh(value);
                value = Mathf.Clamp(value, -1, 1);
                break;
            case ActivationFunctionType.sigmoid:
                value = 1.0f / (1.0f + Mathf.Exp(-value));
                value = Mathf.Clamp(value, 0, 1);
                break;
            case ActivationFunctionType.step:
                if (value >= 0)
                {
                    value = 1;
                }
                else
                {
                    value = 0;
                }
                break;
            case ActivationFunctionType.signum:
                if (value > 0)
                {
                    value = 1;
                }
                else if (value == 0)
                {
                    value = 0;
                }
                else if (value < 0)
                {
                    value = -1;
                }
                break;
            case ActivationFunctionType.abs:
                value = MathF.Abs(value);
                break;
            case ActivationFunctionType.rectlinear:
                value = MathF.Max(0, value);
                break;
        }

        return value;
    }
}
