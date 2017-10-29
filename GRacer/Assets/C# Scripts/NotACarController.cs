using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics;

// Contains the car controller, and the fitness function
public class NotACarController : MonoBehaviour
{
    public GameObject checkpointsObj;

    [HideInInspector] public int carID;
    public bool selfDriving = true;

    [HideInInspector] public int[] checkerStatus; // Checkpoint statuses

    public float currSpeed = 0;
    private float speed = 10;
    private float turnSpeed = 70;
    private float visionLen = 4.2f;
    private bool canDrive = true;

    [HideInInspector] public float totalTravel = 0;
    [HideInInspector] public float totalBonus = 0;
    public float fitness = 0;
    [HideInInspector] public int numOfLaps = 0;

    public static int totalLost = 0;

    public static float[] angles = new float[] { -55, 55 };
    public static List<double[][,]> weightsList;

    private Vector3 prevPos = Vector3.zero; // Position of the car in the previous frame
    private Vector3 defPos; // Default position of the car
    private Quaternion defRot; // Default rotation of the car
    private CarCollisionChecker carColl; // Reference to the collision script of the car

    private float stepSize = 0.59f;
    private float horizontal = 0;
    private float vertical = 0;
    //private int steps = 17;

    private Vector3 currPos = Vector3.zero;
    private float totalTime = 0;

    private UnityStandardAssets.Vehicles.Car.CarController m_Car;
    public MainController mc;

    private void Awake()
    {
        m_Car = GetComponent<UnityStandardAssets.Vehicles.Car.CarController>();

        weightsList = new List<double[][,]>();

        carColl = transform.GetChild(0).GetChild(0).gameObject.GetComponent<CarCollisionChecker>();

        defPos = transform.position;
        defRot = transform.rotation;
    }

    private void Start()
    {
        checkerStatus = new int[CheckpointController.checkpointCount];

        carID = transform.GetSiblingIndex();
        checkerStatus = carColl.cc.ResetCheckpoints(checkerStatus, carID);
    }

    private void Update()
    {
        if (canDrive)
        {
            //Drive();
            BetterDrive();
            CheckCarProgress();
        }

        FitnessFunction();     

        if (canDrive == true && carColl.colliding == true)
        {
            horizontal = 0;
            vertical = 0;
            //totalTime = 0;

            canDrive = false;
            totalLost++;
        } 
    }

    // Calculates the total traveled distance
    private void TravelDistanceCalc()
    {
        totalTravel += prevPos == Vector3.zero ? 0 : Vector3.Distance(prevPos, transform.position);

        prevPos = transform.position;
    }

    // Updates the bonus whenever the car collides with a checkpoint from "CarCollisionChecker" class
    public void BonusCalc(int bonusAmount)
    {
        totalBonus += bonusAmount;
    }

    // The fitness function
    private void FitnessFunction()
    {
        TravelDistanceCalc();

        fitness = totalTravel + totalBonus;
    }

    private void CheckCarProgress()
    {
        if (totalTime < 6)
        {
            if (totalTime > 2)
            {
                if (Vector3.Distance(currPos, transform.position) < 0.5f)
                {
                    carColl.colliding = true;
                    Debug.Log("Terminated Car: " + (carID + 1));
                }
            }

            totalTime += 1 * Time.deltaTime;
        }
        else
        {
            // Stop the car if it does not make progress
            if (Vector3.Distance(currPos, transform.position) < 3)
            {
                carColl.colliding = true;
                Debug.Log("Terminated Car: #" + (carID + 1));
            }

            currPos = transform.position;

            totalTime = 0;
        }   
    }

    // Restart the game for this car
    public void Restart()
    {
        totalTravel = 0;
        totalBonus = 0;
        numOfLaps = 0;
        prevPos = Vector3.zero;

        transform.position = defPos;
        transform.rotation = defRot;

        carColl.colliding = false;
        canDrive = true;

        carColl.pointsPassed = 0;
        checkerStatus = carColl.cc.ResetCheckpoints(checkerStatus, carID);

        //StartCoroutine(WaitSomeTime(0.3f));
    }

    // Waits for "seconds" seconds then lets the car to drive, used whenever restarted
    IEnumerator WaitSomeTime(float seconds)
    {
        yield return new WaitForSeconds(seconds);

        canDrive = true;
    }

    // Car Controller
    private void Drive()
    {
        transform.Translate(/*Input.GetAxis("Vertical") **/ 1 * transform.forward * speed * Time.deltaTime, Space.World);

        if (selfDriving == false)
        {
            transform.Rotate(new Vector3(0, 1 * Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime, 0), Space.World);
        }
        else
        {
            //transform.Rotate(new Vector3(0, 1 * Predict(activationOnLast: true) * turnSpeed * Time.deltaTime, 0), Space.World);
        }

        DoRaycast();
    }

    private void BetterDrive()
    {
        if (selfDriving == false)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            m_Car.Move(h, v, v, 0);
        }
        else
        {
            float[] preds = Predict(activationOnLast: true, lastActivationFunc: "TanH");

            float predictH = preds[0];
            horizontal += FixPrediction(predictH, horizontal, "TanH");

            float predictV = preds[1];
            vertical += FixPrediction(predictV, vertical, "TanH");

            float predBrake = preds[2] > 0 ? 1 : 0;

            m_Car.Move(horizontal, vertical, vertical, predBrake);
        }

        DoRaycast();
    }

    // Configures the key that must be pressed and the way car pedal push/release works
    private float FixPrediction(float prediction, float axisVel, string lastActivationFunc)
    {
        float keyOption = 0;
        
        if (lastActivationFunc == "Sigmoid")
            keyOption = prediction > 0.66f ? 1 : (prediction > 0.33 ? 0 : -1);
        else if (lastActivationFunc == "TanH")
            keyOption = prediction < -0.33f ? -1 : (prediction > 0.33 ? 1 : 0);

        if (keyOption == 0)
        {
            if (Mathf.Abs(axisVel) <= stepSize)
            {
                return -1 * Mathf.Sign(axisVel) * Mathf.Abs(axisVel);
            }
            else
            {
                return -1 * Mathf.Sign(axisVel) * stepSize;
            }
        }
        else 
        {
            float velChange = prediction * stepSize;

            if (Mathf.Abs(axisVel + velChange) > 1)
            {
                return Mathf.Sign(axisVel) - axisVel;
            }
            else
            {
                return axisVel + velChange;
            }
        }
    }

    private double[] DoRaycast()
    {
        // Inputs are the rays and the movement speed
        double[] inputData = new double[angles.Length + 1];
        //double[] inputData = new double[angles.Length];

        int layer_mask = LayerMask.GetMask("Road");
        Ray[] ray = new Ray[angles.Length];
        RaycastHit[] raycastHit = new RaycastHit[angles.Length];

        // Middle ray is longer to detect an upcoming rotation so that the car could slow down
        for (int i = 0; i < ray.Length; i++)
        {
            ray[i].origin = transform.position + transform.forward * 1.3f;
            ray[i].direction = new Vector3(Mathf.Sin(Mathf.Deg2Rad * (angles[i] + transform.rotation.eulerAngles.y)), 0, Mathf.Cos(Mathf.Deg2Rad * (angles[i] + transform.rotation.eulerAngles.y)));

            float currVisionLen = visionLen;
            float currMultiplier = 1.0f;

            /*
            if (i == angles.Length / 2)
            {
                currVisionLen = 0.08f * visionLen;
                currMultiplier = 0.08f;
            }
            */

            if (Physics.Raycast(ray[i], out raycastHit[i], currVisionLen, layer_mask))
            {
                if (raycastHit[i].transform.tag == "Road")
                {
                    Debug.DrawRay(ray[i].origin, ray[i].direction * currVisionLen, Color.red, 0.02f);
                    inputData[i] = Vector3.Distance(ray[i].origin, raycastHit[i].point) / currMultiplier;
                }
                else
                {
                    Debug.DrawRay(ray[i].origin, ray[i].direction * currVisionLen, Color.green, 0.02f);
                    inputData[i] = currVisionLen / currMultiplier;
                }
            }
            else
            {
                Debug.DrawRay(ray[i].origin, ray[i].direction * currVisionLen, Color.green, 0.02f);
                inputData[i] = currVisionLen / currMultiplier;
            }
        }

        inputData[inputData.Length - 1] = currSpeed / 8;

        return inputData;
    }

    public void GetCarSpeed(float speed)
    {
        currSpeed = speed;
    }

    // Forward Propagation
    private float[] Predict(bool activationOnLast = false, string lastActivationFunc = "Linear")
    {
        double[][,] weights = weightsList[carID];
        double[] inputData = DoRaycast();
        double[,] inputD = new double[inputData.Length + 1, 1];
        inputD[0, 0] = 1.0;

        for (int i = 1; i < inputData.Length + 1; i++)
        {
            inputD[i, 0] = inputData[i - 1];
        }

        Matrix<double> input = DenseMatrix.OfArray(inputD);
        Matrix<double> theta = DenseMatrix.OfArray(weights[0]);
        // The activation function can also be learned by the genetic algorithm
        Matrix<double> a = ActivationFunc((theta * input), name: "TanH"); 

        for (int i = 1; i < weights.Length - 1; i++)
        {
            theta = DenseMatrix.OfArray(weights[i]);
            a = ActivationFunc((theta * a.InsertRow(0, DenseVector.OfArray(new double[] { 1 }))), name: "TanH");
        }

        theta = DenseMatrix.OfArray(weights[weights.Length - 1]);

        if (activationOnLast == false)
            a = ((theta * a.InsertRow(0, DenseVector.OfArray(new double[] { 1 }))));
        else
            a = ActivationFunc((theta * a.InsertRow(0, DenseVector.OfArray(new double[] { 1 }))), name: lastActivationFunc);

        float[] prediction = new float[3];
        prediction[0] = (float)a.ToArray()[0, 0]; // Wheel
        prediction[1] = (float)a.ToArray()[1, 0]; // Pedals
        prediction[1] = (float)a.ToArray()[2, 0]; // Brakes

        return prediction;
    }  

    // The activation function for the forward propagation
    private Matrix<double> ActivationFunc(Matrix<double> x, string name)
    {
        if (name == "Sigmoid")
            return 1 / (1 + Matrix.Exp(-x));
        else if (name == "TanH")
            return Matrix.Tanh(x);
        else
            return x;
    }
}
