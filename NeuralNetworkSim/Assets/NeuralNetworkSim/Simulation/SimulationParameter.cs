using UnityEngine;

/*
 * SimulationParameter Class
 * Description : This class is used for the different parameters for the different simulation experiments
*/
[CreateAssetMenu(fileName = "SimulationParameter", menuName = "ScriptableObjects/SimulationParameter", order = 1)]
public class SimulationParameter : ScriptableObject
{
    public string simulationName = "Base";
    public float speciesDistance = 0.5f;
    public float weightAdjust = 0.5f;
    public float mutationChance = 0.5f;
    public int startingMutations = 0;
    public int startingRank = 4;
    public int rankLoseRoundStart = 1;
    public int rankGainedMultiplier = 1;
}
