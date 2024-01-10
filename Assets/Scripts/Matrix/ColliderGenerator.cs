using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColliderGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] ElementMatrix matrix;

    [Header("Settings")]
    [SerializeField] private bool isTrigger;
    [SerializeField] private ElementType elementType;
    [SerializeField] private int chunkSize;

    private GameObject[,] chunks;
    
    void Start() {
        GenerateChunks();
    }
    void Update()
    {
        if(Input.GetMouseButtonDown(0)) {
            //GenerateColliders(Random.Range(0, 9), Random.Range(0, 5));
        }
    }

    private void GenerateChunks() {
        chunks = new GameObject[Mathf.CeilToInt((float) matrix.sizeX / chunkSize), Mathf.CeilToInt((float) matrix.sizeY / chunkSize)];
        for(int y = 0; y < chunks.GetLength(1); y++) {

            for(int x = 0; x < chunks.GetLength(0); x++) {
                chunks[x, y] = new GameObject();
                chunks[x, y].name = "Chunk " + ((y*chunks.GetLength(0))+x);
                Instantiate(chunks[x, y], transform);
                List<List<Vector2>> paths = GenerateColliders(x, y);
                for(int i = 0; i < paths.Count; i++) {
                    if(i == 0 && paths[i].Count > 1 && paths.Count > 1) {
                        for(int j = 1; j < paths.Count; j++) {
                            if(Vector2.Distance(paths[i].First(), paths[j].First()) < 1) {
                                paths[i].Insert(0, paths[j].First());
                            }
                        }
                    }
                    EdgeCollider2D edgeCollider = chunks[x, y].AddComponent<EdgeCollider2D>();
                    List<Vector2> newPoints = new List<Vector2>();
                    LineUtility.Simplify(paths[i], 0.08f, newPoints);
                    edgeCollider.SetPoints(newPoints);
                }
                

                if(chunks[x, y].GetComponent<EdgeCollider2D>() == null) {
                    chunks[x, y].AddComponent<EdgeCollider2D>();
                }
            }
        }
    }

    private List<List<Vector2>> GenerateColliders(int chunkX, int chunkY) {
        Debug.Log("Generating chunk at (" + chunkX + ", " + chunkY + ")");
        List<Vector2> points = new List<Vector2>();
        
        for (int y = chunkY*chunkSize-1; y < (chunkY+1)*chunkSize-1; y++)
        {
            for (int x = chunkX*chunkSize-1; x < (chunkX+1)*chunkSize-1; x++)
            {
                if(matrix.isWithinBounds(x, y) && matrix.matrix[x,y].elementType == elementType) {
                
                    float A = ToPixel(x);
                    float B = ToPixel(x+1);
                    float C = ToPixel(y);
                    float D = ToPixel(y+1);

                    if (matrix.Get(x, y + 1) != null && matrix.Get(x, y + 1).elementType == ElementType.EMPTYCELL) {
                        points.Add(new Vector2(A, D));
                        points.Add(new Vector2(B, D));
                    }

                    if (matrix.Get(x, y - 1) != null && matrix.Get(x, y - 1).elementType == ElementType.EMPTYCELL) {
                        points.Add(new Vector2(A, C));
                        points.Add(new Vector2(B, C));
                    }

                    if (matrix.Get(x - 1, y) != null && matrix.Get(x - 1, y).elementType == ElementType.EMPTYCELL) {
                        points.Add(new Vector2(A, C));
                        points.Add(new Vector2(A, D));
                    }
        
                    if (matrix.Get(x - 1, y) != null && matrix.Get(x + 1, y).elementType == ElementType.EMPTYCELL) {
                        points.Add(new Vector2(B, C));
                        points.Add(new Vector2(B, D));
                    }
                }
            }
        }
        return AddColliders(points);
    }

    private List<List<Vector2>> AddColliders(List<Vector2> points) {

        points = SortVector2ListByRelativeProximity(points);
        List<List<Vector2>> paths = new List<List<Vector2>>();
        List<Vector2> currentPoints = new List<Vector2>();
        for(int i = 0; i < points.Count-1; i++) {
            currentPoints.Add(points[i]);
            float distance = Vector2.Distance(points[i], points[i + 1]);
            if (distance > 1f/16f) {
                paths.Add(currentPoints.ToList());
                currentPoints.Clear();
            }
        }

        if (currentPoints.Count > 0) {
            currentPoints.Add(points[points.Count - 1]);
            paths.Add(currentPoints.ToList());
        }

        return paths;
    }


    private float ToPixel(int val) {
        float pixelVal = val/16f;
        return pixelVal;
    }

    private List<Vector2> SortVector2ListByRelativeProximity(List<Vector2> points) {
        if(points == null || points.Count <= 1) {
            return points;
        }

        List<Vector2> sortedPoints = new List<Vector2>();
        sortedPoints.Add(points[0]);

        while(sortedPoints.Count < points.ToList().Count) {

            Vector2 lastPoint = sortedPoints[sortedPoints.Count-1];
            Vector2 closestPoint = FindClosestPoint(lastPoint, points, sortedPoints);

            if(closestPoint != Vector2.zero) {
                sortedPoints.Add(closestPoint);
            }
            else {
                break;
            }

        }
        return sortedPoints;
    }

    private Vector2 FindClosestPoint(Vector2 point, List<Vector2> points, List<Vector2> excludePoints) {
        Vector2 closestPoint = Vector2.zero;
        float closestDistance = float.MaxValue;

        foreach(Vector2 _point in points) {
            if(!excludePoints.Contains(_point)) {
                float distance = Vector2.Distance(point, _point);

                if(distance < closestDistance) {
                    closestDistance = distance;
                    closestPoint = _point;
                }
            }
        }

        return closestPoint;
    }
}
