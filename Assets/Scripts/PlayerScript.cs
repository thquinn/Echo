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
    static float WALLJUMP_HORIZONTAL_SPEED = 4;
    static float WALLJUMP_VERTICAL_SPEED = 10;
    public static float WALLRUN_CHECK_DISTANCE = .85f;
    static float WALLRUN_INITIAL_VELOCITY = 2.5f;
    static float WALLRUN_INITIAL_FORCE = .2f;
    static float WALLRUN_FORCE_EXPONENT = 1.5f;
    static float WALLRUN_DURATION = 1.5f;
    static float WALLJUMP_COOLDOWN_TIMER = 1f;
    // Non-gameplay variables.
    static float PING_PITCH_DROP_FACTOR = .02f;
    static float FOOTSTEP_RATE_MULTIPLIER = 3f;

    public Rigidbody rb;
    public Camera cam;
    public Material sonarMaterial, goalMaterial;
    public AudioSource sfxPing, sfxSlide;
    public AudioSource[] sfxSteps, sfxSoftSteps, sfxJumps, sfxLandings;
    public AudioMixer mixerPing, mixerAmbience;

    bool jumped, isWallrunning;
    float groundTimer, wallrunTimer, walljumpTimer;
    Vector3 pingLocation;
    float pingTimer, pingPitch;
    float stepTimer = .5f;
    int sfxStepLast, sfxSoftStepLast;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        sonarMaterial.SetFloat("_MaxPingAge", PING_PERIOD);
        goalMaterial.SetFloat("_MaxPingAge", PING_PERIOD);
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
            sfxJumps[Random.Range(0, sfxJumps.Length)].Play();
        }
    }
    void UpdateVelocity() {
        float rawX = Input.GetAxisRaw("Horizontal");
        float rawY = Input.GetAxisRaw("Vertical");
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector3 inputVelocity = transform.forward * rawY * MOVE_SPEED + transform.right * rawX * MOVE_SPEED;
        Vector3 moveVelocity = transform.forward * y * MOVE_SPEED + transform.right * x * MOVE_SPEED;
        float moveMagnitude = moveVelocity.magnitude;
        Vector3 wallrunNormal = Util.GetWallrunNormal(gameObject, inputVelocity, .5f);
        isWallrunning = groundTimer <= 0 && walljumpTimer <= 0 && x != 0 && wallrunNormal != Vector3.zero;
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
            // Wall jumps.
            if (Input.GetButtonDown("Jump")) {
                moveVelocity = wallrunNormal * WALLJUMP_HORIZONTAL_SPEED;
                yVelocity = WALLJUMP_VERTICAL_SPEED;
                walljumpTimer = WALLJUMP_COOLDOWN_TIMER;
            }
        } else if (jumped) {
            yVelocity = JUMP_SPEED;
        }
        jumped = false;
        rb.velocity = new Vector3(moveVelocity.x, yVelocity, moveVelocity.z);
        walljumpTimer = Mathf.Max(0, walljumpTimer - Time.fixedDeltaTime);
    }

    void UpdateSonar() {
        float pingDistance = 0;
        if (Time.time == 0 || Time.time % PING_PERIOD < (Time.time - Time.deltaTime) % PING_PERIOD) {
            // New ping.
            sonarMaterial.SetVector("_PingLocation", transform.position);
            sonarMaterial.SetFloat("_PingDistance", 0);
            sonarMaterial.SetFloat("_PingAge", 0);
            goalMaterial.SetVector("_PingLocation", transform.position);
            goalMaterial.SetFloat("_PingDistance", 0);
            goalMaterial.SetFloat("_PingAge", 0);
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
            goalMaterial.SetFloat("_PingDistance", pingDistance);
            goalMaterial.SetFloat("_PingAge", pingTimer);
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
        if (groundTimer == COYOTE_TIME) {
            float velocityPercent = rb.velocity.magnitude / MOVE_SPEED;
            velocityPercent = Mathf.Pow(velocityPercent, .33f);
            stepTimer += Time.deltaTime * velocityPercent * FOOTSTEP_RATE_MULTIPLIER;
            if (stepTimer > 1) {
                if (velocityPercent < .9f) {
                    sfxSoftStepLast = Util.RangeOtherThan(0, sfxSoftSteps.Length, sfxSoftStepLast);
                    sfxSoftSteps[sfxSoftStepLast].Play();
                } else {
                    sfxStepLast = Util.RangeOtherThan(0, sfxSteps.Length, sfxStepLast);
                    sfxSteps[sfxStepLast].Play();
                }
                stepTimer = stepTimer % 1;
            }
        } else {
            stepTimer = .5f;
        }
        sfxSlide.volume = Mathf.Clamp(sfxSlide.volume + (isWallrunning ? Time.deltaTime * 40 : Time.deltaTime * -10), 0, 1);
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.GetContact(0).normal.y > .9f) {
            AudioSource sfx = sfxLandings[Random.Range(0, sfxLandings.Length)];
            float volume = Mathf.Lerp(0, 1.2f, Mathf.Abs(collision.relativeVelocity.y) * .15f);
            sfx.PlayOneShot(sfx.clip, volume);
        }
    }
}
