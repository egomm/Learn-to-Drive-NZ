using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCCarController : MonoBehaviour
{
    private NavMeshAgent agent;

    private bool started = false;
    private Vector3 startPosition;
    private Vector3 endPosition; // Store the end position
    private Vector3 lastInstantiated = new Vector3(0, 0, 0);

    public float maxStuckTime = 0.25f; // Maximum time considered as stuck
    private Vector3 lastPosition;
    private float timeStuck = 0.0f;
    private bool isStuck = false;
    
    IEnumerator ExecuteDelayedTask() {
        // Wait for the specified delay
        yield return new WaitForSeconds(0.25f);

        // This code will execute after the delay
        // Find a point at the end of the NavMesh initially
        startPosition = FindStartOfNavMesh();
        endPosition = FindEndOfNavMesh();
        if (endPosition != Vector3.zero) {
            NavMeshHit hit;
            if (agent.isOnNavMesh) {
                if (gameObject.tag == "LeftCar") {
                    agent.destination = endPosition;
                } else if (gameObject.tag == "RightCar") {
                    agent.destination = startPosition;
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
            NavMeshHit hit;
            /*if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas)) {
                // Update the end position dynamically during the update loop
                startPosition = FindStartOfNavMesh();
                endPosition = FindEndOfNavMesh();    
                if (RoadGenerator.lastInstantiated != lastInstantiated) {
                    lastInstantiated = RoadGenerator.lastInstantiated;
                    GameObject lastInstantiatedRoad = RoadGenerator.lastInstantiatedRoad;
                    if (lastInstantiatedRoad != null) {
                        if (gameObject.tag == "LeftCar") {
                            if (agent.isOnNavMesh) {
                                agent.destination = endPosition;
                            }
                        } else if (gameObject.tag == "RightCar") {
                            if (agent.isOnNavMesh) {
                                agent.destination = startPosition;
                            }
                        }
                        /*float furthestDistance = 0;
                        float closestDistance = 1000;
                        Vector3 destinationCoordinates = RoadGenerator.lastInstantiated;
                        //Debug.Log(lastInstantiatedRoad.gameObject.transform.childCount);
                        foreach (Transform child in lastInstantiatedRoad.transform) {
                            // Has children
                            if (child.gameObject.transform.childCount > 0) {
                                if (child.name == "Empty") {
                                    destinationCoordinates = child.transform.position;
                                }
                            } else {
                                // Doesn't have children
                                if (gameObject.tag == "LeftCar" && ((child.gameObject.layer) == 6 || (child.gameObject.layer) == 8)) {
                                    // Left lane or turning
                                    Vector3 childCoordinates = child.gameObject.transform.position;
                                    float distance = Vector3.Distance(childCoordinates, Vector3.zero);
                                    if (distance > furthestDistance) {
                                        furthestDistance = distance;
                                        destinationCoordinates = childCoordinates;
                                    }
                                } else if (gameObject.tag == "RightCar" && (child.gameObject.layer) == 7) {
                                    // Right lane
                                    // RIGHT CAR NEEDS TO GO TO CLOSEST DISTANCE
                                    Vector3 childCoordinates = child.gameObject.transform.position;
                                    float distance = Vector3.Distance(childCoordinates, Vector3.zero);
                                    if (distance < closestDistance) {
                                        closestDistance = distance;
                                        destinationCoordinates = childCoordinates;
                                    }
                                }
                            }
                        }
                        Debug.Log("FINISHED CHECKING!");
                        //Debug.Log(destinationCoordinates);
                        float roadDistance = Vector3.Distance(destinationCoordinates, transform.position);
                        if (roadDistance >= furthestRoad) {
                        furthestRoad = roadDistance;
                        agent.destination = destinationCoordinates;
                        }
                        if (agent.isOnNavMesh) {
                            agent.destination = destinationCoordinates;
                        }*/
                    /*}
                }
            }*/
            if (agent.isOnNavMesh) {
                startPosition = FindStartOfNavMesh();
                endPosition = FindEndOfNavMesh();
                if (gameObject.tag == "LeftCar") {
                    agent.destination = endPosition;
                } else if (gameObject.tag == "RightCar") {
                    agent.SetDestination(startPosition);
                }
            } 
            if (!agent.isOnNavMesh || Vector3.Distance(transform.position, MoveCar.position) > 150){
                Debug.Log("DESTROYED SELF");
                Destroy(gameObject);
            }
        }
    }

    Vector3 FindStartOfNavMesh() {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(Vector3.zero, out hit, float.MaxValue, NavMesh.AllAreas)) {
            return hit.position;
        } else {
            // If no valid position found, return the original targetPosition
            return new Vector3(0, 0, 0);
        }
    }

    Vector3 FindEndOfNavMesh() {
        Vector3 origin = Vector3.zero;
        float maxDistance = 1000f; // Maximum distance to search for a point
        NavMeshHit hit;

        Vector3 furthestPoint = Vector3.zero;
        float furthestDistance = 0f;

        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        Vector3[] vertices = navMeshData.vertices;

        foreach (Vector3 vertex in vertices) {
            Vector3 vertexPosition = RoadGenerator.lastInstantiatedRoad.transform.TransformPoint(vertex);

            if (Vector3.Distance(origin, vertexPosition) <= maxDistance) {
                if (NavMesh.SamplePosition(vertexPosition, out hit, maxDistance, 1 << 3)) {
                    float distance = Vector3.Distance(origin, hit.position);
                    if (distance > furthestDistance) {
                        furthestDistance = distance;
                        furthestPoint = hit.position;
                    }
                }
            }
        }

        return furthestPoint;
    }
}
