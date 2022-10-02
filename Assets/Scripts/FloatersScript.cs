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
    public Material floaterMaterial;

    Transform camTransform;
    List<MeshRenderer> floaters;
    Vector3[] velocities, velocityTargets;
    float[] alphas;
    bool[] fadingOut;
    Quaternion specular, specularVelocity, specularAcceleration;
    MaterialPropertyBlock materialPropertyBlock;

    void Start() {
        int numFloaters = Mathf.RoundToInt(RADIUS * RADIUS * RADIUS * DENSITY);
        floaters = new List<MeshRenderer>();
        for (int i = 0; i < numFloaters; i++) {
            floaters.Add(Instantiate(prefabFloater, transform).GetComponent<MeshRenderer>());
        }
        velocities = new Vector3[numFloaters];
        velocityTargets = velocities.Select(v => Random.onUnitSphere).ToArray();
        alphas = new float[numFloaters];
        fadingOut = new bool[numFloaters];
        specular = Random.rotation;
        specularAcceleration = Random.rotation;
        materialPropertyBlock = new MaterialPropertyBlock();
    }
    public void Init() {
        camTransform ??= Camera.main.transform;
        transform.position = camTransform.position;
        foreach (MeshRenderer floater in floaters) {
            floater.transform.localPosition = new Vector3(Random.Range(-RADIUS, RADIUS), Random.Range(-RADIUS, RADIUS), Random.Range(-RADIUS, RADIUS));
        }
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
            float multiplier = Mathf.Min(Mathf.InverseLerp(1, 2, distance), Mathf.InverseLerp(RADIUS, FADE_RADIUS, distance));
            float alpha = alphas[i] * multiplier;
            if (Input.GetKey(KeyCode.Z)) {
                alpha = 1;
            }
            floater.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_Alpha", alpha);
            floater.SetPropertyBlock(materialPropertyBlock);
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
        specularVelocity = Quaternion.Lerp(specularVelocity, Quaternion.identity, .97f);
        specular *= specularVelocity;
        floaterMaterial.SetVector("_SpecularVector", specular * Vector3.up);
    }
}
