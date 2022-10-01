using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FloatersScript : MonoBehaviour
{
    static float RADIUS = 7;
    static float DIAMETER = RADIUS * 2;
    static float DENSITY = 1;
    static float FADE_RADIUS = 5;

    public GameObject prefabFloater;

    Transform camTransform;
    List<MeshRenderer> floaters;
    Vector3[] velocities, velocityTargets;
    float[] alphas;
    bool[] fadingOut;
    Quaternion specular, specularVelocity, specularAcceleration;

    void Start() {
        camTransform = Camera.main.transform;
        transform.position = camTransform.position;
        int numFloaters = Mathf.RoundToInt(RADIUS * RADIUS * RADIUS * DENSITY);
        floaters = new List<MeshRenderer>();
        for (int i = 0; i < numFloaters; i++) {
            GameObject floater = Instantiate(prefabFloater, transform);
            floater.transform.localPosition = new Vector3(Random.Range(-RADIUS, RADIUS), Random.Range(-RADIUS, RADIUS), Random.Range(-RADIUS, RADIUS));
            floaters.Add(floater.GetComponent<MeshRenderer>());
        }
        velocities = new Vector3[numFloaters];
        velocityTargets = velocities.Select(v => Random.onUnitSphere).ToArray();
        alphas = new float[numFloaters];
        fadingOut = new bool[numFloaters];
        specular = Random.rotation;
        specularAcceleration = Random.rotation;
    }

    void Update() {
        Vector3 camPosition = camTransform.position;
        for (int i = 0; i < floaters.Count; i++) {
            MeshRenderer floater = floaters[i];
            Vector3 floaterPosition = floater.transform.position;
            velocities[i] = Util.Damp(velocities[i], velocityTargets[i], .5f, Time.deltaTime);
            floaterPosition += velocities[i] * Time.deltaTime * .1f;
            Vector3 delta = camPosition - floaterPosition;
            if (delta.magnitude > RADIUS) {
                floaterPosition += delta.normalized * DIAMETER;
            }
            floater.transform.position = floaterPosition;
            float distance = delta.magnitude;
            alphas[i] = Util.Damp(alphas[i], fadingOut[i] ? .33f : 1, .5f, Time.deltaTime);
            float multiplier = Mathf.Min(Mathf.InverseLerp(.01f, .1f, distance), Mathf.InverseLerp(FADE_RADIUS, RADIUS, distance));
            float alpha = alphas[i] * multiplier;
            if (Input.GetKey(KeyCode.Z)) {
                alpha = 1;
            }
            floater.material.SetFloat("_Alpha", alpha);
        }
    }

    void FixedUpdate() {
        if (Random.value < .01f) {
            velocityTargets.Shuffle();
        }
        if (Random.value < .01f) {
            for (int i = 0; i < fadingOut.Length; i++) {
                fadingOut[i] = Random.value < .5f;
            }
        }
        if (Random.value < .01f) {
            specularAcceleration = Random.rotation;
        }
        specularVelocity *= specularAcceleration;
        specularVelocity = Quaternion.Lerp(specularVelocity, Quaternion.identity, .99f);
        specular *= specularVelocity;
        floaters[0].sharedMaterial.SetVector("_SpecularVector", specular * Vector3.up);
    }
}
