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

    // Last value added to the dictionary
    public Vector3 lastRoad = new Vector3(0, 0, 0);
    // Dictionary for managing all of the roads with a Vector3 and object
    public static Dictionary<Vector3, object> roadInformation = new Dictionary<Vector3, object>();

    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < 60; i++) {
            Instantiate(straightRoad, new Vector3(0, 0, 13.999f*i), Quaternion.identity);
        }
        Vector3 roadCoordinates = new Vector3(0, 0, 0);
        // Not starting at first road
        if (Dictionary.Count > 0) {

        }

        roadInformation.Add()
    }

    // Update is called once per frame
    void Update() {
        Debug.Log(player.transform.position);
    }
}
