using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

/*
 * GenomesSort Class
 * Description : This class is used to sort genomes by their fitness score
*/
public class GenomesSort : IComparer<Genome>
{
    public int Compare(Genome g1, Genome g2)
    {
        if (g1.GetFitnessScore() == g2.GetFitnessScore())
        {
            return 0;
        }
        else if (g1.GetFitnessScore() > g2.GetFitnessScore())
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
}

/*
 * Genome Class
 * Description : Genome class that is the neural network of the actors within the simulation
*/
public class Genome : FitnessScore
{
    //Genomes current neural network
    NeuralNetwork nNetwork;

    //Previous fitness score of last generation
    float previousFitness;
    //Current rank
    int rank = 0;

    //Current nodes
    List<Node> nodes = new List<Node>();
    //Current connections
    List<Connection> connections = new List<Connection>();

    //Mutation weight adjust value
    float weightAdjust = 0.3f;
    //Chance of mutation
    float mutationChance = 0.4f;
    //Colour of genome
    Color color = Color.red;
    //Size of genome
    float size = 0.6f;
    //Miniumum size of genomes
    float minSize = 0.35f;
    //Maximum size of genomes
    float maxSize = 0.6f;

    //Constructor
    public Genome(NeuralNetwork nNetwork, float weightAdjust, float mutationChance)
    {
        this.nNetwork = nNetwork;
        this.weightAdjust = weightAdjust;
        this.mutationChance = mutationChance;
    }

    //Get the previous generations fitness
    public float GetPreviousFitness()
    {
        return previousFitness;
    }

    //Set the previous generation fitness
    public void SetPreviousFitness(float value)
    {
        previousFitness = value;
    }

    //Get the genome rank
    public int GetRank()
    {
        return rank;
    }

    //Set the genome rank
    public void SetRank(int value)
    {
        rank = value;
    }

    //Get all the nodes of the genome
    public List<Node> GetNodes()
    {
        return nodes;
    }

    //Genome has node
    public bool HasNode(Node node)
    {
        //Loop through all nodes
        foreach(Node n in nodes)
        {
            //If matching innovation numbers, genome has the node
            if (n.GetInnovationNumber() == node.GetInnovationNumber())
            {
                return true;
            }
        }

        return false;
    }

    //Add a node to the genome
    public void AddNode(Node node)
    {
        //Ensure we dont have duplicate nodes
        if (!HasNode(node))
        {
            nodes.Add(node);
        }
    }

    //Get all the hidden nodes in the genome
    public Node[] GetHiddenNodes()
    {
        List<Node> hiddenNodes = new List<Node>();

        for(int i = 0; i < nodes.Count; i++)
        {
            int nodeINum = nodes[i].GetInnovationNumber();
            //If the current node is not an input or output node it is a hidden node
            if (!nNetwork.IsInputNode(nodeINum) && !nNetwork.IsOutputNode(nodeINum))
            {
                hiddenNodes.Add(nodes[i]);
            }
        }

        return hiddenNodes.ToArray();
    }

    //Get the genomes connections
    public List<Connection> GetConnections()
    {
        return connections;
    }

    //Add connection to genome
    public void AddConnection(Connection connection)
    {
        if (!HasConnection(connection.GetStart(), connection.GetEnd()))
        {
            connections.Add(connection);
        }
    }

    //Add a connection along with the nodes it uses
    public void AddConnectionAndNodes(Connection connection)
    {
        Node start = connection.GetStart();
        Node end = connection.GetEnd();

        //If connection doesnt already exist
        if (!HasConnection(start, end))
        {
            //Add the nodes if nodes dont already exist within the genome
            AddNode(nNetwork.GetNode(start.GetInnovationNumber()));
            AddNode(nNetwork.GetNode(end.GetInnovationNumber()));

            //Create new connection
            Connection newCon = nNetwork.GetConnection(start, end);
            newCon.SetWeight(connection.GetWeight());
            connections.Add(newCon);
        }
    }

    //Get the colour of the genome
    public Color GetColor()
    {
        return color;
    }

    //Get a random colour
    public void RandomColor()
    {
        SetColor(new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)));
    }

    //Set the genomes colour
    public void SetColor(Color color)
    {
        this.color = new Color(
            Mathf.Clamp(color.r, 0, 255), 
            Mathf.Clamp(color.g, 0, 255), 
            Mathf.Clamp(color.b, 0, 255)
            );
    }

    //Get the size of the genome
    public float GetSize()
    {
        return size;
    }

    //Set the genomes random size
    public void RandomSize()
    {
        SetSize(Random.Range(minSize, maxSize));
    }

    //Set the current size of the genome
    public void SetSize(float value)
    {
        size = Mathf.Clamp(value, minSize, maxSize);
    }

    //Does the genome a connection between two nodes
    bool HasConnection(Node start, Node end)
    {
        foreach(Connection connection in connections)
        {
            //Check all connections for matching nodes
            if (connection.GetStart().GetInnovationNumber() == start.GetInnovationNumber() && 
                connection.GetEnd().GetInnovationNumber() == end.GetInnovationNumber())
            {
                return true;
            }
        }

        return false;
    }

    //Get a random node
    Node GetRandomNode()
    {
        return nodes[Random.Range(0, nodes.Count)];
    }

    //Get a random hidden node
    Node GetRandomHiddenNode()
    {
        Node[] hiddenNodes = GetHiddenNodes();
        return hiddenNodes[Random.Range(0, hiddenNodes.Length)];
    }

    //Get random start node
    Node GetRandomStartNode()
    {
        return nodes[Random.Range(0, nNetwork.GetInputSize())];
    }

    //Get random connection
    Connection GetRandomConnection()
    {
        return connections[Random.Range(0, connections.Count)];
    }

    //Get the connections of which the node is the end connection
    Connection[] GetConnectedStartConnections(Node node)
    {
        List<Connection> result = new List<Connection>();

        //Search through the connections
        foreach(Connection connection in connections)
        {
            //Is the node a end node to the connection
            if (connection.GetEnd() == node && connection.IsEnabled())
            {
                result.Add(connection);
            }
        }

        return result.ToArray();
    }

    //Get the connections of which the node is the start connection
    Connection[] GetConnectedEndConnections(Node node)
    {
        List<Connection> result = new List<Connection>();

        foreach (Connection connection in connections)
        {
            //Is the node a start node to the connection
            if (connection.GetStart() == node && connection.IsEnabled())
            {
                result.Add(connection);
            }
        }

        return result.ToArray();
    }

    //Get the adjusted weight
    float GetAdjustedWeight(float currentWeight)
    {
        float weightChange = Random.Range(0.0f, weightAdjust);

        //Random chance between weight value going up or down
        if (Random.Range(0, 2) == 1)
        {
            currentWeight += weightChange;
        }
        else
        {
            currentWeight -= weightChange;
        }

        //Clamp weight value
        return Mathf.Clamp(currentWeight, -1.0f, 1.0f);
    }

    //Set nodes activation function to a random activation function
    void NodeSetRandomActivationFunction(Node node)
    {
        node.SetActivationFunction((ActivationFunctionType)Random.Range(0, System.Enum.GetNames(typeof(ActivationFunctionType)).Length));
    }

    //Attempt to mutate
    public Genome AttemptMutate()
    {
        if (Random.Range(0.0f, 1.0f) < mutationChance)
        {
            Mutate();
        }

        return this;
    }

    //Mutate the genome
    public Genome Mutate()
    {
        int chance = Random.Range(0, 10);

        //High chance for a basic mutation
        if (chance < 9)
        {
            float mutChance = Random.Range(0.0f, 1.0f);

            if (mutChance < 0.5f)
            {
                //Weight adjust mutation
                Mutation_WeightAdjust();
            }
            else
            {
                //New connection mutation
                Mutation_NewConnection();
            }
        }
        else
        {
            int mutChance = Random.Range(0, 3);

            //Advanced mutations
            switch (mutChance)
            {
                case 0:
                    //Change the activation function of the node
                    Mutation_NodeChangeActivationFunction();
                    break;
                case 1:
                    //Enable or disable a connection
                    Mutation_EnableDisableConnection();
                    break;
                case 2:
                    //Create new hidden node
                    Mutation_CreateNodeFromConnection();
                    break;
                case 3:
                    //Break structure
                    Mutation_BreakStructure();
                    break;
            }
        }

        return this;
    }

    //Mutation create new connection
    public void Mutation_NewConnection()
    {
        Node node1;
        Node node2;

        //Loop through possible maximum nodes
        for(int i = 0; i < nNetwork.GetMaxNodes(); i++)
        {
            //Get two random hidden nodes
            node1 = GetRandomNode();
            node2 = GetRandomNode();

            //If the connections dont exist and are not the same nodes
            if (node1.GetInnovationNumber() != node2.GetInnovationNumber() &&
                node1.GetX() < node2.GetX() &&
                !HasConnection(node1, node2))
            {
                //Create the new connection
                Connection mutationConnection = nNetwork.GetConnection(node1, node2);
                connections.Add(mutationConnection);
                break;
            }
        }
    }

    //Mutation adjust the weight of connection
    public void Mutation_WeightAdjust()
    {
        if (connections.Count != 0)
        {
            //Get random connection
            Connection connection = GetRandomConnection();
            //Adjust weight
            connection.SetWeight(GetAdjustedWeight(connection.GetWeight()));
        }
    }

    //Mutation enable of disable connection
    public void Mutation_EnableDisableConnection()
    {
        if (connections.Count != 0)
        {
            //Get random connection
            Connection connection = GetRandomConnection();
            //flip enabled status
            connection.SetEnabled(!connection.IsEnabled());
        }
    }

    //Mutation create a new hidden node
    public void Mutation_CreateNodeFromConnection()
    {
        //If we dont have any connections no new nodes can be created
        if (connections.Count != 0)
        {
            //Ensure we havent reached the maximum nodes
            if (nNetwork.GetNodes().Count <= nNetwork.GetMaxNodes())
            {
                Connection connection = GetRandomConnection();

                //Ensure the connection is enabled
                if (connection.IsEnabled())
                {
                    Node start = connection.GetStart();
                    Node end = connection.GetEnd();

                    // create new node
                    Node newNode = nNetwork.CreateNode();
                    // Set new activation function
                    NodeSetRandomActivationFunction(newNode);
                    AddNode(newNode);

                    //Create the new connections between the hidden node
                    Connection connection1 = nNetwork.GetConnection(start, newNode);
                    connections.Add(connection1);

                    Connection connection2 = nNetwork.GetConnection(newNode, end);
                    connections.Add(connection2);

                    //Set nodes positions within NEATGraph
                    newNode.SetX(Random.Range(start.GetX(), end.GetX()));
                    newNode.SetY(Random.Range(start.GetY(), end.GetY()));

                    //Disable the connection
                    connection.SetEnabled(false);
                }
            }
        }
    }

    //Mutation change the activation function
    void Mutation_NodeChangeActivationFunction()
    {
        //Ensure we have hidden nodes to change
        if (GetHiddenNodes().Length > 0)
        {
            NodeSetRandomActivationFunction(GetRandomHiddenNode());
        }
    }

    //Mutation break the structure of a connection from a start node
    void Mutation_BreakStructure()
    {
        if (connections.Count != 0)
        {
            for(int i = 0; i < nNetwork.GetInputSize(); i++)
            {
                //Get a random start node
                Node start = GetRandomStartNode();
                //Get all the connections from the start node
                Connection[] startCons = GetConnectedEndConnections(start);

                //Ensure it has connections
                if (startCons.Length > 0)
                {
                    //Create a list of all found nodes
                    List<Node> removeNodes = new List<Node>();

                    Connection randomConnection = startCons[Random.Range(0, startCons.Length)];
                    Node currentEnd = randomConnection.GetEnd();
                    connections.Remove(randomConnection);

                    //Keep looping until no new connections found
                    bool changedConnections = true;
                    while(changedConnections)
                    {
                        changedConnections = false;

                        //Loop through all hidden nodes
                        foreach (Node n in GetHiddenNodes())
                        {
                            //Get the start nodes of the hidden nodes
                            Connection[] nodeStartConnections = GetConnectedStartConnections(n);

                            //If the connection has no start nodes we found the connection needing to be removed
                            if (nodeStartConnections.Length == 0)
                            {
                                //Remove the node
                                removeNodes.Add(n);
                                Connection[] nodeEndConnections = GetConnectedEndConnections(n);

                                //Get all end connections and remove and loop again
                                foreach (Connection c in nodeEndConnections)
                                {
                                    connections.Remove(c);
                                    changedConnections = true;
                                }
                            }
                        }
                    }

                    //Remove all nodes that had their connections removed
                    foreach(Node n in removeNodes)
                    {
                        nodes.Remove(n);
                    }
                }
            }
        }
    }

    //Calculate the connected node
    float CalculateConnectedNode(Node node, Connection[] startConnections, float[] startValues)
    {
        float result = 0.0f;

        //Calculate all connected nodes into the result
        for (int i = 0; i < startConnections.Length; i++)
        {
            result += startValues[i] * startConnections[i].GetWeight();
        }

        //Process result through activation functions
        return node.GetOutput(result);
    }

    //Multi-threaded task to calculate output of the genome
    public async Task CalculateOutput(Actor actor, float[] inputData)
    {
        //Current output data
        float[] outputData = new float[nodes.Count];
        //List of calculated nodes
        List<Node> calculatedNodes = new List<Node>();

        //Loop through all input nodes and set their output data
        for (int i = 0; i < nNetwork.GetInputSize(); i++)
        {
            Node node = nodes[i];
            outputData[i] = inputData[i];
            calculatedNodes.Add(node);
        }

        //Loop through each node until all nodes are calculated
        int calculationsMade = -1;
        while (calculatedNodes.Count != nodes.Count)
        {
            //If no calculations are made then break the loop
            if (calculationsMade == 0)
            {
                break;
            }

            calculationsMade = 0;

            //Loop through hidden and output nodes
            for (int i = nNetwork.GetInputSize(); i < nodes.Count; i++)
            {
                Node node = nodes[i];

                //continue if already calculated
                if (calculatedNodes.Contains(node))
                {
                    continue;
                }

                //Get all start connections of current node
                Connection[] startConnections = GetConnectedStartConnections(node);

                //If connections exist for current node
                if (startConnections.Length > 0)
                {
                    int calculatedConnections = 0;
                    float[] startValues = new float[startConnections.Length];

                    //Check all previous connected nodes have been calculated
                    for (int j = 0; j < startConnections.Length; j++)
                    {
                        if (startConnections[j].IsEnabled())
                        {
                            Node startNode = startConnections[j].GetStart();

                            if (calculatedNodes.Contains(startNode))
                            {
                                //node has been calculated so can continue
                                startValues[j] = outputData[nodes.IndexOf(startNode)];
                                calculatedConnections++;
                            }
                        }
                    }

                    //if has connections and has all previous nodes calculated
                    if (calculatedConnections == startConnections.Length)
                    {
                        //do calculation
                        outputData[i] = CalculateConnectedNode(node, startConnections, startValues);
                        calculatedNodes.Add(node);
                        calculationsMade++;
                    }
                }
                else
                {
                    //No start connections so calculate output
                    outputData[i] = node.GetOutput(outputData[i]);
                    calculatedNodes.Add(node);
                    calculationsMade++;
                }
            }
        }

        //Process results
        float[] result = new float[nNetwork.GetOutputSize()];
        for (int i = 0; i < nNetwork.GetOutputSize(); i++)
        {
            result[i] = outputData[i + (nodes.Count - nNetwork.GetOutputSize())];
        }
        //Do genomes output
        actor.GenomeOutput(result);
        //Await until all other threads are completed
        await Task.Yield();
    }

    //Save the genome
    public void Save(int speciesID)
    {
        //Setup file paths
        string directory = nNetwork.GetFilePath() + nNetwork.GetGenerationCount() + "/";
        string fileName = speciesID.ToString() + ".json";
        Simulation.CreateFile(directory, fileName);

        //Store the genomes data into genome data
        GenomeData gData = new GenomeData();
        gData.fitnessScore = GetFitnessScore();
        gData.previousFitness = GetPreviousFitness();
        gData.nodes = GetNodes();
        gData.connections = GetConnections();
        gData.color = GetColor();
        gData.size = GetSize();
        gData.rank = GetRank();

        //Convert to JSON and write to file
        string data = JsonUtility.ToJson(gData, true);
        File.WriteAllText(directory + fileName, data);
    }
}
