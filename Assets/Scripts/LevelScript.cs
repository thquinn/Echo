using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    public static float PING_PERIOD = 10;
    static float PING_SPEED = 14.5f;
    static float PING_EXPONENT = .85f;

    public GameObject prefabGoal;
    public Material sonarMaterial;

    public int index;
    [HideInInspector] public float time;
    [HideInInspector] public bool started, done;

    PlayerScript playerScript;
    List<GameObject> goals;
    public float pingTimer;
    bool firstRun;
    GameObject objectWithPulse;

    void Start() {
        time = -1;
        if (index == GameManagerScript.LEVEL_INDEX_HUB) {
            pingTimer = PING_PERIOD - 2;
        } else {
            pingTimer = PING_PERIOD;
        }
        sonarMaterial.SetFloat("_MaxPingAge", PING_PERIOD);
        sonarMaterial.SetVector("_PingLocation", new Vector3(1000, 1000, 1000));
        sonarMaterial.SetFloat("_PingDistance", float.MaxValue);
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        List<GameObject> playerMarkers = Util.GetDirectChildrenWithTag(transform, "PlayerMarker");
        bool introStart = index == GameManagerScript.LEVEL_INDEX_HUB && !PlayerPrefs.HasKey(GameManagerScript.LEVEL_SAVE_KEY_PREFIX + 0);
        int startingIndex = introStart ? 1 : 0;
        Transform targetPlayerTransform = playerMarkers[startingIndex].transform;
        playerScript.transform.position = targetPlayerTransform.position;
        playerScript.transform.rotation = targetPlayerTransform.rotation;
        Camera.main.transform.rotation = Quaternion.identity;
        GameObject startsWithPulse = Util.GetDirectChildWithTag(transform, "StartsWithPulse");
        if (startsWithPulse == null) {
            objectWithPulse = playerScript.gameObject;
            playerScript.hasPulse = true;
        } else {
            objectWithPulse = startsWithPulse;
            playerScript.hasPulse = false;
        }
        List<GameObject> goalMarkers = Util.GetDirectChildrenWithTag(transform, "GoalMarker");
        goals = new List<GameObject>();
        foreach (GameObject goalMarker in goalMarkers) {
            GameObject goal = Instantiate(prefabGoal, goalMarker.transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity, transform);
            if (goalMarkers.Count == 1) {
                goal.GetComponent<GoalScript>().hideWhenFar = true;
            }
            goals.Add(goal);
        }
        GameObject.FindGameObjectWithTag("Floaters").GetComponent<FloatersScript>().Init();
        firstRun = !PlayerPrefs.HasKey(GameManagerScript.LEVEL_SAVE_KEY_PREFIX + index);
    }

    void Update() {
        if (!started && Input.GetMouseButtonDown(0)) {
            time = 0;
            started = true;
        }
        if (goals.Count == 0 && index != GameManagerScript.LEVEL_INDEX_HUB) {
            done = true;
        }
        if (started) {
            UpdateSonar();
            if (!done) {
                time += Time.deltaTime;
            }
        }
    }
    void UpdateSonar() {
        if (pingTimer >= PING_PERIOD && !done) {
            // New ping.
            sonarMaterial.SetVector("_PingLocation", objectWithPulse.transform.position);
            sonarMaterial.SetFloat("_PingDistance", 0);
            sonarMaterial.SetFloat("_PingAge", 0);
            pingTimer %= PING_PERIOD;
            // Play SFX.
            playerScript.PingStart();
        } else {
            pingTimer += Time.deltaTime;
            float pingDistance = Mathf.Pow(pingTimer, PING_EXPONENT) * PING_SPEED;
            sonarMaterial.SetFloat("_PingDistance", pingDistance);
            sonarMaterial.SetFloat("_PingAge", pingTimer);
        }
    }

    public void Collect(GameObject goal) {
        Destroy(goal);
        goals.Remove(goal);
    }
    public bool IsDone() {
        return index != -1 && goals != null && goals.Count == 0;
    }
    public bool ShowTimer() {
        return index >= 0 && started && !firstRun;
    }
}
