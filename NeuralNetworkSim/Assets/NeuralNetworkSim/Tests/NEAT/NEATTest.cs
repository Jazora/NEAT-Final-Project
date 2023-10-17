using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
 * NEATTest Class
 * Description : Integration testing for NEAT
*/
public class NEATTest : MonoBehaviour
{
    //Reference to neural network
    NeuralNetwork nNetwork;

    //List of results compiled
    List<string> results = new List<string>();

    //When the simulation starts
    void Start()
    {
        nNetwork = NeuralNetwork.instance;
        //Start integration testing
        IntegrationTests();

        //Produce test data
        System.DateTime time = System.DateTime.Now;
        string fileName = time.Day + "," + time.Month + "," + time.Year + "-" + time.Hour + "." + time.Minute + ".txt";
        Simulation.CreateFile(GetFilePath(), fileName);
        File.WriteAllLines(GetFilePath() + fileName, results);
    }

    //Get the file path to the tests folder
    string GetFilePath()
    {
        return Simulation.GetDataFilePath() + "/Tests/";
    }

    //Test function
    void Test(string text, bool pass)
    {
        text = text + " : " + pass.ToString();
        Debug.Log(text);
        results.Add(text);
    }

    void IntegrationTests()
    {
        //Test for neural network creation
        Test("Neural Network Created", nNetwork != null);

        //Test creating a node within the neural network
        Node node = nNetwork.GetNode(10);
        Test("Neural Network Create Node", node != null);

        //Test creating a connection in the neural network
        Connection connection = nNetwork.GetConnection(nNetwork.GetNode(11), nNetwork.GetNode(12));
        Test("Neural Network Create Connection", connection != null);

        //Test creating a genome
        Genome genome = nNetwork.NewGenome();
        Test("Genome Created", genome != null);

        //Test updating a genomes fitness score
        genome.SetFitnessScore(1.0f);
        Test("Genome Fitness Add", genome.GetFitnessScore() == 1.0f);

        //Test updating the genomes rank
        genome.SetRank(1);
        Test("Genome Rank Add", genome.GetRank() == 1);

        //Test mutating a genome with a new connection
        genome.Mutation_NewConnection();
        Test("Genome Mutation Add Connection", genome.GetConnections().Count == 1);

        //Test mutating the genomes connection by disabling it
        genome.Mutation_EnableDisableConnection();
        Test("Genome Mutation Enable/Disable Connection", !genome.GetConnections()[0].IsEnabled());
        //Re-enable the connection
        genome.Mutation_EnableDisableConnection();

        //Test mutating the genomes weight change
        float oldWeight = genome.GetConnections()[0].GetWeight();
        genome.Mutation_WeightAdjust();
        Test("Genome Mutation Weight Change", genome.GetConnections()[0].GetWeight() != oldWeight);

        //Test mutating a genome with a new node
        genome.Mutation_CreateNodeFromConnection();
        Test("Genome Mutation Add Hidden Node", genome.GetHiddenNodes().Length == 1);

        //Test saving/loading of genomes
        for (int i = 0; i < 50; i++)
        {
            genome.Mutate();
        }
        int conns = genome.GetConnections().Count;
        genome.Save(-10);
        Genome loadedGenome = nNetwork.LoadGenome(-10);
        Test("Genome Save/Load Connections", loadedGenome.GetConnections().Count == conns);
        Test("Genome Save/Load Nodes", loadedGenome.GetConnections().Count == conns);

        //Test max nodes limit cannot be exceeded
        for(int i = 0; i < nNetwork.GetMaxNodes() + 1; i++)
        {
            nNetwork.GetNode(i);
        }
        Test("Genome Max Node", nNetwork.GetNodes().Count == nNetwork.GetMaxNodes());
    }
}
