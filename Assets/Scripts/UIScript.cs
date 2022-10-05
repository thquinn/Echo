using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScript : MonoBehaviour
{
    public Sprite KEY_PRESSED, KEY_UNPRESSED;

    public GameManagerScript gameManager;
    public TextMeshProUGUI tmpLevelTimer;
    public CanvasGroup canvasGroupFade, canvasGroupClickPrompt;
    public Image[] imageKeys;

    public static string DISPLAY_INPUT_SAVE_KEY = "DISPLAY_INPUT";

    bool fading;
    bool firstFade;

    void Start() {
        if (!Application.isEditor) {
            canvasGroupFade.alpha = 1;
            firstFade = true;
        }
        canvasGroupClickPrompt.alpha = 1;
        if (PlayerPrefs.GetInt(DISPLAY_INPUT_SAVE_KEY, 0) > 0) {
            imageKeys[0].transform.parent.gameObject.SetActive(true);
        }
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F8)) {
            GameObject inputDisplay = imageKeys[0].transform.parent.gameObject;
            inputDisplay.SetActive(!inputDisplay.activeSelf);
            PlayerPrefs.SetInt(DISPLAY_INPUT_SAVE_KEY, inputDisplay.activeSelf ? 1 : 0);
            PlayerPrefs.Save();
        }
        float fadeRate = firstFade ? 1 : 3;
        canvasGroupFade.alpha = Mathf.Clamp01(canvasGroupFade.alpha + Time.deltaTime * (fading ? fadeRate : -fadeRate));
        tmpLevelTimer.enabled = gameManager.currentLevel.ShowTimer();
        if (tmpLevelTimer.enabled) {
            tmpLevelTimer.text = Util.SecondsToTimeString(gameManager.currentLevel.time, true);
        }
        if (gameManager.currentLevel.index != GameManagerScript.LEVEL_INDEX_HUB && !gameManager.currentLevel.started && gameManager.currentLevel.runTime > 5) {
            canvasGroupClickPrompt.alpha += Time.deltaTime;
        } else {
            canvasGroupClickPrompt.alpha = 0;
        }
        for (int i = 0; i < imageKeys.Length; i++) {
            bool pressed = Input.GetKey(new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.Space }[i]);
            imageKeys[i].sprite = pressed ? KEY_PRESSED : KEY_UNPRESSED;
        }
    }

    public void FadeOut() {
        fading = true;
        firstFade = false;
    }
    public void FadeIn() {
        fading = false;
    }
    public float GetFade() {
        return canvasGroupFade.alpha;
    }
    public bool DoneFading() {
        return canvasGroupFade.alpha >= 1;
    }
}
