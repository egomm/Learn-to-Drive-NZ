using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {
    public GameObject player;
    // Serialised field to manage the offfset from the player
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 0);

    // Update is called once per frame
    void LateUpdate() {
        // Update the position and rotation of the camera based on the player
        transform.position = player.transform.position + offset;
        transform.eulerAngles = new Vector3(90, 0, -player.transform.eulerAngles.y);
    }
}
