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
    private float timeStuck = 0.0f;
    private bool restartedCar = false;

    private float safeFollowingDistance = 10f; 
    private float maxSpeed = 14f; // Around 50kmph
    bool stopped = false;

    int attempts = 0;

    IEnumerator ExecuteDelayedTask() {
        // Wait for the specified delay
        yield return new WaitForSeconds(0.25f);

        // This code will execute after the delay
        // Find a point at the end of the NavMesh initially
        startPosition = RoadManager.navMeshStart;
        endPosition = RoadManager.navMeshEnd;
        if (endPosition != Vector3.zero) {
            NavMeshHit hit;
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
        if (gameObject.tag == "LeftCar") {
            agent.areaMask = 1 << 3;
        } else if (gameObject.tag == "RightCar") {
            agent.areaMask = 1 << 4;
        }
        StartCoroutine(ExecuteDelayedTask());
    }

    void Update() {
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
                // Need to do this for the right car
                if (gameObject.CompareTag("LeftCar")) {
                    var prevKey = Vector3.zero;
                    Transform objectBelow = DetectCurrentObjectUnderAgent();
                    foreach (var key in RoadManager.roadInformation.Keys) {
                        // Need to detect BEFORE the car has even get there
                        GameObject objectChecking = RoadManager.roadInformation[key].GameObject;
                        if (objectChecking.CompareTag("Junction")) {
                            if (prevKey != Vector3.zero) {
            
                                if (objectBelow != null) {
                                    // Check if player is on road before intersection
                                    if (prevKey == objectBelow.position) {
                                        stopped = true;
                                        break;
                                    }
                                }
                            }
                        } else {
                            // Check the penultimate 
                            //Transform secondObjectBelow = DetectSecondCurrentObjectUnderAgent();
                            for (int i = 0; i < objectChecking.transform.childCount; i++) {
                                Transform child = objectChecking.transform.GetChild(i);

                                if (child.CompareTag("Junction")) {
                                    if (prevKey != Vector3.zero) {
                                        if (objectBelow != null) {
                                            // Check if player is on a roundabout
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
                        prevKey = key;
                    }
                } else if (gameObject.CompareTag("RightCar")) {
                    Transform objectBelow = DetectCurrentObjectUnderAgent();
                    bool nextJunction = false;
                    foreach (var key in RoadManager.roadInformation.Keys) {
                        // Need to detect BEFORE the car has even get there
                        GameObject objectChecking = RoadManager.roadInformation[key].GameObject;
                        if (nextJunction) {
                            nextJunction = false;
                            if (objectBelow != null) {
                                // Check if player is on road before intersection
                                if (key == objectBelow.position) {
                                    stopped = true;
                                    break;
                                }
                            }
                        }
                        if (objectChecking.CompareTag("Junction")) {
                            nextJunction = true;
                        } else {
                            // Check the penultimate 
                            //Transform secondObjectBelow = DetectSecondCurrentObjectUnderAgent();
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
            Transform objectBelow = DetectCurrentObjectUnderAgent();
            Transform secondObjectBelow = DetectSecondCurrentObjectUnderAgent();
            startPosition = RoadManager.navMeshStart; 
            endPosition = RoadManager.navMeshEnd;
            if (objectBelow && secondObjectBelow && !restartedCar) {
                if (objectBelow.CompareTag("Junction") || secondObjectBelow.CompareTag("Junction")) {
                    agent.speed = 0;
                    StartCoroutine(RestartCar());
                } else {
                    if (gameObject.CompareTag("LeftCar")) {
                        agent.SetDestination(endPosition);
                    } else if (gameObject.CompareTag("RightCar")) {
                        agent.SetDestination(startPosition);
                    }
                    Transform carInFront = FindCarInFront(gameObject.tag);
                    if (carInFront) {
                        float desiredSpeed = CalculateDesiredSpeed(carInFront);
                        agent.speed = desiredSpeed;
                    } else {
                        agent.speed = maxSpeed;
                    }
                }
            }
        }
    }

    Transform DetectCurrentObjectUnderAgent() {
        // Cast a ray downwards from the NavMeshAgent's position
        Ray ray = new Ray(agent.transform.position + Vector3.up * 0.1f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;
            return GetUltimateParentOf(hitObject.transform);
        } else {
            return null;
        }
    }

    Transform DetectSecondCurrentObjectUnderAgent() {
        // Cast a ray downwards from the NavMeshAgent's position
        Ray ray = new Ray(agent.transform.position + Vector3.up * 0.1f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            GameObject hitObject = hit.collider.gameObject;
            Debug.Log(hitObject.transform);
            return GetPenultimateParentOf(hitObject.transform);
        } else {
            return null;
        }
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

    Transform GetPenultimateParentOf(Transform child) {
        Transform parent = child.parent;
        Transform penultimateParent = null;
        Transform ultimateParent = child;

        while (parent != null) {
            penultimateParent = ultimateParent;
            ultimateParent = parent;
            parent = parent.parent;
        }

        return penultimateParent;
    }

    // Only for left car atm
    /*void OnCollisionEnter(Collision collision) {
        if (gameObject.tag == "LeftCar") {
            Transform parent = GetUltimateParentOf(collision.gameObject.transform);
            Debug.Log("ENTERED: " + parent.name);
            bool next = false;
            foreach (var key in RoadManager.activeRoadPrefabs.Keys) {
                //Debug.Log(RoadManager.activeRoadPrefabs[key]);
                if (next) {
                    Debug.Log("Next Road: " + RoadManager.activeRoadPrefabs[key].tag);
                    break;
                }
                if (RoadManager.activeRoadPrefabs[key].Equals(parent.gameObject)) {
                    Debug.Log("Has next");
                    next = true;
                }
            }
        }
    }*/

        // For left cars only currently
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

        // Ensure that the calculated desired speed is not greater than the speed of the car in front
        NavMeshAgent carInFrontAgent = carInFront.GetComponent<NavMeshAgent>();
        Rigidbody carInFrontRigidbody = carInFront.GetComponent<Rigidbody>();

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
