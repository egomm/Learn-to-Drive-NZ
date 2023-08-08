using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour {
    // Class for the road information
    public class RoadInformation {
        public GameObject GameObject;
        public Quaternion Quaternion;

        public RoadInformation(GameObject gameObject, Quaternion quaternion) {
            GameObject = gameObject;
            Quaternion = quaternion;
        }
    }

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
    public static Dictionary<Vector3, RoadInformation> roadInformation = new Dictionary<Vector3, RoadInformation>();
    // Nearby road coordinates
    private List<Vector3> nearbyRoadCoordinates = new List<Vector3>();
    // Active road coordinates 
    private List<Vector3> activeRoadCoordinates = new List<Vector3>();

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
        int roundCount = Random.Range(6, 8);
        // Measure the current angle in degrees
        bool changedAngle = false;
        int currentAngle = 0;
        Vector3 previousRoadCoordinates = new Vector3(0, 0, 0);
        Vector3 roadCoordinates = new Vector3(0, 0, 0);
        int previousRoundabout = 0;
        string previousRoad = "straight";
        for (int i = 0; i < roundCount; i++) {
            // Make sure there can only be a roundabout every 5 roads
            float randomRoadGeneration = Random.Range(0f, 1f);
            GameObject roadType;
            if (i > 0) {
                if (randomRoadGeneration < roundaboutFrequency && (i - previousRoundabout) > 5) {
                    // Roundabout
                    previousRoundabout = i;
                    roadType = twoRoundabout; // Temporary
                    roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                    previousRoad = "roundabout";
                } else if (randomRoadGeneration < curveFrequency + roundaboutFrequency) {
                    // Curve: need to make this adjust to the coordinate
                    // If the current angle is 0 degrees, decrease it by 90 to 270 degrees
                    // This needs work 
                    if (changedAngle) {
                        currentAngle = (currentAngle <= 90) ? 270 : currentAngle - 180;
                    } else {
                        currentAngle = 270;
                        changedAngle = true;
                    }
                    roadType = curvedRoad;
                    // This needs to depend on the curve
                    //Debug.Log(currentAngle);
                    if (previousRoad == "straight") {
                        // This isn't accurate atm
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-5.7f, 0, 31.1f);
                        } else if (currentAngle == 90) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-24.99f, 0, 0.188f);
                        }
                    } else if (previousRoad == "curved") {
                        roadCoordinates = previousRoadCoordinates + new Vector3(-35.8f, 0, 0.373f);
                    } else {
                        // Roundabout (temp)
                        roadCoordinates = previousRoadCoordinates + new Vector3(-5.7f, 0, 31.1f);
                    }
                    previousRoad = "curved";
                } else {
                    // Straight road
                    roadType = straightRoad;
                    // Check what the previous road was
                    if (previousRoad == "straight") {
                        // change to switch case?
                        if (currentAngle == 0) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                        } else if (currentAngle == 90) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(13.999f, 0, 0);
                        } else if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-13.999f, 0, 0);
                        }
                    } else if (previousRoad == "curved") {
                        // The angle isn't being adjusted correctly
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-24.99f, 0, 0.182f);
                        } else {
                            currentAngle = 180;
                            roadCoordinates = previousRoadCoordinates + new Vector3(-24.99f, 0, 0.182f);
                        }
                    }
                    previousRoad = "straight";
                }
            } else {
                // First road 
                roadType = straightRoad;
            }
            Debug.Log(roadType);
            roadInformation.Add(roadCoordinates, new RoadInformation(roadType, Quaternion.Euler(new Vector3(0, currentAngle, 0))));
            if (currentAngle == 180) {
                currentAngle = 90;
            }
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
                if (!activeRoadCoordinates.Contains(roadCoordinate)) {
                    RoadInformation currentRoadInformation = roadInformation[roadCoordinate];
                    Instantiate(currentRoadInformation.GameObject, roadCoordinate, currentRoadInformation.Quaternion);
                    activeRoadCoordinates.Add(roadCoordinate);
                }
            }
        }
    }

    private void UpdateNearbyRoadCoordinates() {
        nearbyRoadCoordinates.Clear();

        // Calculate the squared distance 
        float squaredDistanceThreshold = 10000f; // (100 units distance)^2
        Vector3 playerPosition = player.transform.position;

        foreach (var roadCoordinate in roadInformation.Keys) {
            // Add the road coordinate if it is nearby to the player and hasn't already been added
            if (Vector3.SqrMagnitude(roadCoordinate - playerPosition) < squaredDistanceThreshold && !nearbyRoadCoordinates.Contains(roadCoordinate)) {
                nearbyRoadCoordinates.Add(roadCoordinate);
            }
        }
    }
}
