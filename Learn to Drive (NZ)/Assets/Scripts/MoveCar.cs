using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System;

public class MoveCar : MonoBehaviour {
    public Rigidbody rb;
    public Transform car;
    public float speed;
    [SerializeField] TextMeshProUGUI speedometerText;

    public static Vector3 position = Vector3.zero;

    Vector3 rotationRight = new Vector3(0, 30, 0);
    Vector3 rotationLeft = new Vector3(0, -30, 0);

    Vector3 forward = new Vector3(0, 0, 1);
    Vector3 backward = new Vector3(0, 0, -1);

    private float currentSpeed = 0; // In metres per second
    private bool movingForward = false;
    private bool movingBackward = false;

    private bool onNavMesh = true;

    private List<Vector3> lastValidPositions = new List<Vector3>();

    bool IsOnNavMesh(Vector3 position) {
        UnityEngine.AI.NavMeshHit hit;
        return NavMesh.SamplePosition(position, out hit, 0.1f, NavMesh.AllAreas);
    }

    void OnCollisionEnter(Collision collision) {
        //Debug.Log("ENTERED: " + collision.gameObject.layer);
        //NavMeshHit hit;
        //Debug.Log(NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas));
        onNavMesh = !(collision.gameObject.layer == 0);
    }

    void OnCollisionExit(Collision collision) {
        //Debug.Log("EXIT: " + collision.gameObject.layer);
        if (collision.gameObject.layer == 0) {
            onNavMesh = true;
        }
    }

    void OnCollisionStay(Collision collision) {
        if (transform.position.y < 0.1f) {
            /*int leftLayer = LayerMask.NameToLayer("left");
            int rightLayer = LayerMask.NameToLayer("right");
            int turningLayer = LayerMask.NameToLayer("turning");
            int objectLayer = collision.gameObject.layer;
            onNavMesh = (objectLayer == leftLayer || objectLayer == rightLayer || objectLayer == turningLayer);
            Debug.Log(onNavMesh);
            if (onNavMesh) {
                lastValidPosition = transform.position;
            } else {
                transform.position = lastValidPosition;
            }*/
            //int objectLayer = collision.gameObject.layer;
            //onNavMesh = objectLayer != 0;
            //Debug.Log(onNavMesh);
        }
    }

    void Start() {
        lastValidPositions.Add(new Vector3(0, 0.1f, 0));
        rb = GetComponent<Rigidbody>();
        //navAgent = GetComponent<NavMeshAgent>();

        // Check if the car's starting position is on the NavMesh
        if (!IsOnNavMesh(transform.position)) {
            Debug.Log("Car's starting position is not on the NavMesh.");
        } else {
            Debug.Log("ON NAVMESH!");
        }
    }

    bool CheckIfCarOnNavMesh() {
        NavMeshHit hit;
        return NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas);
        //int desiredNavMeshLayer = NavMesh.GetNavMeshLayerFromName("YourDesiredLayerName");
        int leftLayer = LayerMask.NameToLayer("left");
        int rightLayer = LayerMask.NameToLayer("right");
        int turningLayer = LayerMask.NameToLayer("turning");
        int objectLayer = gameObject.layer;
        //Debug.Log(objectLayer);
        //return true;
       // return (objectLayer == leftLayer || objectLayer == rightLayer || objectLayer == turningLayer);
        //return NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas);
    }

    void FixedUpdate() {
        position = transform.position;
        if (transform.position.y < 0.1f) {
            NavMeshHit hit;
            onNavMesh = NavMesh.SamplePosition(transform.position, out hit, 1f, 1 << 5); //&& transform.position.z > -5.5f;
            if (!onNavMesh) {
                Debug.Log("NOT ON? ROAD??");
            }
            if (onNavMesh) {
                //lastValidPosition = transform.position;
                lastValidPositions.Add(transform.position);
            } else {
                if (lastValidPositions.Count - 1 >= 0) {
                    transform.position = lastValidPositions[lastValidPositions.Count - 1];
                    lastValidPositions.RemoveAt(lastValidPositions.Count - 1);
                }
            }
        }
        // Temporary until a better system is devised
        //GetComponent<Rigidbody>().AddForce(Vector3.down * 10E9f);
        if (Input.GetKey("w") && !movingBackward && currentSpeed >= 0 && onNavMesh) {
            if (movingForward && currentSpeed < speed) {
                currentSpeed += 50f * Time.deltaTime * (float) Math.Exp(-0.4f * currentSpeed);
                //Debug.Log((float)Math.Exp(-0.5f*currentSpeed));
                if (currentSpeed > speed) {
                    currentSpeed = speed;
                }
            }
            movingForward = true;
        } else {
            movingForward = false;
        }
        if (Input.GetKey("s") && !movingForward && currentSpeed <= 0 && onNavMesh) {
            //transform.Translate(backward * currentSpeed * Time.deltaTime);
            currentSpeed -= 10f * Time.deltaTime * (float) Math.Exp(0.4f * currentSpeed);
            movingBackward = true;
        } else {
            movingBackward = false;
        }
        // Not moving
        if (!movingForward && !movingBackward) {
            if (currentSpeed > 0) {
                // Slow down after moving forward
                currentSpeed -= 7.5f * Time.deltaTime * (float) Math.Exp(0.05f * currentSpeed);
                if (currentSpeed < 0) {
                    currentSpeed = 0;
                }
            } else if (currentSpeed < 0) {
                // Slow down after moving backward
                currentSpeed += 7.5f * Time.deltaTime * (float) Math.Exp(0.05f * currentSpeed);
                if (currentSpeed > 0) {
                    currentSpeed = 0;
                }
            }
        }
        transform.Translate(forward * currentSpeed * Time.deltaTime);
        if (currentSpeed != 0) {
            float turnSpeed  = 0.25f * currentSpeed;

            if (Input.GetKey("d")) {
                Quaternion deltaRotationRight = Quaternion.Euler(0f, rotationRight.y * turnSpeed * Time.deltaTime, 0f);
                Quaternion newRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * deltaRotationRight;

                // Set the new rotation while keeping x and z rotations at 0
                transform.rotation = Quaternion.Euler(0f, newRotation.eulerAngles.y, 0f);
            }

            if (Input.GetKey("a")) {
                Quaternion deltaRotationLeft = Quaternion.Euler(0f, rotationLeft.y * turnSpeed * Time.deltaTime, 0f);
                Quaternion newRotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f) * deltaRotationLeft;

                // Set the new rotation while keeping x and z rotations at 0
                transform.rotation = Quaternion.Euler(0f, newRotation.eulerAngles.y, 0f);
            }
        }
        speedometerText.SetText("Speed: " + (int) (Math.Abs(currentSpeed) * 3.6f) + " kmph");
    }
}
