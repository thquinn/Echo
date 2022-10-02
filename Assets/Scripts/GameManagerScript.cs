using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public static string LEVEL_SAVE_KEY = "LevelTime";
    static int LEVEL_INDEX_NONE = -3;
    static int LEVEL_INDEX_INTRO = -2;
    static int LEVEL_INDEX_HUB = -1;

    public GameObject[] prefabLevels;
    public GameObject prefabLevelHub, prefabLevelIntro;

    public PlayerScript playerScript;
    public UIScript uiScript;
    [HideInInspector] public LevelScript currentLevel;

    int nextLevel;

    void Start() {
        if (!PlayerPrefs.HasKey(LEVEL_SAVE_KEY + LEVEL_INDEX_INTRO)) {
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
            uiScript.Invoke("FadeIn", 1);
        }
        if (currentLevel.IsDone()) {
            PlayerPrefs.SetFloat(LEVEL_SAVE_KEY + currentLevel.index, currentLevel.time);
            PlayerPrefs.Save();
                FadeToLevel(LEVEL_INDEX_HUB);
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
        Debug.Assert(currentLevel.index == index);
        playerScript.SetLevel(currentLevel);
    }
}
