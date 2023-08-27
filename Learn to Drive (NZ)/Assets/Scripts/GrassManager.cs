using UnityEngine;
using System.Collections.Generic;

public class GrassManager : MonoBehaviour {
    public GameObject grassPrefab;
    public Transform player;
    private float interval = 10f; // Interval between grass planes
    private int planeCount = 15;  // Initial number of grass planes in each row/column
    private float yOffset = -0.1f; // Offset to place grass slightly below the player

    private Dictionary<int, Dictionary<int, GameObject>> grassPlanesDict = new Dictionary<int, Dictionary<int, GameObject>>();
    private Vector3 lastPlayerPosition;

    private void Start() {
        lastPlayerPosition = player.position;
        UpdateGrassPlanes();
    }

    private void Update() {
        Vector3 playerMovement = player.position - lastPlayerPosition;

        if (playerMovement.magnitude >= interval) {
            UpdateGrassPlanes();
            lastPlayerPosition = player.position;
        }
    }

    void AddGrassPlane(int x, int z, GameObject grassPlane) {
        if (!grassPlanesDict.ContainsKey(x)) {
            grassPlanesDict[x] = new Dictionary<int, GameObject>();
        }

        // Modify this part to properly store the grass plane
        if (!grassPlanesDict[x].ContainsKey(z)) {
            grassPlanesDict[x].Add(z, grassPlane);
        }
    }

    private void SpawnGrassPlaneGrid() {
        int halfPlaneCount = Mathf.FloorToInt(planeCount / 2);

        for (int x = -halfPlaneCount; x <= halfPlaneCount; x++) {
            for (int z = -halfPlaneCount; z <= halfPlaneCount; z++) {
                Vector3 spawnPosition = new Vector3(x * interval, yOffset, z * interval);
                GameObject spawnedGrass = Instantiate(grassPrefab, spawnPosition, Quaternion.identity);
                AddGrassPlane(x, z, spawnedGrass);
            }
        }
    }

    private void UpdateGrassPlanes() {
        int playerXIndex = Mathf.FloorToInt(player.position.x / interval);
        int playerZIndex = Mathf.FloorToInt(player.position.z / interval);
        int halfPlaneCount = Mathf.FloorToInt(planeCount / 2);

        int minXIndex = playerXIndex - halfPlaneCount;
        int maxXIndex = playerXIndex + halfPlaneCount;
        int minZIndex = playerZIndex - halfPlaneCount;
        int maxZIndex = playerZIndex + halfPlaneCount;


        // Create a list of keys to remove
        List<int> keysToRemoveOuter = new List<int>();

        foreach (var xIndex in grassPlanesDict.Keys) {
            List<int> keysToRemoveInner = new List<int>();

            foreach (var zIndex in grassPlanesDict[xIndex].Keys) {
                if (xIndex < minXIndex || xIndex >= maxXIndex || zIndex < minZIndex || zIndex >= maxZIndex) {
                    Destroy(grassPlanesDict[xIndex][zIndex]);
                    keysToRemoveInner.Add(zIndex);
                }
            }

            // Remove grass planes from the inner dictionary
            foreach (var key in keysToRemoveInner) {
                grassPlanesDict[xIndex].Remove(key);
            }

            // Add outer dictionary keys to remove if inner dictionary is empty
            if (grassPlanesDict[xIndex].Count == 0) {
                keysToRemoveOuter.Add(xIndex);
            }
        }

        // Remove empty inner dictionaries
        foreach (var key in keysToRemoveOuter) {
            grassPlanesDict.Remove(key);
        }


        // Add new grass planes within the updated range
        for (int x = minXIndex; x <= maxXIndex; x++) {
            for (int z = minZIndex; z <= maxZIndex; z++) {
                if (!grassPlanesDict.ContainsKey(x) || !grassPlanesDict[x].ContainsKey(z)) {
                    Vector3 spawnPosition = new Vector3(x * interval, yOffset, z * interval);
                    GameObject spawnedGrass = Instantiate(grassPrefab, spawnPosition, Quaternion.identity);
                    AddGrassPlane(x, z, spawnedGrass);
                }
            }
        }
    }
}
