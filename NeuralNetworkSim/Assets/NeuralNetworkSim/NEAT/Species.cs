using System.Collections.Generic;

/*
 * SpeciesSort Class
 * Description : This class is used to sort species by their fitness score
*/
public class SpeciesSort : IComparer<Species>
{
    public int Compare(Species s1, Species s2)
    {
        if (s1.GetFitnessScore() == s2.GetFitnessScore())
        {
            return 0;
        }
        else if (s1.GetFitnessScore() > s2.GetFitnessScore())
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
 * SpeciesSort Class
 * Description : This class is used to sort species by their fitness score in reverse
*/
public class SpeciesSortReverse : IComparer<Species>
{
    public int Compare(Species s1, Species s2)
    {
        if (s1.GetFitnessScore() == s2.GetFitnessScore())
        {
            return 0;
        }
        else if (s1.GetFitnessScore() > s2.GetFitnessScore())
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }
}

/*
 * Species Class
 * Description : Species determined by species distance within the neural network
*/
public class Species : FitnessScore
{
    //List of genomes of the current species
    List<Genome> genomes = new List<Genome>();

    //Constructor
    public Species(Genome genome)
    {
        genomes.Add(genome);
    }

    //Add genome to species
    public void AddGenome(Genome genome)
    {
        genomes.Add(genome);
    }

    //Get the genomes of the species
    public List<Genome> GetGenomes()
    {
        return genomes;
    }
}
