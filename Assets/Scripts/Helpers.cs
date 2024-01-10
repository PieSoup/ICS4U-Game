using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Helpers : MonoBehaviour
{
    // This method checks if two vectors are close enough together that they are considered the same value
    public static bool EpsilonEquals(Vector3 vector, Vector3 other, float epsilon) {
        return Mathf.Abs(vector.x - other.x) < epsilon &&
               Mathf.Abs(vector.y - other.y) < epsilon &&
               Mathf.Abs(vector.z - other.z) < epsilon;
    }
    // This method gets the distance between two points on the matrix
    public static int DistanceBetweenTwoPoints(int x1, int x2, int y1, int y2) {
        return Mathf.CeilToInt(Mathf.Sqrt((float)(Math.Pow(x1 - x2, 2) + Mathf.Pow(y1 - y2, 2))));
    }
}
