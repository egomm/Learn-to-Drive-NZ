using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCCarController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Vector3 endPosition; // Store the end position
    private Vector3 lastInstantiated = new Vector3(0, 0, 0);

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Find a point at the end of the NavMesh initially
        endPosition = FindEndOfNavMesh();
        if (endPosition != Vector3.zero)
        {
            agent.destination = endPosition; // Set the default destination to the end of the NavMesh
        }
    }

    void Update() {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas)) {
            // Update the end position dynamically during the update loop
            endPosition = FindEndOfNavMesh();
            if (RoadGenerator.lastInstantiated != lastInstantiated) {
                lastInstantiated = RoadGenerator.lastInstantiated;
                Debug.Log(RoadGenerator.lastInstantiated);
                agent.destination = RoadGenerator.lastInstantiated;
            }
        }
    }

    Vector3 FindEndOfNavMesh()
    {
        Vector3 startPosition = transform.position;
        NavMeshHit hit;
        Vector3 endPosition = Vector3.zero;

        // Raycast from the player's starting position outward to find the end of the NavMesh
        if (NavMesh.SamplePosition(startPosition, out hit, 1000f, NavMesh.AllAreas))
        {
            endPosition = hit.position;
        }

        return endPosition;
    }
}
