using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainController : MonoBehaviour
{
    public Transform carsParentTr;
    public Text generationText;
    public Text numOfLapsText;
    public Text bestTravelText;
    public Text speedText;

    public static int numOfCars;
    
    public static float mutationRate = 0.05f;
    private int bestIndNum = 2;

    private int inputs; // ray count + speed
    private int outputs = 3;
    public static int hiddenLayers = 1;
    public static int numOfNeurons = 10; // Total number of neurons
    private int dnaSize;

    public GeneticAlgorithm<double> ga;
    private NotACarController[] carControllers;
    private System.Random random;

    private void Awake()
    {
        Application.runInBackground = true;

        inputs = NotACarController.angles.Length + 1;

        numOfCars = carsParentTr.childCount;
        carControllers = new NotACarController[carsParentTr.childCount];

        for (int i = 0; i < carsParentTr.childCount; i++)
        {
            carControllers[i] = carsParentTr.GetChild(i).GetComponent<NotACarController>();
        }

        int neuPerLayer = numOfNeurons / hiddenLayers;
        dnaSize = (inputs + 1)*neuPerLayer + (hiddenLayers - 1)*(neuPerLayer + 1)*neuPerLayer + outputs*(neuPerLayer + 1);
    }

    private void Start()
    {
        random = new System.Random();
        ga = new GeneticAlgorithm<double>(numOfCars, dnaSize, random, GetRandomGene, FitnessFunction, bestIndNum, mutationRate);

        for (int i = 0; i < ga.Population.Count; i++)
        {
            NotACarController.weightsList.Add(InitWeights(ga.Population[i]));
        }

        generationText.text = "Generation: " + 1;
    }

    private void Update()
    {
        Debug.Log(NotACarController.totalLost);
        if (NotACarController.totalLost == numOfCars)
        {
            for (int i = 0; i < carsParentTr.childCount; i++)
            {
                carsParentTr.GetChild(i).GetComponent<NotACarController>().Restart();
            }

            NotACarController.weightsList.Clear();
            NotACarController.totalLost = 0;

            ga.CreateNewGeneration();

            generationText.text = "Generation: " + ga.Generation;

            for (int i = 0; i < ga.Population.Count; i++)
            {
                NotACarController.weightsList.Add(InitWeights(ga.Population[i]));
            }
            //StartCoroutine(NewGeneration(0.5f)); // Create the new generation after some time 
        }

        // Show the travel distance and lap of the best car
        float highestFitness = 0;
        int bestCarID = 0;

        for (int i = 0; i < carsParentTr.childCount; i++)
        {
            if (carControllers[i].fitness > highestFitness)
            {
                highestFitness = carControllers[i].fitness;
                bestCarID = i;
            }
        }

        Camera.main.GetComponent<CameraController>().camRider = carsParentTr.GetChild(bestCarID);

        numOfLapsText.text = "Lap Number: " + carControllers[bestCarID].numOfLaps.ToString();
        bestTravelText.text = "Best #" + (bestCarID + 1) + " - Total: " + carControllers[bestCarID].totalTravel.ToString("F0") + " Meters" +
            " + " + carControllers[bestCarID].totalBonus;
        speedText.text = "Speed: " + carControllers[bestCarID].currSpeed.ToString("F0");

        // If we click a button, save the weights and stop the simulation

        //if (Input.GetKeyDown(KeyCode.Escape))
    }

    IEnumerator NewGeneration(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        for (int i = 0; i < carsParentTr.childCount; i++)
        {
            carsParentTr.GetChild(i).GetComponent<NotACarController>().Restart();
        }

        NotACarController.weightsList.Clear();
        NotACarController.totalLost = 0;

        ga.CreateNewGeneration();

        generationText.text = "Generation: " + ga.Generation;

        for (int i = 0; i < ga.Population.Count; i++)
        {
            NotACarController.weightsList.Add(InitWeights(ga.Population[i]));
        }
    }

    // Convert the DNA into neural network readable format
    public double[][,] InitWeights(DNA<double> dna)
    {
        double[][,] weights = new double[hiddenLayers + 1][,];
        int counter = 0;

        for (int k = 0; k < hiddenLayers + 1; k++)
        {
            int neuronsInEachLayer = numOfNeurons / hiddenLayers;

            int rowLen = 0;
            int colLen = 0;

            if (k == 0)
            {
                rowLen = neuronsInEachLayer;
                colLen = inputs + 1;
            }
            else if (k == hiddenLayers)
            {
                rowLen = outputs;
                colLen = neuronsInEachLayer + 1;               
            }
            else
            {
                rowLen = neuronsInEachLayer;
                colLen = neuronsInEachLayer + 1;
            }

            weights[k] = new double[rowLen, colLen];

            for (int i = 0; i < rowLen; i++)
            {
                for (int j = 0; j < colLen; j++)
                {
                    weights[k][i, j] = dna.Genes[counter];

                    counter++;
                }
            }
        }

        return weights;
    }

    // Create a random neuron initializer
    private double GetRandomGene(double gene = 0)
    {
        // If a gene is given, then add a random noise to it, if not then initialize
        //double rand = System.Math.Round(gene + (gene == 0 ? Random.Range(-10f, 10f) : Random.Range(-5f, 5f)), 1);
        double rand = System.Math.Round(gene == 0 ? Random.Range(-1f, 1f) : (gene * Random.Range(0.5f, 1.5f)), 2);

        return rand;
    }

    private float FitnessFunction(int index)
    {
        float fitness = carsParentTr.GetChild(index).GetComponent<NotACarController>().fitness;

        return fitness;
    }
}