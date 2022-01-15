using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public Vector3 rotateDirection;
    public float rotateSpeed;

    private void Update()
    {
        transform.Rotate(rotateDirection * rotateSpeed * Time.deltaTime);
    }
}
