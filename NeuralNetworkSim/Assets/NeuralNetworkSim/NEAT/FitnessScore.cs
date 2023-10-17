
/*
 * FitnessScore Class
 * Description : This abstract class is used for fitness score properties
*/
public abstract class FitnessScore
{
    //Current fitness score
    float fitnessScore = 0.0f;

    //Get fitness score
    public float GetFitnessScore()
    {
        return fitnessScore;
    }

    //Set fitness score
    public void SetFitnessScore(float value)
    {
        fitnessScore = value;
    }
}
