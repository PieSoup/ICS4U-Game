using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// This class simply just sets the sorting layer of the matrix so that it appears in front of the background
[ExecuteInEditMode]
public class SortingLayer : MonoBehaviour
{
    
    [SerializeField] MeshRenderer meshRenderer;

    void Start()
    {
        meshRenderer.sortingLayerName = "Foreground";
        meshRenderer.sortingOrder = -1;
    }

}
