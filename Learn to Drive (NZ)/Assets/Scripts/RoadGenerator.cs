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
    // Alternative Curved Road 
    public GameObject alternativeCurvedRoad;
    // Two roundabout prefab
    public GameObject twoRoundabout;
    // Three roundabout prefab
    public GameObject threeRoundabout;
    // Four roundabout prefab
    public GameObject fourRoundabout;
    // Three intersection prefab (right)
    public GameObject threeIntersectionRight;
    // Three intersection prefab (left)
    public GameObject threeIntersectionLeft;
    // Four intersection
    public GameObject fourIntersection;

    // Reference to the player
    public GameObject player;

    // Previous player position
    private Vector3 lastPlayerPosition;

    // Last instantiated 
    public static Vector3 lastInstantiated = new Vector3(0, 0, 0);
    // Last instantiated road
    public static GameObject lastInstantiatedRoad = null;

    // Last value added to the dictionary
    public Vector3 lastRoad = new Vector3(0, 0, 0); // Redundant?
    // Dictionary for managing all of the roads with a Vector3 and object
    public static Dictionary<Vector3, RoadInformation> roadInformation = new Dictionary<Vector3, RoadInformation>();
    // Nearby road coordinates
    private List<Vector3> nearbyRoadCoordinates = new List<Vector3>();
    // Active road coordinates 
    private List<Vector3> activeRoadCoordinates = new List<Vector3>();
    // Active road prefabs 
    public static Dictionary<Vector3, GameObject> activeRoadPrefabs = new Dictionary<Vector3, GameObject>();

    // Roundabout frequency
    private float roundaboutFrequency = 0.1f;
    private float curveFrequency = 0.05f;
    private float intersectionFrequency = 0.15f;
    private float straightFrequency = 0.7f;

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
        int previousIntersection = 0;
        string previousRoad = "straight";
        bool lastUsedAlternative = false;
        for (int i = 0; i < roundCount; i++) {
            // Make sure there can only be a roundabout every 5 roads
            float randomRoadGeneration = Random.Range(0f, 1f);
            GameObject roadType;
            int alternativeAngle = -1;
            if (i > 0) {
                if (randomRoadGeneration < roundaboutFrequency && (i - previousRoundabout) > 3 && (i - previousIntersection) > 3 && (i - previousCurve) > 1) {
                    // Roundabout
                    previousRoundabout = i;
                    float twoRoundaboutFrequency = 0.15f;
                    float threeRoundaboutFrequency = 0.40f;
                    float fourRoundaboutFrequency = 0.45f;
                    float randomRoundaboutGeneration = Random.Range(0f, 1f);
                    Debug.Log(randomRoundaboutGeneration);
                    if (randomRoundaboutGeneration < twoRoundaboutFrequency) {
                        roadType = twoRoundabout;
                    } else if (randomRoundaboutGeneration < twoRoundaboutFrequency + threeRoundaboutFrequency) {
                        roadType = threeRoundabout;
                    } else {
                        roadType = fourRoundabout;
                    }
                    if (previousRoad == "straight" || previousRoad == "fourIntersection") {
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
                    if (previousRoad == "straight" || previousRoad == "fourIntersection") {
                        // This isn't accurate atm
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-5.7f, 0, 31.1f);
                        } else {
                            // WAS 24.99 BEFORE
                            roadType = alternativeCurvedRoad;
                            roadCoordinates = previousRoadCoordinates + new Vector3(-24.99f, 0, 0.188f);
                        }
                    } else if (previousRoad == "curved") {
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-11.403f, 0, 48.131f);
                        } else {
                            roadType = alternativeCurvedRoad;
                            roadCoordinates = previousRoadCoordinates + new Vector3(-35.8f, 0, 0.373f);
                        }
                    } else {
                        // Roundabout (temp)
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-5.7f, 0, 59.37f);
                        } else {
                            roadType = alternativeCurvedRoad;
                            roadCoordinates = previousRoadCoordinates + new Vector3(-53.23f, 0, 0.19f);
                        }
                    }
                    previousRoad = "curved";
                } else if (randomRoadGeneration < curveFrequency + roundaboutFrequency + intersectionFrequency && (i - previousRoundabout) > 3 && (i - previousIntersection) > 3 && (i - previousCurve) > 1) {
                    // Intersection 
                    // NEED TO ALSO DO THREE INTERSECTION
                    previousIntersection = i;
                    roadCoordinates = previousRoadCoordinates + new Vector3(1, 0, 0);
                    Debug.Log("Need intersection: Current Angle: " + currentAngle + " Coordinates: " + roadCoordinates + " Previous: " + previousRoad);
                    //roadType = fourIntersection;
                    string intersectionType = "threeIntersection";
                    //roadType = fourIntersection;
                    // Test four intersection first
                    if (intersectionType == "fourIntersection") {
                        roadType = fourIntersection;
                        if (previousRoad == "straight") {
                            if (currentAngle == 0 || currentAngle == 180) {
                                roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                            } else if (currentAngle == 270) {
                                roadCoordinates = previousRoadCoordinates + new Vector3(-13.999f, 0, 0);
                            }
                            if (lastUsedAlternative) {
                                alternativeAngle = currentAngle + 90;
                                roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                            }
                        }
                    } else {
                        roadType = threeIntersectionLeft;
                        // Three intersection (type will depend if left or right)
                        if (previousRoad == "straight") {
                            if (currentAngle == 0 || currentAngle == 180) {
                                //currentAngle += 90;
                                roadType = threeIntersectionLeft;
                                roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                            } else if (currentAngle == 270) {
                                //currentAngle -= 90;
                                roadType = threeIntersectionRight;
                                roadCoordinates = previousRoadCoordinates + new Vector3(-13.999f, 0, 0);
                            }
                            if (lastUsedAlternative) {
                                Debug.Log("LAST USED ALT!");
                                alternativeAngle = currentAngle + 90;
                                roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                            }
                        }
                    }
                    // Temp
                    previousRoad = "fourIntersection";

                    /*if (previousRoad == "straight") {
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
                    }*/
                    //previousRoad = "intersection"; // change this to intersection four
                } else {
                    // Straight road
                    roadType = straightRoad;
                    // Check what the previous road was
                    if (previousRoad == "straight" || previousRoad == "fourIntersection") {
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
            if (previousRoad == "fourIntersection") {
                if (currentAngle == 0 || currentAngle == 180) {
                    currentAngle -= 90;
                    if (currentAngle == -90) {
                        currentAngle = 270;
                    } 
                } else if (currentAngle == 270) {
                    currentAngle = 0;
                }
                Debug.Log("CURRENT: " + currentAngle);
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
            bool addedNewRoad = false;
            foreach (var roadCoordinate in nearbyRoadCoordinates) {
                if (!activeRoadCoordinates.Contains(roadCoordinate)) {
                    RoadInformation currentRoadInformation = roadInformation[roadCoordinate];
                    GameObject roadPrefab = currentRoadInformation.GameObject;
                    Quaternion roadRotation = currentRoadInformation.Quaternion;
                    GameObject roadInstance = Instantiate(roadPrefab, roadCoordinate, roadRotation);
                    lastInstantiated = roadCoordinate;
                    lastInstantiatedRoad = roadInstance;
                    activeRoadCoordinates.Add(roadCoordinate);
                    activeRoadPrefabs[roadCoordinate] = roadInstance;
                    addedNewRoad = true;
                }
            }
            if (addedNewRoad) {
                // Remove existing NavMeshSurface components
                NavMeshSurface[] existingNavMeshSurfaces = gameObject.GetComponents<NavMeshSurface>();
                foreach (NavMeshSurface existingSurface in existingNavMeshSurfaces) {
                    Destroy(existingSurface);
                }

                // Add NavMeshSurface components for each desired NavMesh
                NavMeshSurface leftLaneNavMesh = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface rightLaneNavMesh = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface turningNavMesh = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface combinedNavMesh = gameObject.AddComponent<NavMeshSurface>();

                //turningNavMesh.layerMask = LayerMask.GetMask("turning");
                //turningNavMesh.defaultArea = 5;
                //turningNavMesh.BuildNavMesh();

                // Set properties for the combined NavMesh
                // This used to be all three
                combinedNavMesh.layerMask = LayerMask.GetMask("left", "right");
                combinedNavMesh.BuildNavMesh();

                Debug.Log("BAKING?");
                leftLaneNavMesh.layerMask = LayerMask.GetMask("left", "turning");
                // This SEEMS to be working, although does it even have any affect?
                leftLaneNavMesh.defaultArea = 3;
                leftLaneNavMesh.BuildNavMesh();

                rightLaneNavMesh.layerMask = LayerMask.GetMask("right", "turning");
                rightLaneNavMesh.defaultArea = 4;
                rightLaneNavMesh.BuildNavMesh();
            }
            // After adding new roads, remove far away roads
            float squaredDistanceThreshold = 10000f; // (100 units)^2
            Vector3 playerPosition = player.transform.position;
            List<Vector3> coordinatesToRemove = new List<Vector3>();
            foreach (var roadCoordinate in activeRoadCoordinates) {
                if (Vector3.SqrMagnitude(roadCoordinate - playerPosition) > squaredDistanceThreshold) {
                    // Remove the road coordinate and destroy it 
                    coordinatesToRemove.Add(roadCoordinate);
                    Destroy(activeRoadPrefabs[roadCoordinate]);
                }
            }
            foreach(var coordinate in coordinatesToRemove) {
                activeRoadCoordinates.Remove(coordinate);
                activeRoadPrefabs.Remove(coordinate);
            }
        }
    }

    private void UpdateNearbyRoadCoordinates() {
        nearbyRoadCoordinates.Clear();

        // Calculate the squared distance 
        float squaredDistanceThreshold = 5625f; // (75 units distance)^2
        Vector3 playerPosition = player.transform.position;

        foreach (var roadCoordinate in roadInformation.Keys) {
            // Add the road coordinate if it is nearby to the player and hasn't already been added
            if (Vector3.SqrMagnitude(roadCoordinate - playerPosition) < squaredDistanceThreshold && !nearbyRoadCoordinates.Contains(roadCoordinate)) {
                nearbyRoadCoordinates.Add(roadCoordinate);
            }
        }
    }
}
