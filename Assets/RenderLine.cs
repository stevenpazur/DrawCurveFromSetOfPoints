using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderLine : MonoBehaviour
{
    //public float railWidth = 1;
    public int subdivisions = 4;
    public float railDisperseSpeed = 1;
    public Transform endpiece;
    public Transform starterInstance;
    [HideInInspector] public Transform endpieceInstance;
    public Transform route;
    public GlobalRailVariables grv;
    public RiderailMeshGenerator meshGenerator;
    private List<Vector3> points, lastPos;

    public LineRenderer lineRenderer;
    private float vertexCount;

    private bool placedStarter, placedEndpiece = false;

    [HideInInspector] public int pointIndex;
    private List<Vector3> pointListG;

    private float animationFrequency;

    [HideInInspector]
    public List<Vector3> pList;

    private void Start()
    {
        points = new List<Vector3>();
        lastPos = new List<Vector3>();
        LoadPoints();
    }

    private void LoadPoints()
    {
        points.Clear();

        for(int x = 0; x < route.childCount; x++)
        {
            points.Add(route.GetChild(x).position);
        }
        lastPos = points;

        vertexCount = points.Count * subdivisions;
    }

    private void UpdatePointLocations()
    {
        for(int x = 0; x < points.Count; x++)
        {
            if(route.GetChild(x).position != lastPos[x])
            {
                LoadPoints();
            }
        }
    }

    private void Update()
    {
        switch (grv.railState)
        {
            case true:
                UpdatePointLocations();

                var pointList = new List<Vector3>();

                for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
                {
                    var tangents = new List<Vector3>();

                    for (int point = 0; point < points.Count - 1; point++)
                    {
                        var tan = Vector3.Lerp(points[point], points[point + 1], ratio);
                        tangents.Add(tan);
                    }

                    var curves = TangentsToCurves(tangents, ratio);

                    while (curves.Count > 1)
                    {
                        tangents.Clear();
                        for (int curveCount = 0; curveCount < curves.Count; curveCount++)
                        {
                            tangents.Add(curves[curveCount]);
                        }

                        curves.Clear();
                        curves = TangentsToCurves(tangents, ratio);
                    }

                    pointList.Add(curves[0] - grv.transform.position);
                }

                pointListG = pointList;
                //print("count = " + pointListG.Count);
                //lineRenderer.startWidth = railWidth;
                //lineRenderer.endWidth = railWidth;

                // place the starter piece and orient it properly
                if (!placedStarter)
                {
                    // q is the rotation calculated by looking at the first and second rail segments
                    var q = Quaternion.FromToRotation(-Vector3.forward, pointList[0] - pointList[1]);
                    // position and rotate it properly.
                    //starterInstance = Instantiate(starter, pointList[0] + grv.transform.position, q);
                    starterInstance.SetParent(route.parent);

                    // drop pointlist.count into a variable
                    pointIndex = pointList.Count;
                    // calculate how fast the rail should go brrrrr
                    animationFrequency = 0.02f * (subdivisions / (railDisperseSpeed * 20));
                    // animate the mf
                    InvokeRepeating("AnimateRail", animationFrequency, animationFrequency);

                    placedStarter = true;
                }

                break;
            case false:
                lineRenderer.positionCount = 0;
                break;
        }
    }

    private void AnimateRail()
    {
        if (grv.railState)
        {
            if (pointIndex > 2)
                pointIndex--;
            lineRenderer.positionCount = pointListG.Count - pointIndex;
            var pointArr = new List<Vector3>();
            for (int x = 0; x < pointListG.Count - pointIndex; x++)
            {
                pointArr.Add(pointListG[x]);
                pList = pointArr;
            }
            lineRenderer.SetPositions(pointArr.ToArray());
            meshGenerator.CreateShape();

            if (!placedEndpiece)
            {
                endpieceInstance = Instantiate(endpiece, pointArr[pointArr.Count - 1] + grv.transform.position, Quaternion.identity);
                endpieceInstance.SetParent(route.parent);

                placedEndpiece = true;
            }
            else
            {
                endpieceInstance.position = pointArr[pointArr.Count - 1] + grv.transform.position;
                if (pointArr.Count > 2)
                {
                    endpieceInstance.rotation = Quaternion.FromToRotation(-Vector3.forward, pointArr[pointArr.Count - 2] - pointArr[pointArr.Count - 1]);
                }
            }
        }
    }

    private List<Vector3> TangentsToCurves(List<Vector3> tangents, float ratio)
    {
        var curves = new List<Vector3>();

        for(int x = 0; x < tangents.Count - 1; x++)
        {
            Vector3 vec = Vector3.Lerp(tangents[x], tangents[x + 1], ratio);
            curves.Add(vec);
        }

        return curves;
    }
}
