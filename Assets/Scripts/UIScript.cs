using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    public GameManagerScript gameManager;
    public TextMeshProUGUI tmpLevelTimer;
    public CanvasGroup canvasGroupFade, canvasGroupClickPrompt;

    bool fading;
    bool firstFade;

    void Start() {
        if (!Application.isEditor) {
            canvasGroupFade.alpha = 1;
            firstFade = true;
        }
        canvasGroupClickPrompt.alpha = 1;
    }

    void Update() {
        float fadeRate = firstFade ? 1 : 3;
        canvasGroupFade.alpha = Mathf.Clamp01(canvasGroupFade.alpha + Time.deltaTime * (fading ? fadeRate : -fadeRate));
        tmpLevelTimer.enabled = gameManager.currentLevel.ShowTimer();
        if (tmpLevelTimer.enabled) {
            tmpLevelTimer.text = Util.SecondsToTimeString(gameManager.currentLevel.time, true);
        }
        if (Time.time > 5 && gameManager.currentLevel.index == GameManagerScript.LEVEL_INDEX_INTRO && !gameManager.currentLevel.started) {
            canvasGroupClickPrompt.alpha += Time.deltaTime;
        } else {
            canvasGroupClickPrompt.alpha = 0;
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
