using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoveCar : MonoBehaviour {
    public Rigidbody rb;
    public Transform car;
    public float speed;
    public float turnSpeed;
    [SerializeField] TextMeshProUGUI speedometerText;

    Vector3 rotationRight = new Vector3(0, 30, 0);
    Vector3 rotationLeft = new Vector3(0, -30, 0);

    Vector3 forward = new Vector3(0, 0, 1);
    Vector3 backward = new Vector3(0, 0, -1);

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        // Temporary until a better system is devised
        //GetComponent<Rigidbody>().AddForce(Vector3.down * 10E9f);
        float currentSpeed = 0;
        if (Input.GetKey("w")) {
            transform.Translate(forward * speed * Time.deltaTime);
            currentSpeed = speed;
        }
        if (Input.GetKey("s")) {
            transform.Translate(backward * speed * Time.deltaTime);
            currentSpeed = speed;
        }
        if (Input.GetKey("d")) {
            Quaternion deltaRotationRight = Quaternion.Euler(rotationRight * turnSpeed * Time.deltaTime);
            rb.MoveRotation(rb.rotation * deltaRotationRight);
        }

        if (Input.GetKey("a")) {
            Quaternion deltaRotationLeft = Quaternion.Euler(rotationLeft * turnSpeed * Time.deltaTime);
            rb.MoveRotation(rb.rotation * deltaRotationLeft);
        }
        speedometerText.SetText("Speed: " + currentSpeed * 3.6f + " kmph");
    }
}
