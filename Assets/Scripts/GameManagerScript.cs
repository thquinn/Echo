using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static string LEVEL_SAVE_ENTER_KEY_PREFIX = "LevelEnter_";
    public static string LEVEL_SAVE_TIME_KEY_PREFIX = "LevelTime_";
    static int LEVEL_INDEX_NONE = -3;
    public static int LEVEL_INDEX_INTRO = -2;
    public static int LEVEL_INDEX_HUB = -1;
    static float SCONCE_DISTANCE = 8f;
    static float SCONCE_ANGULAR_DISTANCE = Mathf.PI / 7;

    public GameObject[] prefabLevels;
    public GameObject prefabLevelHub, prefabLevelIntro;
    public GameObject prefabSconce;

    public PlayerScript playerScript;
    public UIScript uiScript;
    [HideInInspector] public LevelScript currentLevel;

    int nextLevel;

    void Start() {
        if (!PlayerPrefs.HasKey(LEVEL_SAVE_TIME_KEY_PREFIX + LEVEL_INDEX_INTRO)) {
            LoadLevel(LEVEL_INDEX_INTRO);
        } else {
            LoadLevel(LEVEL_INDEX_HUB);
        }
        nextLevel = LEVEL_INDEX_NONE;
    }

    void Update() {
        if (nextLevel != LEVEL_INDEX_NONE && uiScript.DoneFading()) {
            LoadLevel(nextLevel);
            nextLevel = LEVEL_INDEX_NONE;
            uiScript.Invoke("FadeIn", .5f);
        }
        if (currentLevel.IsDone()) {
            string key = LEVEL_SAVE_TIME_KEY_PREFIX + currentLevel.index;
            if (currentLevel.time < PlayerPrefs.GetFloat(key, float.MaxValue)) {
                PlayerPrefs.SetFloat(key, currentLevel.time);
                PlayerPrefs.Save();
            }
            FadeToLevel(LEVEL_INDEX_HUB);
        } else if (playerScript.transform.position.y < -3) {
            // Falling off.
            FadeToLevel(currentLevel.index == LEVEL_INDEX_INTRO ? LEVEL_INDEX_INTRO : LEVEL_INDEX_HUB);
        }
    }

    public void ClickedSconce(int index) {
        FadeToLevel(index);
    }
    void FadeToLevel(int index) {
        nextLevel = index;
        uiScript.FadeOut();
    }
    void LoadLevel(int index) {
        GameObject prefab;
        if (index == LEVEL_INDEX_INTRO) {
            prefab = prefabLevelIntro;
        } else if (index == LEVEL_INDEX_HUB) {
            prefab = prefabLevelHub;
        } else {
            prefab = prefabLevels[index];
        }
        if (currentLevel != null) {
            Destroy(currentLevel.gameObject);
        }
        currentLevel = Instantiate(prefab).GetComponent<LevelScript>();
        currentLevel.index = index;
        playerScript.SetLevel(currentLevel);
        // Instantiate sconces.
        if (index == LEVEL_INDEX_HUB) {
            currentLevel.started = true;
            bool allBeaten = true;
            float total = 0;
            for (int i = 0; i < prefabLevels.Length; i++) {
                GameObject sconce = Instantiate(prefabSconce, currentLevel.transform);
                float angleMultipler = (prefabLevels.Length - 1) / -2f + i;
                float angle = SCONCE_ANGULAR_DISTANCE * -angleMultipler;
                sconce.transform.localPosition = new Vector3(Mathf.Cos(angle) * SCONCE_DISTANCE, 0, Mathf.Sin(angle) * SCONCE_DISTANCE);
                sconce.transform.localRotation = Quaternion.Euler(0, -angle * Mathf.Rad2Deg + 90, 0);
                sconce.GetComponent<SconceScript>().Init(i);
                if (PlayerPrefs.HasKey(LEVEL_SAVE_TIME_KEY_PREFIX + i)) {
                    total += PlayerPrefs.GetFloat(LEVEL_SAVE_TIME_KEY_PREFIX + i);
                } else {
                    allBeaten = false;
                    break;
                }
            }
            if (allBeaten) {
                currentLevel.tmpTotalTime.text = Util.SecondsToTimeString(total);
            }
        }
    }
}
