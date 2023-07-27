using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement :MonoBehaviour {

    public float speed = 6;
    public float smoothMoveTime = .1f;
    public float turnSpeed = 8;

    float angle;
    float smoothInputMagnitude;
    float smoothMoveVelocity;
    Vector3 velocity;

    Rigidbody rigidbody;
    bool disabled;

    public static event System.Action OnPlayerWin;

    // Start is called before the first frame update
    void Start () {
        rigidbody = GetComponent<Rigidbody>();
        Guard.OnPlayerSpotted += Disable;
    }

    // Update is called once per frame
    void Update () {
        Vector3 inputDir = Vector3.zero;

        if (!disabled) {
            inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        }

        float inputMagnitude = inputDir.magnitude;
        smoothInputMagnitude = Mathf.SmoothDamp(smoothInputMagnitude, inputMagnitude, ref smoothMoveVelocity, smoothMoveTime);

        // Movement
        //transform.Translate(transform.forward * speed * Time.deltaTime * smoothInputMagnitude, Space.World);
        velocity = transform.forward * speed * smoothInputMagnitude;

        //Rotation
        float playerAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
        angle = Mathf.LerpAngle(angle, playerAngle, Time.deltaTime * turnSpeed * inputMagnitude);
        //transform.eulerAngles = Vector3.up * angle;
    }
    void FixedUpdate () {
        rigidbody.MoveRotation(Quaternion.Euler(Vector3.up * angle));
        rigidbody.MovePosition(rigidbody.position + velocity * Time.deltaTime);
    }

    private void Disable () {
        disabled = true;
    }

    private void OnDestroy () {
        Guard.OnPlayerSpotted -= Disable;
    }

    private void OnTriggerEnter (Collider hitCollider) {
        if (hitCollider.tag == "Finish") {
            Disable();
            if (OnPlayerWin != null) {
                OnPlayerWin();
            }
        }
    }
}
