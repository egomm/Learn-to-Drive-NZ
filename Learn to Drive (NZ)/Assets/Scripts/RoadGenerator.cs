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

    // Reference to the NPC car 
    public GameObject carNPC;

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
                    if (randomRoundaboutGeneration < twoRoundaboutFrequency) {
                        roadType = twoRoundabout;
                    } else if (randomRoundaboutGeneration < twoRoundaboutFrequency + threeRoundaboutFrequency) {
                        roadType = threeRoundabout;
                    } else {
                        roadType = fourRoundabout;
                    }
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
                } else if (randomRoadGeneration < curveFrequency + roundaboutFrequency && (i - previousIntersection) > 2) {
                    // Curve: need to make this adjust to the coordinate
                    // STILL NEED TO FIX
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
                    previousIntersection = i;
                    roadCoordinates = previousRoadCoordinates + new Vector3(1, 0, 0);
                    float intersectionType = Random.Range(0f, 1f);
                    if (intersectionType < 0.2f) {
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
                            previousRoad = "fourIntersection";
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
                                alternativeAngle = currentAngle + 90;
                                roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                            }
                        }
                        previousRoad = "threeIntersection";
                    }
                } else {
                    // Straight road
                    roadType = straightRoad;
                    // Check what the previous road was
                    if (previousRoad == "straight" || previousRoad.Contains("Intersection")) {
                        // change to switch case?
                        if (currentAngle == 0 || currentAngle == 180) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                        } else if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-13.999f, 0, 0);
                        } else {
                            roadCoordinates = previousRoadCoordinates + new Vector3(13.999f, 0, 0);
                        }
                        if (lastUsedAlternative) {
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
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-42.2f, 0, 0);
                            lastUsedAlternative = false;
                        } else {
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
            int angle = currentAngle;
            if (alternativeAngle >= 0) {
                angle = alternativeAngle;
                lastUsedAlternative = true;
            } else if (previousRoad != "roundabout") {
                lastUsedAlternative = false;
            }
            if (previousRoad == "roundabout") {
                lastUsedAlternative = false;
            }
            if (previousRoad == "threeIntersection") {
                if (currentAngle == 0 || currentAngle == 180) {
                    currentAngle -= 90;
                    if (currentAngle == -90) {
                        currentAngle = 270;
                    } 
                } else if (currentAngle == 270) {
                    currentAngle = 0;
                }
            }
            if (!roadInformation.ContainsKey(roadCoordinates)) {
                roadInformation.Add(roadCoordinates, new RoadInformation(roadType, Quaternion.Euler(new Vector3(0, angle, 0))));
                previousRoadCoordinates = roadCoordinates;
            } else {
                i--;
            }
        }

        UpdateNearbyRoadCoordinates();
    }

    Transform FindChildWithTagRecursively(Transform parent, string tag) {
        if (parent.CompareTag(tag)) {
            return parent;
        }

        foreach (Transform child in parent) {
            // Recur the method
            Transform result = FindChildWithTagRecursively(child, tag);
            if (result != null) {
                return result;
            }
        }

        return null;
    }

    List<Transform> FindAllBarriersRecursively(Transform parent) {
        List<Transform> barrierList = new List<Transform>();

        foreach (Transform child in parent) {
            if (child.gameObject.layer == 9) {
                barrierList.Add(child);
            }

            List<Transform> childBarriers = FindAllBarriersRecursively(child);
            barrierList.AddRange(childBarriers);
        }

        return barrierList;
    }

    Vector3 getAddon(float angle) {
        if (angle == 270) {
            return new Vector3(0, 0, -1.95f);
        } else if (angle == 0) {
            return new Vector3(-1.95f, 0, 0);
        } else if (angle == 90) {
            return new Vector3(0, 0, 1.95f);
        } else {
            return new Vector3(1.95f, 0, 0);
        }
    }

    void Update() {
        // Check if the player has moved to a new position, then update nearbyRoadCoordinates
        if (Vector3.Distance(player.transform.position, lastPlayerPosition) >= 10f) {
            UpdateNearbyRoadCoordinates();
            lastPlayerPosition = player.transform.position;
        }

        // Instantiate nearby road objects at a controlled frequency
        if (Time.frameCount % 10 == 0) {
            bool addedNewRoad = false;
            GameObject roadInstance = null;
            foreach (var roadCoordinate in nearbyRoadCoordinates) {
                if (!activeRoadCoordinates.Contains(roadCoordinate)) {
                    RoadInformation currentRoadInformation = roadInformation[roadCoordinate];
                    GameObject roadPrefab = currentRoadInformation.GameObject;
                    Quaternion roadRotation = currentRoadInformation.Quaternion;
                    roadInstance = Instantiate(roadPrefab, roadCoordinate, roadRotation);
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
                NavMeshSurface combinedNavMesh = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface onlyLeft = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface onlyRight = gameObject.AddComponent<NavMeshSurface>();

                leftLaneNavMesh.layerMask = LayerMask.GetMask("left", "turning", "leftNPC");
                leftLaneNavMesh.defaultArea = 3;
                leftLaneNavMesh.BuildNavMesh();

                rightLaneNavMesh.layerMask = LayerMask.GetMask("right", "turning", "rightNPC");
                rightLaneNavMesh.defaultArea = 4;
                rightLaneNavMesh.BuildNavMesh();

                // Set properties for the combined NavMesh
                combinedNavMesh.layerMask = LayerMask.GetMask("left", "right", "turning");
                combinedNavMesh.defaultArea = 5;
                combinedNavMesh.BuildNavMesh();

                // Set properties for the only left navmesh 
                onlyLeft.layerMask = LayerMask.GetMask("left", "turning");
                onlyLeft.defaultArea = 6;
                onlyLeft.BuildNavMesh();

                // Set properties for the only right navmesh 
                onlyRight.layerMask = LayerMask.GetMask("right", "turning");
                onlyRight.defaultArea = 7;
                onlyRight.BuildNavMesh();

                // Add cars onto this new road depending on what is 
                Transform leftChildWithTargetTag = FindChildWithTagRecursively(roadInstance.transform, "SpawnLeft");
                Transform closestBarrier = null;
                if (leftChildWithTargetTag != null) {
                    Debug.Log("Found child with target tag: " + leftChildWithTargetTag.position);
                    /*List<Transform> allBarriers = FindAllBarriersRecursively(roadInstance.transform);
                    float closestBarrierDistance = -1;
                    foreach (Transform barrier in allBarriers) {
                        Debug.Log(barrier);
                        Debug.Log(barrier.position);
                        // Do something with each barrier Transform
                        float barrierDistance = Vector3.Distance(barrier.position, leftChildWithTargetTag.position);
                        if (closestBarrierDistance == -1) {
                            closestBarrierDistance = barrierDistance;
                            closestBarrier = barrier;
                        } else if (barrierDistance < closestBarrierDistance) {
                            closestBarrierDistance = barrierDistance;
                            closestBarrier = barrier;
                        }
                    }
                    if (closestBarrier != null) {
                        Debug.Log("CLOSEST: " + closestBarrier.position);
                    }*/
                    // Spawn car(s) here
                    int angle = (int) leftChildWithTargetTag.eulerAngles.y;
                    Vector3 addon = Vector3.zero;
                    Debug.Log(angle);
                    GameObject leftCarPrefab = Instantiate(carNPC, leftChildWithTargetTag.position + getAddon(angle), leftChildWithTargetTag.rotation);
                    leftCarPrefab.tag = "LeftCar";
                    Debug.Log(leftChildWithTargetTag.position);
                } else {
                    Debug.Log("No child with target tag found.");
                }

                Transform rightChildWithTargetTag = FindChildWithTagRecursively(roadInstance.transform, "SpawnRight");
                if (rightChildWithTargetTag != null) {
                    Debug.Log("Found child with target tag: " + rightChildWithTargetTag.position);
                    // Spawn car(s) here
                    // What about barriers?
                    // NEED TO SPAWN A RIGHT CAR!!
                    int angle = (int) rightChildWithTargetTag.eulerAngles.y;
                    GameObject rightCarPrefab = Instantiate(carNPC, rightChildWithTargetTag.position + getAddon(angle), rightChildWithTargetTag.rotation);
                    rightCarPrefab.tag = "RightCar";
                    Debug.Log(rightChildWithTargetTag.position);
                } else {
                    Debug.Log("No child with target tag found.");
                }

                // Find the most recently placed right lane and spawn a car (only on a straight?)
                Transform leftSpawnTransform = null;
                Transform rightSpawnTransform = null;
                foreach (Transform child in roadInstance.transform) {
                    int childLayer = child.gameObject.layer;
                    if (childLayer == 6) {
                         leftSpawnTransform = child; 
                    } else if (childLayer == 7) {
                        rightSpawnTransform = child;
                    }
                }
                // THIS IS TOO MUCH
                float spawnCarRandom = Random.Range(0f, 1f);
                if (spawnCarRandom < 0.25f) {
                    if (leftSpawnTransform != null) {
                        Quaternion carRotation = Quaternion.Euler(0, leftSpawnTransform.rotation.y - 180, 0);
                        GameObject leftCarPrefab = Instantiate(carNPC, leftSpawnTransform.position + getAddon(carRotation.eulerAngles.y), carRotation);
                        leftCarPrefab.tag = "LeftCar";
                    }
                } 
                if (spawnCarRandom >= 0.25f && spawnCarRandom < 0.5f) {
                    if (rightSpawnTransform != null) {
                        Quaternion carRotation = Quaternion.Euler(0, rightSpawnTransform.rotation.y - 180, 0);
                        GameObject rightCarPrefab = Instantiate(carNPC, rightSpawnTransform.position + getAddon(carRotation.eulerAngles.y), carRotation);
                        rightCarPrefab.tag = "RightCar";
                    }
                }
            }
            // After adding new roads, remove far away roads
            float squaredDistanceThreshold = 15625f; // (125 units)^2
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
