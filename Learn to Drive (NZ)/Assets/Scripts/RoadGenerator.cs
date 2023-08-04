using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour {
    // Straight road prefab
    public GameObject straightRoad;
    // Curved road prefab
    public GameObject curvedRoad;
    // Two roundabout prefab
    public GameObject twoRoundabout;
    // Three roundabout prefab
    public GameObject threeRoundabout;
    // Four roundabout prefab
    public GameObject fourRoundabout;

    // Reference to the player
    public GameObject player;

    // Previous player position
    private Vector3 lastPlayerPosition;

    // Last value added to the dictionary
    public Vector3 lastRoad = new Vector3(0, 0, 0);
    // Dictionary for managing all of the roads with a Vector3 and object
    public static Dictionary<Vector3, GameObject> roadInformation = new Dictionary<Vector3, GameObject>();
    // Nearby road coordinates
    private List<Vector3> nearbyRoadCoordinates = new List<Vector3>();

    // Roundabout frequency
    private float roundaboutFrequency = 0.075f;
    private float curveFrequency = 0.225f;
    private float straightFrequency = 0.7f;

    // Start is called before the first frame update
    void Start() {
        /*for (int i = 0; i < 60; i++) {
            Instantiate(straightRoad, new Vector3(0, 0, 13.999f*i), Quaternion.identity);
        }*/
        // Generate between 600 and 800 roads
        int roundCount = Random.Range(600, 800);
        Vector3 previousRoadCoordinates = new Vector3(0, 0, 0);
        Vector3 roadCoordinates = new Vector3(0, 0, 0);
        int previousRoundabout = 0;
        for (int i = 0; i < roundCount; i++) {
            // Make sure there can only be a roundabout every 5 roads
            float randomRoadGeneration = Random.Range(0f, 1f);
            GameObject roadType;
            if (randomRoadGeneration < roundaboutFrequency && (i - previousRoundabout) > 5) {
                // Roundabout
                previousRoundabout = i;
                roadType = twoRoundabout; // Temporary
            } else if (randomRoadGeneration < curveFrequency + roundaboutFrequency) {
                // Curve
                roadType = curvedRoad;
            } else {
                // Straight road
                roadType = straightRoad;
            }
            if (roadInformation.Count > 0) {
                // Not starting at the first road
                roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
            }
            roadInformation.Add(roadCoordinates, roadType);
            previousRoadCoordinates = roadCoordinates;
        }

        UpdateNearbyRoadCoordinates();
    }

    void Update() {
        //Debug.Log(player.transform.position);

        // Check if the player has moved to a new position, then update nearbyRoadCoordinates
        if (Vector3.Distance(player.transform.position, lastPlayerPosition) >= 10f) {
            UpdateNearbyRoadCoordinates();
            lastPlayerPosition = player.transform.position;
        }

        // Instantiate nearby road objects at a controlled frequency
        if (Time.frameCount % 10 == 0) {
            foreach (var roadCoordinate in nearbyRoadCoordinates) {
                Instantiate(roadInformation[roadCoordinate], roadCoordinate, Quaternion.identity);
            }
        }
    }

    private void UpdateNearbyRoadCoordinates() {
        nearbyRoadCoordinates.Clear();

        // Calculate the squared distance 
        float squaredDistanceThreshold = 10000f; // (100 units distance)^2
        Vector3 playerPosition = player.transform.position;

        foreach (var roadCoordinate in roadInformation.Keys) {
            if (Vector3.SqrMagnitude(roadCoordinate - playerPosition) < squaredDistanceThreshold) {
                nearbyRoadCoordinates.Add(roadCoordinate);
            }
        }
    }
}
