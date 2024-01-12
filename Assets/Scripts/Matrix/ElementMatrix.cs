using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

// Class for the general matrix that stores the elements
public class ElementMatrix : MonoBehaviour
{
    // Main matrix of elements
    public Element[,] matrix {get; set;}
    // Initialize field variables
    private PlaneGenerator plane;
    private MeshFilter meshFilter;

    private MapGenerator mapGenerator;

    public int sizeX {get; private set;}
    public int sizeY {get; private set;}

    [SerializeField] private Material planeMaterial;
    private Texture2D texture;

    private List<int> shuffledXIndexes;

    public static BitArray stepped = new BitArray(1);
    public static Vector3 gravity = new Vector3(0f, -5f, 0f);

    public readonly int maxPlayers = 2;
    public Player[] players;
    // Initialize other scripts
    private void Awake() {
        TryGetComponent(out meshFilter);
        
        plane = GetComponent<PlaneGenerator>();
        meshFilter.GetComponent<Renderer>().material = planeMaterial;

        mapGenerator = GetComponent<MapGenerator>();
    }
    // Initialize values and run starting methods for each script
    private void Start() {

        sizeX = plane.SizeX;
        sizeY = plane.SizeY;

        mapGenerator.sizeX = sizeX;
        mapGenerator.sizeY = sizeY;

        texture = new Texture2D(sizeX, sizeY, TextureFormat.ARGB32, false);
        planeMaterial.mainTexture = texture;

        matrix = new Element[sizeX,sizeY];
        matrix = mapGenerator.GenerateMatrix();

        players = new Player[maxPlayers]; 

        shuffledXIndexes = GenerateShuffledXIndexes(sizeX);

        stepped.Set(0, true);
        for(int i = 0; i < players.Length; i++) {
            CreatePlayer((sizeX/2)+30, sizeY/2);
        }
    }
    // Update the matrix visual
    private void Update() {
        DrawTexture();
    }
    // Run physics calculations in fixed update so that they are not fps based
    private void FixedUpdate() {
        stepped.Set(0, !stepped.Get(0));
        ReshuffleXIndexes();
        StepAll();
        StepPlayers();
    }
    // Loop through all the elements and run the step method
    private void StepAll() {
        for(int y = 0; y < sizeY; y++) {
            foreach(int x in shuffledXIndexes) {
                Element element = matrix[x, y];
                element.Step(this);
            }
        }
    }
    // Step all of the players in the scene
    private void StepPlayers() {
        for(int i = 0; i < players.Length; i++) {
            Player player = players[i];
            if(player == null) {
                continue;
            }
            player.Step(this);
        }
    }
    // Create players at certain spawn locations
    private Player CreatePlayer(int x, int y) {
        int index = -1;
        for(int i = 0; i < players.Length; i++) {
            Player player = players[i];
            if(player == null) {
                index = i;
                break;
            }
        }
        if(index == -1) {
            return null;
        }
        Player newPlayer;
        if(index % 2 == 0) {
            newPlayer = new Player(30, 30, 7, 27, index, PlayerType.FIRE, this);
        }
        else {
            newPlayer = new Player(sizeX - 30, sizeY - 30, 7, 27, index, PlayerType.WATER, this);
        }

        players[index] = newPlayer;
        return newPlayer;
    }
    // Get the player at a certain index
    public Player GetPlayer(int index) {
        if(index < 0 || index >= maxPlayers) {
            return null;
        }
        return players[index];
    }
    // Shuffle the indexes that are loop through when stepping to add randomness
    private void ReshuffleXIndexes() {
        System.Random rand = new System.Random();
        shuffledXIndexes = shuffledXIndexes.OrderBy(_ => rand.Next()).ToList();

    }
    // Generate the initial list of x indexes
    private List<int> GenerateShuffledXIndexes(int size) {
        List<int> list = new List<int>(size);
        for(int i = 0; i < size; i++) {
            list.Add(i);
        }
        return list;
    }
    // Convert pixel coordinates to matrix
    public int ToMatrix(int val) {
        return val * 16;
    }
    // Convert float pixel coordinates to matrix coordinates
    public int ToMatrix(float val) {
        return Mathf.RoundToInt(val * 16f);
    }
    // Spawn elements depending on pixel coordinates using a brush  
    public void SpawnElementByPixelWithBrush(float pixelX, float pixelY, Vector3 velocity, ElementType elementType, int brushSize, CreationController.BRUSHTYPE brushType) {

        int matrixX = ToMatrix(pixelX);
        int matrixY = ToMatrix(pixelY);

        SpawnElementByMatrixWithBrush(matrixX, matrixY, velocity, elementType, brushSize, brushType);
    }
    // Spawn elements depending on matrix coordinates using a brush
    private void SpawnElementByMatrixWithBrush(int matrixX, int matrixY, Vector3 velocity, ElementType elementType, int brushSize, CreationController.BRUSHTYPE brushType) {

        int halfBrush = Mathf.FloorToInt(brushSize / 2);

        for(int x = matrixX - halfBrush; x <= matrixX + halfBrush; x++) {
            for(int y = matrixY - halfBrush; y <= matrixY + halfBrush; y++) {
                if(brushType.Equals(CreationController.BRUSHTYPE.CIRCLE)) {
                    
                    int distance = Helpers.DistanceBetweenTwoPoints(matrixX, x, matrixY, y);

                    if(distance < halfBrush) {
                        SpawnElementByMatrix(x, y, velocity, elementType);
                    }
                }
                else {
                    SpawnElementByMatrix(x, y, velocity, elementType);
                }
            }
        }
    }
    // Set elements at matrix coordinates if it is within the bounds of the world
    public void SpawnElementByMatrix(int x, int y, Vector3 velocity, ElementType elementType) {
        if(!isWithinBounds(x, y)) return;
        if(Get(x, y).elementType != ElementType.PLAYERSEGMENT) {
            SetElementAtIndex(x, y, elementType.CreateElementByMatrix(x, y, velocity));
        }
    }
    // Method that uses a matrix traversal algorithm to fill in large areas if multiple points were skipping between frames
    public void SpawnElementBetweenTwoPoints(Vector3 pos1, Vector3 pos2, Vector3 velocity, ElementType elementType, int brushSize, CreationController.BRUSHTYPE brushType) {
        // Uses a similar method of traversing the matrix as the elements do to move
        int matrixX1 = ToMatrix(pos1.x);
        int matrixY1 = ToMatrix(pos1.y);
        int matrixX2 = ToMatrix(pos2.x);
        int matrixY2 = ToMatrix(pos2.y);

        if(Helpers.EpsilonEquals(pos1, pos2, 0.01f)) { 
            SpawnElementByMatrixWithBrush(matrixX1, matrixY1, velocity, elementType, brushSize, brushType);
            return;
        }

        int xDifference = matrixX2 - matrixX1;
        int yDifference = matrixY2 - matrixY1;

        bool xDifferenceIsLarger = Mathf.Abs(xDifference) > Mathf.Abs(yDifference);

        int xModifier = xDifference > 0 ? 1 : -1;
        int yModifier = yDifference > 0 ? 1 : -1;

        int max = Mathf.Max(Mathf.Abs(xDifference), Mathf.Abs(yDifference));
        int min = Mathf.Min(Mathf.Abs(xDifference), Mathf.Abs(yDifference));

        float slope = (min == 0 || max == 0) ? 0 : ((float) (min + 1) / (max + 1));

        int smallCount;

        for(int i = 1; i <= max; i++) {
            smallCount = Mathf.FloorToInt(i * slope);
            int xIncrease, yIncrease;

            if(xDifferenceIsLarger) {
                xIncrease = i;
                yIncrease = smallCount;
            }
            else {
                xIncrease = smallCount;
                yIncrease = i;
            }

            int currentX = matrixX1 + (xIncrease * xModifier);
            int currentY = matrixY1 + (yIncrease * yModifier);

            if(isWithinBounds(currentX, currentY)) {
                SpawnElementByMatrixWithBrush(currentX, currentY, velocity, elementType, brushSize, brushType);
            }
        }
    }
    // Create a new element and set its coordinates
    public void SetElementAtIndex(int x, int y, Element newElement) {
        if(isWithinBounds(x, y)) {
            matrix[x, y] = newElement;
            newElement.SetCoordinatesByMatrix(x, y);
        }
    }
    // Get the element at certain coordinates
    public Element Get(int x, int y) {
        if(isWithinBounds(x, y)) {
            return matrix[x, y];
        }
        else {
            return null;
        }
    }

   // Simply check if the given coordinates are within the bounds of the matrix
    public bool isWithinBounds(int x, int y) {

        if(x >= 0 && x < sizeX && y >= 0 && y < sizeY) {
            return true;
        }
        return false;
    }
    // Refresh the texture2d depending on the matrix
    private void DrawTexture() {

        Color[] colors = new Color[sizeX * sizeY];
        
        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                int index = y * sizeX + x;
                Color newColor = matrix[x,y].elementColor;
                colors[index] = newColor;
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colors);
        texture.Apply();
    }
}