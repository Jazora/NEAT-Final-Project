using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/*
 * SimulationSlime Class
 * Description : The simulation script for the slime simulation
*/
public class SimulationSlime : Simulation
{
    //Singleton to the slime simulation
    public static SimulationSlime instance;

    //Amount of wins required in the slime simulation
    [SerializeField]
    int winCount = 3;
    int currentWinCount = 0;

    //Amount of food that spawns
    [SerializeField]
    int foodCount = 5;

    //Debug move to enable integration testing
    [Header("Debug")]
    [SerializeField]
    bool debug = false;

    //Gameobjects of the slime and food gameobjects
    [Header("GameObjects")]
    [SerializeField]
    GameObject actorGameObject;
    [SerializeField]
    GameObject foodGameObject;

    //history of previous genomes of the round
    List<Genome> genomeHistory = new List<Genome>();

    //List of slimes and food in the current simulation
    List<ActorSlime> slimeList = new List<ActorSlime>();
    List<GameObject> foodList = new List<GameObject>();

    //If the current simulation is running, pause during the NEATGraph
    bool simulationRunning = false;
    //UI components
    [Header("UI")]
    [SerializeField]
    Toggle continuousSimulation;
    [SerializeField]
    Text toggleButtonText;
    [SerializeField]
    Text roundTimeText;
    //Generation round time
    [SerializeField]
    float roundTime = 60.0f;
    float currentRoundTime;

    //When the simulation starts
    void Start()
    {
        //Set singleton
        instance = this;
        //Load the data of the neural network
        GetNeuralNetwork().Load();

        // Run integration testing
        if (debug) 
        {
            //Disable UI to show integration tests are running
            toggleButtonText.transform.parent.gameObject.SetActive(false);
            continuousSimulation.transform.gameObject.SetActive(false);
            gameObject.AddComponent<NEATTest>();
        }
    }

    //When the simulation ticks
    void Update()
    {
        //Check for user mouse input 
        if (Input.GetMouseButtonDown(0))
        {
            NEATGraph nGraph = NEATGraph.instance;

            if (nGraph.IsGraphDisplayed())
            {
                if (!simulationRunning)
                {
                    simulationRunning = true;
                }

                nGraph.ClearDisplay();
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D[] hitObjects = Physics2D.RaycastAll(ray.origin, ray.direction);

            //Render the genome of the actors selected
            foreach (RaycastHit2D hitObj in hitObjects)
            {
                if (hitObj.transform != null)
                {
                    Collider2D actorObj = hitObj.collider;
                    if (actorObj.tag == "Interact") // confirm we hit an actor
                    {
                        simulationRunning = false;
                        nGraph.RenderGenome(actorObj.GetComponentInParent<ActorSlime>());
                    }
                }
            }
        }

        //If the NEATGraph is open and 'M' is pressed mutate the genome
        if (Input.GetKeyDown(KeyCode.M))
        {
            NEATGraph nGraph = NEATGraph.instance;

            if (nGraph.IsGraphDisplayed())
            {
                nGraph.MutateButton();
            }
        }
    }

    //Fixed tick update, Multi-threaded
    async void FixedUpdate()
    {
        //Ensure we are simulating
        if (IsSimulating())
        {
            //Update round timer
            UpdateTimer();

            //If all slimes are dead end round
            if (slimeList.Count == 0)
            {
                currentWinCount = 0;
                EndSimLoop();
            }

            // found actor that can survive
            if (currentRoundTime <= 0) 
            {
                currentWinCount++;

                if (currentWinCount >= winCount)
                {
                    EndSim();

                    // start new simulation instance or new parameters
                    if (StartNewSimulationType()) 
                    {
                        StartSimLoop();
                    }
                }
                else
                {
                    EndSimLoop();
                }
            }

            //Multi-threaded calculating the output of each genome
            Task[] task = new Task[slimeList.Count];
            for (int i = 0; i < slimeList.Count; i++)
            {
                //Calculate the output of the genome
                task[i] = slimeList[i].GetGenome().CalculateOutput(slimeList[i], slimeList[i].GetGenomeInput());
            }
            //Wait until all outputs have been calculated
            await Task.WhenAll(task);
        }
    }

    //Update the round timer
    void UpdateTimer()
    {
        currentRoundTime -= Time.fixedDeltaTime;
        roundTimeText.text = currentRoundTime.ToString();
    }

    //Toggle the simulation, Used for UI buttons
    public void SimulationToggle()
    {
        if (IsSimulating())
        {
            EndSimLoop();
        }
        else
        {
            StartSimLoop();
        }
    }

    //Start new simulation loop
    void StartSimLoop()
    {
        //Spawn all the food
        for (int i = 0; i < foodCount; i++)
        {
            SpawnRandomFood();
        }

        //Load all genome species
        LoadAllSpecies();

        //Update UI
        currentRoundTime = Time.fixedDeltaTime + roundTime;
        simulationRunning = true;
        toggleButtonText.text = "End";

        //Add to the generation count
        GetNeuralNetwork().AddGenerationCount(1);
    }

    //End the simulation
    void EndSim()
    {
        simulationRunning = false;
        //Clear the simulation screen
        ClearSimulation();
        //Update UI
        toggleButtonText.text = "Start";
        roundTimeText.text = "";

        //Evaluate the fitness of all the genomes
        GetNeuralNetwork().EvaluateFitness(genomeHistory.ToArray());
        //Clear the history
        genomeHistory.Clear();
    }

    //End simulation with the chance of continueing simulating
    void EndSimLoop()
    {
        EndSim();

        //Check if simulation is still to run
        if (continuousSimulation.isOn)
        {
            StartSimLoop();
        }
    }

    //Is the simulation running
    public bool IsSimulating()
    {
        return simulationRunning;
    }

    //Spawn the slime actors
    public override void SpawnActor(Genome genome, Vector3 position)
    {
        //Create the actors
        GameObject objActor = Instantiate(actorGameObject, position, Quaternion.identity);
        ActorSlime actor = objActor.GetComponent<ActorSlime>();

        //Setup the actors genomes
        actor.Setup(genome);
        //Add to the slime list
        slimeList.Add(objActor.GetComponent<ActorSlime>());
    }

    //Remove an actor from the simulation
    public void RemoveActor(ActorSlime actor, bool clear)
    {
        //Add the actor genome to the history
        genomeHistory.Add(actor.GetGenome());

        //If the actor is being removed other than from the ClearSimulation method
        //Used when the actor is killed from starvation
        if (!clear)
        {
            slimeList.Remove(actor);
        }

        //Destroy the actor object
        Destroy(actor.gameObject);
    }

    //Spawn food in a random camera position
    void SpawnRandomFood()
    {
        Vector2 randomCameraPosition = Camera.main.ViewportToWorldPoint(new Vector2(Random.value, Random.value));
        SpawnFood(new Vector3(randomCameraPosition.x, randomCameraPosition.y, 0));
    }

    //Spawn new food type without going over the limit
    void SpawnFood(Vector3 position)
    {
        if (foodList.Count < foodCount)
        {
            foodList.Add(Instantiate(foodGameObject, position, Quaternion.identity));
        }
    }

    //Remove the food from the simulation
    public void RemoveFood(GameObject food, bool clear)
    {
        //If the food is being removed other than from the ClearSimulation method
        //Used when the actor picks up food
        if (!clear)
        {
            foodList.Remove(food);
            //Spawn new food
            SpawnRandomFood();
        }

        Destroy(food);
    }

    //Clear the simulation screen
    void ClearSimulation()
    {
        //Clear all slimes
        foreach (ActorSlime act in slimeList)
        {
            RemoveActor(act, true);
        }

        slimeList.Clear();

        //Clear all food
        foreach (GameObject fd in foodList)
        {
            RemoveFood(fd, true);
        }

        foodList.Clear();
    }
}
