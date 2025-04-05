using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using TerrainDemo;

public class DebugChunkScript : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    public int worldSeed = 1;

    //Perlin Noise Settings
    [Range(0f, 100f)] public float scale = 16f; //for terrain generation
    [Range(1, 10)] public int octaves = 4; //for terrain generation
    public float persistance = 0.3f; //for terrain generation
    public float lacunarity = 2.3f; //for terrain generation

    //Terrain Settings
    public int width = 16;
    public int height = 32;
    [Range(0f, 1f)] public float iso_val = 0.5f; // Isosurface Value, surface that represents points of a constant value
    float[,,] terrainMap; //3D storage for terrain values that we will sample when generating the mesh of type TerrainPoint
    public float BaseTerrainHeight = 16f; //minimum height of terrain before modification (i.e sea level)
    public float TerrainHeightRange = 12f; //the max height above BaseTerrainHeight our terrain will generate to
    Vector3Int _position = new Vector3Int(0, 0, 0);

    //Marching Cube Settings
    public bool smoothTerrain; //Toggle for smoothing terrain
    public bool flatShaded = true; //Toggle for triangles sharing points for rendering
    public bool useLists = true; //Toggle between list-based and array-based storage
    public bool debugChunkWireframe = true; //Draw chunk bounding wireframe when chunk is selected
    public bool debugChunkWireframePersistence = false; //Draw chunk bounding wireframe even when chunk is not selected
    public bool debugChunkVoxelVal = true; //Draw grayscale sphere gizmos when chunk is selected
    public bool debugChunkVoxelValPersistence = false; //Draw grayscale sphere gizmos even when chunk is notselected

    List<Vector3> verticesList = new List<Vector3>();
    List<int> trianglesList = new List<int>();

    Vector3[] verticesArray;
    int[] trianglesArray;
    int vertexCount = 0;
    int triangleCount = 0;

    public TerrainDemo.Logger MyLogger { get; } = new TerrainDemo.Logger();

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            MyLogger.LogInfo("Starting DebugChunkScript");

            meshFilter = GetComponent<MeshFilter>();
            meshCollider = GetComponent<MeshCollider>();
            terrainMap = new float[width + 1, height + 1, width + 1];
            PopulateTerrainMap();
            verticesArray = new Vector3[width * width * height * 15]; //max 5 triangles per voxel, 3 points each
            trianglesArray = new int[width * width * height * 5]; //max 5 triangles per voxel
            CreateMeshData();
        }
        catch (Exception ex)
        {
            MyLogger.LogError(ex);
        }
    }

    // Update is called once per frame
    void Update()
    {
        try {
            terrainMap = new float[width + 1, height + 1, width + 1];
            PopulateTerrainMap();
            ClearMeshData();
            CreateMeshData();
            BuildMesh();
        }
        catch (Exception ex)
        {
            MyLogger.LogError(ex);
        }
    }

    //Basic Perlin Noise sampler
    /*
	public float GetTerrianHeight (int x, int z)
    {
		return (float)TerrainHeightRange * Mathf.Clamp(Mathf.PerlinNoise((float)x / 16f * 1.5f, (float)z / 16f * 1.5f), 0.0f, 1.0f) + BaseTerrainHeight; //the 16f and 1.5f are made up coefficients
	}
	*/

    // The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger than the width/height of our mesh.
    void PopulateTerrainMap()
    {
        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    //Using clamp to bound PerlinNoise as it intends to return a value 0.0f-1.0f but may sometimes be slightly out of that range
                    //Multipying by height will return a value in the range of 0-height
                    float thisHeight = GetTerrianHeight(x + _position.x, z + _position.z, scale, octaves, persistance, lacunarity, worldSeed);
                    //float thisHeight = (float)height * Mathf.PerlinNoise((float)x / 16f * 1.5f + 0.001f, (float)z / 16f * 1.5f + 0.001f);
                    //float thisHeight = (float)height * Mathf.PerlinNoise((float)x / scale * 1.5f + 0.001f, (float)z / scale * 1.5f + 0.001f);

                    //y points below thisHeight will be negative (below terrain) and y points above this Height will be positve and will render 
                    terrainMap[x, y, z] = (float)y - thisHeight;
                }
            }
        }
    }

    //Configurable Perlin Noise sampler
    public float GetTerrianHeight(int x, int z, float scale, int octaves, float persistance, float lacunarity, int worldSeed)
    {
        System.Random prng = new System.Random(worldSeed);
        int offsetX = prng.Next(int.MinValue, int.MaxValue); //To be added as an offset to the sampled points
        int offsetZ = prng.Next(int.MinValue, int.MaxValue);

        float amplitude = 1;
        float frequency = 1.5f;
        float noiseHeight = 0;
        float perlinValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / scale * frequency + (float)worldSeed; //not working with offsetX, using worldSeed directly for now
            float sampleZ = z / scale * frequency + (float)worldSeed;

            perlinValue = Mathf.Clamp(Mathf.PerlinNoise(sampleX, sampleZ), 0.0f, 1.0f);
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return (float)TerrainHeightRange * noiseHeight + BaseTerrainHeight;
    }

    void ClearMeshData()
    {
        verticesList.Clear();
        trianglesList.Clear();
        verticesArray = new Vector3[width * width * height * 15]; //max 5 triangles per voxel, 3 points each
        trianglesArray = new int[width * width * height * 5]; //max 5 triangles per voxel
        vertexCount = 0;
        triangleCount = 0;
    }

    void CreateMeshData()
    {
        ClearMeshData();

        //looking at the cubes, not the points, so you only need to loop the width number and not the width + 1 numbers
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    MarchCube(new Vector3Int(x, y, z));
                }
            }
        }

        BuildMesh();
    }

    void BuildMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = useLists ? verticesList.ToArray() : verticesArray;
        mesh.triangles = useLists ? trianglesList.ToArray() : trianglesArray;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    void MarchCube(Vector3Int position)
    {
        //Sample terrain values at each corner of the cube
        float[] cube = new float[8]; //8 corners in a cube
        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(position + CornerTable[i]);
        }

        // Get the configuration index of this cube.
        int configIndex = GetCubeConfiguration(cube);

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
                    {
                        return; // Prevent out-of-bounds exception
                    }

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
                    float val1 = cube[CornerIndex(Constants.EdgeTable[indice, 0])];
                    float val2 = cube[CornerIndex(Constants.EdgeTable[indice, 1])];

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
                    if (flatShaded)
                    {
                        verticesList.Add(vertPosition);
                        trianglesList.Add(verticesList.Count - 1);
                    }
                    else
                    {
                        trianglesList.Add(VertListForIndice(vertPosition));
                    }
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

    int VertListForIndice(Vector3 vert)
    {

        // Loop through all the vertices currently in the vertices list.
        for (int i = 0; i < verticesList.Count; i++)
        {

            // If we find a vert that matches ours, then simply return this index.
            if (verticesList[i] == vert)
                return i;

        }

        // If we didn't find a match, add this vert to the list and return last index.
        verticesList.Add(vert);
        return verticesList.Count - 1;

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

    float SampleTerrain(Vector3Int point)
    {
        return terrainMap[point.x, point.y, point.z];
    }

    int GetCubeConfiguration(float[] cube)
    {

        // Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {

            // If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
            // the surface, the bit would look like 00100000, which represents the integer value 32.
            if (cube[i] > iso_val)
                configurationIndex |= 1 << i;

        }

        return configurationIndex;

    }

    // Draw Gizmos for each cube corner with grayscale based on value

    void OnDrawGizmos()
    {
        if (debugChunkWireframePersistence)
        {
            //Draw even when object is not selected in scene view
            DrawChunkBounds();
        }

        if (debugChunkVoxelValPersistence)
        {
            //Draw even when object is not selected in scene view
            DrawChunkVoxelVal();
        }
    }


    void OnDrawGizmosSelected()
    {

        if (debugChunkWireframe) //Draw bounds when selected
        {
            DrawChunkBounds();
        }
        if (debugChunkVoxelVal) //Draw spheres when selected
        {
            DrawChunkVoxelVal();
        }
    }

    //Debug grayscale spheres on voxel coordinates based on terrainMap array values
    void DrawChunkVoxelVal()
    {
        if (terrainMap == null)
            return;

        Gizmos.color = Color.white;

        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    float val = terrainMap[x, y, z];

                    // Convert the scalar value to a grayscale color.
                    float intensity = Mathf.Clamp01(val); // assumes values roughly between 0–1
                    Gizmos.color = new Color(intensity, intensity, intensity);

                    // Offset by the chunk's world position
                    Vector3 position = new Vector3(x, y, z) + transform.position;
                    Gizmos.DrawSphere(position, 0.1f); // Small sphere at the grid point
                }
            }
        }
    }

    //Debug wireframe cube around chunk boundary
    void DrawChunkBounds()
    {
        Gizmos.color = Color.yellow;

        // Define the size and position of the bounding box
        Vector3 center = new Vector3(width / 2f, height / 2f, width / 2f) + transform.position;
        Vector3 size = new Vector3(width, height, width);

        Gizmos.DrawWireCube(center, size);
    }

    Vector3Int[] CornerTable = new Vector3Int[8] {

        new Vector3Int(0, 0, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(1, 1, 1),
        new Vector3Int(0, 1, 1)

    };
}