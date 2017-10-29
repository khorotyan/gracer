using System.Linq;
using UnityEngine;

// Only enables the checkpoint in front of the player
public class CheckpointController : MonoBehaviour
{
    public static int checkpointCount; // Number of checkpoints in the race track  
    [HideInInspector] public int[] currActive; // Current active checkpoint

    private void Awake()
    {
        checkpointCount = transform.childCount;
        currActive = new int[MainController.numOfCars];

        currActive = currActive.Select(item => 0).ToArray();
    }

    private void Update()
    {
        
    }

    // Resent all the checkpoints to its initial state
    public int[] ResetCheckpoints(int[] statuses, int carID)
    {
        currActive[carID] = 0;
        statuses[0] = 1;

        for (int i = 1; i < checkpointCount; i++)
        {
            statuses[i] = 0;
        }

        return statuses;
    }

    // Enable and disable the necessary checkpoints
    public int[] ConfigureCheckers(int[] statuses, int carID)
    {
        statuses[currActive[carID]] = 0;

        currActive[carID]++;

        if (currActive[carID] == checkpointCount)
        {
            currActive[carID] = 0;
        }

        statuses[currActive[carID]] = 1;

        return statuses;
    }
}
