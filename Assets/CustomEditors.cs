#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Transform))]
[CanEditMultipleObjects]
public class CustomEditors : Editor
{
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("Random Rotation")) {
            foreach (Object t in targets) {
                (t as Transform).rotation = Random.rotation;
            }
        }
    }
}
#endif
