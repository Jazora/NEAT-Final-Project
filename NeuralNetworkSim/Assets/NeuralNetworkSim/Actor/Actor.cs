using UnityEngine;

/*
 * Actor Class
 * Description : Actor class used within Unity's GameObjects
*/
public class Actor : MonoBehaviour
{
    //Genome of the actor
    Genome genome;

    //Get the genome of the actor
    public Genome GetGenome()
    {
        return genome;
    }

    //Set the genome of the actor
    public void SetGenome(Genome genome)
    {
        this.genome = genome;
    }

    //Input layer of the genomes neural network
    public virtual float[] GetGenomeInput()
    {
        return new float[0] {};
    }

    //Output layer of the genomes neural network
    public virtual void GenomeOutput(float[] outputData)
    {

    }
}
