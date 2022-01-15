using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToTheLeftToTheRight : MonoBehaviour
{
    public Transform railParent, railOffset;
    private Vector3 transform2;

    // left side
    public Transform leftRoute;
    private Vector3[] leftPoints;
    public Transform dollycartLeft;
    public float tLeft = 0;

    public Transform rightRoute;
    private Vector3[] rightPoints;
    public Transform dollycartRight;
    public float tRight;

    private void Start()
    {
        transform2 = railOffset.position;
        leftPoints = new Vector3[leftRoute.childCount];

        for(int i = 0; i < leftRoute.childCount; i++)
        {
            leftPoints[i] = leftRoute.GetChild(i).position;
        }

        rightPoints = new Vector3[rightRoute.childCount];

        for(int i = 0; i < rightRoute.childCount; i++)
        {
            rightPoints[i] = rightRoute.GetChild(i).position;
        }
    }

    private void Update()
    {
        float uLeft = 1 - tLeft;

        Vector3 targetPosLeft = uLeft * uLeft * leftPoints[0] + 2 * uLeft * tLeft * leftPoints[1] + tLeft * tLeft * leftPoints[2] + leftRoute.position - railParent.position - transform2;
        dollycartLeft.position = targetPosLeft;

        float uRight = 1 - tRight;

        Vector3 targetPosRight = uRight * uRight * rightPoints[0] + 2 * uRight * tRight * rightPoints[1] + tRight * tRight * rightPoints[2] + rightRoute.position - railParent.position - transform2;
        dollycartRight.position = targetPosRight;
    }
}
