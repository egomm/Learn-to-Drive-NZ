using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MoveCar : MonoBehaviour {
    // Rigidbody of the car 
    public Rigidbody rb;
    // Transform of the car 
    public Transform car;
    // Speed of the car (maximum speed it can reach)
    public float speed;
    
    // Speed text
    public TextMeshProUGUI speedometerText;
    public TextMeshProUGUI speedLimitText;

    // Timer text 
    public TextMeshProUGUI timerText;
    public static float elapsedTime = 0f;

    public static Vector3 position = Vector3.zero;

    // For the movements 
    Vector3 rotationRight = new Vector3(0, 30, 0);
    Vector3 rotationLeft = new Vector3(0, -30, 0);
    Vector3 forward = new Vector3(0, 0, 1);
    Vector3 backward = new Vector3(0, 0, -1);

    private float currentSpeed = 0; // In metres per second
    private bool movingForward = false;
    private bool movingBackward = false;

    // Manage if the car is on the navmesh
    private bool onNavMesh = true;

    private List<Vector3> lastValidPositions = new List<Vector3>();

    // Prevent the player from going back! 
    private float maximumZValue = 0; 
    private float minimumXValue = 0;

    // Manage the player's score 
    public TextMeshProUGUI scoreText;
    public static int playerScore = 100;

    // Last stopped 
    Vector3 lastStopped = Vector3.zero;

    // For warnings 
    public Button confirmButton;
    public TextMeshProUGUI warningText;
    public GameObject warningPanel;
    private bool canShowSpeedingWarning = true; 
    private bool canShowFollowWarning = true;
    private bool canShowWrongSideWarning = true;
    private bool canShowRoadWarning = true;
    // This is calculated as a distance 
    private Vector3 lastIntersectionWarning = Vector3.zero;

    // HUD
    public TextMeshProUGUI informationText;

    // Indicators
    public TextMeshProUGUI leftIndicatorText;
    public TextMeshProUGUI rightIndicatorText;

    // Was the last state a turn? 
    private bool lastStateTurn = false;

    private bool hasIndicated = false;

    private bool leftActive = false;
    private bool rightActive = false;

    // For turning
    private bool turnLeft = false;
    private Vector3 turnLeftCoordinates = Vector3.zero;
    private bool turnRight = false;
    private Vector3 turnRightCoordinates = Vector3.zero;

    List<Vector3> intersectionsPassed = new List<Vector3>(); 

    private float lastUpdated = 0;

    // Function for toggling the left indicator 
    public void ToggleLeftIndicator() {
        leftActive = !leftActive;
        if (leftActive) {
            rightActive = false; // Deactivate right indicator
        }
        UpdateIndicatorText();
    }

    // Function for toggling the right indicator
    public void ToggleRightIndicator() {
        rightActive = !rightActive;
        if (rightActive) {
            leftActive = false; // Deactivate left indicator
        }
        UpdateIndicatorText();
    }

    // Function for updating the indicator text 
    void UpdateIndicatorText() {
        // The text is green if the player is indicating else the text is red
        leftIndicatorText.text = leftActive ? "<color=green>Indicating Left</color>" : "<color=red>Indicating Left</color>";
        rightIndicatorText.text = rightActive ? "<color=green>Indicating Right</color>" : "<color=red>Indicating Right</color>";
    }

    // Function for getting the ultimate parent of a child object 
    Transform GetUltimateParentOf(Transform child) {
        Transform parent = child.parent;
        // Keep checking the parent until there are no more parents 
        while (parent != null) {
            child = parent;
            parent = parent.parent;
        }
        return child;
    }

    // Use a raycast to check if there is a junction under the player (the car)
    bool CheckForJunctionUnderCar() {
        RaycastHit hit;

        // Create a ray from the car's position towards the ground
        Ray ray = new Ray(transform.position + Vector3.up, Vector3.down);

        // Perform the raycast
        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            Transform parent = GetUltimateParentOf(hit.collider.transform);
            foreach (Transform child in parent) {
                // Check if the child has a junction tag
                if (child.CompareTag("Junction")) {
                    Transform colliderParent = hit.collider.transform.parent.transform;
                    // Return if the junction found matches with the colliders parent, or the parent of the colliders 
                    if (ReferenceEquals(child.transform, colliderParent) || ReferenceEquals(child.transform, colliderParent.parent.transform)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // Deactive the warning panel 
    private void DeactivateWarningPanel() {
        // Unfreeze the game
        Time.timeScale = 1;
        if (playerScore < 50) {
            // Player failed 
            SceneManager.LoadScene("Game Over");
        }
        // Hide the warning
        warningPanel.SetActive(false);
    }

    // Function for formatting the time into minutes:seconds
    private string FormatTime(float time) {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void Start() {
        // Clear all the static variables
        playerScore = 100;
        elapsedTime = 0;
        position = Vector3.zero;
        // Add a button click listener
        confirmButton.onClick.AddListener(DeactivateWarningPanel);
        lastValidPositions.Add(new Vector3(0, 0.1f, 0));
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        position = transform.position;
        if (currentSpeed == 0f) {
            // The last position where the player's car came to a stop
            lastStopped = transform.position;
        }

        // Ensure that the player can't drive back to the start position
        if (position.z > maximumZValue) {
            maximumZValue = position.z; 
        }
        if (position.x < minimumXValue) {
            minimumXValue = position.x;
        }

        // Wait until the player is on the ground
        if (transform.position.y < 0.1f) {
            NavMeshHit hit;
            // Check if the player is on the navmesh (to constrain movement)
            onNavMesh = NavMesh.SamplePosition(transform.position, out hit, 1f, 1 << 5) && transform.position.z > (maximumZValue - 25f) && transform.position.x < (minimumXValue + 25f);
            // Check if the player is on the road (to alert the player)
            bool onRoad = NavMesh.SamplePosition(transform.position, out hit, 1f, 1 << 10);
            if (!onRoad && canShowRoadWarning) {
                // Alert the player and minus off 5 score
                playerScore -= 5; 
                warningText.text = "Stay on the road to avoid potential crashes.";
                warningPanel.SetActive(true);
                // Freeze the game
                Time.timeScale = 0;
                canShowRoadWarning = false;
                // Reset the warning after 2 seconds
                StartCoroutine(ResetRoadWarningCooldown());
            }

            // Maximum size of the list for memory
            int maxPositions = 1000;

            // Manage if the player is on navmesh
            if (onNavMesh) {
                // Add the current position of the player to the list of valid positions
                lastValidPositions.Add(transform.position);

                // Check if the list size exceeds the maximum allowed size
                if (lastValidPositions.Count > maxPositions) {
                    int excess = lastValidPositions.Count - maxPositions;
                    lastValidPositions.RemoveRange(0, excess);
                }
            } else {
                // If the player is not on navmesh, make sure they go back onto navmesh
                if (lastValidPositions.Count > 0) {
                    transform.position = lastValidPositions[lastValidPositions.Count - 1];
                    // Remove this position from the list of valid positions as it has already been checked
                    lastValidPositions.RemoveAt(lastValidPositions.Count - 1);
                }
            }
        }

        // Update the HUD text
        bool showTurnLeft = false;
        bool showTurnRight = false;
        foreach (var key in RoadManager.activeRoadPrefabs.Keys) {
            if (RoadManager.activeRoadPrefabs[key] != null) {
                string roadName = RoadManager.activeRoadPrefabs[key].name;
                if (roadName.Contains("intersection-left")) {
                    // If the intersection is within 30 units and is after the player position, alert the player that there is an intersection soon 
                    if (Vector3.Distance(transform.position, key) < 30 && Vector3.Distance(Vector3.zero, key) > Vector3.Distance(Vector3.zero, transform.position)) {
                        if (!intersectionsPassed.Contains(key)) {
                            turnLeft = true;
                            turnLeftCoordinates = key;
                            intersectionsPassed.Add(key);
                        } else {
                            showTurnLeft = true;
                        }
                    }
                } else if (roadName.Contains("intersection-right")) {
                    // If the intersection is within 30 units and is after the player position, alert the player that there is an intersection soon 
                    if (Vector3.Distance(transform.position, key) < 30 && Vector3.Distance(Vector3.zero, key) > Vector3.Distance(Vector3.zero, transform.position)) {
                        if (!intersectionsPassed.Contains(key)) {
                            turnRight = true;
                            turnRightCoordinates = key;
                            intersectionsPassed.Add(key);
                        } else {
                            showTurnRight = true;
                        }
                    }
                } else {
                    // Stop alerting the player of an upcoming intersection if the player has passed the intersection
                    if (Vector3.Distance(Vector3.zero, transform.position) > Vector3.Distance(Vector3.zero, turnLeftCoordinates)) {
                        turnLeft = false; 
                    }
                    if (Vector3.Distance(Vector3.zero, transform.position) > Vector3.Distance(Vector3.zero, turnRightCoordinates)) {
                        turnRight = false;
                    }
                }
            }
        }

        // Display the text depending on if an intersetion is coming up
        if (turnLeft) {
            informationText.text = "Turn Left";
        } else if (turnRight) {
            informationText.text = "Turn Right";
        } else {
            informationText.text = "Continue Straight";
            if (lastStateTurn) {
                if (!hasIndicated) {
                    // Alert the player and minus off 5 score 
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

        // If the player reversed backwards, alert the player to turn
        // No penalty for making the same mistake twice on the same intersection
        if (showTurnLeft) {
            informationText.text = "Turn Left";
        } else if (showTurnRight) {
            informationText.text = "Turn Right";
        }

        if (turnLeft || turnRight) {
            lastStateTurn = true;
        }

        // Toggle indicators 
        // Prevent the users from clicking too often with a 200ms delay
        if (Input.GetKey(KeyCode.LeftArrow)) {
            if (Time.time - lastUpdated > 0.2f) {
                ToggleLeftIndicator();
                if (turnLeft) {
                    hasIndicated = true;
                }
                lastUpdated = Time.time;
            }
        } else if (Input.GetKey(KeyCode.RightArrow)) {
            if (Time.time - lastUpdated > 0.2f) {
                ToggleRightIndicator();
                if (turnRight) {
                    hasIndicated = true;
                }
                lastUpdated = Time.time;
            }
        }

        //GetComponent<Rigidbody>().AddForce(Vector3.down * 10E9f);
        if (Input.GetKey("w") && !movingBackward && currentSpeed >= 0 && onNavMesh) {
            if (movingForward && currentSpeed < speed) {
                // Increase the current speed based on a negative exponential model
                currentSpeed += 50f * Time.deltaTime * (float) Math.Exp(-0.4f * currentSpeed);
                if (currentSpeed > speed) {
                    currentSpeed = speed;
                }
            }
            movingForward = true;
        } else {
            movingForward = false;
        }
        if (Input.GetKey("s") && !movingForward && currentSpeed <= 0 && onNavMesh) {
            // Decrease the current speed based on a positive exponential model
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
        // Allow for the player to turn if the player isnt stationary
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

        // Set the speed text with coloured text depending on the player's speed
        string speedColour = "green";
        if (speedInKmph > 50) {
            speedColour = "red";
        } else if (speedInKmph >= 48) {
            speedColour = "orange";
        } else if (speedInKmph >= 45) {
            speedColour = "yellow";
        }
        speedometerText.text = $"Speed: <color={speedColour}>" + speedInKmph + "</color> kmph";
        speedLimitText.text = $"Speed Limit: <color={speedColour}>50</color> kmph";

        // Increment the elapsed time by the time since the last frame 
        elapsedTime += Time.deltaTime;
        // Set the timer text with coloured text depending on the elapsed time
        string timeColour = "red";
        if (elapsedTime >= 120) {
            timeColour = "orange";
        } else if (elapsedTime >= 240) {
            timeColour = "yellow";
        } else if (elapsedTime >= 360) {
            timeColour = "green";
        }
        timerText.text = $"Time: <color={timeColour}>" + FormatTime(elapsedTime) + "</color>";
        // Check speed
        if (speedInKmph > 50 && canShowSpeedingWarning) {
            // Warn the player and decrease the player score by 2
            playerScore -= 2;
            warningText.text = "Speeding is Dangerous: Slow down.";
            warningPanel.SetActive(true);
            // Freeze the game
            Time.timeScale = 0;
            canShowSpeedingWarning = false;
            // Allow for this warning to show again after 2 seconds
            StartCoroutine(ResetSpeedingWarningCooldown());
        }

        // Find all cars driving on the left side of the road
        GameObject[] leftCars = GameObject.FindGameObjectsWithTag("LeftCar");

        // Iterate over these cars to check if the player is following too close to the other cars
        foreach (GameObject carObject in leftCars) {
            Vector3 toCar = carObject.transform.position - transform.position;
            // Following too close
            if (toCar.magnitude < 4f && canShowFollowWarning) {
                playerScore -= 5;
                warningText.text = "Maintain a Safe Following Distance.";
                warningPanel.SetActive(true);
                // Freeze the game
                Time.timeScale = 0;
                canShowFollowWarning = false;
                StartCoroutine(ResetFollowWarningCooldown());
                break;
            }
        }

        // Check if the player is on the right lane
        NavMeshHit hitNavMesh;
        if (NavMesh.SamplePosition(transform.position, out hitNavMesh, 0.5f, 1 << 8) && canShowWrongSideWarning) {
            playerScore -= 10;
            // The car will be warned if they are on the left side but aren't following the instructions
            // Although this is fine as the car should be following the instructions in a driving test
            warningText.text = "Drive on the Left Side of the Road.";
            warningPanel.SetActive(true);
            // Freeze the game
            Time.timeScale = 0;
            canShowWrongSideWarning = false;
            StartCoroutine(ResetWrongSideWarningCooldown());
        } 

        // Check if stopped 
        if (CheckForJunctionUnderCar() && Vector3.Distance(transform.position, lastIntersectionWarning) > 50) {
            // Check if the player stopped within 10 units of the junction
            if (Vector3.Distance(transform.position, lastStopped) > 10) {
                // Warn the player and decrease the player score by 5
                playerScore -= 5;
                warningText.text = "You must come to a complete stop to give way at junctions (Roundabouts and Intersections).";
                warningPanel.SetActive(true);
                // Freeze the game
                Time.timeScale = 0;
            }
            lastIntersectionWarning = transform.position;
        }

        // Set score text with appropriate colouring
        string scoreColour = "green";
        if (playerScore < 50) {
            scoreColour = "red";
        } else if (playerScore < 70) {
            scoreColour = "orange";
        } else if (playerScore < 90) {
            scoreColour = "yellow";
        }
        scoreText.text = $"Score: <color={scoreColour}>" + playerScore + "</color>";
    }

    // The alerts can show again after a time (in seconds) has been exceeded
    private IEnumerator ResetSpeedingWarningCooldown() {
        yield return new WaitForSeconds(2f);
        canShowSpeedingWarning = true;
    }

    private IEnumerator ResetFollowWarningCooldown() {
        yield return new WaitForSeconds(2f);
        canShowFollowWarning = true;
    }

    private IEnumerator ResetWrongSideWarningCooldown() {
        yield return new WaitForSeconds(2f);
        canShowWrongSideWarning = true;
    }

    private IEnumerator ResetRoadWarningCooldown() {
        yield return new WaitForSeconds(2f);
        canShowRoadWarning = true;
    }
}
