using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform camRider;

    private Vector3 camOffset;

    private void Start()
    {
        camOffset = transform.position - camRider.position;
    }

    private void LateUpdate()
    {
        transform.position = camRider.position + camOffset;
    }
}
