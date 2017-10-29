using System;

public class DNA<T>
{
    public double[] Genes { get; private set; }
    public float Fitness { get; private set; }

    private Random random;
    private Func<double, double> GetRandomGene;
    private Func<int, float> FitnessFunction;
    
    public DNA(int size, Random random, Func<double, double> GetRandomGene, Func<int, float> FitnessFunction, 
        bool shouldInitGenes = true)
    {
        Genes = new double[size];
        this.random = random;
        this.GetRandomGene = GetRandomGene;
        this.FitnessFunction = FitnessFunction;

        // Initialize the genes
        if (shouldInitGenes == true)
        {
            for (int i = 0; i < Genes.Length; i++)
            {
                Genes[i] = GetRandomGene(0);
            }
        }
    }

    // Natural Selection, how likely an individual is to reproduce
    public float CalculateFitness(int index)
    {
        Fitness = FitnessFunction(index);

        return Fitness;
    }

    // Crossover, returns a new DNA from 2 parents
    public DNA<T> Crossover(DNA<T> parent2)
    {
        DNA<T> child = new DNA<T>(Genes.Length, random, GetRandomGene, FitnessFunction, shouldInitGenes: false);

        for (int i = 0; i < Genes.Length; i++)
        {
            child.Genes[i] = random.NextDouble() < 0.5 ? Genes[i] : parent2.Genes[i];
        }

        return child;
    }

    // Mutation, mutationRate - how likely it is to mutate a gene
    public void Mutate(float mutationRate)
    {
        // Goes through all the genes and decides whether it will have a mutation
        for (int i = 0; i < Genes.Length; i++)
        {
            if (random.NextDouble() < mutationRate)
            {
                Genes[i] = GetRandomGene(Genes[i]);
            }
        }
    }
}
