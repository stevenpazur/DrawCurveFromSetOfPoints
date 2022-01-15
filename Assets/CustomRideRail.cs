using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CustomRideRail : MonoBehaviour
{
    // requirements: route is an empty gameobject with several child gameobjects that act as points to a bezier curve.
    // Hierarchy example:
    //  - route
    //      - point1
    //      - point2
    public Transform route;
    public float speed = 0; // how fast the player rides the rail
    public float toSpeed; // how fast the player rides the rail
    public LayerMask ignoreLayers;
    public float railScanSphere = 1;
    public float railScanOffet = 2.3f;
    [HideInInspector] public RiderailMeshGenerator[] nearbyRailsLeft, nearbyRailsRight;

     /*
     * curve length is approximate.  Exact measurements     *
     * take up too much computing power for unity to handle *
     * so margins of error are necessary for decent speed   *
     * calculations.                                        *
     */
    public float arcApproxMarginOfError = 0.05f; 

    private List<Vector3> points, lastPos; // points 1 through points length
    private List<int> coefficients; // coefficients needed to calculate quadratic equation
    private List<float> termsT; // t is 0 thru 1.  t^n, t^(n-1), ..., t^0.
    private List<float> termsU; // u is 1 - t.  u^0, u^1, ..., u^n.

    private int N; // number of points (p0, p1, p2, etc)

    [HideInInspector] private float t = 0; // 0 < t < 1
    private float u; // 1 - t
    private bool m_Started;

    // Start is called before the first frame update
    void Start()
    {
        m_Started = true;

        // add child transform positions to points array
        points = new List<Vector3>();
        lastPos = new List<Vector3>();
        PopulatePointsArray();

        // add coefficients to an array
        coefficients = new List<int>();
        CalculateCoefficients();

        // more arrays yay
        termsT = new List<float>();
        termsU = new List<float>();

        //railScanCubeLeft = railScanCube - Vector3.right * 2.5f;

        nearbyRailsLeft = new RiderailMeshGenerator[10];
        nearbyRailsRight = new RiderailMeshGenerator[10];
    }

    private void PopulatePointsArray()
    {
        // add the points and instantiate a dot at each one.
        points.Clear();
        for(int r = 0; r < route.childCount; r++)
        {
            points.Add(route.GetChild(r).position);
        }
        lastPos = points;

        N = points.Count;
    }

    // Add t^5, t^4, etc and u^0, u^1, etc to arrays.
    private void GetTsAndUs(float t, float u)
    {
        termsU.Clear();
        termsT.Clear();
        for(int x = 0; x < N; x++)
        {
            termsU.Add((float)Math.Pow(u, x));
        }

        for(int y = N - 1; y >= 0; y--)
        {
            termsT.Add((float)Math.Pow(t, y));
        }
    }

    // Use Pascal's Pyramid to find coefficients to a polynomial.
    private void CalculateCoefficients()
    {
        coefficients.Clear();

        int prev = 1;
        coefficients.Add(prev);

        for(int i = 1; i < N; i++)
        {
            int curr = (prev * (N - i)) / i;
            coefficients.Add(curr);
            prev = curr;
        }
    }

    private void UpdatePointPositions()
    {
        for(int x = 0; x < points.Count; x++)
        {
            if(route.GetChild(x).position != lastPos[x])
            {
                PopulatePointsArray();
            }
        }
    }

    // coefficient * T term * U term * point position.  Sum of these products gives obj position at time t.
    public Vector3 GetPosition()
    {
        Vector3 sum = Vector3.zero;
        for(int z = 0; z < N; z++)
        {
            Vector3 vec = coefficients[z] * termsT[z] * termsU[z] * points[z];
            sum += vec;
        }

        return sum;
    }

    public Vector3 GetPositionAtT(float tparam)
    {
        Vector3 sum = Vector3.zero;
        List<float> Ts = new List<float>();
        List<float> Us = new List<float>();
        for (int x = 0; x < N; x++)
        {
            Ts.Add((float)Math.Pow(tparam, x));
        }

        for (int y = N - 1; y >= 0; y--)
        {
            Us.Add((float)Math.Pow(1 - tparam, y));
        }
        for (int z = 0; z < N; z++)
        {
            Vector3 vec = coefficients[z] * Ts[z] * Us[z] * points[z];
            sum += vec;
        }
        return sum;
    }

    public float GetT()
    {
        return t;
    }

    // Approx. the length of the curve.  Used to normalize the player speed so it is easier to set the speed variable inside the inspector window.
    public float FindLengthOfCurve()
    {
        float cont_net = 0;
        for(int x = 0; x < points.Count - 1; x++)
        {
            cont_net += (points[x] - points[x + 1]).magnitude;
        }

        float chord = (points[0] - points[points.Count - 1]).magnitude;
        float approx_arc_length = (cont_net + chord) / 2.0f;
        return approx_arc_length + arcApproxMarginOfError;
    }

    // Update is called once per frame
    private void Update()
    {
        UpdatePointPositions();

        Collider[] hitCollidersLeft = new Collider[10];
        Collider[] hitCollidersRight = new Collider[10];
        Physics.OverlapBoxNonAlloc(transform.position - transform.right * railScanOffet, Vector3.one * railScanSphere, hitCollidersLeft, Quaternion.identity, ignoreLayers);
        Physics.OverlapBoxNonAlloc(transform.position + transform.right * railScanOffet, Vector3.one * railScanSphere, hitCollidersRight, Quaternion.identity, ignoreLayers);
        //for(int j = 0; j < 10; j++)
        //{
        //    hitCollidersLeft[j] = null;
        //    hitCollidersRight[j] = null;
        //}
        for (int i = 0; i < 10; i++)
        {
            if (hitCollidersLeft[i] != null)
            {
                if (hitCollidersLeft[i].transform != transform.parent.GetComponentInChildren<RiderailMeshGenerator>().transform)
                {
                    nearbyRailsLeft[i] = hitCollidersLeft[i].transform.GetComponent<RiderailMeshGenerator>();
                    //print("left of 456 " + nearbyRailsLeft[i]);
                }
            }
            else
            {
                hitCollidersLeft[i] = null;
                nearbyRailsLeft[i] = null;
            }
            if (hitCollidersRight[i] != null)
            {
                if (hitCollidersRight[i].transform != transform.parent.GetComponentInChildren<RiderailMeshGenerator>().transform)
                {
                    nearbyRailsRight[i] = hitCollidersRight[i].transform.GetComponent<RiderailMeshGenerator>();
                    //print("right of 123 " + nearbyRailsRight[i]);
                }
            }
            else
            {
                hitCollidersRight[i] = null;
                nearbyRailsRight[i] = null;
            }
        }

        // increment t all the time.
        if (t < 1)
        {
            t += .02f * (speed / FindLengthOfCurve());
        }

        u = 1.0f - t;
        GetTsAndUs(u, t);

        transform.position = GetPosition();

        if(speed > 0)
            transform.LookAt(GetPositionAtT(GetT() + .01f));
    }

    public void SetPosition(float tparam)
    {
        t = tparam;
        u = 1 - tparam;
    }

    private void OnDrawGizmos()
    {
        if (m_Started)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position + transform.right * railScanOffet, railScanSphere);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position - transform.right * railScanOffet, railScanSphere);
        }
    }
}
