using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Code {
    public class Util {
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
