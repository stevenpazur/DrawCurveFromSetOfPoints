using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class RiderailMeshGenerator : MonoBehaviour
{
    [HideInInspector] public Mesh mesh;
    //private Mesh mesh2, mesh3;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    public int step = 1;
    public float thickness, thickness2;
    public RenderLine rl;
    public CustomRideRail rail;
    public int bevelSides = 8;
    public int texSegments = 2;

    private int addsMesh = 0;
    public Transform lookLeft, lookRight;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        //mesh2 = new Mesh();
        //mesh3 = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        //lookLeft.GetComponent<MeshFilter>().mesh = mesh2;
    }

    public void CreateShape()
    {
        CreateShape(rl.subdivisions, bevelSides, out vertices, out triangles, out uvs, thickness);

        UpdateMesh();
    }

    void CreateShape(int subdivs, int bevels, out Vector3[] verticesArr, out int[] trianglesArr, out Vector2[] uvsArr, float thickness)
    {
        int length = (rl.route.childCount * subdivs + 1) * (bevels + 1);
        verticesArr = new Vector3[length];

        for (int index = 0, x = 0; x < rl.pList.Count; x++)
        {
            for (int y = 0; y <= bevels; y++)
            {
                int side = y + 1;
                if (side >= bevels)
                    side -= bevels;
                AddAVertex(verticesArr, rl.pList[x], index, side, thickness);
                index++;
            }
        }

        // rotate verts to line up
        Quaternion rotation;
        for (int v = 0; v < rl.pList.Count - 1; v++)
        {
            // calculate rotation vector
            rotation = Quaternion.FromToRotation(-Vector3.forward, rl.pList[v] - rl.pList[v + 1]);

            // get verts around rl.pList point
            for (int i = 0; i <= bevels; i++)
            {
                verticesArr[i + (v * (bevels + 1))] = rotation * (verticesArr[i + (v * (bevels + 1))] - rl.pList[v]) + rl.pList[v];
            }
        }

        int triangleLength = rl.route.childCount * subdivs * bevels * 6;
        trianglesArr = new int[triangleLength];

        for (int index = 0, y = 0; y < rl.pList.Count - 1; y++)
        {
            for (int x = 0; x < bevels; x++)
            {
                trianglesArr[index] = x + y * (bevels + 1);
                trianglesArr[index + 1] = x + 1 + y * (bevels + 1);
                trianglesArr[index + 2] = bevels + 1 + x + y * (bevels + 1);
                trianglesArr[index + 3] = trianglesArr[index + 2];
                trianglesArr[index + 4] = trianglesArr[index + 1];
                trianglesArr[index + 5] = trianglesArr[index + 2] + 1;
                index += 6;
            }
        }

        uvsArr = new Vector2[vertices.Length];

        for (int index = 0, z = 0; z < rl.pList.Count / (step + 1); z++)
        {
            for (int x = 0; x <= bevelSides; x++)
            {
                for (int y = 0; y <= step; y++)
                {
                    float frac = (float)x / (float)bevelSides;
                    uvsArr[index] = new Vector2(frac / step, y / step);
                    index++;
                }
            }
        }

        UpdateMesh();
    }

    float ConvertAnglesToRadians(float angle)
    {
        float radians = (angle * 2f * Mathf.PI) / 360f;
        return radians;
    }

    void AddAVertex(Vector3[] vertices, Vector3 initialPosition, int i, int side, float thickness)
    {
        float angleInRads = ConvertAnglesToRadians((360f / bevelSides) * side);
        if(i >= 0 && i < vertices.Length)
            vertices[i] = initialPosition + new Vector3(Mathf.Cos(angleInRads) * thickness, Mathf.Sin(angleInRads) * thickness, 0) - rl.route.parent.position;
        else
            print("index " + i + " is being called, but is outside the bounds of the array");
    }

    void UpdateMesh()
    {
        UpdateMesh(mesh, vertices, triangles, uvs);
        //UpdateMesh(mesh2, vertices2, triangles2, uvs2);
        //UpdateMesh(mesh3, vertices3, triangles3, uvs3);

        if (rl.starterInstance != null)
        {
            rl.starterInstance.localScale = Vector3.one * (thickness / .3f);
        }
        if (rl.endpieceInstance != null)
        {
            rl.endpieceInstance.localScale = Vector3.one * (thickness / .3f);
        }

        MeshCollider mc = transform.gameObject.GetComponent<MeshCollider>();
        if (mc == null)
        {
            if (addsMesh >= 3)
            {
                transform.gameObject.AddComponent<MeshCollider>();
            }
            else
            {
                addsMesh++;
            }
        }
        else
        {
            StartCoroutine(SetMesh(mc, mesh));
        }
    }

    void UpdateMesh(Mesh mesh, Vector3[] vertices, int[] triangles, Vector2[] uvs)
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    private IEnumerator SetMesh(MeshCollider mc, Mesh mesh)
    {
        yield return null;
        yield return null;
        yield return null;
        mc.sharedMesh = mesh;
    }

    private void OnDrawGizmos()
    {
        //if (vertices2 == null)
        //{
        //    return;
        //}
        //for (int i = 0; i < vertices2.Length; i++)
        //{
        //    Gizmos.DrawSphere(vertices2[i] + lookLeft.position, .1f);
        //}
        //if (vertices3 == null)
        //{
        //    return;
        //}
        //for (int i = 0; i < vertices2.Length; i++)
        //{
        //    Gizmos.DrawSphere(vertices3[i] + lookRight.position, .1f);
        //}
    }
}