using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveChildren : MonoBehaviour
{
    public Transform[] movers;
    public bool neg;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < movers.Length; i++)
        {
            if(movers[i] != null)
                if (!neg)
                    movers[i].position += transform.position;
                else
                    movers[i].position -= transform.position;
        }
        transform.position = Vector3.zero;
    }
}
