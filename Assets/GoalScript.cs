using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalScript : MonoBehaviour
{
    public MeshRenderer meshRenderer;

    Vector3 originalPosition;
    MaterialPropertyBlock materialPropertyBlock;

    Quaternion angularVelocity, targetAngularVelocity;

    void Start() {
        originalPosition = transform.localPosition;
        materialPropertyBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(materialPropertyBlock);
        materialPropertyBlock.SetFloat("_Alpha", .66f);
        meshRenderer.SetPropertyBlock(materialPropertyBlock);
        angularVelocity = Random.rotation;
        targetAngularVelocity = Random.rotation;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            targetAngularVelocity = Random.rotation;
        }
        transform.localPosition = originalPosition + new Vector3(0, Mathf.Sin(Time.time * 5 / Mathf.PI) * .25f, 0);
        angularVelocity = Quaternion.Lerp(angularVelocity, targetAngularVelocity, 0.005f);
        transform.localRotation *= Quaternion.Lerp(Quaternion.identity, angularVelocity, .01f);
    }
    void FixedUpdate() {

    }
}
