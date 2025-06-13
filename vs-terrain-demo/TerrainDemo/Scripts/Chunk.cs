using System.Collections.Generic;
using UnityEngine;
using TerrainDemo;

public class Chunk
{
    #region Privates
    private Vector3Int _chunkPosition;
    private Vector3[] _verticesArray;
    private int[] _trianglesArray;
    private int _vertexCount = 0;
    private int _triangleCount = 0;
    private ChunkOptions _chunkOptions = null;
    private readonly List<Vector3> _verticesList = new List<Vector3>();
    private readonly List<int> _trianglesList = new List<int>();
    private readonly MeshFilter _meshFilter;
    private readonly MeshCollider _meshCollider;
    private readonly MeshRenderer _meshRenderer;
    private float[,,] _terrainMap;
    #endregion

    #region Public
    public readonly GameObject chunkObject;
    private readonly Vector3Int _position;
    #endregion

    public TerrainDemo.Logger MyLogger { get; } = new TerrainDemo.Logger();

    private Chunk() { }

    public Chunk(Vector3Int position, ChunkOptions chunkOptions) //Public Constructor
    {
        _chunkOptions = chunkOptions ?? new ChunkOptions();
        chunkObject = new GameObject
        {
            name = string.Format("Chunk {0}, {1}", position.x, position.z)
        };

        _chunkPosition = position;
        _position = position;
        chunkObject.transform.position = _chunkPosition;

        _meshFilter = chunkObject.AddComponent<MeshFilter>();
        _meshCollider = chunkObject.AddComponent<MeshCollider>();
        _meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        Material defaultMaterial = new Material(Shader.Find("Standard")); // Load the built-in Default-Diffuse material
        _meshRenderer.material = defaultMaterial;

        _terrainMap = new float[chunkOptions.Width + 1, chunkOptions.Height + 1, chunkOptions.Width + 1]; //needs to be plus one or you'll get an index out of range error
    }

    public void SetOptions(ChunkOptions chunkOptions)
    {
        if(chunkOptions != null)
        {
            _chunkOptions = chunkOptions;
        }
    }

    public void Render()
    {
        PopulateTerrainMap(_position, _chunkOptions.WorldSizeInChunks, _chunkOptions.Scale, _chunkOptions.Octaves, _chunkOptions.Persistance, _chunkOptions.Lacunarity);
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
        for (int x = 0; x < _chunkOptions.Width + 1; x++)
        {
            for (int y = 0; y < _chunkOptions.Height + 1; y++)
            {
                for (int z = 0; z < _chunkOptions.Width + 1; z++)
                {
                    if (_chunkOptions.Noise2D)
                    {
                        //Using clamp to bound PerlinNoise as it intends to return a value 0.0f-1.0f but may sometimes be slightly out of that range
                        //Multipying by chunkOptions.Height will return a value in the range of 0-height
                        float thisHeight = GetTerrianHeight(x + _position.x, z + _position.z, scale, octaves, persistance, lacunarity, _chunkOptions.WorldSeed);

                        //y points below thisHeight will be negative (below terrain) and y points above this chunkOptions.Height will be positve and will render 
                        _terrainMap[x, y, z] = (float)y - thisHeight;
                    }
                    else if (_chunkOptions.DebugUseSplineShaping)
                    {
                        //Continentalness, Erosion, Peaks & Valleys Spline Noise Shaping
                        float thisHeight = GetTerrianHeightSpline(x + _position.x, z + _position.z,_chunkOptions.WorldSeed);

                        //>0 = solid, <0 = air
                        Debug.Log(thisHeight);
                        _terrainMap[x, y, z] = (float)y - thisHeight;
                    }
                    else if (!_chunkOptions.Noise2D)
                    {

                        //3D Perlin Noise Function
                        float noiseValue = GetTerrianHeight3D(x + _position.x, y + _position.y, z + _position.z, scale, octaves, persistance, lacunarity, _chunkOptions.WorldSeed);

                        //need to adjust parameters (namely Base Terrain Height) to visualize result. Removed notion of thisHeight which is purely surface level thinking
                        _terrainMap[x, y, z] = noiseValue;
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

        return (float)_chunkOptions.TerrainHeightRange * noiseHeight + _chunkOptions.BaseTerrainHeight;
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

        return (float)_chunkOptions.TerrainHeightRange * noiseHeight + _chunkOptions.BaseTerrainHeight;
        
    }

    //Spline noise shaper with C, E, P&V
    public float GetTerrianHeightSpline(int x, int z, int seed)
    {
        System.Random prng = new System.Random(seed);
        int offsetX = prng.Next(int.MinValue, int.MaxValue); //To be added as an offset to the sampled points
        int offsetZ = prng.Next(int.MinValue, int.MaxValue);

        float splineScale = 0.01f;
        float continentalnessScale = 0.002f;
        float erosionScale = 0.01f;
        float peaksValleysScale = 0.05f;
        float warpStrength = 30f;

        float nx = x * splineScale;
        float nz = z * splineScale;

        // Continentalness: base landmass shape
        float continentalness = Mathf.PerlinNoise(x * continentalnessScale, z * continentalnessScale);
        continentalness = Mathf.SmoothStep(0f, 1f, continentalness);

        // Erosion: smooth vs. jagged control
        float erosion = Mathf.PerlinNoise(x * erosionScale, z * erosionScale);
        erosion = Mathf.Clamp01(erosion);
        float erosionEffect = Mathf.Lerp(1f, 0.3f, erosion);

        // Peaks & Valleys: warped local vertical variation
        float warpX = Mathf.PerlinNoise(x * 0.1f, z * 0.1f) * warpStrength;
        float warpZ = Mathf.PerlinNoise(x * 0.1f + 1000, z * 0.1f + 1000) * warpStrength;
        float pv = Mathf.PerlinNoise((x + warpX) * peaksValleysScale, (z + warpZ) * peaksValleysScale);
        float peaksValleys = Mathf.Sin(pv * Mathf.PI); // wavy pattern
        float applyPV = (continentalness > 0.5f && erosion < 0.4f) ? 1f : 0f;

        float terrainHeight = continentalness * _chunkOptions.BaseTerrainHeight * erosionEffect + peaksValleys * (float)_chunkOptions.TerrainHeightRange * applyPV;

        return terrainHeight;
    }

    void ClearMeshData()
    {
        _verticesList.Clear();
        _trianglesList.Clear();
        _verticesArray = new Vector3[_chunkOptions.Width * _chunkOptions.Width * _chunkOptions.Height * 15]; //max 5 triangles per voxel, 3 points each
        _trianglesArray = new int[_chunkOptions.Width * _chunkOptions.Width * _chunkOptions.Height * 5]; //max 5 triangles per voxel
        _vertexCount = 0;
        _triangleCount = 0;
    }

    void CreateMeshData()
    {
        ClearMeshData();

        //looking at the cubes, not the points, so you only need to loop the _width number and not the _width + 1 numbers
        for (int x = 0; x < _chunkOptions.Width; x++)
        {
            for (int y = 0; y < _chunkOptions.Height; y++)
            {
                for (int z = 0; z < _chunkOptions.Width; z++)
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
        mesh.vertices = _chunkOptions.UseLists ? _verticesList.ToArray() : _verticesArray;
        mesh.triangles = _chunkOptions.UseLists ? _trianglesList.ToArray() : _trianglesArray;
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }

    void MarchCube(Vector3Int position)
    {
        //Sample terrain values at each corner of the cube
        float[] cube = new float[8]; //8 corners in a cube
        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(position + Constants.CornerTableInt[i]);
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
                if (!_chunkOptions.UseLists)
                    if (edgeIndex >= _trianglesArray.Length || edgeIndex >= _verticesArray.Length)
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
                if (_chunkOptions.SmoothTerrain)
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
                    difference = (_chunkOptions.IsoVal - val1) / difference;

                    // Calculate the point along the edge that passes through.
                    vertPosition = vert1 + ((vert2 - vert1) * difference);
                }
                else
                {
                    // Default to center point of the edge
                    vertPosition = (vert1 + vert2) / 2f;
                }

                // Add to our vertices and triangles list and incremement the edgeIndex.
                if (_chunkOptions.UseLists)
                {
                    if (_chunkOptions.FlatShaded)
                    {
                        _verticesList.Add(vertPosition);
                        _trianglesList.Add(_verticesList.Count - 1);
                    }
                    else
                    {
                        _trianglesList.Add(VertListForIndice(vertPosition));
                    }
                }
                else
                {
                    if (_vertexCount < _verticesArray.Length && _triangleCount < _trianglesArray.Length)
                    {
                        _verticesArray[_vertexCount] = vertPosition;
                        _trianglesArray[_triangleCount] = _vertexCount;
                        _vertexCount++;
                        _triangleCount++;
                    }
                }

                edgeIndex++;


            }
        }
    }

    int VertListForIndice(Vector3 vert)
    {

        // Loop through all the vertices currently in the vertices list.
        for (int i = 0; i < _verticesList.Count; i++)
        {

            // If we find a vert that matches ours, then simply return this index.
            if (_verticesList[i] == vert)
                return i;

        }

        // If we didn't find a match, add this vert to the list and return last index.
        _verticesList.Add(vert);
        return _verticesList.Count - 1;

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
        return _terrainMap[point.x, point.y, point.z];
    }

    int GetCubeConfiguration(float[] cube)
    {

        // Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
        int configurationIndex = 0;
        for (int i = 0; i < 8; i++)
        {

            // If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
            // the surface, the bit would look like 00100000, which represents the integer value 32.
            if (cube[i] > _chunkOptions.IsoVal)
                configurationIndex |= 1 << i;

        }

        return configurationIndex;

    }
}
