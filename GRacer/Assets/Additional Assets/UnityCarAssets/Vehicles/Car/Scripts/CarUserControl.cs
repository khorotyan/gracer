using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
    [RequireComponent(typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        private float time = 0;
        private bool start = false;
        private CarController m_Car; // the car controller we want to use

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            // get the car controller
            m_Car = GetComponent<CarController>();
        }

        private void FixedUpdate()
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                start = true;
            }

            // pass the input to the car!
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            float handbrake = CrossPlatformInputManager.GetAxis("Jump");
            
            gg(v);
            m_Car.Move(h, v, v, 0);
        }

        private void gg(float value)
        {
            if (start == true)
            {
                Debug.Log(value);
                /*
                if (time < 0.1)
                    time += 1 * Time.deltaTime;
                else
                {
                    Debug.Log(value);
                    time = 0;
                }
                */
            }
        }
    }
}
