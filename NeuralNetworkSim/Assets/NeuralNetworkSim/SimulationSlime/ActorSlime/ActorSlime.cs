using System.Collections.Generic;
using UnityEngine;

/*
 * ActorSlime Class
 * Description : Actor slime class used within the slime simulation
*/
public class ActorSlime : Actor
{
    //Sprites for the body
    [Header("GameObjects")]
    SpriteRenderer outlineRender;
    [SerializeField]
    SpriteRenderer bodyRender;

    [Header("Movement")]
    //Movespeed
    [SerializeField]
    float moveSpeed = 1.0f;
    //Current direction the actor is moving
    [SerializeField]
    Vector2 moveDirection = new Vector2(0, 0);
    //Store current position for multi-threading
    Vector3 currentPosition;

    //Senses
    [Header("Senses")]
    //All food sensed
    [SerializeField]
    List<GameObject> foodSensed = new List<GameObject>();
    //Closest food
    [SerializeField]
    GameObject closestFood = null;
    Vector3 lastFoodPosition;
    //Change target parameters
    int delayChangeTarget = 60;
    int currentDelayChangeTarget = 60;

    //Current hunger
    int hunger = 0;
    [Header("Hunger")]
    //Hunger starve time
    [SerializeField]
    int starvationTime = 500;

    [Header("Points")]
    //Points awarded for food collection
    [SerializeField]
    float foodScore = 10;

    //When the simulation starts
    void Start()
    {
        outlineRender = GetComponent<SpriteRenderer>();
    }

    //Setup of the actor slime
    public void Setup(Genome genome)
    {
        //Set genome and update colour and size
        SetGenome(genome);
        UpdateColour();
        UpdateSize();
        //Calculate current food
        CalculateSenses();
    }

    //Simulation loop
    void FixedUpdate()
    {
        //Ensure the simulation is running
        if (SimulationSlime.instance.IsSimulating())
        {
            //Update closest food
            CalculateSenses();
            //Move
            Move();

            //Check for starvation
            if (hunger >= starvationTime)
            {
                Death();
            }

            hunger++;
        }
    }

    //Calculate all senses
    void CalculateSenses()
    {
        //Calculate the closest food
        ClosestFood();
        if (closestFood != null)
        {
            //Update position
            lastFoodPosition = closestFood.transform.position;
        }
    }

    //Movement logic
    void Move()
    {
        //Update the position of the slime
        transform.position += new Vector3(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed, 0);
        currentPosition = transform.position;

        //Update appearance matching its direction
        if (moveDirection.x > 0)
        {
            outlineRender.flipX = true;
            bodyRender.flipX = true;
        }
        else if (moveDirection.x < 0)
        {
            outlineRender.flipX = false;
            bodyRender.flipX = false;
        }

        //Check if close enough to food to eat it
        if (closestFood != null)
        {
            float currentDistance = Vector3.Distance(transform.position, closestFood.transform.position);

            //Distance to food before consuming
            //We dont use the size of the slime of the actor as to not produce biased results
            if (currentDistance < 0.2)
            {
                TouchFood(closestFood);
            }
        }
    }

    //Calculate the closest food
    void ClosestFood()
    {
        //Check for enough time delay between changing targets
        if (currentDelayChangeTarget >= delayChangeTarget)
        {
            GameObject result = null;
            float distance = 0.0f;

            //Check through each food sensed for the closest
            foreach (GameObject food in foodSensed)
            {
                if (food == null)
                {
                    continue;
                }

                float currentDistance = Vector3.Distance(transform.position, food.transform.position);
                if (currentDistance < distance || distance == 0.0f)
                {
                    result = food;
                    distance = currentDistance;
                }
            }

            //Update closest food
            closestFood = result;
            //Update change timer delay
            currentDelayChangeTarget = 0;
        }
        else
        {
            //Update change target delay
            currentDelayChangeTarget++;
        }
    }

    //Add sensed food to the list
    public void AddSensedFood(GameObject food)
    {
        if (!foodSensed.Contains(food))
        {
            foodSensed.Add(food);
            ClosestFood();
        }
    }

    //Remove sensed food from the list
    public void RemoveSensedFood(GameObject food)
    {
        foodSensed.Remove(food);
        ClosestFood();
    }

    //Consume food
    public void TouchFood(GameObject foodObject)
    {
        //Ensure the object is the food we are targeting to avoid falsely grabbing wrong food
        if (foodObject == closestFood)
        {
            SimulationSlime.instance.RemoveFood(foodObject, false);
            Destroy(foodObject);
            hunger = 0;
            //Update fitness score
            Genome g = GetGenome();
            g.SetFitnessScore(g.GetFitnessScore() + foodScore);
            //Check for new food
            CalculateSenses();
        }
    }

    //When the actor has died remove the actor
    public void Death()
    {
        SimulationSlime.instance.RemoveActor(this, false);
    }

    //Update the size of the actor
    void UpdateSize()
    {
        float size = GetGenome().GetSize();
        transform.localScale = new Vector3(size, size, 1);
    }

    //Update the colour of the body of the slime
    void UpdateColour()
    {
        bodyRender.color = GetGenome().GetColor();
    }

    //Get the input values for the genome neural network
    public override float[] GetGenomeInput()
    {
        return new float[] {
            currentPosition.x - lastFoodPosition.x,
            currentPosition.y - lastFoodPosition.y
        };
    }

    //Results outputed from the neural network to change move direction
    public override void GenomeOutput(float[] outputData)
    {
        //Reverse the outputs
        outputData[1] = -outputData[1];
        outputData[2] = -outputData[2];

        moveDirection = new Vector2(outputData[2] + outputData[3], outputData[0] + outputData[1]);
    }
}
