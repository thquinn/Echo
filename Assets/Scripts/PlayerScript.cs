using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerScript : MonoBehaviour
{
    static float PING_PERIOD = 10;
    static float PING_SPEED = 14f;
    static float PING_EXPONENT = .85f;
    static float MOVE_SPEED = 10;
    static float LOOK_SENSITIVITY = .4f;
    static float JUMP_SPEED = 5;
    static float COYOTE_TIME = .33f;
    static float JUMP_DELAY = .2f;
    public static float WALLRUN_CHECK_DISTANCE = .85f;
    public static float WALLRUN_INITIAL_VELOCITY = 2.5f;
    public static float WALLRUN_INITIAL_FORCE = .2f;
    public static float WALLRUN_FORCE_EXPONENT = 1.5f;
    public static float WALLRUN_DURATION = 1.5f;
    // Non-gameplay variables.
    static float PING_PITCH_DROP_FACTOR = .02f;

    public Rigidbody rb;
    public Camera cam;
    public Material sonarMaterial;
    public AudioSource sfxPing, sfxSlide;
    public AudioMixer mixerPing;

    bool jumped, isWallrunning;
    float groundTimer, wallrunTimer;
    Vector3 pingLocation;
    float pingTimer, pingPitch;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        sonarMaterial.SetFloat("_MaxPingAge", PING_PERIOD);
    }

    void Update() {
        UpdateLook();
        UpdateControls();
        UpdateSonar();
        UpdateSFX();
        if (Application.isEditor) {
            if (Input.GetKeyDown(KeyCode.F1)) {
                transform.position = Vector3.zero;
            }
        }
    }
    private void FixedUpdate() {
        UpdateVelocity();
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
    void UpdateControls() {
        if (groundTimer < 0) {
            groundTimer = Mathf.Min(0, groundTimer + Time.deltaTime);
        } else if (Util.IsOnGround(gameObject, 8, .5f, .1f)) {
            groundTimer = COYOTE_TIME;
            wallrunTimer = 0;
        } else {
            groundTimer = Mathf.Max(0, groundTimer - Time.deltaTime);
        }
        if (groundTimer > 0 && !jumped && Input.GetButtonDown("Jump")) {
            jumped = true;
            groundTimer = -JUMP_DELAY;
        }
    }
    void UpdateVelocity() {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector3 moveVelocity = transform.forward * y * MOVE_SPEED + transform.right * x * MOVE_SPEED;
        float moveMagnitude = moveVelocity.magnitude;
        Vector3 wallrunNormal = Util.GetWallrunNormal(gameObject, moveVelocity, .5f);
        isWallrunning = groundTimer <= 0 && moveVelocity.sqrMagnitude > 0 && wallrunNormal != Vector3.zero;
        float yVelocity = rb.velocity.y;
        if (isWallrunning) {
            if (wallrunTimer == 0) {
                yVelocity = Mathf.Max(yVelocity, WALLRUN_INITIAL_VELOCITY);
            } else if (wallrunTimer < WALLRUN_DURATION) {
                float wallrunPercent = wallrunTimer / WALLRUN_DURATION;
                wallrunPercent = Mathf.Pow(wallrunPercent, WALLRUN_FORCE_EXPONENT);
                float wallrunForce = WALLRUN_INITIAL_FORCE * (1 - wallrunPercent);
                yVelocity += wallrunForce;
            }
            // Remove the component of the movement vector that points into the wall and reset the magnitude.
            moveVelocity -= wallrunNormal * Vector3.Dot(moveVelocity, wallrunNormal);
            moveVelocity = moveVelocity.normalized * moveMagnitude;
            wallrunTimer += Time.fixedDeltaTime;
            sfxSlide.transform.localPosition = moveVelocity.normalized;
        } else if (jumped) {
            yVelocity = JUMP_SPEED;
        }
        jumped = false;
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
            pingPitch = 1;
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
        pingPitch -= Mathf.Sqrt(proximity * .1f) * PING_PITCH_DROP_FACTOR * Time.deltaTime;
        mixerPing.SetFloat("Pitch", pingPitch);
        float fadeFactor = Mathf.InverseLerp(PING_PERIOD - 2, PING_PERIOD, pingTimer);
        mixerPing.SetFloat("Volume", Mathf.Pow(proximity, 1.25f) * -.5f + fadeFactor * -35);
        float proximityAndTime = pingTimer * (1 / (proximity + 1));
        float mixerEffectIntensity = proximityAndTime / (proximityAndTime + 1);
        mixerPing.SetFloat("Flange_Drymix", 1 - mixerEffectIntensity);
        mixerPing.SetFloat("Flange_Wetmix", mixerEffectIntensity);
    }
    void UpdateSFX() {
        sfxSlide.volume = Mathf.Clamp(sfxSlide.volume + (isWallrunning ? Time.deltaTime * 40 : Time.deltaTime * -10), 0, .75f);
    }
}
