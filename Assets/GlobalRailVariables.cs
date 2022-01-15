using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalRailVariables : MonoBehaviour
{
    public bool railState = false;
    public Color _teamColor;
    public Color _teamColor2;
    public Transform triggerObject;
    public ChangeColor colorChanger;
    public RiderailMeshGenerator myMesh;

    private void Start()
    {
        Material mat = colorChanger.M_paint;
        mat.SetColor(colorChanger.color1, _teamColor);
        mat.SetColor(colorChanger.color2, _teamColor2);
        Material mat2 = colorChanger.Riderail;
        mat2.SetColor("_EmissionColor", _teamColor);
    }

    public void FlipSwitch()
    {
        railState = !railState;
        colorChanger.actedOn = railState;

        MeshCollider meshCollider = myMesh.GetComponent<MeshCollider>();
        Renderer meshRenderer = myMesh.GetComponent<Renderer>();
        RenderLine renderLine = transform.parent.GetComponentInChildren<RenderLine>();
        int endPoint = renderLine.subdivisions * renderLine.route.childCount + 1;

        meshRenderer.enabled = railState;
        if(renderLine.endpieceInstance != null)
            renderLine.endpieceInstance.gameObject.SetActive(railState);
        if (meshCollider != null)
        {
            meshCollider.enabled = railState;
        }
        renderLine.pointIndex = endPoint;
    }
}
