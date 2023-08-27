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
    private bool isStuck = false;

    private float safeFollowingDistance = 10f; 
    private float maxSpeed = 14f; // Around 50kmph

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
        }
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
        if (started) {
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
            }
        }
    }

    void OnCollisionEnter(Collision collision) {
        Debug.Log("ENTERED: " + collision.gameObject.layer);
    }

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
                if (angleToCar < 90f && toCar.magnitude < closestDistance) {
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
