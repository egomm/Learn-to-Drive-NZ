using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

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
    private float roundaboutFrequency = 0.2f;//0.075f;
    private float curveFrequency = 0.05f;//0.225f;
    private float straightFrequency = 0.75f;//0.7f;

    // Start is called before the first frame update
    void Start() {
        /*for (int i = 0; i < 60; i++) {
            Instantiate(straightRoad, new Vector3(0, 0, 13.999f*i), Quaternion.identity);
        }*/
        // Generate between 600 and 800 roads
        int roundCount = Random.Range(600, 800);
        // Measure the current angle in degrees
        bool changedAngle = false;
        int currentAngle = 0;
        Vector3 previousRoadCoordinates = new Vector3(0, 0, 0);
        Vector3 roadCoordinates = new Vector3(0, 0, 0);
        int previousRoundabout = 0;
        int previousCurve = 0;
        string previousRoad = "straight";
        bool lastUsedAlternative = false;
        for (int i = 0; i < roundCount; i++) {
            // Make sure there can only be a roundabout every 5 roads
            float randomRoadGeneration = Random.Range(0f, 1f);
            GameObject roadType;
            int alternativeAngle = -1;
            if (i > 0) {
                if (randomRoadGeneration < roundaboutFrequency && (i - previousRoundabout) > 1 && (i - previousCurve) > 1) {
                    // Roundabout
                    previousRoundabout = i;
                    roadType = twoRoundabout; // Temporary
                    //Debug.Log("Roundabout: " + currentAngle);
                    //Debug.Log(previousRoadCoordinates);
                    if (previousRoad == "straight") {
                        if (currentAngle != 90) {
                            alternativeAngle = currentAngle + 90;
                        }
                        if (currentAngle == 0 || currentAngle == 90) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 42.2f);
                        } else {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-40f, 0, 0);
                        }
                    } else if (previousRoad == "curved") {
                        // In this case the current angle is 90
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-53.23f, 0, 0.19f);
                        } else {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-5.7f, 0, 59.328f);
                        }
                        if (currentAngle == 270) {
                            alternativeAngle = currentAngle + 90;
                        }
                    }
                    previousRoad = "roundabout";
                } else if (randomRoadGeneration < curveFrequency + roundaboutFrequency) {
                    // Curve: need to make this adjust to the coordinate
                    // If the current angle is 0 degrees, decrease it by 90 to 270 degrees
                    previousCurve = i;
                    // This needs work 
                    if (changedAngle) {
                        currentAngle = (currentAngle <= 90) ? 270 : currentAngle - 180;
                    } else {
                        currentAngle = 270;
                        changedAngle = true;
                    }
                    roadType = curvedRoad;
                    // This needs to depend on the curve
                    if (previousRoad == "straight") {
                        // This isn't accurate atm
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-5.7f, 0, 31.1f);
                        } else {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-24.99f, 0, 0.188f);
                        }
                    } else if (previousRoad == "curved") {
                        //Debug.Log("Last curved: " + currentAngle);
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-11.403f, 0, 48.131f);
                        } else {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-35.8f, 0, 0.373f);
                        }
                    } else {
                        // Roundabout (temp)
                        if (currentAngle == 270) {
                            //Debug.Log("CURVED R!: " + currentAngle);
                            roadCoordinates = previousRoadCoordinates + new Vector3(-5.7f, 0, 59.37f);
                        } else {
                            //Debug.Log("CURVED R: " + currentAngle);
                            roadCoordinates = previousRoadCoordinates + new Vector3(-53.23f, 0, 0.19f);
                        }
                    }
                    previousRoad = "curved";
                } else {
                    // Straight road
                    roadType = straightRoad;
                    // Check what the previous road was
                    if (previousRoad == "straight") {
                        // change to switch case?
                        if (currentAngle == 0 || currentAngle == 180) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                        } else if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-13.999f, 0, 0);
                        } else {
                            roadCoordinates = previousRoadCoordinates + new Vector3(13.999f, 0, 0);
                        }
                        //Debug.Log("Last used ALT?: " + lastUsedAlternative);
                        if (lastUsedAlternative) {
                            //Debug.Log("LAst used ALT: " + currentAngle);
                            alternativeAngle = currentAngle + 90;
                            roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                        }
                    } else if (previousRoad == "curved") {
                        // The angle isn't being adjusted correctly
                        // Angle 0 should? be fine
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-24.99f, 0, 0.182f);
                        } else {
                            currentAngle -= 90; // TEMP WAS ALTANGLE BEFORE
                            roadCoordinates = previousRoadCoordinates + new Vector3(-5.7f, 0, 31.111f);
                        }
                    } else {
                        // Roundabout (temp)
                        /*THIS IS WHERE THE ONlY ISSUE IS CURRENTLY!*/
                        //alternativeAngle = currentAngle + 90;
                        if (currentAngle == 270) {
                            //Debug.Log("Straight Roundabout!: " + currentAngle);
                            roadCoordinates = previousRoadCoordinates + new Vector3(-42.2f, 0, 0);
                            lastUsedAlternative = false;
                        } else {
                            //Debug.Log("Straight Roundabout: " + currentAngle);
                            roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 42.2f);
                            if (lastUsedAlternative) {
                                //alternativeAngle = currentAngle + 90;
                                //roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                            }
                        }
                    }
                    previousRoad = "straight";
                }
            } else {
                // First road 
                roadType = straightRoad;
            }
            //Debug.Log(roadType);
            int angle = currentAngle;
            if (alternativeAngle >= 0) {
                angle = alternativeAngle;
                lastUsedAlternative = true;
                //Debug.Log("Using alt angle of: " + alternativeAngle);
            } else if (previousRoad != "roundabout") {
                lastUsedAlternative = false;
            }/* else {
                lastUsedAlternative = true;
            }*/
            if (previousRoad == "roundabout") {
                lastUsedAlternative = false;
            }
            //Debug.Log(angle);
            //Debug.Log(roadCoordinates);
            //Debug.Log(lastUsedAlternative);
            if (!roadInformation.ContainsKey(roadCoordinates)) {
                roadInformation.Add(roadCoordinates, new RoadInformation(roadType, Quaternion.Euler(new Vector3(0, angle, 0))));
                previousRoadCoordinates = roadCoordinates;
            } else {
                i--;
            }
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
                    GameObject roadPrefab = currentRoadInformation.GameObject;
                    Quaternion roadRotation = currentRoadInformation.Quaternion;

                    GameObject roadInstance = Instantiate(roadPrefab, roadCoordinate, roadRotation);

                    /*NavMeshSurface navMeshRoad = roadInstance.GetComponent<NavMeshSurface>();
                    if (navMeshRoad != null) {
                        navMeshRoad.BuildNavMesh();
                    } else {
                        Debug.Log("No NavMeshSurface component found on the instantiated road object.");
                    }
                    foreach (Transform child in roadInstance.transform) {
                        GameObject childObject = child.gameObject;
                        bool isLeftLane = childObject.layer == LayerMask.NameToLayer("left");
                        bool isRightLane = childObject.layer == LayerMask.NameToLayer("right");
                        if (isLeftLane || isRightLane) {
                            NavMeshSurface navMeshSurface = child.GetComponent<NavMeshSurface>();
                            if (navMeshSurface != null) {
                                navMeshSurface.BuildNavMesh();
                            } else {
                                Debug.Log("No NavMeshSurface component found on the instantiated road object.");
                            }
                        }
                    }*/
                    Debug.Log("Adding?");
                    Debug.Log(roadInstance);
                    
                    // Add NavMeshSurface components for each desired NavMesh
                    NavMeshSurface leftLaneNavMesh = gameObject.AddComponent<NavMeshSurface>();
                    NavMeshSurface rightLaneNavMesh = gameObject.AddComponent<NavMeshSurface>();
                    NavMeshSurface combinedNavMesh = gameObject.AddComponent<NavMeshSurface>();
                    /*
                                         NavMeshSurface leftLaneNavMesh = roadInstance.AddComponent<NavMeshSurface>();
                    NavMeshSurface rightLaneNavMesh = roadInstance.AddComponent<NavMeshSurface>();
                    NavMeshSurface combinedNavMesh = roadInstance.AddComponent<NavMeshSurface>();*/

                    // Set properties for the left lane NavMesh
                    //leftLaneNavMesh.collectObjects = CollectObjects.Volume;
                    leftLaneNavMesh.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                    leftLaneNavMesh.layerMask = LayerMask.GetMask("left");
                    leftLaneNavMesh.BuildNavMesh();

                    rightLaneNavMesh.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                    rightLaneNavMesh.layerMask = LayerMask.GetMask("right");
                    rightLaneNavMesh.BuildNavMesh();

                    // Set properties for the combined NavMesh
                    combinedNavMesh.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
                    combinedNavMesh.layerMask = LayerMask.GetMask("left", "right");
                    combinedNavMesh.BuildNavMesh();

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
