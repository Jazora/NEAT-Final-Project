using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
 * NeuralNetwork Class
 * Description : This class is used as the source for all nodes and connections that exist in all genomes
*/
public class NeuralNetwork
{
    //Singleton of the neural network
    public static NeuralNetwork instance;

    //Simulation name
    string simulationName;
    //Max number of nodes supported
    int maxNodes;
    //Input size of the neural network
    int inputSize;
    //Ouput size of the neural network
    int outputSize;
    //Node names of the input and output nodes
    string[] nodeNames;

    //The activation function for output nodes
    ActivationFunctionType outputActivationFunction = ActivationFunctionType.tanh;

    //Nodes stored within the neural network
    Dictionary<int, Node> nodes = new Dictionary<int, Node>();
    //Connections stored within the neural network
    Dictionary<int, Connection> connections = new Dictionary<int, Connection>();

    //Current species distance limit
    float speciesDistance = 1.0f;
    //Current generation
    int generationCount = 0;
    //Current species count
    int speciesCount = 0;
    //Connection weight adjust value
    float weightAdjust = 0.3f;
    //Genomes chance at mutations
    float mutationChance = 0.3f;
    //Rank gained multiplier
    int rankGainedMultiplier = 1;

    //Constructor
    public NeuralNetwork(string simulationName, int maxNodes, int inputSize, int outputSize, string[] nodeNames, 
        ActivationFunctionType outputActivationFunction, float speciesDistance, float weightAdjust, float mutationChance, int rankGainedMultiplier)
    {
        this.simulationName = simulationName;
        this.maxNodes = maxNodes;
        this.inputSize = inputSize;
        this.outputSize = outputSize;
        this.nodeNames = nodeNames;
        this.outputActivationFunction = outputActivationFunction;
        this.speciesDistance = speciesDistance;
        this.weightAdjust = weightAdjust;
        this.mutationChance = mutationChance;
        this.rankGainedMultiplier = rankGainedMultiplier;

        //Set singleton
        instance = this;
    }

    //Get the input size
    public int GetInputSize()
    {
        return inputSize;
    }

    //Get output size
    public int GetOutputSize()
    {
        return outputSize;
    }

    //Get the current node name for the graph
    public string GetNodeName(int iNum)
    {
        if (IsInputNode(iNum) || IsOutputNode(iNum))
        {
            return nodeNames[iNum] + "\n";
        }
        else
        {
            return "";
        }
    }

    //Is the innovation number a input node
    public bool IsInputNode(int iNumber)
    {
        if (iNumber >= 0 && iNumber < inputSize)
        {
            return true;
        }

        return false;
    }

    //Is the innovation number a output node
    public bool IsOutputNode(int iNumber)
    {
        if (iNumber >= inputSize  && iNumber < (inputSize + outputSize))
        {
            return true;
        }

        return false;
    }

    //Get the max amount of nodes
    public int GetMaxNodes()
    {
        return maxNodes;
    }

    //Get all nodes
    public Dictionary<int, Node> GetNodes()
    {
        return nodes;
    }

    //Get all connections
    public Dictionary<int, Connection> GetConnections()
    {
        return connections;
    }

    //Get amount of generations has passed
    public int GetGenerationCount()
    {
        return generationCount;
    }

    //Add to the generation count
    public void AddGenerationCount(int value)
    {
        generationCount += value;
    }

    //Get the current species count
    public int GetSpeciesCount()
    {
        return speciesCount;
    }

    //Create a new node
    public Node CreateNode()
    {
        //Ensure we havent reached max nodes
        if (nodes.Count >= GetMaxNodes())
        {
            return null;
        }

        int newINumber = nodes.Count;
        Node newNode = new Node(newINumber);

        NEATGraph nGraph = NEATGraph.instance;

        //Apply nodes positioning to graphs position
        if (IsInputNode(newINumber))
        {
            newNode.SetX(-nGraph.GetGraphX());
            newNode.SetY(nGraph.GetGraphY() - (newINumber * nGraph.GetGraphYOffset()));
        }
        else if (IsOutputNode(newINumber))
        {
            newNode.SetX(nGraph.GetGraphX());
            newNode.SetY(nGraph.GetGraphY() - ((newINumber - inputSize) * nGraph.GetGraphYOffset()));
            newNode.SetActivationFunction(outputActivationFunction);
        }

        nodes.Add(newINumber, newNode);
        return newNode;
    }

    //Get a node that exists if it doesnt exist create a new node
    public Node GetNode(int nodeNum)
    {
        Node result;
        if (!nodes.TryGetValue(nodeNum, out result))
        {
            result = CreateNode();
        }

        return result;
    }

    //Get a random weight for a connection
    float GetRandomWeight()
    {
        return Random.Range(-1.0f, 1.0f);
    }

    //Create clone of connection
    Connection CopyConnection(Connection connection)
    {
        Connection newConnection = new Connection(connection.GetInnovationNumber(), GetNode(connection.GetStart().GetInnovationNumber()), 
                                                  GetNode(connection.GetEnd().GetInnovationNumber()), GetRandomWeight());
        return newConnection;
    }

    //Create a new connection
    Connection CreateConnection(Node start, Node end)
    {
        Connection newConnection = new Connection(connections.Count, start, end, GetRandomWeight());
        //Use max nodes as hashed number for unique ID
        connections.Add(start.GetInnovationNumber() * maxNodes + end.GetInnovationNumber(), newConnection);
        return newConnection;
    }

    //Get existing connection
    public Connection GetConnection(Node start, Node end)
    {
        Connection result;
        //Find connection based off connection hashed number
        if (connections.TryGetValue(start.GetInnovationNumber() * maxNodes + end.GetInnovationNumber(), out result))
        {
            result = CopyConnection(result);
        }
        else
        {
            result = CreateConnection(start, end);
        }

        return result;
    }

    //Create a new genome
    public Genome NewGenome()
    {
        Genome newGenome = new Genome(this, weightAdjust, mutationChance);

        for (int i = 0; i < inputSize + outputSize; i++)
        {
            //add input and output nodes to the new genome
            newGenome.AddNode(GetNode(i));
        }

        //Set random attributes
        newGenome.RandomColor();
        newGenome.RandomSize();

        return newGenome;
    }

    //Get the distance between genomes for speciation, also known as species distance
    float GenomeDistance(Genome g1, Genome g2)
    {
        Genome hGenome = g1; // highest connection count
        Genome lGenome = g2; // lowest connection count

        if (g2.GetConnections().Count > g1.GetConnections().Count)
        {
            hGenome = g2;
            lGenome = g1;
        }

        Connection[] hConnections = hGenome.GetConnections().ToArray();
        Connection[] lConnections = lGenome.GetConnections().ToArray();

        //Coefficients
        float excessCoefficient = 1;
        float disjointCoefficient = 1;
        float weightCoefficient = 0.5f;

        //Setup excess, disjoint, matching and weight difference values
        int excess = hConnections.Length - lConnections.Length;
        int disjoint = 0;
        int matching = 0;
        float weightDifference = 0.0f;

        if (lConnections.Length > 0) // ensure the lowest connection has connections
        {
            int lConIndex = 0;

            for (int i = 0; i < hConnections.Length; i++)
            {
                //Connection innovation numbers
                int hInnoNum = hConnections[i].GetInnovationNumber();
                int lInnoNum = lConnections[lConIndex].GetInnovationNumber();

                if (hInnoNum == lInnoNum) // if the connections are the same we found matching
                {
                    weightDifference += hConnections[i].GetWeight() - lConnections[lConIndex].GetWeight();
                    lConIndex++;
                    matching++;
                }
                else if (hInnoNum < lInnoNum) // found a disjoint
                {
                    disjoint++;
                }
                else if (hInnoNum > lInnoNum) // found a disjoint
                {
                    i--;
                    lConIndex++;
                    disjoint++;
                }

                if (lConIndex >= lConnections.Length)
                {
                    weightDifference = weightDifference / matching; // find the total weight difference
                    break;
                }
            }
        }
        else
        {
            //All other existing connections just count as disjoint
            for(int i = 0; i < hConnections.Length; i++)
            {
                disjoint++;
            }
        }

        //Normalize value
        int normalize = hGenome.GetConnections().Count + hGenome.GetNodes().Count;

        //If lower genome is higher in count, becomes new normalize value
        if ((lGenome.GetConnections().Count + lGenome.GetNodes().Count) > normalize)
        {
            normalize = lGenome.GetConnections().Count + lGenome.GetNodes().Count;
        }

        //Normalize value is 1 when value is the size of just output and input nodes
        if (normalize <= (GetInputSize() + GetOutputSize()))
        {
            normalize = 1;
        }

        //Species Distance calculation
        return (excessCoefficient * excess / normalize) + 
               (disjointCoefficient * disjoint / normalize) + 
               (weightCoefficient * weightDifference / normalize);
    }

    //Cross two genomes together, creating a new genome
    public Genome CrossGenomes(Genome g1, Genome g2)
    {
        Genome newGenome = NewGenome();

        Genome hGenome = g1; // highest connection count
        Genome lGenome = g2; // lowest connection count

        if (g2.GetConnections().Count > g1.GetConnections().Count)
        {
            hGenome = g2;
            lGenome = g1;
        }

        Connection[] hConnections = hGenome.GetConnections().ToArray();
        Connection[] lConnections = lGenome.GetConnections().ToArray();

        if (lConnections.Length > 0) // ensure we have connections on lowest count
        {
            int lConIndex = 0;

            //Crossconnections
            for (int i = 0; i < hConnections.Length; i++)
            {
                //Connection innovation numbers
                int hInnoNum = hConnections[i].GetInnovationNumber();
                int lInnoNum = lConnections[lConIndex].GetInnovationNumber();

                //If both connections have the same innovation number random pick between the two
                if (hInnoNum == lInnoNum)
                {
                    if (Random.Range(0, 2) == 1)
                    {
                        newGenome.AddConnectionAndNodes(GetConnection(hConnections[i].GetStart(), hConnections[i].GetEnd()));
                    }
                    else
                    {
                        newGenome.AddConnectionAndNodes(GetConnection(lConnections[lConIndex].GetStart(), lConnections[lConIndex].GetEnd()));
                    }

                    lConIndex++;
                }
                else if (hInnoNum < lInnoNum) // if connection doesnt exist between the two genomes add to new genome
                {
                    newGenome.AddConnectionAndNodes(GetConnection(hConnections[i].GetStart(), hConnections[i].GetEnd()));
                }
                else if (hInnoNum > lInnoNum) // if connection doesnt exist between the two genomes add to new genome
                {
                    newGenome.AddConnectionAndNodes(GetConnection(lConnections[lConIndex].GetStart(), lConnections[lConIndex].GetEnd()));
                    i--;
                    lConIndex++;
                }

                //For excess connections just add to new genome
                if (lConIndex >= lConnections.Length)
                {
                    //Add excess connections
                    for (int j = lConIndex; j < hConnections.Length; j++)
                    {
                        newGenome.AddConnectionAndNodes(GetConnection(hConnections[j].GetStart(), hConnections[j].GetEnd()));
                    }

                    break;
                }
            }
        }
        else
        {
            //Add all connections from highest
            for (int i = 0; i < hConnections.Length; i++)
            {
                newGenome.AddConnectionAndNodes(GetConnection(hConnections[i].GetStart(), hConnections[i].GetEnd()));
            }
        }

        //Set a colour to the genome between the two genomes
        newGenome.SetColor(new Color(
                (hGenome.GetColor().r + lGenome.GetColor().r) / 2,
                (hGenome.GetColor().g + lGenome.GetColor().g) / 2,
                (hGenome.GetColor().b + lGenome.GetColor().b) / 2
                ));

        //Get size between the two genomes
        newGenome.SetSize((hGenome.GetSize() + lGenome.GetSize()) / 2);

        return newGenome;
    }

    //Speciate between all the genomes
    List<Species> Speciation(Genome[] genomes)
    {
        //Create a list of species
        List<Species> species = new List<Species>();

        for(int i = 0; i < genomes.Length; i++)
        {
            Species similarSpecies = null;

            //Loop through all species and genomes finding similar structures
            foreach(Species spec in species)
            {
                foreach(Genome geno in spec.GetGenomes())
                {
                    //If the genome is the correct distance add to species
                    if (GenomeDistance(genomes[i], geno) < speciesDistance)
                    {
                        similarSpecies = spec;
                        break;
                    }
                }

                if (similarSpecies != null)
                {
                    break;
                }
            }

            //If found a similar species then add to it else create new species
            if (similarSpecies != null)
            {
                similarSpecies.AddGenome(genomes[i]);
            }
            else
            {
                species.Add(new Species(genomes[i]));
            }
        }

        return species;
    }

    //Evaluate the fitness of the genome population
    public void EvaluateFitness(Genome[] genomes)
    {
        //Speciate all genomes
        List<Species> species = Speciation(genomes);

        //Sort all species genomes
        for (int i = 0; i < species.Count; i++)
        {
            //Sort genomes by fitness
            GenomesSort gSort = new GenomesSort();
            species[i].GetGenomes().Sort(gSort);
            //Set the fitness score of the species by the highest genome
            species[i].SetFitnessScore(species[i].GetGenomes()[0].GetFitnessScore());
        }

        //Reverse the species order
        SpeciesSortReverse sSortReverse = new SpeciesSortReverse();
        species.Sort(sSortReverse);

        if (species.Count > 1)
        {
            //Assign a rank to each species based on their position
            float previousScore = species[0].GetFitnessScore();
            int currentRank = 0;
            foreach (Species s in species)
            {
                if (s.GetFitnessScore() > previousScore)
                {
                    previousScore = s.GetFitnessScore();
                    currentRank++;
                }

                s.GetGenomes()[0].SetRank(s.GetGenomes()[0].GetRank() + (currentRank * rankGainedMultiplier));
            }
        }

        //Return the species list back into order
        SpeciesSort sSort = new SpeciesSort();
        species.Sort(sSort);

        //Do crossover
        if (species.Count > 1)
        {
            //Create a new species with the top two species combined
            species.Add(new Species(CrossGenomes(species[0].GetGenomes()[0], species[1].GetGenomes()[0])));
            //Give the new crossed species 1 generation
            species[species.Count - 1].GetGenomes()[0].SetRank(1);
        }

        //Set species count
        speciesCount = species.Count;
        //Sort species again with new species
        species.Sort(sSort);

        //Save all the genomes
        for (int i = 0; i < species.Count; i++)
        {
            species[i].GetGenomes()[0].Save(i);
        }
        
        Save();
    }

    //Get the file path to the simulations
    public string GetFilePath()
    {
        return Simulation.GetDataFilePath() + "Sims/" + simulationName + "/";
    }

    //Load the neural network
    public void Load()
    {
        string path = GetFilePath() + "NeuralNetwork.json";

        //Check if neural network data exists
        if (File.Exists(path))
        {
            string data = File.ReadAllText(path);
            NeuralNetworkData loadedNData = JsonUtility.FromJson<NeuralNetworkData>(data);

            maxNodes = loadedNData.maxNodes;
            inputSize = loadedNData.inputSize;
            outputSize = loadedNData.outputSize;
            nodeNames = loadedNData.nodeNames;

            //All node data to the neural network
            for (int i = 0; i < loadedNData.nodes.Count; i++)
            {
                nodes.Add(loadedNData.nodeKeys[i], loadedNData.nodes[i]);
            }

            //Add all the connection data to the neural network
            for (int i = 0; i < loadedNData.connections.Count; i++)
            {
                connections.Add(loadedNData.connectionKeys[i], loadedNData.connections[i]);
            }

            generationCount = loadedNData.generationCount;
            speciesCount = loadedNData.speciesCount;
        }
        else
        {
            //If no data exists create new neural network
            for (int i = 0; i < inputSize + outputSize; i++)
            {
                CreateNode();
            }
        }
    }

    //Save the neural network data
    public void Save()
    {
        //Set file paths
        string directory = GetFilePath();
        string fileName = "NeuralNetwork.json";
        Simulation.CreateFile(directory, fileName);

        //Save neural network data
        NeuralNetworkData nData = new NeuralNetworkData();
        nData.maxNodes = maxNodes;
        nData.inputSize = inputSize;
        nData.outputSize = outputSize;
        nData.nodeNames = nodeNames;

        foreach(KeyValuePair<int, Node> node in nodes)
        {
            nData.nodeKeys.Add(node.Key);
            nData.nodes.Add(GetNode(node.Value.GetInnovationNumber()));
        }

        foreach (KeyValuePair<int, Connection> connection in connections)
        {
            nData.connectionKeys.Add(connection.Key);
            nData.connections.Add(CopyConnection(connection.Value));
        }

        nData.speciesCount = speciesCount;
        nData.generationCount = generationCount;

        //Write to JSON
        string data = JsonUtility.ToJson(nData, true);
        File.WriteAllText(directory + fileName, data);
    }

    //Load history of simulation type
    public Genome[] LoadGenomeHistory()
    {
        Genome[] loadedGenomes = new Genome[generationCount];

        //Loop through all generations collecting the top genomes
        int copyRoundCount = generationCount;
        for (int i = 1; i <= copyRoundCount; i++)
        {
            generationCount = i;
            loadedGenomes[i - 1] = LoadGenome(0);
        }

        return loadedGenomes;
    }

    //Load all genomes
    public Genome[] LoadGenomes()
    {
        Genome[] loadedGenomes = new Genome[speciesCount];

        //Load all species
        for (int i = 0; i < speciesCount; i++)
        {
            loadedGenomes[i] = LoadGenome(i);
        }

        return loadedGenomes;
    }

    //Load a single genome
    public Genome LoadGenome(int speciesID)
    {
        //Get the path of the genome
        string path = GetFilePath() + generationCount + "/" + speciesID.ToString() + ".json";

        //Ensure the genomes file exists
        if (File.Exists(path))
        {
            string data = File.ReadAllText(path);

            //Load all the genome data
            GenomeData loadedGData = JsonUtility.FromJson<GenomeData>(data);
            Genome loadedGenome = NewGenome();

            loadedGenome.SetFitnessScore(loadedGData.fitnessScore);
            loadedGenome.SetPreviousFitness(loadedGData.previousFitness);

            foreach (Node n in loadedGData.nodes)
            {
                loadedGenome.AddNode(GetNode(n.GetInnovationNumber()));
            }
            
            foreach(Connection c in loadedGData.connections)
            {
                Connection conn = CopyConnection(c);
                conn.SetEnabled(c.IsEnabled());
                conn.SetWeight(c.GetWeight());
                loadedGenome.AddConnection(conn);
            }

            loadedGenome.SetColor(loadedGData.color);
            loadedGenome.SetSize(loadedGData.size);
            loadedGenome.SetRank(loadedGData.rank);

            //Return the genome
            return loadedGenome;
        }

        //If genome files do not exist create a new genome
        return NewGenome();
    }
}
