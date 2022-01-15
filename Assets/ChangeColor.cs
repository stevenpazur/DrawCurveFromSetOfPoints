using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    public RiderailMeshGenerator riderailMesh;
    public GlobalRailVariables grv;
    public bool actedOn;
    public string activated;
    public string color1;
    public string color2;
    public string baseTexture;
    public Material M_paint, Riderail;
    public Texture2D deactivatedTexture, activatedTexture;

    void Update()
    {
        Material sphereMaterial;
        

        if (actedOn)
        {
            var sphere = FindSphere();
            if (sphere != null)
            {
                sphereMaterial = sphere.GetComponent<Renderer>().material;
                sphereMaterial.SetFloat(activated, 1);
                sphereMaterial.SetTexture(baseTexture, activatedTexture);
                //riderailMesh.GetComponent<Renderer>().material.SetColor("_EmissionColor", grv._teamColor);
            }
        }
        else
        {
            var sphere = FindSphere();
            if(sphere != null)
            {
                sphereMaterial = sphere.GetComponent<Renderer>().material;
                sphereMaterial.SetFloat(activated, 0);
                sphereMaterial.SetTexture(baseTexture, deactivatedTexture);
                //riderailMesh.GetComponent<Renderer>().material.SetColor("_EmissionColor", grv._teamColor2);
            }
        }
    }

    Transform FindSphere()
    {
        for(int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).tag == "Sphere")
            {
                return transform.GetChild(i);
            }
        }
        return null;
    }
}
