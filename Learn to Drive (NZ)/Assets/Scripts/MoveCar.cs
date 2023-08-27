using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System;
using UnityEngine.UI;

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

    // Prevent the player from going back! 
    private float maximumZValue = 0; 
    private float minimumXValue = 0;

    // Manage the player's score 
    public TextMeshProUGUI scoreText;
    private int playerScore = 100;

    // Last stopped 
    Vector3 lastStopped = Vector3.zero;

    // For warnings 
    public Button confirmButton;
    public TextMeshProUGUI warningText;
    public GameObject warningPanel;
    private bool canShowFollowWarning = true;
    private bool canShowWrongSideWarning = true;
    private Vector3 lastIntersectionWarning = Vector3.zero;

    // HUD
    public TextMeshProUGUI informationText;

    // Indicators
    public TextMeshProUGUI leftIndicatorText;
    public TextMeshProUGUI rightIndicatorText;

    private bool usedLeft = false;
    private bool usedRight = false;

    // Was the last state a turn? 
    private bool lastStateTurn = false;

    private bool hasIndicated = false;

    private bool leftActive = false;
    private bool rightActive = false;

    private float lastUpdated = 0;

    public void ToggleLeftIndicator() {
        leftActive = !leftActive;
        if (leftActive) {
            rightActive = false; // Deactivate right indicator
        }
        UpdateIndicatorText();
    }

    public void ToggleRightIndicator() {
        rightActive = !rightActive;
        if (rightActive) {
            leftActive = false; // Deactivate left indicator
        }
        UpdateIndicatorText();
    }

    void UpdateIndicatorText()
    {
        leftIndicatorText.text = leftActive ? "<color=green>Indicating Left</color>" : "<color=red>Indicating Left</color>";
        rightIndicatorText.text = rightActive ? "<color=green>Indicating Right</color>" : "<color=red>Indicating Right</color>";
    }

    bool IsOnNavMesh(Vector3 position) {
        UnityEngine.AI.NavMeshHit hit;
        return NavMesh.SamplePosition(position, out hit, 0.1f, NavMesh.AllAreas);
    }

    Transform GetUltimateParentOf(Transform child) {
        Transform parent = child.parent;
        while (parent != null)
        {
            child = parent;
            parent = parent.parent;
        }
        return child;
    }

    bool CheckForJunctionUnderCar() {
        RaycastHit hit;

        // Create a ray from the car's position towards the ground
        Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            Transform parent = GetUltimateParentOf(hit.collider.transform);
            if (parent.CompareTag("Junction")) {
                return true;
            } else {
                foreach (Transform child in parent) {
                    if (child.CompareTag("Junction")) {
                        if (child.transform.position == hit.collider.transform.parent.position) {
                            return true;
                        } else {
                            Debug.LogWarning(child.transform.position);
                            Debug.LogWarning("AAA: " + hit.collider.transform.parent.position);
                        }
                    }
                }
            }
        }
        return false;
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

    private void DeactivatePanel() {
        Time.timeScale = 1;
        warningPanel.SetActive(false);
    }


    void Start() {
        confirmButton.onClick.AddListener(DeactivatePanel);
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
        if (currentSpeed < 1f) {
            lastStopped = transform.position;
        }
        if (position.z > maximumZValue) {
            maximumZValue = position.z; 
        }
        if (position.x < minimumXValue) {
            minimumXValue = position.x;
        }
        if (transform.position.y < 0.1f) {
            NavMeshHit hit;
            onNavMesh = NavMesh.SamplePosition(transform.position, out hit, 1f, 1 << 5) && transform.position.z > (maximumZValue - 25f) && transform.position.x < (minimumXValue + 25f);
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

        // Update the HUD text
        bool turnLeft = false;
        bool turnRight = false;
        foreach (var key in RoadManager.activeRoadPrefabs.Keys) {
            string roadName = RoadManager.activeRoadPrefabs[key].name;
            if (roadName.Contains("intersection-left")) {
                // If within 30 blocks and after the player
                if (Vector3.Distance(transform.position, key) < 30 && Vector3.Distance(Vector3.zero, key) > Vector3.Distance(Vector3.zero, transform.position)) {
                    turnLeft = true;
                }
            } else if (roadName.Contains("intersection-right")) {
                if (Vector3.Distance(transform.position, key) < 30 && Vector3.Distance(Vector3.zero, key) > Vector3.Distance(Vector3.zero, transform.position)) {
                    turnRight = true;
                }
            }
        }

        if (turnLeft) {
            informationText.text = "Turn Left";
        } else if (turnRight) {
            informationText.text = "Turn Right";
        } else {
            informationText.text = "Continue Straight";
            if (lastStateTurn) {
                if (!hasIndicated) {
                    // alert 
                    playerScore -= 5;
                    warningText.text = "You must indicate when turning at an intersection.";
                    warningPanel.SetActive(true);
                    // Freeze the game
                    Time.timeScale = 0;
                }
                hasIndicated = false;
                lastStateTurn = false;
                leftActive = false; 
                rightActive = false;
                UpdateIndicatorText();
            }
        }

        if (turnLeft || turnRight) {
            lastStateTurn = true;
        }

        // Toggle indicators
        if (Time.time - lastUpdated > 0.2f) {
            if (Input.GetKey(KeyCode.LeftArrow)) {
                ToggleLeftIndicator();
                if (turnLeft) {
                    hasIndicated = true;
                }
            } else if (Input.GetKey(KeyCode.RightArrow)) {
                ToggleRightIndicator();
                if (turnRight) {
                    hasIndicated = true;
                }
            }
            lastUpdated = Time.time;
        }

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
            // Base turn speed of 2.5
            float turnSpeed = 2.5f + 0.2f * currentSpeed;

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

        // Calculate speed in kmph
        int speedInKmph = (int) (Mathf.Abs(currentSpeed) * 3.6f);

        // Set the speed text with colored dynamic value
        string speedColour = speedInKmph > 50 ? "red" : "green";
        speedometerText.text = $"Speed: <color={speedColour}>" + speedInKmph + "</color> kmph";

        // Check speed
        if (speedInKmph > 50) {
            playerScore -= 2;
            warningText.text = "Speeding is Dangerous: Slow down.";
            warningPanel.SetActive(true);
            // Freeze the game
            Time.timeScale = 0;
        }

        // Check following distance 
        Transform carInFront = null;
        float closestDistance = Mathf.Infinity;

        // Find all cars (excluding right cars)
        GameObject[] leftCars = GameObject.FindGameObjectsWithTag("LeftCar");

        foreach (GameObject carObject in leftCars) {
            Vector3 toCar = carObject.transform.position - transform.position;
            // Following too close
            if (toCar.magnitude < 4f && canShowFollowWarning) {
                playerScore -= 3;
                warningText.text = "Maintain a Safe Following Distance.";
                warningPanel.SetActive(true);
                // Freeze the game
                Time.timeScale = 0;
                canShowFollowWarning = false;
                StartCoroutine(ResetFollowWarningCooldown());
                break;
            }
        }

        // Check if on right lane
        NavMeshHit hitNavMesh;
        if (NavMesh.SamplePosition(transform.position, out hitNavMesh, 0.5f, 1 << 8) && canShowWrongSideWarning) {
            playerScore -= 10;
            warningText.text = "Drive on the Left Side of the Road.";
            warningPanel.SetActive(true);
            // Freeze the game
            Time.timeScale = 0;
            canShowWrongSideWarning = false;
            StartCoroutine(ResetWrongSideWarningCooldown());
        } 

        // Check if stopped 
        if (CheckForJunctionUnderCar() && Vector3.Distance(transform.position, lastIntersectionWarning) > 50) {
            // Big range since the detection can be poor 
            if (Vector3.Distance(transform.position, lastStopped) > 20) {
                playerScore -= 5;
                warningText.text = "You must come to a complete stop at junctions (Roundabouts and Intersections).";
                warningPanel.SetActive(true);
                // Freeze the game
                Time.timeScale = 0;
            }
            lastIntersectionWarning = transform.position;
        }

        // Set score text
        string scoreColour = "green";
        if (playerScore < 50) {
            scoreColour = "red";
            // Fail the player here
        } else if (playerScore < 70) {
            scoreColour = "orange";
        } else if (playerScore < 90) {
            scoreColour = "yellow";
        }
        scoreText.text = $"Score: <color={scoreColour}>" + playerScore + "</color>";
    }

    private IEnumerator ResetFollowWarningCooldown() {
        yield return new WaitForSeconds(2f);
        canShowFollowWarning = true;
    }

    private IEnumerator ResetWrongSideWarningCooldown() {
        yield return new WaitForSeconds(2f);
        canShowWrongSideWarning = true;
    }
}
