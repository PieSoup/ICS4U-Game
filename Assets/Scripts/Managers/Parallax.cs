using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class uses a parallax factor to change how much the background moves depending on where the player moves
public class Parallax : MonoBehaviour
{
    public Camera cam;
    public Transform subject;

    Vector2 startPos;
    float startZ;

    Vector2 travel => (Vector2)cam.transform.position - startPos;
    float distanceFromSubject => transform.position.z - subject.position.z;
    float clippingPlane => cam.transform.position.z + (distanceFromSubject > 0 ? cam.farClipPlane : cam.nearClipPlane);
    float parallaxFactor => Mathf.Abs(distanceFromSubject) / clippingPlane;

    void Start() {
        startPos = transform.position;
        startZ = transform.position.z;
    }

    void Update() {
        // Update position depending on how much the player has moved and the parallax factor
        Vector2 newPos = startPos + travel * parallaxFactor;
        transform.position = new Vector3(newPos.x, newPos.y, startZ);
    }

}
