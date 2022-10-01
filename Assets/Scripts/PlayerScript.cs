using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerScript : MonoBehaviour
{
    static float PING_PERIOD = 10;
    static float PING_SPEED = 13f;
    static float PING_EXPONENT = .9f;
    static float MOVE_SPEED = 10;
    static float LOOK_SENSITIVITY = .4f;
    static float JUMP_SPEED = 5;

    public Rigidbody rb;
    public Camera cam;
    public Material sonarMaterial;
    public AudioSource sfxPing;
    public AudioMixer mixerPing;

    bool jumped;
    Vector3 pingLocation;
    float pingTimer;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        sonarMaterial.SetFloat("_MaxPingAge", PING_PERIOD);
    }

    void Update() {
        UpdateLook();
        UpdateSonar();
        if (!jumped && Input.GetButtonDown("Jump")) {
            jumped = true;
        }
    }
    private void FixedUpdate() {
        UpdateMove();
    }

    void UpdateLook() {
        float x = Input.GetAxis("Mouse X") * LOOK_SENSITIVITY;
        float y = Input.GetAxis("Mouse Y") * LOOK_SENSITIVITY;
        transform.Rotate(0, x, 0);
        float thetaX = cam.transform.localRotation.eulerAngles.x;
        if (thetaX > 180) {
            thetaX -= 360;
        }
        thetaX = Mathf.Clamp(thetaX - y, -90, 90);
        cam.transform.localRotation = Quaternion.Euler(thetaX, 0, 0);
    }
    void UpdateMove() {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector3 moveVelocity = transform.forward * y * MOVE_SPEED + transform.right * x * MOVE_SPEED;
        float yVelocity = rb.velocity.y;
        if (jumped) {
            yVelocity = JUMP_SPEED;
            jumped = false;
        }
        rb.velocity = new Vector3(moveVelocity.x, yVelocity, moveVelocity.z);
    }

    void UpdateSonar() {
        float pingDistance = 0;
        if (Time.time == 0 || Time.time % PING_PERIOD < (Time.time - Time.deltaTime) % PING_PERIOD) {
            // New ping.
            sonarMaterial.SetVector("_PingLocation", transform.position);
            sonarMaterial.SetFloat("_PingDistance", 0);
            sonarMaterial.SetFloat("_PingAge", 0);
            pingTimer = 0;
            pingLocation = transform.position;
            // Play SFX.
            sfxPing.Play();
        } else {
            pingTimer += Time.deltaTime;
            pingDistance = Mathf.Pow(pingTimer, PING_EXPONENT) * PING_SPEED;
            sonarMaterial.SetFloat("_PingDistance", pingDistance);
            sonarMaterial.SetFloat("_PingAge", pingTimer);
        }
        float proximity = Mathf.Abs((cam.transform.position - pingLocation).magnitude - pingDistance);
        float fadeFactor = Mathf.InverseLerp(PING_PERIOD - 2, PING_PERIOD, pingTimer);
        mixerPing.SetFloat("Volume", Mathf.Pow(proximity, 1.25f) * -.5f + fadeFactor * -20);
        float proximityAndTime = pingTimer * (1 / (proximity + 1));
        float mixerEffectIntensity = proximityAndTime / (proximityAndTime + 1);
        mixerPing.SetFloat("Flange_Drymix", 1 - mixerEffectIntensity);
        mixerPing.SetFloat("Flange_Wetmix", mixerEffectIntensity);
    }
}
