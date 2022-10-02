using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalScript : MonoBehaviour
{
    Vector3 initialPosition;

    void Start() {
        initialPosition = transform.localPosition;
    }

    void Update() {
        transform.localPosition = initialPosition + new Vector3(0, Mathf.Sin(Time.time), 0);
        transform.Rotate(0, Time.deltaTime * 10, 0);
    }
}
