using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SconceScript : MonoBehaviour
{
    public GameObject nameplate;
    public MeshRenderer meshRenderer;
    public TextMeshPro tmpName, tmpTime;

    public int index;
    Camera cam;
    float lookTimer;

    void Start() {
        tmpName.color = new Color(1, 1, 1, 0);
        tmpTime.color = new Color(1, 1, 1, 0);
        cam = Camera.main;
    }
    public void Init(int index) {
        this.index = index;
        tmpName.text = "Level " + (index + 1);
        tmpTime.text = "";
    }

    public void Look() {
        lookTimer = 5;
    }
    void Update() {
        lookTimer -= Time.deltaTime;
        Color c = tmpName.color;
        c.a = Mathf.Clamp01(c.a + Time.deltaTime * (lookTimer > 0 ? 2 : -2));
        tmpName.color = c;
        tmpTime.color = c;
        if (lookTimer > 0) {
            Quaternion lookRotation = Quaternion.LookRotation(nameplate.transform.position - cam.transform.position);
            nameplate.transform.rotation = Util.Damp(nameplate.transform.rotation, lookRotation, .001f, Time.deltaTime);
        } else {
            nameplate.transform.localRotation = Quaternion.identity;
        }
    }
}
