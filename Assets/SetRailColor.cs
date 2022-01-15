using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetRailColor : MonoBehaviour
{
    public Color _teamColor;

    // Start is called before the first frame update
    void Start()
    {
        print(GetComponent<Renderer>().material.GetColor("_EmissionColor"));
        GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(_teamColor.r * .65f, _teamColor.g * .65f, _teamColor.b * .65f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
