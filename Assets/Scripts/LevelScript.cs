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

    PlayerScript playerScript;
    List<GameObject> goals;
    [HideInInspector] public float pingTimer;

    void Start() {
        sonarMaterial.SetFloat("_MaxPingAge", PING_PERIOD);
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        Transform targetPlayerTransform = Util.GetDirectChildWithTag(transform, "PlayerMarker").transform;
        playerScript.transform.position = targetPlayerTransform.position;
        playerScript.transform.rotation = targetPlayerTransform.rotation;
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
    }

    void Update() {
        UpdateSonar();
    }
    void UpdateSonar() {
        if (Time.time == 0 || Time.time % PING_PERIOD < (Time.time - Time.deltaTime) % PING_PERIOD) {
            // New ping.
            sonarMaterial.SetVector("_PingLocation", playerScript.transform.position);
            sonarMaterial.SetFloat("_PingDistance", 0);
            sonarMaterial.SetFloat("_PingAge", 0);
            pingTimer = 0;
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
}
