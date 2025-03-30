using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using TerrainDemo;

public class CubeGeneratorScript : MonoBehaviour
{
    MeshFilter meshFilter;
    [Range(0f, 1f)] public float iso_val = 0.5f; // Isosurface Value, surface that represents points of a constant value
    public bool useLists = true; // Toggle between list-based and array-based storage
    public bool generateChunk = false; // Toggle to generate a chunk of voxels
    public bool useManualCornerValues = true; // Toggle between manual corner values and Perlin noise
    [Range(1, 16)] public int chunkSize = 8; // Define chunk size
    public float perlinScale = 0.1f; // Scale for Perlin noise terrain generation
    public bool smoothTerrain = false; // Toggle for smoothing terrain

    List<Vector3> verticesList = new List<Vector3>();
    List<int> trianglesList = new List<int>();

    Vector3[] verticesArray;
    int[] trianglesArray;
    int vertexCount = 0;
    int triangleCount = 0;

    //Float values for each corner of the cube, these are in the voxel grid
    [Range(0f, 1f)] public float cube_corner_val0 = 0f;
    [Range(0f, 1f)] public float cube_corner_val1 = 0f;
    [Range(0f, 1f)] public float cube_corner_val2 = 0f;
    [Range(0f, 1f)] public float cube_corner_val3 = 0f;
    [Range(0f, 1f)] public float cube_corner_val4 = 0f;
    [Range(0f, 1f)] public float cube_corner_val5 = 0f;
    [Range(0f, 1f)] public float cube_corner_val6 = 0f;
    [Range(0f, 1f)] public float cube_corner_val7 = 0f;

    // Create an array of floats representing each corner of a cube and get the value from our terrainMap.
    float[] cube_corner_vals = new float[8];

    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        ClearMeshData();
        cube_corner_vals = new float[8]; //Set to zero
    }

    // Update is called once per frame
    void Update()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        ClearMeshData();

        if (generateChunk)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float height = Mathf.PerlinNoise(x * perlinScale, z * perlinScale) * chunkSize;
                    for (int y = 0; y < chunkSize; y++)
                    {
                        if (useManualCornerValues)
                        {
                            cube_corner_vals = new float[] { cube_corner_val0, cube_corner_val1, cube_corner_val2, cube_corner_val3, cube_corner_val4, cube_corner_val5, cube_corner_val6, cube_corner_val7 };
                        }
                        else
                        {
                            float terrainValue = y < height ? 1f : 0f;
                            cube_corner_vals = new float[] { terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue };
                        }
                        GenerateVoxel(new Vector3(x, y, z), cube_corner_vals[0]);
                    }
                }
            }
        }
        else
        {
            if (useManualCornerValues)
            {
                cube_corner_vals = new float[] { cube_corner_val0, cube_corner_val1, cube_corner_val2, cube_corner_val3, cube_corner_val4, cube_corner_val5, cube_corner_val6, cube_corner_val7 };
            }
            else
            {
                float terrainValue = Mathf.PerlinNoise(0, 0); // Default Perlin noise for single voxel
                cube_corner_vals = new float[] { terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue };
            }
            GenerateVoxel(Vector3.zero, cube_corner_vals[0]);
        }

        BuildMesh();

        stopwatch.Stop();
        //UnityEngine.Debug.Log($"Execution Time ({(useLists ? "Lists" : "Arrays")}): {stopwatch.ElapsedMilliseconds} ms");
    }

    void GenerateVoxel(Vector3 position, float terrainValue)
    {
        //cube_corner_vals = new float[] { cube_corner_val0, cube_corner_val1, cube_corner_val2, cube_corner_val3, cube_corner_val4, cube_corner_val5, cube_corner_val6, cube_corner_val7 };

        // Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {
            // If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
            // the surface, the bit would look like 00100000, which represents the integer value 32.
            if (cube_corner_vals[i] > iso_val)
                configurationIndex |= 1 << i;
        }
        //UnityEngine.Debug.Log(configurationIndex); //change this to a gui text at some point

        MarchCube(position, configurationIndex);
    }

    // Helper function to find corner index from position
    int CornerIndex(Vector3 cornerPos)
    {
        for (int i = 0; i < 8; i++)
        {
            if (Constants.CornerTable[i] == cornerPos)
                return i;
        }
        return 0; // Default to first corner (safe fallback)
    }

    void MarchCube(Vector3 position, int configIndex)
    {

        // If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
        if (configIndex == 0 || configIndex == 255)
            return;

        // Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
        int edgeIndex = 0;
        for (int i = 0; i < 5; i++)
        {
            for (int p = 0; p < 3; p++)
            {
                if (!useLists)
                    if (edgeIndex >= trianglesArray.Length || edgeIndex >= verticesArray.Length)
                        return; // Prevent out-of-bounds exception

                // Get the current indice. We increment triangleIndex through each loop.
                int indice = Constants.TriangleTable[configIndex, edgeIndex];

                // If the current edgeIndex is -1, there are no more indices and we can exit the function.
                if (indice == -1)
                    return;

                // Get the vertices for the start and end of this edge.
                Vector3 vert1 = position + Constants.EdgeTable[indice, 0];
                Vector3 vert2 = position + Constants.EdgeTable[indice, 1];

                // Get the midpoint of this edge.
                Vector3 vertPosition;
                if (smoothTerrain)
                {
                    // Linear Interpolate to find the edge position
                    // Get the terrain values at either end of our current edge from the cube array created above.
                    float val1 = cube_corner_vals[CornerIndex(Constants.EdgeTable[indice, 0])];
                    float val2 = cube_corner_vals[CornerIndex(Constants.EdgeTable[indice, 1])];

                    // Calculate the difference between the terrain values.
                    float difference = val2 - val1;

                    // If the difference is 0, then the terrain passes through the middle.
                    // Can we delete this check?
                    /*
					if (difference == 0)
						difference = iso_val;
					else
					*/
                    difference = (iso_val - val1) / difference;

                    // Calculate the point along the edge that passes through.
                    vertPosition = vert1 + ((vert2 - vert1) * difference);
                }
                else
                {
                    // Default to center point of the edge
                    vertPosition = (vert1 + vert2) / 2f;
                }
                // Add to our vertices and triangles list and incremement the edgeIndex.
                if (useLists)
                {
                    verticesList.Add(vertPosition);
                    trianglesList.Add(verticesList.Count - 1);
                }
                else
                {
                    if (vertexCount < verticesArray.Length && triangleCount < trianglesArray.Length)
                    {
                        verticesArray[vertexCount] = vertPosition;
                        trianglesArray[triangleCount] = vertexCount;
                        vertexCount++;
                        triangleCount++;
                    }
                }

                edgeIndex++;


            }
        }
    }

    void ClearMeshData()
    {
        if (useLists)
        {
            verticesList.Clear();
            trianglesList.Clear();
        }
        else
        {
            int maxVertices = chunkSize * chunkSize * chunkSize * 15;
            verticesArray = new Vector3[maxVertices]; // Adjust preallocation size
            trianglesArray = new int[maxVertices * 3];
            vertexCount = 0;
            triangleCount = 0;
        }
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = useLists ? verticesList.ToArray() : verticesArray;
        mesh.triangles = useLists ? trianglesList.ToArray() : trianglesArray;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    // Draw Gizmos for each cube corner with grayscale based on value
    void OnDrawGizmos()
    {
        if (cube_corner_vals == null || Constants.CornerTable == null)
            return;

        if (generateChunk)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        Vector3 voxelPosition = new Vector3(x, y, z);
                        for (int i = 0; i < 8; i++)
                        {
                            Vector3 cornerPosition = voxelPosition + Constants.CornerTable[i];
                            float intensity = Mathf.Clamp01(cube_corner_vals[i]);
                            Gizmos.color = new Color(intensity, intensity, intensity);
                            Gizmos.DrawSphere(cornerPosition, 0.1f);
                        }
                    }
                }
            }
        }
        else
        {
            Vector3 position = transform.position;
            for (int i = 0; i < 8; i++)
            {
                Vector3 cornerPosition = position + Constants.CornerTable[i];
                float intensity = Mathf.Clamp01(cube_corner_vals[i]);
                Gizmos.color = new Color(intensity, intensity, intensity);
                Gizmos.DrawSphere(cornerPosition, 0.1f);
            }
        }
    }
}
