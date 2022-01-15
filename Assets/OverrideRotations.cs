using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverrideRotations : MonoBehaviour
{
    public Transform[] transforms;
    public Transform rotatedTransform;
    public float rotationDeg = 30f;

    private void Awake()
    {
        float rotY = rotatedTransform.rotation.eulerAngles.y;
        rotatedTransform.rotation = Quaternion.identity;
        for(int i = 0; i < transforms.Length; i++)
        {
            transforms[i].rotation = Quaternion.Euler(new Vector3(0, rotY, 0));
        }
    }
}
