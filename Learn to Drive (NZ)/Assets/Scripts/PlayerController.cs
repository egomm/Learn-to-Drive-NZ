using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float rpm;
    [SerializeField] float horsePower = 0;
    [SerializeField] float turnSpeed = 45;
    private float horizontalInput;
    private float forwardInput;
    private Rigidbody playerRb;
    //[SerializeField] GameObject centreOfMass;
    [SerializeField] TextMeshProUGUI speedometerText;
    [SerializeField] TextMeshProUGUI rpmText;
    [SerializeField] List<WheelCollider> allWheels;
    [SerializeField] int wheelsOnGround;

    void Start() {
        playerRb = GetComponent<Rigidbody>();
        //playerRb.centerOfMass = centreOfMass.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate() {
        horizontalInput = Input.GetAxis("Horizontal");
        forwardInput = Input.GetAxis("Vertical");

        // We'll move the vehicle forward
        // transform.Translate(Vector3.forward * Time.deltaTime * speed * forwardInput);
        // transform.Rotate(Vector3.up, Time.deltaTime * turnSpeed * horizontalInput);
        if (isOnGround()) {
            playerRb.AddRelativeForce(Vector3.forward * horsePower * forwardInput);
            transform.Rotate(Vector3.up, Time.deltaTime * turnSpeed * horizontalInput);

            speed = Mathf.RoundToInt(playerRb.velocity.magnitude * 3.6f);
            // Set a maximum speed 
            if (playerRb.velocity.magnitude > 30) {
                GetComponent<Rigidbody>().velocity = Vector3.ClampMagnitude(GetComponent<Rigidbody>().velocity, 20);
            }
            speedometerText.SetText("Speed: " + speed + " kmph");

            //rpm = Mathf.RoundToInt((speed % 30) * 40);
            rpm = Mathf.RoundToInt((60 * speed) / (2 * Mathf.PI));
            rpmText.SetText("RPM: " + rpm);
        }
    }

    bool isOnGround() {
        wheelsOnGround = 0;
        foreach (WheelCollider wheel in allWheels) {
            if (wheel.isGrounded) {
                wheelsOnGround++;
            }
        }
        Debug.Log(wheelsOnGround);
        return true;
        //return wheelsOnGround == 4;
    }
}
