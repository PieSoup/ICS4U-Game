using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// This class procedurally generates the map using cellular automata
public class MapGenerator : MonoBehaviour
{
    // Initialize field variables
    private Element[,] matrix;

    public int sizeX {private get; set;}
    public int sizeY {private get; set;}

    [SerializeField] private string seed;
    [SerializeField] private bool useRandomSeed;
    [Range(0, 100)]
    [SerializeField] private int randomFillPercent;

    // This method randomly fills the matrix with noise, and then smooths the noise using cellular automata
    public Element[,] GenerateMatrix() {

        matrix = new Element[sizeX,sizeY];
        RandomFillMatrix();

        for (int i = 0; i < 5; i++)
        {
            SmoothMap();
        }
        return matrix;
    }
    // Using a random seed based on the system time, simply randomly fill each pixel either empty or stone based on a random number
    private void RandomFillMatrix() {
        
        if (useRandomSeed) {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                if (x == 0 || x == sizeX - 1 || y == 0 || y == sizeY - 1){
                    matrix[x,y] = ElementType.STONE.CreateElementByMatrix(x, y, Vector3.zero);
                }
                else{
                    if (pseudoRandom.Next(0,100) < randomFillPercent) {
                        matrix[x,y] = ElementType.STONE.CreateElementByMatrix(x, y, Vector3.zero);
                    }
                    else {
                        matrix[x,y] = ElementType.EMPTYCELL.CreateElementByMatrix(x, y, Vector3.zero);
                    }
                }
            }
        }
    }
    // Cellular automata is used to smooth the map:
    private void SmoothMap() {
        // First, get the number of surrounding elements in a certain range for each element
        int[,] neighbourWallTiles = new int[sizeX,sizeY];
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                neighbourWallTiles[x,y] = GetSurroundingElementCount(x, y, 20);
            }
        }
        // After the number of elements has been determined, set it to either empty or stone based on how many filled cells there are around it
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                if (neighbourWallTiles[x,y] > 840) {
                    matrix[x,y] = ElementType.STONE.CreateElementByMatrix(x, y, Vector3.zero);
                }
                else if (neighbourWallTiles[x,y] < 840) {
                    matrix[x,y] = ElementType.EMPTYCELL.CreateElementByMatrix(x, y, Vector3.zero);
                }
            }
        }
    }
    // Loop through the elements in a range around an element and return the number of filled elements in it
    private int GetSurroundingElementCount(int x, int y, int distance) {

        int elementCount = 0;
        for (int neighbourX = x - distance; neighbourX <= x + distance; neighbourX++)
        {
            for (int neighbourY = y - distance; neighbourY <= y + distance; neighbourY++)
            {
                if(neighbourX >= 0 && neighbourX < sizeX && neighbourY >= 0 && neighbourY < sizeY) {
                    if(neighbourX != x || neighbourY != y) {
                        elementCount += matrix[neighbourX,neighbourY].elementType == ElementType.STONE ? 1 : 0;
                    }
                }
                else {
                    elementCount++;
                }
            }
        }
        return elementCount;
    }
}
