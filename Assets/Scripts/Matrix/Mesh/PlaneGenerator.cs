using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class creates a mesh of quads to draw the texture of the matrix to
public class PlaneGenerator : MonoBehaviour
{
    // Initialize fields
    private MeshFilter meshFilter;

    [Header("Dimensions:")]
    [SerializeField] private int sizeX;
    [SerializeField] private int sizeY;

    public int SizeX {get{return sizeX;}}
    public int SizeY {get{return sizeY;}}

    private Plane plane;
    // Get the mesh filter component
    private void Awake() {
        TryGetComponent(out meshFilter);
    }
    // Create a new Plane mesh
    private void Start() {

        if(meshFilter) {
            plane = new Plane(meshFilter.mesh, sizeX+1, sizeY+1);
        }
    }
}
// Class to store relevant mesh variables for any shape
public abstract class ProdeduralShape {

    protected Mesh mesh;
    protected Vector3[] vertices;
    protected int[] triangles;
    protected Vector2[] uvs;

    public ProdeduralShape(Mesh mesh) {
        this.mesh = mesh;
    }
}
// Plane class to create plane mesh, inherting from the procedural shape
public class Plane : ProdeduralShape {

    private int sizeX, sizeY;
    // Constructor to set the size, run the super constructor, and create the mesh
    public Plane(Mesh mesh, int sizeX, int sizeY) : base(mesh) {

        this.sizeX = sizeX;
        this.sizeY = sizeY;
        CreateMesh();

    }
    // Create vertices, traignles, scale the mesh, and create the uvs
    private void CreateMesh() {

        CreateVertices();
        CreateTriangles();
        //ScaleMesh((1f/16f));
        CreateUVs();
        // Set the actual mesh variables to the generated ones
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

    }
    // Create vertices for the plane by just looping through the size and creating a new one
    private void CreateVertices() {

        vertices = new Vector3[sizeX * sizeY];

        for (int y = 0; y < sizeY; y++)
        {
            for (int x = 0; x < sizeX; x++)
            {
                vertices[x + y * sizeX] = new Vector3(x, y);
            }
        }
    }
    //This method creates triangles that define the mesh structure.
    // The resulting triangles are stored in the 'triangles' array for rendering purposes.
    private void CreateTriangles() {

        // Calculate the total number of triangles needed based on the mesh size.
        triangles = new int[3 * 2 * (sizeX * sizeY - sizeX - sizeY + 1)];

        // Counter to keep track of the current vertex in the triangles array.
        int triangleVertexCount = 0;

        // Loop through each vertex in the mesh.
        for (int vertex = 0; vertex < sizeX * sizeY - sizeX; vertex++) {

            // Check if the current vertex is not at the right edge of the mesh.
            if (vertex % sizeX != (sizeX - 1)) {

                // Define indices for the vertices of two triangles forming a square.
                int A = vertex;
                int B = A + sizeX;
                int C = B + 1;

                // First triangle (ABC).
                triangles[triangleVertexCount] = A;
                triangles[triangleVertexCount + 1] = B;
                triangles[triangleVertexCount + 2] = C;

                // Move to the next row in the mesh.
                B += 1;
                C = A + 1;

                // Second triangle (ACB).
                triangles[triangleVertexCount + 3] = A;
                triangles[triangleVertexCount + 4] = B;
                triangles[triangleVertexCount + 5] = C;

                // Increment the triangle vertex counter for the next iteration.
                triangleVertexCount += 6;
            }
        }
    }
    // This method creates UV coordinates for each vertex in the mesh.
    // UV coordinates are used to map textures onto the mesh during rendering.
    private void CreateUVs() {

        // Initialize an array to store UV coordinates for each vertex.
        uvs = new Vector2[sizeX * sizeY];

        // Counter to keep track of the current UV index in the uvs array.
        int uvIndexCounter = 0;

        // Loop through each vertex in the vertices array.
        foreach (Vector3 vertex in vertices) {

            // Calculate UV coordinates based on the vertex position and mesh size.
            uvs[uvIndexCounter] = new Vector2(vertex.x / (sizeX - 1), vertex.y / ((sizeY - 1f)));

            // Increment the UV index counter for the next iteration.
            uvIndexCounter++;
        }
    }
    // This method scales the mesh by a certain factor
    private void ScaleMesh(float scaleFactor) {
        // Loop through the vertices and scale them
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] *= scaleFactor;
        }
    }
}
