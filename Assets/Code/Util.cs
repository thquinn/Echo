using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code {
    public class Util {
        static LayerMask layerMaskTerrain, layerMaskSconce;
        static Util() {
            layerMaskTerrain = LayerMask.GetMask("Terrain");
            layerMaskSconce = LayerMask.GetMask("Sconce");
        }

        public static string SecondsToTimeString(float time, bool monospaced = false) {
            float minutes = Mathf.FloorToInt(time / 60);
            float seconds = time % 60;
            if (monospaced) { 
                return string.Format("<mspace=.66em>{0}</mspace>:<mspace=.66em>{1:00}</mspace>.<mspace=.66em>{2:00}</mspace>", minutes, Mathf.FloorToInt(seconds), Mathf.FloorToInt((seconds % 1) / .01f));
            }
            return string.Format("{0}:{1:00.00}", minutes, seconds);
        }

        public static float Damp(float source, float target, float smoothing, float dt) {
            return Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
        }
        public static Vector2 Damp(Vector2 source, Vector2 target, float smoothing, float dt) {
            return Vector2.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
        }
        public static Vector3 Damp(Vector3 source, Vector3 target, float smoothing, float dt) {
            return Vector3.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
        }
        public static Quaternion Damp(Quaternion source, Quaternion target, float smoothing, float dt) {
            return Quaternion.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
        }

        public static int RangeOtherThan(int min, int max, int exclude) {
            if (min == max - 1 && exclude == min) {
                return exclude;
            }
            int output = exclude;
            while (output == exclude) {
                output = Random.Range(min, max);
            }
            return output;
        }

        public static GameObject GetDirectChildWithTag(Transform transform, string tag) {
            foreach (Transform child in transform) {
                if (child.tag == tag) {
                    return child.gameObject;
                }
            }
            return null;
        }
        public static List<GameObject> GetDirectChildrenWithTag(Transform transform, string tag) {
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in transform) {
                if (child.tag == tag) {
                    children.Add(child.gameObject);
                }
            }
            return children;
        }

        public static bool IsOnGround(GameObject go, int numChecks, float radius, float height) {
            Vector3 position = go.transform.position;
            position.y += height * .5f;
            for (int i = -1; i < numChecks; i++) {
                RaycastHit hitInfo;
                if (i == -1) {
                    Physics.Raycast(position, Vector3.down, out hitInfo, height, layerMaskTerrain);
                } else {
                    float theta = 2 * Mathf.PI / numChecks * i;
                    position.x += Mathf.Cos(theta) * radius;
                    position.z += Mathf.Sin(theta) * radius;
                    Physics.Raycast(position, Vector3.down, out hitInfo, height, layerMaskTerrain);
                }
                if (hitInfo.collider != null && Vector3.Dot(hitInfo.normal, Vector3.up) > .5f) {
                    return true;
                }
            }
            return false;
        }
        public static Vector3 GetWallrunNormal(GameObject go, Vector3 moveDirection, float height) {
            moveDirection.Normalize();
            Vector3 position = go.transform.position;
            position.y += height;
            RaycastHit hitInfo;
            Physics.Raycast(position, moveDirection, out hitInfo, PlayerScript.WALLRUN_CHECK_DISTANCE, layerMaskTerrain);
            Debug.DrawLine(position, position + moveDirection * PlayerScript.WALLRUN_CHECK_DISTANCE, Color.white, 2);
            if (hitInfo.collider == null) {
                return Vector3.zero;
            }
            float yComponent = Vector3.Dot(hitInfo.normal, Vector3.up);
            return yComponent > -.05f && yComponent < .2f ? hitInfo.normal : Vector3.zero;
        }
        public static SconceScript GetLookedSconce(Camera cam) {
            RaycastHit hitInfo;
            Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, 10, layerMaskSconce);
            if (hitInfo.collider == null) {
                return null;
            }
            return hitInfo.collider.GetComponent<SconceScript>();
        }
    }

    public static class ArrayExtensions {
        public static T[] Shuffle<T>(this T[] array) {
            int n = array.Length;
            for (int i = 0; i < n; i++) {
                int r = i + UnityEngine.Random.Range(0, n - i);
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
            return array;
        }
    }
}
