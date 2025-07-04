using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TerrainDemo;

public class DebugWorldGenerator : MonoBehaviour
{
    #region Privates
    private readonly Dictionary<Vector3Int, Chunk> _chunks = new Dictionary<Vector3Int, Chunk>(); //Storage for currently loaded Chunks
    private ChunkOptions _options = null;
    private float[,] globalContinentalnessMap;
    private float[,] globalErosionMap;
    private float[,] globalPeaksValleysMap;
    private float[,] globalPeaksValleysBoolMap;
    private float[,] globalOctave1Map;
    #endregion

    #region DebugProperties
    public int WorldSizeInChunks = 2;
    public int WorldSeed = 1;
    public bool Noise2D = true;
    public float Scale = 16f;
    [Range(1, 10)]
    public int Octaves = 4;
    public float Persistance = 0.3f;
    public float Lacunarity = 2.3f;
    public int Width = 16;
    public int Height = 32;
    [Range(0f, 1f)]
    public float IsoVal = 0.5f;
    public float BaseTerrainHeight = 16f;
    public float TerrainHeightRange = 12f;
    public bool SmoothTerrain;
    public bool FlatShaded = true;
    public bool UseLists = true;
    public bool DebugChunkWireframe = true;
    public bool DebugChunkWireframePersistence = false;
    public bool DebugChunkVoxelVal = true;
    public bool DebugChunkVoxelValPersistence = false;
    public bool DebugUseSplineShaping = true;
    public bool DebugSaveNoiseMaps = false;
    #endregion

    void Start()
    {
        _options = GetOptions();
        Generate();
    }

    
    void Generate()
    {
        _chunks.Clear();

        int worldPixelSize = _options.Width * WorldSizeInChunks + 1; // +1 for overlap
        globalContinentalnessMap = new float[worldPixelSize, worldPixelSize];
        globalErosionMap = new float[worldPixelSize, worldPixelSize];
        globalPeaksValleysMap = new float[worldPixelSize, worldPixelSize];
        globalPeaksValleysBoolMap = new float[worldPixelSize, worldPixelSize];
        globalOctave1Map = new float[worldPixelSize, worldPixelSize];

        for (int x = 0; x < WorldSizeInChunks; x++)
        {
            for (int z = 0; z < WorldSizeInChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * _options.Width, 0, z * _options.Width);
                var chunk = new Chunk(chunkPos, _options);
                chunk.Render();
                _chunks.Add(chunkPos, chunk);
                _chunks[chunkPos].chunkObject.transform.SetParent(transform); //put chunks under transform of the World Generator object

                WriteChunkNoiseToGlobalMap(chunk.GetLocalContinentalnessMap(), globalContinentalnessMap, x, z, _options.Width);
                WriteChunkNoiseToGlobalMap(chunk.GetLocalErosionMap(), globalErosionMap, x, z, _options.Width);
                WriteChunkNoiseToGlobalMap(chunk.GetLocalPeaksValleysMap(), globalPeaksValleysMap, x, z, _options.Width);
                WriteChunkNoiseToGlobalMap(chunk.GetLocalPeaksValleysBoolMap(), globalPeaksValleysBoolMap, x, z, _options.Width);
                WriteChunkNoiseToGlobalMap(chunk.GetLocalOctave1Map(), globalOctave1Map, x, z, _options.Width);
            }
        }
        Debug.Log(string.Format("{0} x {0} world generated.", (WorldSizeInChunks * _options.Width)));
        SaveNoiseMapAsImage(globalContinentalnessMap, "GlobalContinentalnessMap");
        SaveNoiseMapAsImage(globalErosionMap, "GlobalErosionMap");
        SaveNoiseMapAsImage(globalPeaksValleysMap, "GlobalPeaksValleysMap");
        SaveNoiseMapAsImage(globalPeaksValleysBoolMap, "GlobalPeaksValleysBoolMap");
        SaveNoiseMapAsImage(globalOctave1Map, "GlobalOctave1Map");
    }

    //creates a new chunk at a given Vector3Int position if it doesn't already exist:
    public void AddChunk(Vector3Int chunkPos)
    {
        if (_chunks.ContainsKey(chunkPos))
            return;

        var chunk = new Chunk(chunkPos, _options);
        chunk.Render();
        _chunks.Add(chunkPos, chunk);
        chunk.chunkObject.transform.SetParent(transform); // Parent under world
    }

    //safely removes a chunk and destroys its GameObject:
    public void RemoveChunk(Vector3Int chunkPos)
    {
        if (_chunks.TryGetValue(chunkPos, out Chunk chunk))
        {
            if (chunk.chunkObject != null)
            {
                DestroyImmediate(chunk.chunkObject);
            }
            _chunks.Remove(chunkPos);
        }
    }

    void ReGenerate()
    {
        // Check if the size/dimensions have changed
        if (_options.WorldSizeInChunks != WorldSizeInChunks || 
            _options.Width != Width || 
            _options.Height != Height)
        {
            Debug.Log("World dimensions changed. Redrawing the world...");
            _options = GetOptions(); // Update options
            Generate(); // Completely redraw the world
            return;
        }

        // Otherwise, update and re-render existing chunks
        foreach (var chunk in _chunks)
        {
            chunk.Value.SetOptions(_options);
            chunk.Value.ReRender();
        }
    }

    void Update()
    {
        var newOptions = GetOptions();

        if(newOptions.Equals(_options)) {
            return;
        }

        Debug.Log("props updated");

        int oldSize = _options.WorldSizeInChunks;
        int newSize = newOptions.WorldSizeInChunks;

        // If size increased → add new chunks
        if (newSize > oldSize)
        {
            // Add new border columns (right side)
            for (int x = oldSize; x < newSize; x++)
            {
                for (int z = 0; z < newSize; z++)
                {
                    Vector3Int chunkPos = new Vector3Int(x * Width, 0, z * Width);
                    if (!_chunks.ContainsKey(chunkPos))
                    {
                        AddChunk(chunkPos);
                    }
                }
            }

            // Add new border rows (bottom side), excluding the corner already added
            for (int x = 0; x < oldSize; x++)
            {
                for (int z = oldSize; z < newSize; z++)
                {
                    Vector3Int chunkPos = new Vector3Int(x * Width, 0, z * Width);
                    if (!_chunks.ContainsKey(chunkPos))
                    {
                        AddChunk(chunkPos);
                    }
                }
            }

            Debug.Log($"Expanded world from {oldSize}x{oldSize} to {newSize}x{newSize} chunks.");
            _options = newOptions;
            return;
        }

        // If size decreased → remove out-of-bound chunks
        if (newSize < oldSize)
        {
            List<Vector3Int> toRemove = new List<Vector3Int>();

            foreach (var kvp in _chunks)
            {
                Vector3Int pos = kvp.Key;
                int xChunk = pos.x / Width;
                int zChunk = pos.z / Width;

                if (xChunk >= newSize || zChunk >= newSize)
                {
                    toRemove.Add(pos);
                }
            }

            foreach (var pos in toRemove)
            {
                RemoveChunk(pos);
            }

            Debug.Log($"Shrunk world from {oldSize}x{oldSize} to {newSize}x{newSize} chunks.");
            _options = newOptions;
            return;
        }

        _options = newOptions;
        ReGenerate();
    }

    public void WriteChunkNoiseToGlobalMap(float[,] localMap, float[,] globalMap, int chunkX, int chunkZ, int chunkSize)
    {
        int globalX = 0;
        int globalZ = 0;

        for (int x = 0; x <= chunkSize; x++) // include edge
        {
            Debug.Log($"GlobalX {globalX}");
            for (int z = 0; z <= chunkSize; z++)
            {
                globalX = chunkX * chunkSize + x;
                globalZ = chunkZ * chunkSize + z;

                // (Not Protecting bounds)
                //globalMap[globalX, globalZ] = localMap[x, z];

                // Protect bounds
                //*
                if (globalX < globalMap.GetLength(0) && globalZ < globalMap.GetLength(1))
                {
                    globalMap[globalX, globalZ] = localMap[x, z];
                }
                //*/
            }
        }
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
        byte[] pngData = texture.EncodeToPNG();

        string folderPath = Path.Combine(Application.dataPath, "../Logs/NoiseDebug");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, name + ".png");
        File.WriteAllBytes(filePath, pngData);
        Debug.Log("Saved " + name + " to " + filePath);
    }

    private ChunkOptions GetOptions() {
        return new ChunkOptions
        {
            WorldSizeInChunks = WorldSizeInChunks,
            WorldSeed = WorldSeed,
            Noise2D = Noise2D,
            Scale = Scale,
            Octaves = Octaves,
            Persistance = Persistance,
            Lacunarity = Lacunarity,
            Width = Width,
            Height = Height,
            IsoVal = IsoVal,
            BaseTerrainHeight = BaseTerrainHeight,
            TerrainHeightRange = TerrainHeightRange,
            SmoothTerrain = SmoothTerrain,
            FlatShaded = FlatShaded,
            UseLists = UseLists,
            DebugChunkWireframe = DebugChunkWireframe,
            DebugChunkWireframePersistence = DebugChunkWireframePersistence,
            DebugChunkVoxelVal = DebugChunkVoxelVal,
            DebugChunkVoxelValPersistence = DebugChunkVoxelValPersistence,
            DebugUseSplineShaping = DebugUseSplineShaping,
            DebugSaveNoiseMaps = DebugSaveNoiseMaps
        };
    }
}