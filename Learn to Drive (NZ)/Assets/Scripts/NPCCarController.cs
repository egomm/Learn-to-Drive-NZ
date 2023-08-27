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
            NavMeshHit hit;
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
                } 
                if (!agent.isOnNavMesh || Vector3.Distance(transform.position, MoveCar.position) > 150){
                    Debug.Log("DESTROYED SELF");
                    Destroy(gameObject);
                }
            }
        }
    }
}
