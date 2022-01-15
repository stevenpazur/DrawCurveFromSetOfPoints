using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed = 2;

    private bool allowInput = true;
    private bool pickedSpotOnRail = false;

    private CharacterController cc;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;

    private float rideLimit = 1;
    public float railTransformOffset = 0.5f;
    private bool touchingRail = false;
    private Vector3 hitPoint;
    private Transform hitTrans;

    private int numOfJumps = 0;
    private bool hanging = false, endingTheRide = false, goLeft = false, goRight = false;
    public bool onRail = false;
    private bool playerIsFacingCamera;

    // rail parts
    CustomRideRail railScript = null;
    RenderLine rl = null;
    int iresult = 0;

    private void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        playerIsFacingCamera = Vector3.Dot(transform.forward, (Camera.main.transform.position - transform.position).normalized) > 0;

        cc.transform.position = transform.position;
        groundedPlayer = cc.isGrounded;
        //print("cc.isGrounded=" + groundedPlayer);
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
        }
        if (groundedPlayer)
        {
            numOfJumps = 0;
            railTransformOffset = 0;
        }

        if (Input.GetButtonDown("Jump") && groundedPlayer)
        {
            Jump();
        }

        if (allowInput)
        {
            Vector3 right = Camera.main.transform.right;
            right.y = 0;
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0;

            Vector3 move = right * Input.GetAxis("Horizontal") + forward * Input.GetAxis("Vertical");
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
                cc.Move(move * Time.deltaTime * movementSpeed);
            if (move != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(move);
        }

        GetComponent<CapsuleCollider>().enabled = allowInput;

        playerVelocity.y += gravityValue * Time.deltaTime;
        cc.Move(playerVelocity * Time.deltaTime);

        if (touchingRail)
        {
            onRail = true;
            railScript = hitTrans.GetComponent<RiderailMeshGenerator>().rail;
            rl = hitTrans.GetComponent<RiderailMeshGenerator>().rl;

            if (railScript != null && rl != null)
            {
                Vector3[] pointList = new Vector3[rl.lineRenderer.positionCount];
                rl.lineRenderer.GetPositions(pointList);

                // get point on rail that is closest to player's contact point
                //iresult = 0;
                if (!pickedSpotOnRail)
                {
                    float closeness = 999;
                    for (int i = 0; i < pointList.Length; i++)
                    {
                        float mag = (pointList[i] - hitPoint).magnitude;
                        if (closeness > mag)
                        {
                            closeness = mag;
                            iresult = i;
                        }
                    }

                    pickedSpotOnRail = true;
                    //print(iresult);
                }

                if (iresult < pointList.Length)
                {
                    float tparam = (float)iresult / (float)pointList.Length;

                    if (tparam < rideLimit)
                    {
                        //StartCoroutine(LerpPlayerAlongRail(transform.position, newPlayerPos, 1, tparam, railScript));
                        StartCoroutine(RideTheRail(tparam, railScript));
                    }
                }
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.tag == "RideRail")
        {
            //print("hit");
            touchingRail = true;
            pickedSpotOnRail = false;
            hitPoint = hit.point;
            hitTrans = hit.transform;
        }
    }

    private IEnumerator RideTheRail(float tparam, CustomRideRail rail)
    {
        if (allowInput)
        {
            rail.SetPosition(tparam);
        }

        // riding rail now
        //indicator.rotation = rail.transform.rotation;
        gravityValue = 0;

        allowInput = false;
        if (rail.GetT() < 1 && !goLeft && !goRight)
        {
            transform.SetParent(rail.transform);
            rail.speed = rail.toSpeed;
            yield return null;
            transform.position = rail.transform.position + Vector3.up * railTransformOffset;
            transform.rotation = rail.transform.rotation;
        }
        else
        {
            transform.SetParent(null);
        }

        LeftJumpPath leftPath = rail.GetComponentInChildren<Logics>().GetComponentInChildren<LeftJumpPath>();
        ToTheLeftToTheRight paths = leftPath.transform.parent.GetComponentInChildren<ToTheLeftToTheRight>();
        Transform newParent = leftPath.transform;
        if (goLeft && rail.nearbyRailsLeft.Length > 0)
        {
            transform.SetParent(newParent);
            //paths.tLeft = 0;
            //yield return null;
            transform.position = newParent.position;
            if (paths.tLeft <= 1)
            {
                paths.tLeft += .05f;
                //yield return new WaitForSeconds(.1f);
            }
            else
            {
                gravityValue = -9.81f;
                CustomRideRail newRail = rail.nearbyRailsLeft[0].transform.parent.GetComponentInChildren<CustomRideRail>();
                rail = newRail;
                railScript = rail;
                ResetAllButCurrentRail(rail);
                transform.SetParent(rail.transform);
                touchingRail = false;
                allowInput = true;
                cc.Move(Vector3.left * 0.0001f);
                goLeft = false;
            }
        }
        else
        {
            //yield return null;
            paths.tLeft = 0;
        }

        RightJumpPath rightPath = rail.GetComponentInChildren<Logics>().GetComponentInChildren<RightJumpPath>();
        ToTheLeftToTheRight paths2 = rightPath.transform.parent.GetComponentInChildren<ToTheLeftToTheRight>();
        Transform newParent2 = rightPath.transform;
        if (goRight && rail.nearbyRailsRight.Length > 0)
        {
            transform.SetParent(newParent2);
            transform.position = newParent2.position;
            if (paths2.tRight <= 1)
            {
                paths2.tRight += .05f;
                //yield return new WaitForSeconds(.1f);
            }
            else
            {
                gravityValue = -9.81f;
                print(rail.nearbyRailsRight[1]);
                CustomRideRail newRail = rail.nearbyRailsRight[0].transform.parent.GetComponentInChildren<CustomRideRail>();
                rail = newRail;
                railScript = rail;
                ResetAllButCurrentRail(rail);
                transform.SetParent(rail.transform);
                touchingRail = false;
                allowInput = true;
                cc.Move(Vector3.right * 0.0001f);
                goRight = false;
            }
        }
        else
        {
            //yield return null;
            paths2.tRight = 0;
        }

        StartCoroutine(ScanForLeftRightInputs(rail));

        #region RailJumpFunctionality

        if (railTransformOffset > 0)
        {
            if (!hanging)
                railTransformOffset -= 9.81f * Time.deltaTime * 1.5f;
        }
        else
        {
            railTransformOffset = 0;
            numOfJumps = 0;
        }

        if (Input.GetButtonDown("Jump") && rail.GetT() < .97f)
        {
            //print("yumpers on rail");
            if (numOfJumps == 0 && railTransformOffset <= .01f)
            {
                numOfJumps++;
                for (int x = 10; x > 0; x--)
                {
                    yield return new WaitForSeconds(.02f);
                    railTransformOffset += x * 10 * .01f;
                }

                // hang time
                hanging = true;
                int hangTimeFrames = 15;
                for (int x = 0; x < hangTimeFrames; x++)
                {
                    yield return new WaitForSeconds(.02f);
                }
                hanging = false;
            }
        }

        #endregion

        // dismount
        if (rail.GetT() >= rideLimit && !goLeft && !goRight)
        {
            StartCoroutine(AllowInputControlAfterTime(rail, .8f));
        }
    }

    private void JumpLeft(RiderailMeshGenerator[] meshes)
    {
        if (meshes.Length > 0)
        {
            if (meshes[0] != null)
            {
                goLeft = true;
            }
        }
    }

    private void JumpRight(RiderailMeshGenerator[] meshes)
    {
        if (meshes.Length > 0)
        {
            if (meshes[0] != null)
            {
                goRight = true;
            }
        }
    }

    private void ResetAllButCurrentRail(CustomRideRail currentRail)
    {
        var rails = FindObjectsOfType<CustomRideRail>();

        for(int i = 0; i < rails.Length; i++)
        {
            if(rails[i] != currentRail)
            {
                rails[i].SetPosition(0);
                rails[i].speed = 0;
            }
        }
    }

    private IEnumerator ScanForLeftRightInputs(CustomRideRail rail)
    {
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            

            if (Input.GetKeyDown(KeyCode.D))
            {
                if (playerIsFacingCamera)
                {
                    if (!goRight)
                    {
                        JumpLeft(rail.nearbyRailsLeft);
                        yield return new WaitForSeconds(1);
                    }
                }
                else
                {
                    if (!goLeft)
                    {
                        JumpRight(rail.nearbyRailsRight);
                        yield return new WaitForSeconds(1);
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                if (playerIsFacingCamera)
                {
                    if (!goLeft)
                    {
                        JumpRight(rail.nearbyRailsRight);
                        yield return new WaitForSeconds(1);
                    }
                }
                else
                {
                    if (!goRight)
                    {
                        JumpLeft(rail.nearbyRailsLeft);
                        yield return new WaitForSeconds(1);
                    }
                }
            }
        }
    }

    private IEnumerator AllowInputControlAfterTime(CustomRideRail rail, float timeDelay)
    {
        allowInput = false;
        transform.SetParent(null);

        gravityValue = -9.81f;
        hanging = false;
        if (!endingTheRide)
        {
            if (railTransformOffset <= 0)
            {
                StartCoroutine(JumpAndForward(rail));
            }
            else
            {
                StartCoroutine(JustForward(rail));
            }
        }
        rail.speed = 0;
        touchingRail = false;

        yield return new WaitForSeconds(.2f);
        if (rail.transform.childCount == 0)
        {
            rail.SetPosition(0);
        }
        yield return new WaitForSeconds(timeDelay);
        allowInput = true;

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "RailTrigger")
        {
            if (other.GetComponent<RailComponents>() != null)
            {
                GlobalRailVariables grv = other.GetComponent<RailComponents>().myGrv;
                grv.FlipSwitch();
            }
        }
    }

    void Jump()
    {
        numOfJumps++;
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
    }

    private IEnumerator JumpAndForward(CustomRideRail rail)
    {
        onRail = false;
        numOfJumps++;
        endingTheRide = true;
        playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        Vector3 calculatedDir = rail.route.GetChild(rail.route.childCount - 1).position - rail.route.GetChild(rail.route.childCount - 2).position;
        for (int x = 0; x < 30; x++)
        {
            cc.Move(calculatedDir * .02f);
            yield return new WaitForSeconds(0.02f);

            Vector3 eulerRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, eulerRotation.y, 0);

            if (x == 29)
            {
                rail.SetPosition(0);
            }
        }
        endingTheRide = false;
    }

    private IEnumerator JustForward(CustomRideRail rail)
    {
        onRail = false;
        endingTheRide = true;
        Vector3 calculatedDir = rail.route.GetChild(rail.route.childCount - 1).position - rail.route.GetChild(rail.route.childCount - 2).position;
        for (int x = 0; x < 30; x++)
        {
            cc.Move(calculatedDir * .02f);
            yield return new WaitForSeconds(0.02f);

            Vector3 eulerRotation = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(0, eulerRotation.y, 0);

            if (x == 29)
            {
                rail.SetPosition(0);
            }
        }
        endingTheRide = false;
    }
}
