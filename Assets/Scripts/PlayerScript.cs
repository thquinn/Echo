using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlayerScript : MonoBehaviour
{
    static float MOVE_SPEED = 10;
    static float MOVE_INERTIA = .1f;
    static float STOP_INERTIA = .5f;
    static float WALLJUMP_INERTIA = .02f;
    static float LOOK_SENSITIVITY = .4f;
    static float JUMP_SPEED = 6.5f;
    static float COYOTE_TIME = .33f;
    static float JUMP_DELAY = .2f;
    static float WALLRUN_MIN_SPEED = 5f;
    static float WALLRUN_ACCELERATION = 10f;
    static float WALLRUN_INITIAL_VELOCITY = 2.5f;
    static float WALLRUN_INITIAL_FORCE = .2f;
    static float WALLRUN_FORCE_EXPONENT = 1.5f;
    static float WALLRUN_DURATION = 1.5f;
    static float WALLJUMP_COOLDOWN_TIMER = 1f;
    static float WALLJUMP_HORIZONTAL_SPEED = 5;
    static float WALLJUMP_VERTICAL_SPEED = 4;
    public static float WALLRUN_CHECK_DISTANCE = .85f;
    // Non-gameplay variables.
    static float PING_PITCH_DROP_FACTOR = .02f;
    static float FOOTSTEP_RATE_MULTIPLIER = 3f;

    public Material sonarMaterial;
    public Rigidbody rb;
    public Camera cam;
    public AudioSource sfxPing, sfxTwinkle, sfxSlide;
    public AudioSource[] sfxSteps, sfxSoftSteps, sfxJumps, sfxLandings;
    public AudioMixer mixerPing, mixerAmbience;

    public GameManagerScript gameManager;
    public UIScript uiScript;
    LevelScript levelScript;
    [HideInInspector] public bool hasPulse;
    bool isGrounded, jumped, walljumped, isWallrunning;
    float groundTimer, wallrunTimer, walljumpCooldown, walljumpCoyoteTime;
    Vector3 lastWallrunNormal;
    float pingPitch;
    float stepTimer = .5f;
    int sfxStepLast, sfxSoftStepLast;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void SetLevel(LevelScript levelScript) {
        this.levelScript = levelScript;
        rb.velocity = Vector3.zero;
    }

    void Update() {
        if (levelScript.started) {
            UpdateLook();
            UpdateControls();
        }
        UpdateSonar();
        UpdateSFX();
        if (Application.isEditor) {
            if (Input.GetKeyDown(KeyCode.F1)) {
                transform.position = Vector3.zero;
            }
        }
    }
    private void FixedUpdate() {
        if (levelScript.started) {
            UpdateVelocity();
        }
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
        SconceScript lookedSconce = Util.GetLookedSconce(cam);
        if (lookedSconce != null) {
            lookedSconce.Look();
            if (Input.GetMouseButtonDown(0)) {
                gameManager.ClickedSconce(lookedSconce.index);
            }
        }
    }
    void UpdateControls() {
        isGrounded = Util.IsOnGround(gameObject, 8, .5f, .1f);
        if (groundTimer < 0) {
            groundTimer = Mathf.Min(0, groundTimer + Time.deltaTime);
        } else if (isGrounded) {
            groundTimer = COYOTE_TIME;
            wallrunTimer = 0;
            walljumpCooldown = 0;
        } else {
            groundTimer = Mathf.Max(0, groundTimer - Time.deltaTime);
        }
        bool jumpButton = Input.GetButtonDown("Jump");
        if (groundTimer > 0 && !jumped && jumpButton) {
            jumped = true;
            groundTimer = -JUMP_DELAY;
            sfxJumps[Random.Range(0, sfxJumps.Length)].Play();
        }
        walljumpCooldown = Mathf.Max(0, walljumpCooldown - Time.fixedDeltaTime);
        if (!isGrounded && walljumpCooldown <= 0 && walljumpCoyoteTime > 0 && jumpButton) {
            walljumped = true;
            walljumpCooldown = WALLJUMP_COOLDOWN_TIMER;
            walljumpCoyoteTime = 0;
        }
    }
    void UpdateVelocity() {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.sqrMagnitude > 1) {
            input.Normalize();
        }
        Vector3 inputVelocity = transform.forward * input.y * MOVE_SPEED + transform.right * input.x * MOVE_SPEED;
        Vector3 moveVelocity;
        if (walljumpCooldown > 0) {
            moveVelocity = Vector3.Lerp(rb.velocity, inputVelocity, WALLJUMP_INERTIA);
        } else {
            moveVelocity = new Vector3(
                Mathf.Lerp(rb.velocity.x, inputVelocity.x, inputVelocity.x == 0 ? STOP_INERTIA : MOVE_INERTIA),
                0,
                Mathf.Lerp(rb.velocity.z, inputVelocity.z, inputVelocity.z == 0 ? STOP_INERTIA : MOVE_INERTIA)
            );
        }
        float moveMagnitude = moveVelocity.magnitude;
        Vector3 wallrunNormal = Util.GetWallrunNormal(gameObject, inputVelocity, .5f);
        isWallrunning = groundTimer <= 0 && walljumpCooldown <= 0 && input.x != 0 && moveMagnitude > WALLRUN_MIN_SPEED && wallrunNormal != Vector3.zero && Mathf.Abs(Vector3.Dot(wallrunNormal, transform.forward)) < .66f;
        float yVelocity = rb.velocity.y;
        if (isWallrunning) {
            lastWallrunNormal = wallrunNormal;
            walljumpCoyoteTime = COYOTE_TIME;
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
            moveVelocity = moveVelocity.normalized * Mathf.Min(MOVE_SPEED, moveMagnitude + Time.fixedDeltaTime * WALLRUN_ACCELERATION);
            wallrunTimer += Time.fixedDeltaTime;
            sfxSlide.transform.localPosition = moveVelocity.normalized;
        } else if (jumped) {
            yVelocity = JUMP_SPEED;
        } else if (walljumped) {
            sfxJumps[Random.Range(0, sfxJumps.Length)].Play();
            moveVelocity += lastWallrunNormal * WALLJUMP_HORIZONTAL_SPEED;
            yVelocity = WALLJUMP_VERTICAL_SPEED;
            wallrunTimer = 0;
            walljumped = false;
        }
        jumped = false;
        rb.velocity = new Vector3(moveVelocity.x, yVelocity, moveVelocity.z);
    }

    void UpdateSonar() {
        Vector3 pingLocation = sonarMaterial.GetVector("_PingLocation");
        float pingDistance = sonarMaterial.GetFloat("_PingDistance");
        float proximity = Mathf.Abs((cam.transform.position - pingLocation).magnitude - pingDistance);
        pingPitch -= Mathf.Sqrt(proximity * .1f) * PING_PITCH_DROP_FACTOR * Time.deltaTime;
        mixerPing.SetFloat("Pitch", pingPitch);
        float fadeFactor = Mathf.InverseLerp(LevelScript.PING_PERIOD - 2, LevelScript.PING_PERIOD, levelScript.pingTimer);
        float pingVolume = Mathf.Pow(proximity, 1.25f) * -.5f + fadeFactor * -35;
        pingVolume -= 40 * uiScript.GetFade();
        mixerPing.SetFloat("Volume", pingVolume);
        float twinkleVolume = Mathf.Lerp(0f, .05f, Mathf.InverseLerp(0, -40, pingVolume));
        sfxTwinkle.volume = Util.Damp(sfxTwinkle.volume, twinkleVolume, sfxTwinkle.volume < twinkleVolume ? .75f : .001f, Time.deltaTime);
        float proximityAndTime = levelScript.time * (1 / (proximity + 1));
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
    public void PingStart() {
        pingPitch = 1;
        sfxPing.Play();
    }

    private void OnCollisionEnter(Collision collision) {
        if (collision.GetContact(0).normal.y > .9f) {
            AudioSource sfx = sfxLandings[Random.Range(0, sfxLandings.Length)];
            float volume = Mathf.Lerp(0, 1.2f, Mathf.Abs(collision.relativeVelocity.y) * .1f);
            sfx.PlayOneShot(sfx.clip, volume);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Goal") {
            levelScript.Collect(other.gameObject);
        }
    }
}
