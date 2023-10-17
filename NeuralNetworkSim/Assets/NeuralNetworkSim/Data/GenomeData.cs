using System.Collections.Generic;
using UnityEngine;

/*
 * GenomeData
 * Description : Used for JSON for saving and loading, Class is serialized to support JSON
*/
[System.Serializable]
public class GenomeData
{
    //Fitness score
    [SerializeField]
    public float fitnessScore = 0;

    //Previous fitness score
    [SerializeField]
    public float previousFitness = 0;

    //Nodes of the genome
    [SerializeField]
    public List<Node> nodes = new List<Node>();

    //Connections of the genome
    [SerializeField]
    public List<Connection> connections = new List<Connection>();

    //Colour of the genome
    [SerializeField]
    public Color color;
    
    //Size of the genome
    [SerializeField]
    public float size;

    //Rank of the genome
    [SerializeField]
    public int rank;
}
