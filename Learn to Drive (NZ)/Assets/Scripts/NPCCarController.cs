using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using System.Linq;

public class NPCCarController : MonoBehaviour
{
    private NavMeshAgent agent;

    private bool started = false;
    private Vector3 startPosition;
    private Vector3 endPosition; // Store the end position
    private Vector3 lastInstantiated = new Vector3(0, 0, 0);
    private List<GameObject> passedObjects = new List<GameObject>();

    private Vector3 lastPosition;
    private bool restartedCar = false;

    private float safeFollowingDistance = 10f; 
    private float maxSpeed = 14f; // Around 50kmph
    bool stopped = false;

    int attempts = 0;

    // IEnumerator for delaying the start 
    IEnumerator ExecuteDelayedTask() {
        // Wait for 0.25s
        yield return new WaitForSeconds(0.25f);

        // Find a point at the end of the NavMesh initially
        startPosition = RoadManager.navMeshStart;
        endPosition = RoadManager.navMeshEnd;
        if (endPosition != Vector3.zero) {
            if (agent.isOnNavMesh) {
                if (gameObject.tag == "LeftCar") {
                    agent.SetDestination(endPosition);
                } else if (gameObject.tag == "RightCar") {
                    agent.SetDestination(startPosition);
                }
                started = true;
            }
        }
        if (!started) {
            StartCoroutine(ExecuteDelayedTask());
            attempts++;
            // What is this car doing?
            if (attempts > 40) {
                Destroy(gameObject);
            }
        }
    }

    // IEnumerator for restarting the car after having stopped at a junction
    IEnumerator RestartCar() {
        // Wait for 0.5-2 seconds
        restartedCar = true;
        float stoppingTime = UnityEngine.Random.Range(0.5f, 2f);        
        yield return new WaitForSeconds(stoppingTime);
        stopped = false;
        restartedCar = false;
    }

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        // Mask the navmesh depending on the type of car (left lane car or right lane car)
        if (gameObject.tag == "LeftCar") {
            agent.areaMask = 1 << 3;
        } else if (gameObject.tag == "RightCar") {
            agent.areaMask = 1 << 4;
        }
        StartCoroutine(ExecuteDelayedTask());
    }

    void Update() {
        // Check if the car has started and is driving
        if (started && !stopped) {
            // To reduce lag
            if (Time.frameCount % 10 == 0) {
                if (agent.isOnNavMesh) {
                    startPosition = RoadManager.navMeshStart;
                    endPosition = RoadManager.navMeshEnd;
                    if (gameObject.tag == "LeftCar") {
                        agent.SetDestination(endPosition);
                    } else if (gameObject.tag == "RightCar") {
                        agent.SetDestination(startPosition);
                    }
                    Transform carInFront = FindCarInFront(gameObject.tag);
                    if (carInFront) {
                        // Calculate desired speed based on the speed of the car in front
                        float desiredSpeed = CalculateDesiredSpeed(carInFront);
                        // Set the NavMesh Agent's speed
                        agent.speed = desiredSpeed;
                    } else {
                        // No car in front, drive at max speed
                        agent.speed = maxSpeed;
                    }
                } 
                // Destroy logic both left and right car
                if (!agent.isOnNavMesh || Vector3.Distance(transform.position, MoveCar.position) > 125){
                    Destroy(gameObject);
                }
                // Destroy logic for only right car
                if (gameObject.tag == "RightCar") {
                    // Destroy if within 5 units of destination (startPosition)
                    if (Vector3.Distance(transform.position, startPosition) <= 5) {
                        Destroy(gameObject);
                    }
                }
                
                // If the car is a left lane car
                if (gameObject.CompareTag("LeftCar")) {
                    var prevKey = Vector3.zero;
                    Transform objectBelow = DetectCurrentObjectUnderAgent();
                    foreach (var key in RoadManager.roadInformation.Keys) {
                        // Detect before the car is at the roundabout 
                        GameObject objectChecking = RoadManager.roadInformation[key].GameObject;
                        if (objectChecking.CompareTag("Junction")) {
                            if (prevKey != Vector3.zero) {
                                if (objectBelow != null) {
                                    // Check if player is on road a before a junction
                                    if (prevKey == objectBelow.position) {
                                        stopped = true;
                                        break;
                                    }
                                }
                            }
                        } else {
                            // If the object that is being checked is not a junction, check the children
                            for (int i = 0; i < objectChecking.transform.childCount; i++) {
                                Transform child = objectChecking.transform.GetChild(i);
                                if (child.CompareTag("Junction")) {
                                    if (prevKey != Vector3.zero) {
                                        if (objectBelow != null) {
                                            // Check if player is on a road before a junction
                                            if (objectBelow.position == prevKey) {
                                                stopped = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            if (stopped) {
                                break;
                            }
                        }
                        // Set the previous key to the current key for managing if the previous road was a junction
                        prevKey = key;
                    }
                } else if (gameObject.CompareTag("RightCar")) { // If the car is a right lane car
                    Transform objectBelow = DetectCurrentObjectUnderAgent();
                    bool nextJunction = false;
                    foreach (var key in RoadManager.roadInformation.Keys) {
                        // Detect before the car is at the roundabout 
                        GameObject objectChecking = RoadManager.roadInformation[key].GameObject;
                        if (nextJunction) {
                            nextJunction = false;
                            if (objectBelow != null) {
                                // Check if player is on a road before a junction
                                if (key == objectBelow.position) {
                                    stopped = true;
                                    break;
                                }
                            }
                        }
                        // Check if the next road is a junction
                        if (objectChecking.CompareTag("Junction")) {
                            nextJunction = true;
                        } else {
                            // Check the children of the parent object
                            for (int i = 0; i < objectChecking.transform.childCount; i++) {
                                Transform child = objectChecking.transform.GetChild(i);
                                if (child.CompareTag("Junction")) {
                                    nextJunction = true;
                                }
                            }
                            if (stopped) {
                                break;
                            }
                        }
                    }
                }
            }
        } else if (stopped) {
            // If the player has been stopped at a roundabout, check the object below 
            // If the object below is not a junction, continue driving forward, else stop momentarily
            Transform objectBelow = DetectCurrentObjectUnderAgent();
            Transform secondObjectBelow = DetectSecondCurrentObjectUnderAgent();
            startPosition = RoadManager.navMeshStart; 
            endPosition = RoadManager.navMeshEnd;
            if (objectBelow && secondObjectBelow && !restartedCar) {
                if (objectBelow.CompareTag("Junction") || secondObjectBelow.CompareTag("Junction")) {
                    // Stop the car for 0.5 to 2 seconds
                    agent.speed = 0;
                    StartCoroutine(RestartCar());
                } else {
                    if (gameObject.CompareTag("LeftCar")) {
                        agent.SetDestination(endPosition);
                    } else if (gameObject.CompareTag("RightCar")) {
                        agent.SetDestination(startPosition);
                    }
                    // Manage the speed of the NPC car depending on the speed of the car in front
                    Transform carInFront = FindCarInFront(gameObject.tag);
                    if (carInFront) {
                        float desiredSpeed = CalculateDesiredSpeed(carInFront);
                        agent.speed = desiredSpeed;
                    } else {
                        // No car in front, the NPC car can go at maximum speed
                        agent.speed = maxSpeed;
                    }
                }
            }
        }
    }

    Transform DetectCurrentObjectUnderAgent() {
        // Cast a ray downwards from the NavMeshAgent's position
        Ray ray = new Ray(agent.transform.position + Vector3.up, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            GameObject hitObject = hit.collider.gameObject;
            // Return the ultimate parent of the object below the player
            return GetUltimateParentOf(hitObject.transform);
        } else {
            return null;
        }
    }

    Transform DetectSecondCurrentObjectUnderAgent() {
        // Cast a ray downwards from the NavMeshAgent's position
        Ray ray = new Ray(agent.transform.position + Vector3.up, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            GameObject hitObject = hit.collider.gameObject;
            // Return the penultimate parent of the object below the player
            return GetPenultimateParentOf(hitObject.transform);
        } else {
            return null;
        }
    }

    // Function for getting the ultimate parent of an object
    Transform GetUltimateParentOf(Transform child) {
        Transform parent = child.parent;

        // Continue iterating until the object has no parent
        while (parent != null) {
            child = parent;
            parent = parent.parent;
        }
        return child;
    }

    // Function for getting the penultimate (second) parent of an object 
    Transform GetPenultimateParentOf(Transform child) {
        Transform parent = child.parent;
        Transform penultimateParent = null;
        Transform ultimateParent = child;

        // Continue iterating until the object has no parent
        while (parent != null) {
            penultimateParent = ultimateParent;
            ultimateParent = parent;
            parent = parent.parent;
        }

        return penultimateParent;
    }

    // Function for finding the car in the front with a given tag
    private Transform FindCarInFront(string tag) {
        Transform carInFront = null;
        float closestDistance = Mathf.Infinity;

        // Find all cars (excluding right cars)
        GameObject[] leftCars = GameObject.FindGameObjectsWithTag(tag);
        GameObject[] playerCars = new GameObject[0];
        if (tag == "LeftCar") {
            playerCars = GameObject.FindGameObjectsWithTag("Player");
        } 

        // Combine the arrays of cars using Concat 
        GameObject[] allCars = leftCars.Concat(playerCars).ToArray();
        foreach (GameObject carObject in allCars) {
            // Exclude self
            if (carObject != gameObject) {
                Vector3 toCar = carObject.transform.position - transform.position;
                float angleToCar = Vector3.Angle(transform.forward, toCar);
                // Check if the car is in front and closer than the current closest car
                if (angleToCar < 30f && toCar.magnitude < closestDistance) {
                    carInFront = carObject.transform;
                    closestDistance = toCar.magnitude;
                }
            }
        }

        return carInFront;
    }

    // Function for calculating the desired speed of the NPC car 
    // This depends on the speed of the car in front and the distance between the cars
    private float CalculateDesiredSpeed(Transform carInFront) {
        // Randomise following distance
        safeFollowingDistance = UnityEngine.Random.Range(7.5f, 10f);

        // Calculate distance to the car in front
        float distanceToCarInFront = Vector3.Distance(transform.position, carInFront.position);

        // Calculate the factor for adjusting speed based on distance
        float distanceFactor = distanceToCarInFront / safeFollowingDistance;

        // If distanceFactor is greater than 1, use maxSpeed
        if (distanceFactor > 1f) {
            return maxSpeed;
        }

        // Calculate the desired speed based on safe following distance
        float desiredSpeed = Mathf.Lerp(0f, maxSpeed, distanceFactor);

        // The car will have a NavMeshAgent or Rigidbody depending on if it is an NPC car or a normal car
        NavMeshAgent carInFrontAgent = carInFront.GetComponent<NavMeshAgent>();
        Rigidbody carInFrontRigidbody = carInFront.GetComponent<Rigidbody>();

        // Ensure that the calculated desired speed is not greater than the speed of the car in front
        if (carInFrontAgent) {
            float speedOfCarInFront = carInFrontAgent.velocity.magnitude;
            desiredSpeed = Mathf.Min(desiredSpeed, speedOfCarInFront);
        } else if (carInFrontRigidbody) {
            float speedOfCarInFront = carInFrontRigidbody.velocity.magnitude;
            desiredSpeed = Mathf.Min(desiredSpeed, speedOfCarInFront);
        }

        return desiredSpeed;
    }
}
