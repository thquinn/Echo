using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScript : MonoBehaviour
{
    static float ALPHA = .3f;

    public MeshRenderer meshRenderer;

    Vector3 originalPosition;
    MaterialPropertyBlock materialPropertyBlock;

    [HideInInspector] public bool hideWhenFar;
    Transform playerTransform;
    Quaternion angularVelocity, targetAngularVelocity;
    float rand;

    void Start() {
        originalPosition = transform.localPosition;
        materialPropertyBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetFloat("_Alpha", ALPHA);
        meshRenderer.SetPropertyBlock(materialPropertyBlock);
        angularVelocity = Random.rotation;
        targetAngularVelocity = Random.rotation;
        rand = Random.Range(0f, 100f);
    }

    void Update() {
        if (hideWhenFar) {
            playerTransform ??= GameObject.FindGameObjectWithTag("Player").transform;
            float distance = (transform.position - playerTransform.position).magnitude;
            meshRenderer.GetPropertyBlock(materialPropertyBlock);
            materialPropertyBlock.SetFloat("_Alpha", Mathf.Lerp(0, ALPHA, Mathf.InverseLerp(15, 10, distance)));
            meshRenderer.SetPropertyBlock(materialPropertyBlock);
        }
        transform.localPosition = originalPosition + new Vector3(0, Mathf.Sin(Time.time * 5 / Mathf.PI + rand) * .25f, 0);
    }
    void FixedUpdate() {
        angularVelocity = Quaternion.Lerp(angularVelocity, targetAngularVelocity, 0.005f);
        transform.localRotation *= Quaternion.Lerp(Quaternion.identity, angularVelocity, .005f);
    }
}
