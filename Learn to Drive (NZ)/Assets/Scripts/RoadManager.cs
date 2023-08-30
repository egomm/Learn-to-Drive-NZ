using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class RoadManager : MonoBehaviour {
    // Class for managing the road information
    public class RoadInformation {
        public GameObject GameObject;
        public Quaternion Quaternion;

        // Manage the position and the rotation 
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

    // Reference to the NPC cars
    public GameObject blueCarNPC;
    public GameObject blackCarNPC;

    // Previous player position
    private Vector3 lastPlayerPosition;

    // Last instantiated 
    public static Vector3 lastInstantiated = new Vector3(0, 0, 0);
    // Last instantiated road
    public static GameObject lastInstantiatedRoad = null;

    // For the NPC cars
    public static Vector3 navMeshStart = Vector3.zero;
    public static Vector3 navMeshEnd = Vector3.zero;
    private int maxCarCount = 8;

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

    // Road frequency
    private float roundaboutFrequency = 0.1f;
    private float curveFrequency = 0.05f;
    private float intersectionFrequency = 0.15f;

    // Start is called before the first frame update
    void Start() {
        // Clear all static variables
        roadInformation.Clear();
        activeRoadPrefabs.Clear();
        navMeshStart = Vector3.zero;
        navMeshEnd = Vector3.zero;
        lastInstantiated = new Vector3(0, 0, 0);
        lastInstantiatedRoad = null;

        // Generate 2500 roads (this is around 50km worth and should last the player for at least an hour if they somehow manage to last)
        int roundCount = 2500;
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
        // Spawn four roads before the first road
        for (int i = 0; i < 4; i++) {
            Vector3 tempRoadCoordinates = new Vector3(0, 0, (i - 4) * 13.99f);
            roadInformation.Add(tempRoadCoordinates, new RoadInformation(straightRoad, Quaternion.Euler(new Vector3(0, 0, 0))));
            UpdateNearbyRoadCoordinates();
        }
        for (int i = 0; i < roundCount; i++) {
            float randomRoadGeneration = Random.Range(0f, 1f);
            GameObject roadType;
            int alternativeAngle = -1;
            /* Although the coordinates seem random in here, the positions of the roads have been determined through excessive
             * trialling and testing. These coordinates are determined by the angle of the previous road, what the previous
             * road was, and what the current road is. It also accounts for roads like curves that will change the 
             * current angle by 90 or 180 degrees dependent on the situation.
            */
            if (i > 0) {
                // There can only be a a junction every 4 roads 
                if (randomRoadGeneration < roundaboutFrequency && (i - previousRoundabout) > 3 && (i - previousIntersection) > 3 && (i - previousCurve) > 1) {
                    // Roundabout
                    previousRoundabout = i;
                    float twoRoundaboutFrequency = 0.15f;
                    float threeRoundaboutFrequency = 0.40f;
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
                    // Curve
                    // If the current angle is 0 degrees, decrease it by 90 to 270 degrees
                    previousCurve = i;
                    // Manage if the angle had been changed before (usually only for the first curve)
                    if (changedAngle) {
                        currentAngle = (currentAngle <= 90) ? 270 : currentAngle - 180;
                    } else {
                        if (currentAngle == 270) {
                            currentAngle = 90;
                        } else {
                            currentAngle = 270;
                        }
                        changedAngle = true;
                    }
                    roadType = curvedRoad;
                    // This needs to depend on the curve
                    if (previousRoad == "straight") {
                        if (currentAngle == 270) {
                            roadCoordinates = previousRoadCoordinates + new Vector3(-5.7f, 0, 31.1f);
                        } else {
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
                        // Roundabout
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
                                roadType = threeIntersectionLeft;
                                roadCoordinates = previousRoadCoordinates + new Vector3(0, 0, 13.999f);
                            } else if (currentAngle == 270) {
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
                // Add the road into the list of road information
                roadInformation.Add(roadCoordinates, new RoadInformation(roadType, Quaternion.Euler(new Vector3(0, angle, 0))));
                previousRoadCoordinates = roadCoordinates;
            } else {
                i--;
            }
        }

        UpdateNearbyRoadCoordinates();
    }

    // Recursive function for finding a child of a parent with a given tag
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

    // Function for getting addon coordinates for a road to ensure that cars are spawned on the correct part of the road
    Vector3 GetAddon(float angle) {
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
                // Remove existing NavMeshSurface components to avoid baking the same navmesh multiple times
                NavMeshSurface[] existingNavMeshSurfaces = gameObject.GetComponents<NavMeshSurface>();
                foreach (NavMeshSurface existingSurface in existingNavMeshSurfaces) {
                    Destroy(existingSurface);
                }

                // Add NavMeshSurface components for each NavMesh
                NavMeshSurface leftLaneNavMesh = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface rightLaneNavMesh = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface combinedNavMesh = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface onlyLeft = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface onlyRight = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface carRight = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface carLeft = gameObject.AddComponent<NavMeshSurface>();
                NavMeshSurface combinedAll = gameObject.AddComponent<NavMeshSurface>();

                // Set properties for the left lane navmesh
                leftLaneNavMesh.layerMask = LayerMask.GetMask("left", "turning", "leftNPC");
                leftLaneNavMesh.defaultArea = 3;
                leftLaneNavMesh.BuildNavMesh();

                // Set properties for the right lane navmesh
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

                // Set properties for the car right navmesh
                carRight.layerMask = LayerMask.GetMask("right");
                carRight.defaultArea = 8; 
                carRight.BuildNavMesh();

                // Set properties for the car left navmesh
                carLeft.layerMask = LayerMask.GetMask("left");
                carLeft.defaultArea = 9;
                carLeft.BuildNavMesh();

                // Set properties for the combined all navmesh
                combinedAll.layerMask = LayerMask.GetMask("left", "turning", "leftNPC", "right", "rightNPC");
                combinedAll.defaultArea = 10;
                combinedAll.BuildNavMesh();

                // Spawn cars on the spawn left tag
                Transform leftChildWithTargetTag = FindChildWithTagRecursively(roadInstance.transform, "SpawnLeft");
                if (leftChildWithTargetTag != null) {
                    // Spawn car here
                    int angle = (int) leftChildWithTargetTag.eulerAngles.y;
                    Vector3 addon = Vector3.zero;
                    float carTypeRandom = Random.Range(0f, 1f);
                    GameObject leftCarPrefab;
                    if (carTypeRandom <= 0.5f) {
                        leftCarPrefab = Instantiate(blueCarNPC, leftChildWithTargetTag.position + GetAddon(angle), leftChildWithTargetTag.rotation);
                    } else {
                        leftCarPrefab = Instantiate(blackCarNPC, leftChildWithTargetTag.position + GetAddon(angle), leftChildWithTargetTag.rotation);
                    }
                    leftCarPrefab.tag = "LeftCar";
                }

                // Spawn cars on the spawn right tag
                Transform rightChildWithTargetTag = FindChildWithTagRecursively(roadInstance.transform, "SpawnRight");
                if (rightChildWithTargetTag != null) {
                    // Spawn car here
                    int angle = (int) rightChildWithTargetTag.eulerAngles.y;
                    // Determine what colour of cat to spawn
                    float carTypeRandom = Random.Range(0f, 1f);
                    GameObject rightCarPrefab;
                    if (carTypeRandom <= 0.5f) {
                        rightCarPrefab = Instantiate(blueCarNPC, rightChildWithTargetTag.position + GetAddon(angle), rightChildWithTargetTag.rotation);
                    } else {
                        rightCarPrefab = Instantiate(blackCarNPC, rightChildWithTargetTag.position + GetAddon(angle), rightChildWithTargetTag.rotation);
                    }
                    rightCarPrefab.tag = "RightCar";
                }

                // Find the most recently placed right lane and spawn a car (only on a straight?)
                Transform leftSpawnTransform = null;
                Transform rightSpawnTransform = null;
                foreach (Transform child in roadInstance.transform) {
                    int childLayer = child.gameObject.layer;
                    // Check if this child is a left lane or a right lane
                    if (childLayer == 6) {
                         leftSpawnTransform = child; 
                    } else if (childLayer == 7) {
                        rightSpawnTransform = child;
                    }
                }

                float spawnCarRandom = Random.Range(0f, 1f);
                if (spawnCarRandom <= 0.5f) {
                    if (leftSpawnTransform != null) {
                        Quaternion carRotation = Quaternion.Euler(0, leftSpawnTransform.rotation.y, 0);
                        float carTypeRandom = Random.Range(0f, 1f);
                        GameObject leftCarPrefab;
                        if (carTypeRandom <= 0.5f) {
                            leftCarPrefab = Instantiate(blueCarNPC, leftSpawnTransform.position + GetAddon(carRotation.eulerAngles.y), carRotation);
                        } else {
                            leftCarPrefab = Instantiate(blackCarNPC, leftSpawnTransform.position + GetAddon(carRotation.eulerAngles.y), carRotation);
                        }
                        leftCarPrefab.tag = "LeftCar";
                    }
                } else { 
                    if (rightSpawnTransform != null) {
                        Quaternion carRotation = Quaternion.Euler(0, rightSpawnTransform.rotation.y - 180, 0);
                        float carTypeRandom = Random.Range(0f, 1f);
                        GameObject rightCarPrefab;
                        if (carTypeRandom <= 0.5f) {
                            rightCarPrefab = Instantiate(blueCarNPC, rightSpawnTransform.position + GetAddon(carRotation.eulerAngles.y), carRotation);
                        } else {
                            rightCarPrefab = Instantiate(blackCarNPC, rightSpawnTransform.position + GetAddon(carRotation.eulerAngles.y), carRotation);
                        }
                        rightCarPrefab.tag = "RightCar";
                    }
                }
            }
            // After adding new roads, remove far away roads
            float squaredDistanceThreshold = 22500f; // (150 units)^2
            Vector3 playerPosition = player.transform.position;
            List<Vector3> coordinatesToRemove = new List<Vector3>();
            foreach (var roadCoordinate in activeRoadCoordinates) {
                // Check if this road is within range
                if (Vector3.SqrMagnitude(roadCoordinate - playerPosition) > squaredDistanceThreshold) {
                    // Remove the road coordinate and destroy the prefab  
                    coordinatesToRemove.Add(roadCoordinate);
                    Destroy(activeRoadPrefabs[roadCoordinate]);
                }
            }
            // Remove each coordinate now (cannot remove an item from a dictionary while iterating over it)
            foreach (var coordinate in coordinatesToRemove) {
                activeRoadCoordinates.Remove(coordinate);
                activeRoadPrefabs.Remove(coordinate);
            }

            // Managed everything, so remove the overload of cars before updating update the navmesh start/end positions
            GameObject[] cars = GameObject.FindGameObjectsWithTag("LeftCar");

            // Check if the number of cars exceeds the maximum allowed count
            if (cars.Length > maxCarCount) {
                // Calculate distances of each car to Vector3.zero
                float[] distancesToZero = new float[cars.Length];
                for (int i = 0; i < cars.Length; i++) {
                    distancesToZero[i] = Vector3.Distance(cars[i].transform.position, Vector3.zero);
                }

                // Sort cars based on distances
                System.Array.Sort(distancesToZero, cars);

                // Remove overflow cars beyond 8 cars
                for (int i = maxCarCount; i < cars.Length; i++) {
                    Destroy(cars[i]);
                }
            }
            navMeshStart = FindStartOfNavMesh();
            navMeshEnd = FindEndOfNavMesh();
        }
    }

    // Function for finding the start of navmesh based on the active roads
    Vector3 FindStartOfNavMesh() {
        float closestDistance = float.MaxValue;
        Vector3 closestPoint = Vector3.zero;

        // Iterate over each of the active roads
        foreach (var key in activeRoadPrefabs.Keys) {
            Vector3 startOfRoad = FindStartOfNavMeshOfRoad(activeRoadPrefabs[key]);
            float distance = Vector3.Distance(Vector3.zero, startOfRoad);
            // Find the closest road to the origin with navmesh
            if (distance < closestDistance) {
                closestDistance = distance;
                closestPoint = startOfRoad;
            }
        }
        return closestPoint;
    }

    // Function for finding the start of navmesh of a road
    Vector3 FindStartOfNavMeshOfRoad(GameObject roadObject) {
        NavMeshHit hit;
        Vector3 closestPoint = Vector3.zero;
        float closestDistance = float.MaxValue;

        // Ensure the provided road object is not null
        if (roadObject != null) {
            // Iterate over the children in the object to find the closest point to the origin
            foreach (Transform child in roadObject.transform) {
                Vector3 childPosition = child.position;
                // Bitwise shift left operator used to make sure the navmesh layer matches correctly (id: 8)
                if (NavMesh.SamplePosition(childPosition, out hit, float.MaxValue, 1 << 8)) {
                    float distance = Vector3.Distance(Vector3.zero, hit.position);
                    // Find the closest point to the origin
                    if (distance < closestDistance) {
                        closestDistance = distance;
                        closestPoint = hit.position;
                    }
                }

                // Recurse into the children's children
                if (child.childCount > 0) {
                    Vector3 grandchildClosestPoint = FindStartOfNavMeshOfRoad(child.gameObject);
                    float grandchildDistance = Vector3.Distance(Vector3.zero, grandchildClosestPoint);
                    // Find the closest point to the origin
                    if (grandchildDistance < closestDistance) {
                        closestDistance = grandchildDistance;
                        closestPoint = grandchildClosestPoint;
                    }
                }
            }
        }
        return closestPoint;
    }

    // Function for finding the end of the navmesh on the active roads
    Vector3 FindEndOfNavMesh() {
        var keys = activeRoadPrefabs.Keys;
        float furthestDistance = 0f;
        Vector3 furthestPoint = Vector3.zero;
        // Iterate over all of the active roads 
        foreach (var key in keys) {
            Vector3 endOfRoad = FindEndOfNavMeshOfRoad(activeRoadPrefabs[key]);
            float distance = Vector3.Distance(Vector3.zero, endOfRoad);
            // Check if this is further than the current furthest
            if (distance > furthestDistance) {
                furthestDistance = distance;
                furthestPoint = endOfRoad;
            }
        }
        return furthestPoint;
    }

    // Function for finding the end of the navmesh of a specific road 
    public static Vector3 FindEndOfNavMeshOfRoad(GameObject roadObject) {
        NavMeshHit hit;
        Vector3 furthestPoint = Vector3.zero;
        Vector3 origin = Vector3.zero;
        float furthestDistance = 0f;

        // Ensure the provided road object isn't null
        if (roadObject != null) {
            foreach (Transform child in roadObject.transform) {
                Vector3 childPosition = child.position;
                // Bitwise shift left operator used to make sure the navmesh layer matches correctly (id: 9)
                if (NavMesh.SamplePosition(childPosition, out hit, float.MaxValue, 1 << 9)) {
                    float distance = Vector3.Distance(origin, hit.position);
                    // Check if this point is further than the current furthest point
                    if (distance > furthestDistance) {
                        furthestDistance = distance;
                        furthestPoint = hit.position;
                    }
                }

                // Recurse into children
                if (child.childCount > 0) {
                    Vector3 grandchildFurthestPoint = FindEndOfNavMeshOfRoad(child.gameObject);
                    float grandchildDistance = Vector3.Distance(origin, grandchildFurthestPoint);
                    // Check if this point is further than the current furthest point
                    if (grandchildDistance > furthestDistance) {
                        furthestDistance = grandchildDistance;
                        furthestPoint = grandchildFurthestPoint;
                    }
                }
            }
        }

        return furthestPoint;
    }

    // Function for updating the road coordinates
    private void UpdateNearbyRoadCoordinates() {
        // Clear the nearby road coordinates, then update them
        nearbyRoadCoordinates.Clear();

        // Calculate the squared distance 
        float squaredDistanceThreshold = 15625f; // (125 units distance)^2
        Vector3 playerPosition = player.transform.position;

        foreach (var roadCoordinate in roadInformation.Keys) {
            // Add the road coordinate if it is nearby to the player and hasn't already been added
            if (Vector3.SqrMagnitude(roadCoordinate - playerPosition) < squaredDistanceThreshold && !nearbyRoadCoordinates.Contains(roadCoordinate)) {
                nearbyRoadCoordinates.Add(roadCoordinate);
            }
        }
    }
}
