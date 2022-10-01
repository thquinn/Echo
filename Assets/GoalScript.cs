using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScript : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    public float f, f2;

    Vector3 originalPosition;
    MaterialPropertyBlock materialPropertyBlock;

    Quaternion angularVelocity, angularAcceleration;

    void Start() {
        originalPosition = transform.localPosition;
        materialPropertyBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetFloat("_Alpha", 1);
        meshRenderer.SetPropertyBlock(materialPropertyBlock);
        angularVelocity = Random.rotation;
        angularAcceleration = Random.rotation;
    }

    void Update() {
        transform.localPosition = originalPosition + new Vector3(0, Mathf.Sin(Time.time * 5 / Mathf.PI) * .25f, 0);
        angularVelocity = Quaternion.Lerp(angularAcceleration, Quaternion.identity, f2) * angularVelocity;
        angularVelocity = Quaternion.Lerp(angularVelocity, Quaternion.identity, f);
        transform.localRotation *= angularVelocity;
    }
    void FixedUpdate() {
        if (Random.value < .01f) {
            angularAcceleration = Random.rotation;
        }
    }
}
