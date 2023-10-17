using System.Collections.Generic;
using UnityEngine;

/*
 * NeuralNetworkData
 * Description : Used for JSON for saving and loading, Class is serialized to support JSON
*/
[System.Serializable]
public class NeuralNetworkData
{
    //Max nodes supported
    [SerializeField]
    public int maxNodes = 100;

    //Input size of genomes
    [SerializeField]
    public int inputSize = 0;

    //Output size of genomes
    [SerializeField]
    public int outputSize = 0;

    //List of node names
    [SerializeField]
    public string[] nodeNames;

    //Innovation numbers of stored nodes
    [SerializeField]
    public List<int> nodeKeys = new List<int>();

    //Nodes stored
    [SerializeField]
    public List<Node> nodes = new List<Node>();

    //Innovation numbers of stored connections
    [SerializeField]
    public List<int> connectionKeys = new List<int>();

    //Connections stored
    [SerializeField]
    public List<Connection> connections = new List<Connection>();

    //Current generation
    [SerializeField]
    public int generationCount;

    //Amount of species stored
    [SerializeField]
    public int speciesCount;
}
