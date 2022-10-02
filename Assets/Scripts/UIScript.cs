using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScript : MonoBehaviour
{
    public CanvasGroup canvasGroupFade;

    bool fading;
    bool firstFade;

    void Start() {
        firstFade = true;
        if (Application.isEditor) {
            canvasGroupFade.alpha = 0;
            firstFade = false;
        }
    }

    void Update() {
        float fadeRate = firstFade ? 1 : 3;
        canvasGroupFade.alpha = Mathf.Clamp01(canvasGroupFade.alpha + Time.deltaTime * (fading ? fadeRate : -fadeRate));
    }

    public void FadeOut() {
        fading = true;
        firstFade = false;
    }
    public void FadeIn() {
        fading = false;
    }
    public bool DoneFading() {
        return canvasGroupFade.alpha >= 1;
    }
}
