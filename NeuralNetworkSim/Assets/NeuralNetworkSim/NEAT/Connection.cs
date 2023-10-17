using UnityEngine;

/*
 * Connection Class
 * Description : Connections used within genomes and the neural network, serialized to support JSON
*/
[System.Serializable]
public class Connection : InnovationNumber
{
    //Start node
    [SerializeField]
    Node start;
    //End node
    [SerializeField]
    Node end;
    //Enabled genome
    [SerializeField]
    bool enabled = true;
    //Current weight of connection
    [SerializeField]
    float weight;

    //Constructor
    public Connection(int iNumber, Node start, Node end, float weight)
    {
        SetInnovationNumber(iNumber);
        this.start = start;
        this.end = end;
        this.weight = weight;
    }

    //Get start node
    public Node GetStart()
    {
        return start;
    }

    //Get end node
    public Node GetEnd()
    {
        return end;
    }

    //Set is enabled
    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    //Is connection enabled
    public bool IsEnabled()
    {
        return enabled;
    }

    //Set connection weight
    public void SetWeight(float weight)
    {
        this.weight = weight;
    }

    //Get weight
    public float GetWeight()
    {
        return weight;
    }
}
