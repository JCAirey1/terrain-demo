using System.Collections.Generic;
using UnityEngine;
using TerrainDemo;
using System.IO;

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

    float[,] continentalnessMap;
    float[,] erosionMap;
    float[,] peaksValleysMap;
    float[,] peaksValleysBoolMap;
    float[,] octave1Map;

    #endregion

    #region Public
    public readonly GameObject chunkObject;
    private readonly Vector3Int _position;
    #endregion

    public TerrainLogger Logger { get; } = new TerrainLogger();

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
        if (chunkOptions != null)
        {
            _chunkOptions = chunkOptions;
        }
    }

    public void Render()
    {
        PopulateTerrainMap();
        CreateMeshData();
        BuildMesh();
    }

    public void ReRender()
    {
        CreateMeshData();
        BuildMesh();
    }

    // The data points for terrain are stored at the corners of our "cubes", so the terrainMap needs to be 1 larger than the width/height of our mesh.
    void PopulateTerrainMap()
    {
        continentalnessMap = new float[_chunkOptions.Width + 1, _chunkOptions.Width + 1];
        erosionMap = new float[_chunkOptions.Width + 1, _chunkOptions.Width + 1];
        peaksValleysMap = new float[_chunkOptions.Width + 1, _chunkOptions.Width + 1];
        peaksValleysBoolMap = new float[_chunkOptions.Width + 1, _chunkOptions.Width + 1];
        octave1Map = new float[_chunkOptions.Width + 1, _chunkOptions.Width + 1];

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
                        float thisHeight = GetTerrianHeight(x, _position.x, z, _position.z);

                        //y points below thisHeight will be negative (below terrain) and y points above this chunkOptions.Height will be positve and will render 
                        _terrainMap[x, y, z] = (float)y - thisHeight;
                    }
                    else if (_chunkOptions.DebugUseSplineShaping)
                    {
                        //Continentalness, Erosion, Peaks & Valleys Spline Noise Shaping
                        float thisHeight = GetTerrianHeightSpline(x, _position.x, z, _position.z);

                        //>0 = solid, <0 = air
                        //Debug.Log(thisHeight);
                        _terrainMap[x, y, z] = (float)y - thisHeight;
                    }
                    else if (!_chunkOptions.Noise2D)
                    {

                        //3D Perlin Noise Function
                        float noiseValue = GetTerrianHeight3D(x + _position.x, y + _position.y, z + _position.z);

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

        //SaveNoiseMapAsImage(continentalnessMap, "ContinentalnessMap");
        //SaveNoiseMapAsImage(erosionMap, "ErosionMap");
        //SaveNoiseMapAsImage(peaksValleysMap, "PeaksValleysMap");
    }

    //Configurable Perlin Noise sampler
    public float GetTerrianHeight(int voxel_x, int world_position_x, int voxel_z, int world_position_z)
    {
        //System.Random prng = new System.Random(seed);
        //int offsetX = prng.Next(int.MinValue, int.MaxValue); //To be added as an offset to the sampled points
        //int offsetZ = prng.Next(int.MinValue, int.MaxValue);

        float amplitude = 1;
        float frequency = 1.5f;
        float noiseHeight = 0;
        float perlinValue = 0;

        int x = voxel_x + world_position_x;
        int z = voxel_z + world_position_z;

        for (int i = 0; i < _chunkOptions.Octaves; i++)
        {
            float sampleX = x / _chunkOptions.Scale * frequency + (float)_chunkOptions.WorldSeed; //not working with offsetX, using worldSeed directly for now
            float sampleZ = z / _chunkOptions.Scale * frequency + (float)_chunkOptions.WorldSeed;

            perlinValue = Mathf.Clamp(Mathf.PerlinNoise(sampleX, sampleZ), 0.0f, 1.0f);
            noiseHeight += perlinValue * amplitude;

            amplitude *= _chunkOptions.Persistance;
            frequency *= _chunkOptions.Lacunarity;

            if (i == 1)
            {
                octave1Map[voxel_x, voxel_z] = perlinValue;
            }
        }
        return (float)_chunkOptions.TerrainHeightRange * noiseHeight + _chunkOptions.BaseTerrainHeight;
    }

    //3D Perlin Noise Alternative Noise Function
    public float GetTerrianHeight3D(int x, int y, int z)
    {
        //System.Random prng = new System.Random(_chunkOptions.WorldSeed);
        //int offsetX = prng.Next(int.MinValue, int.MaxValue); //To be added as an offset to the sampled points
        //int offsetY = prng.Next(int.MinValue, int.MaxValue);
        //int offsetZ = prng.Next(int.MinValue, int.MaxValue);

        float amplitude = 1f;
        float frequency = 1.5f;
        float noiseHeight = 0f;

        for (int i = 0; i < _chunkOptions.Octaves; i++)
        {
            float sampleX = x / _chunkOptions.Scale * frequency + (float)_chunkOptions.WorldSeed; //not working with offsetX, using worldSeed directly for now
            float sampleY = y / _chunkOptions.Scale * frequency + (float)_chunkOptions.WorldSeed;
            float sampleZ = z / _chunkOptions.Scale * frequency + (float)_chunkOptions.WorldSeed;

            // Fake 3D Perlin noise by combining 2D noise samples
            float perlinXY = Mathf.PerlinNoise(sampleX, sampleY);
            float perlinXZ = Mathf.PerlinNoise(sampleX, sampleZ);
            float perlinYZ = Mathf.PerlinNoise(sampleY, sampleZ);

            // Average the samples
            float perlinValue = (perlinXY + perlinXZ + perlinYZ) / 3f;

            noiseHeight += perlinValue * amplitude;

            amplitude *= _chunkOptions.Persistance;
            frequency *= _chunkOptions.Lacunarity;
        }

        return (float)_chunkOptions.TerrainHeightRange * noiseHeight + _chunkOptions.BaseTerrainHeight;
    }

    //Spline noise shaper with C, E, P&V
    public float GetTerrianHeightSpline(int voxel_x, int world_position_x, int voxel_z, int world_position_z)
    {
        //System.Random prng = new System.Random(_chunkOptions.WorldSeed);
        //int offsetX = prng.Next(int.MinValue, int.MaxValue); //To be added as an offset to the sampled points
        //int offsetZ = prng.Next(int.MinValue, int.MaxValue);

        float splineScale = 0.01f;
        float continentalnessScale = 0.01f;
        float erosionScale = 0.02f;
        float peaksValleysScale = 0.05f;
        float warpStrength = 30f;

        int x = voxel_x + world_position_x;
        int z = voxel_z + world_position_z;

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

        //float terrainHeight = continentalness * _chunkOptions.BaseTerrainHeight * erosionEffect + peaksValleys * (float)_chunkOptions.TerrainHeightRange * applyPV;
        float terrainHeight = continentalness * _chunkOptions.BaseTerrainHeight * erosionEffect * (1 + (peaksValleys-0.5f) * applyPV);

        try
        {
            continentalnessMap[voxel_x, voxel_z] = continentalness;
            erosionMap[voxel_x, voxel_z] = erosion;
            peaksValleysMap[voxel_x, voxel_z] = peaksValleys;
            peaksValleysBoolMap[voxel_x, voxel_z] = applyPV;
        }
        catch { }

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
        int configIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            cube[i] = SampleTerrain(position + Constants.CornerTableInt[i]);
            
            if (cube[i] > _chunkOptions.IsoVal)
                configIndex |= 1 << i;
        }

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

    public float[,,] TerrainMap
    {
        get => _terrainMap;
    }
    
    public float[,] GetLocalContinentalnessMap()
    {
        return continentalnessMap;
    }
    public float[,] GetLocalErosionMap()
    {
        return erosionMap;
    }
    public float[,] GetLocalPeaksValleysMap()
    {
        return peaksValleysMap;
    }
    public float[,] GetLocalPeaksValleysBoolMap()
    {
        return peaksValleysBoolMap;
    }
    public float[,] GetLocalOctave1Map()
    {
        return octave1Map;
    }

    //helper method to save noise maps to local png file for inspection
    void SaveNoiseMapAsImage(float[,] noiseMap, string name)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        Texture2D texture = new Texture2D(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = Mathf.Clamp01(noiseMap[x, y]);
                Color color = new Color(value, value, value);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        byte[] pngData = null;
        pngData = texture.EncodeToPNG();

        string folderPath = Path.Combine(Application.dataPath, "NoiseDebug");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, name + ".png");
        File.WriteAllBytes(filePath, pngData);
        Debug.Log("Saved " + name + " to " + filePath);
    }
}
