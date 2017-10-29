using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Checks the collision of the car with the walls and the checkpoints
public class CarCollisionChecker : MonoBehaviour
{
    [HideInInspector] public bool colliding = false;

    public int pointsPassed = 0;
    public static int bonus = 10;

    private GameObject car1Obj;
    public GameObject checkpointsObj;

    private NotACarController nacc;
    [HideInInspector] public CheckpointController cc;

    private void Awake()
    {
        nacc = transform.parent.parent.GetComponent<NotACarController>();
        cc = checkpointsObj.GetComponent<CheckpointController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Road")
        {
            colliding = true;
        }

        if (other.tag == "Checkpoint")
        {
            int temp = 0;
            int.TryParse(other.transform.name, out temp);

            if (cc.currActive[nacc.carID] == temp)
            {
                pointsPassed++;
                nacc.BonusCalc(pointsPassed * bonus); // Calculate the total bonus
                nacc.checkerStatus = cc.ConfigureCheckers(nacc.checkerStatus, nacc.carID); // Enable and disable the necessary checkpoints
            }   
        }

        if (other.tag == "LapDetector")
        {
            nacc.numOfLaps++;
        }
    }
}
