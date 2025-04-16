using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainDemo;


public class Chunk
{
    public readonly GameObject chunkObject;
    private readonly Vector3Int _position;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    MeshRenderer meshRenderer;
    Vector3Int chunkPosition;
    public int chunkSeed = 1;
    private int _worldSizeInChunks;

    //Perlin Noise Settings
    public bool noise2D = true; //True for 2D Perlin Noise, false for 3D Perlin Noise
    public float scale = 16f; //for terrain generation
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

    //Marching Cube Settings
    public bool smoothTerrain { get; set; } = false; //Toggle for smoothing terrain {ability to set from outside of class}
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

    private Chunk()
    {

    }

    public Chunk(Vector3Int position, int chunkSeed, int worldSizeInChunks) //Public Constructor
    {
        Debug.Log("Chunk init");

        chunkObject = new GameObject();
        chunkObject.name = string.Format("Chunk {0}, {1}", position.x, position.z);
        chunkPosition = position;
        _position = position;
        chunkObject.transform.position = chunkPosition;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshCollider = chunkObject.AddComponent<MeshCollider>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        Material defaultMaterial = new Material(Shader.Find("Standard")); // Load the built-in Default-Diffuse material
        meshRenderer.material = defaultMaterial;

        terrainMap = new float[width + 1, height + 1, width + 1]; //needs to be plus one or you'll get an index out of range error
        _worldSizeInChunks = worldSizeInChunks;
    }

    public void Render()
    {
        PopulateTerrainMap(_position, _worldSizeInChunks, scale, octaves, persistance, lacunarity);
        CreateMeshData();
        BuildMesh();
    }

    public void ReRender()
    {
        CreateMeshData();
        BuildMesh();
    }

    // The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger than the width/height of our mesh.
    void PopulateTerrainMap(Vector3Int _position, int WorldSizeInChunks, float scale, int octaves, float persistance, float lacunarity)
    {
        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    if (noise2D)
                    {
                        //Using clamp to bound PerlinNoise as it intends to return a value 0.0f-1.0f but may sometimes be slightly out of that range
                        //Multipying by height will return a value in the range of 0-height
                        float thisHeight = GetTerrianHeight(x + _position.x, z + _position.z, scale, octaves, persistance, lacunarity, chunkSeed);

                        //y points below thisHeight will be negative (below terrain) and y points above this Height will be positve and will render 
                        terrainMap[x, y, z] = (float)y - thisHeight;
                    }
                    else if (!noise2D)
                    {

                        //3D Perlin Noise Function
                        float noiseValue = GetTerrianHeight3D(x + _position.x, y + _position.y, z + _position.z, scale, octaves, persistance, lacunarity, chunkSeed);

                        //need to adjust parameters (namely Base Terrain Height) to visualize result. Removed notion of thisHeight which is purely surface level thinking
                        terrainMap[x, y, z] = noiseValue;
                    }
                    else
                    {
                        Debug.Log("UNEXPECTED ERROR WITH NOISE FUNCITON SELECTION");
                        return;
                    }
                }
            }
        }
    }

    //Configurable Perlin Noise sampler
    public float GetTerrianHeight(int x, int z, float scale, int octaves, float persistance, float lacunarity, int seed)
    {
        System.Random prng = new System.Random(seed);
        int offsetX = prng.Next(int.MinValue, int.MaxValue); //To be added as an offset to the sampled points
        int offsetZ = prng.Next(int.MinValue, int.MaxValue);

        float amplitude = 1;
        float frequency = 1.5f;
        float noiseHeight = 0;
        float perlinValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / scale * frequency + (float)seed; //not working with offsetX, using worldSeed directly for now
            float sampleZ = z / scale * frequency + (float)seed;

            perlinValue = Mathf.Clamp(Mathf.PerlinNoise(sampleX, sampleZ), 0.0f, 1.0f);
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistance;
            frequency *= lacunarity;
        }

        return (float)TerrainHeightRange * noiseHeight + BaseTerrainHeight;
    }

    //3D Perlin Noise Alternative Noise Function
    public float GetTerrianHeight3D(int x, int y, int z, float scale, int octaves, float persistance, float lacunarity, int seed)
    {
        System.Random prng = new System.Random(seed);
        int offsetX = prng.Next(int.MinValue, int.MaxValue); //To be added as an offset to the sampled points
        int offsetY = prng.Next(int.MinValue, int.MaxValue);
        int offsetZ = prng.Next(int.MinValue, int.MaxValue);

        float amplitude = 1f;
        float frequency = 1.5f;
        float noiseHeight = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / scale * frequency + (float)seed; //not working with offsetX, using worldSeed directly for now
            float sampleY = y / scale * frequency + (float)seed;
            float sampleZ = z / scale * frequency + (float)seed;

            // Fake 3D Perlin noise by combining 2D noise samples
            float perlinXY = Mathf.PerlinNoise(sampleX, sampleY);
            float perlinXZ = Mathf.PerlinNoise(sampleX, sampleZ);
            float perlinYZ = Mathf.PerlinNoise(sampleY, sampleZ);

            // Average the samples
            float perlinValue = (perlinXY + perlinXZ + perlinYZ) / 3f;

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
