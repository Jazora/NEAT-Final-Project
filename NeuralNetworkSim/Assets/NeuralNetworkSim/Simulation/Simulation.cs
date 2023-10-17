using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
 * Simulation Class
 * Description : This is the main simulation class, used for running all features for simulation
*/
public class Simulation : MonoBehaviour
{
    //Current neural network of the simulation
    NeuralNetwork nNetwork;

    //Is data collection enabled
    [Header("Data Collection")]
    [SerializeField]
    bool dataCollection = false;
    [SerializeField]
    //The amount of simulation instances to run
    int dataCollectionAmount = 1;
    //The current simulation instance running
    int currentDataCollection = 0;
    //This value is used in the editor to skip over other simulation parameters
    [SerializeField]
    int currentSimulationParameter = 0;
    [SerializeField]
    List<SimulationParameter> simulationParameters = new List<SimulationParameter>();
    List<SimulationData> simulationData = new List<SimulationData>();

    //Name of the current simulation
    [Header("Neural Network")]
    [SerializeField]
    string simulationName;
    //Maximum nodes allowed in a neural network, limit is used for innovation numbers for connections
    [SerializeField]
    int maxNodes = 1000;
    //Current input size of simulation
    [SerializeField]
    int inputSize = 2;
    //The current output size of the simulation
    [SerializeField]
    int outputSize = 2;
    //The node names for the input and output layer
    [SerializeField]
    string[] nodeNames;
    //The output nodes activation function types
    [SerializeField]
    ActivationFunctionType outputActivationFunction = ActivationFunctionType.tanh;
    //The species distance required
    [SerializeField]
    float speciesDistance = 1.0f;
    //The connection weight adjustment value
    [SerializeField]
    float weightAdjust = 0.3f;
    //The chance a genome mutates
    [SerializeField]
    float mutationChance = 0.3f;

    [Header("Simulation")]
    //The actor count of the current simulation
    [SerializeField]
    int actorCount = 5;
    //The amount of starting mutations for genomes
    [SerializeField]
    int startingMutations = 2;
    //The starting rank for genomes
    [SerializeField]
    int startingRank = 1;
    //The amount of rank lose after each generation
    [SerializeField]
    int rankLoseRoundStart = 1;
    //The multiplier on rank gained
    [SerializeField]
    int rankGainedMultiplier = 1;

    //Create a new neural network
    void CreateNeuralNetwork()
    {
        nNetwork = new NeuralNetwork(simulationName + currentDataCollection, maxNodes, inputSize, outputSize, nodeNames, outputActivationFunction, speciesDistance,
                                     weightAdjust, mutationChance, rankGainedMultiplier);
    }

    //Update the current simulation with new parameters
    void UpdateSimulationType()
    {
        simulationName = simulationParameters[currentSimulationParameter].simulationName;
        speciesDistance = simulationParameters[currentSimulationParameter].speciesDistance;
        weightAdjust = simulationParameters[currentSimulationParameter].weightAdjust;
        mutationChance = simulationParameters[currentSimulationParameter].mutationChance;
        startingMutations = simulationParameters[currentSimulationParameter].startingMutations;
        startingRank = simulationParameters[currentSimulationParameter].startingRank;
        rankLoseRoundStart = simulationParameters[currentSimulationParameter].rankLoseRoundStart;
        rankGainedMultiplier = simulationParameters[currentSimulationParameter].rankGainedMultiplier;
    }

    void Awake()
    {
        //if data collecting load simulation parameters
        if (dataCollection)
        {
            UpdateSimulationType();
        }

        CreateNeuralNetwork();
    }

    //Get the current neural network
    public NeuralNetwork GetNeuralNetwork()
    {
        return nNetwork;
    }

    //Store new simulation data to be analysed after completion
    void AddSimulationData()
    {
        SimulationData sData = new SimulationData();
        sData.generationCount = nNetwork.GetGenerationCount();
        Genome g = nNetwork.LoadGenome(0); // last highest genome
        sData.fitnessScore = g.GetFitnessScore();
        sData.rankScore = g.GetRank();
        sData.nodesCount = g.GetNodes().Count;
        sData.connectionsCount = g.GetConnections().Count;

        simulationData.Add(sData);
    }

    //Determine if to start a new simulation parameters
    protected bool StartNewSimulationType()
    {
        if (dataCollection) // is collecting data
        {
            SaveCSVGenerationHistory();
            AddSimulationData();
            //Move onto next simulation instance
            currentDataCollection++;

            if (currentDataCollection < dataCollectionAmount) // continue the current data collection
            {
                CreateNeuralNetwork();
                return true;
            }
            else // attempt start new parameters
            {
                //Save previous results to CSV
                SaveCSVDataCollection();
                simulationData = new List<SimulationData>();
                currentSimulationParameter++;

                if (currentSimulationParameter < simulationParameters.Count) // start new simulation type
                {
                    //Reset current simulation test instance
                    currentDataCollection = 0;
                    //Start new simulation parameters
                    UpdateSimulationType();
                    CreateNeuralNetwork();
                    return true;
                }
            }
        }

        return false;
    }

    //Get the amount of actors
    public int GetActorCount()
    {
        return actorCount;
    }

    //Get the starting rank of the genomes
    public int GetStartingRank()
    {
        return startingRank;
    }

    //Load all species data
    protected void LoadAllSpecies()
    {
        int count = 0;
        Genome[] genomes = GetNeuralNetwork().LoadGenomes();
        Vector2 centerCameraPosition = Camera.main.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));

        float genomeRanksCount = 0f;
        for (int i = 0; i < genomes.Length; i++) // count the amount of genome ranks
        {
            genomeRanksCount += genomes[i].GetRank();
        }

        for (int i = 0; i < genomes.Length; i++)
        {
            //Work out the percentage of a genomes rank compared to population
            float genomeRankPercentage = (genomes[i].GetRank() / genomeRanksCount);

            //If entire species is the entire population lower the value to allow new species
            if (genomeRankPercentage == 1f)
            {
                genomeRankPercentage = 0.8f;
            }

            //The amount of actors to reproduce
            int reproduceCount = Mathf.RoundToInt(actorCount * genomeRankPercentage);
            for (int j = 0; j < reproduceCount; j++)
            {
                //Load genome of that type
                LoadSpecies(i, centerCameraPosition);
                count++;
            }
        }

        //For missing actors just fill in with new genomes
        for (int i = count; i < actorCount; i++)
        {
            // create new genome for non species
            LoadSpecies(-1, centerCameraPosition);
        }
    }

    //Load one species type
    protected void LoadSpecies(int speciesID, Vector3 position)
    {
        //Load the genomes data
        Genome genome = GetNeuralNetwork().LoadGenome(speciesID);

        // only spawn valid genomes
        if (genome.GetRank() > 0)
        {
            //If fitness was improved dont reduce rank
            if (genome.GetPreviousFitness() > genome.GetFitnessScore())
            {
                genome.SetRank(genome.GetRank() - rankLoseRoundStart);
            }

            genome.SetPreviousFitness(genome.GetFitnessScore());
            genome.SetFitnessScore(0);
        }
        else
        {
            //Load new genome if rank was too low
            genome = GetNeuralNetwork().LoadGenome(-1);

            //starting mutations
            for (int i = 0; i < startingMutations; i++) 
            {
                genome.Mutate();
            }

            // starting rank of new genomes
            genome.SetRank(startingRank); 
        }

        //Attempt to mutate each genomes
        genome.AttemptMutate();
        //Spawn the actor
        SpawnActor(genome, position);
    }

    //Spawn actor class for inheritance
    public virtual void SpawnActor(Genome genome, Vector3 position)
    {
        
    }

    //Save the generation history for each top genome
    void SaveCSVGenerationHistory()
    {
        string directory = GetDataFilePath() + "Results/" + simulationName + "/";
        string fileName = currentDataCollection.ToString() + "_generationhistory.csv";
        CreateFile(directory, fileName);

        CSVData data = new CSVData();
        // load top genomes of each generation history
        Genome[] genomeHistory = nNetwork.LoadGenomeHistory(); 

        for (int i = 0; i < genomeHistory.Length; i++)
        {
            data.NewData(i.ToString());
            data.NewLine(genomeHistory[i].GetFitnessScore().ToString());
        }

        File.WriteAllText(directory + fileName, data.ToString());
    }

    void SaveCSVDataCollection()
    {
        // Save Fitness Data
        string directory = GetDataFilePath() + "Results/" + simulationName + "/";
        string fileName = simulationName + "_fitness.csv";
        CreateFile(directory, fileName);

        CSVData data = new CSVData();

        for (int i = 0; i < simulationData.Count; i++)
        {
            data.NewData(i.ToString());
            data.NewLine(simulationData[i].fitnessScore.ToString());
        }

        File.WriteAllText(directory + fileName, data.ToString());

        //Save Generations Count
        fileName = simulationName + "_generations.csv";
        CreateFile(directory, fileName);

        data = new CSVData();

        for (int i = 0; i < simulationData.Count; i++)
        {
            data.NewData(i.ToString());
            data.NewLine(simulationData[i].generationCount.ToString());
        }

        File.WriteAllText(directory + fileName, data.ToString());

        //Save RankScore
        fileName = simulationName + "_rankscore.csv";
        CreateFile(directory, fileName);

        data = new CSVData();

        for (int i = 0; i < simulationData.Count; i++)
        {
            data.NewData(i.ToString());
            data.NewLine(simulationData[i].rankScore.ToString());
        }

        File.WriteAllText(directory + fileName, data.ToString());

        //Save Nodes Count
        fileName = simulationName + "_nodescount.csv";
        CreateFile(directory, fileName);

        data = new CSVData();

        for (int i = 0; i < simulationData.Count; i++)
        {
            data.NewData(i.ToString());
            data.NewLine(simulationData[i].nodesCount.ToString());
        }

        File.WriteAllText(directory + fileName, data.ToString());

        //Save Connections Count
        fileName = simulationName + "_connectionscount.csv";
        CreateFile(directory, fileName);

        data = new CSVData();

        for (int i = 0; i < simulationData.Count; i++)
        {
            data.NewData(i.ToString());
            data.NewLine(simulationData[i].connectionsCount.ToString());
        }

        File.WriteAllText(directory + fileName, data.ToString());
    }

    //Get file path to data folder
    public static string GetDataFilePath()
    {
        string path = Application.dataPath + "/Data/";

        if (!File.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }

    //Create file and directories
    public static void CreateFile(string directory, string filename)
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        if (!File.Exists(directory + filename))
        {
            File.Create(directory + filename).Dispose();
        }
    }
}
