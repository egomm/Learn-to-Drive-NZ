using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject player;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, 0);

    // Update is called once per frame
    void LateUpdate() {
        transform.position = player.transform.position + offset;
        //transform.eulerAngles = new Vector3(90, 0, player.transform.eulerAngles.y);
        transform.eulerAngles = new Vector3(90, 0, -player.transform.eulerAngles.y);
    }
}
