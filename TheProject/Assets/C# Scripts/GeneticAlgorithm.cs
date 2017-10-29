using System;
using System.Collections.Generic;

public class GeneticAlgorithm<T>
{
    public List<DNA<T>> Population { get; private set; }
    public int Generation { get; private set; }
    public float BestFitness { get; private set; }
    public double[] BestGenes { get; private set; }
    public float MutationRate { get; set; }
    public int GenerSurvNum { get; set; }

    private List<DNA<T>> newPopulation;
    private MainController mc;

    public GeneticAlgorithm(int numOfCars, int dnaSize, Random random, Func<double, double> GetRandomGene,
         Func<int, float> FitnessFunction, int generSurvNum, float mutationRate = 0.01f)
    {
        Generation = 1;
        MutationRate = mutationRate;
        GenerSurvNum = generSurvNum;

        Population = new List<DNA<T>>(numOfCars);
        newPopulation = new List<DNA<T>>(numOfCars);

        BestGenes = new double[dnaSize]; // Save the best genes into a file in file for weight initialization later

        // Initializes the genes of the cars (the weights of the neural network)
        for (int i = 0; i < numOfCars; i++)
        {
            Population.Add(new DNA<T>(dnaSize, random, GetRandomGene, FitnessFunction, shouldInitGenes: true));
        }
    }

    // Creates the new generation
    public void CreateNewGeneration()
    {
        if (Population.Count <= 0)
        {
            return;
        }

        CalculateFitness();
        Population.Sort(CompareDNA);
        newPopulation.Clear();

        // Do Selection, Crossover, and Mutate to get the new population
        for (int i = 0; i < Population.Count; i++)
        {
            if (i < GenerSurvNum)
            {
                newPopulation.Add(Population[i]);
            }
            else
            {
                DNA<T> parent1 = ChooseParent(0);
                DNA<T> parent2 = ChooseParent(1);

                DNA<T> child = parent1.Crossover(parent2);

                child.Mutate(MutationRate);

                newPopulation.Add(child);
            }         
        }

        List<DNA<T>> tmp = Population;
        Population = newPopulation;
        newPopulation = tmp;

        Generation++;
    }

    public int CompareDNA(DNA<T> a, DNA<T> b)
    {
        if (a.Fitness > b.Fitness)
        {
            return -1; // First object should come before the second object in the list
        }
        else if (a.Fitness < b.Fitness)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    public void CalculateFitness()
    {
        DNA<T> best = Population[0];

        for (int i = 0; i < Population.Count; i++)
        {
            Population[i].CalculateFitness(i);

            if (Population[i].Fitness > best.Fitness)
            {
                best = Population[i];
            }
        }

        BestFitness = best.Fitness;
        best.Genes.CopyTo(BestGenes, 0);
    }

    private DNA<T> ChooseParent(int id)
    {
        return Population[id];
    }
}
