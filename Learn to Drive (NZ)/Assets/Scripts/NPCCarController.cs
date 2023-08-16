using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCCarController : MonoBehaviour {
    public Transform goal;

    void Start() {
    }

    bool CheckIfCarOnNavMesh() {
        NavMeshHit hit;
        //int desiredNavMeshLayer = NavMesh.GetNavMeshLayerFromName("YourDesiredLayerName");
        return NavMesh.SamplePosition(transform.position, out hit, 0.5f, NavMesh.AllAreas);
    }

    void Update() {
        if (CheckIfCarOnNavMesh()) {
            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            agent.destination = goal.position;
        }
    }
}
