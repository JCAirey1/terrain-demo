using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainDemo;

public class DebugWorldGenerator : MonoBehaviour
{
    #region Privates
    private readonly Dictionary<Vector3Int, Chunk> _chunks = new Dictionary<Vector3Int, Chunk>(); //Storage for currently loaded Chunks
    private ChunkOptions _options = null;
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
    #endregion

    void Start()
    {
        _options = GetOptions();
        Generate();
    }

    void Generate()
    {
        _chunks.Clear();

        for (int x = 0; x < WorldSizeInChunks; x++)
        {
            for (int z = 0; z < WorldSizeInChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * _options.Width, 0, z * _options.Width);
                var chunk = new Chunk(chunkPos, _options);
                chunk.Render();
                _chunks.Add(chunkPos, chunk);
                _chunks[chunkPos].chunkObject.transform.SetParent(transform); //put chunks under transform of the World Generator object
            }
        }
        Debug.Log(string.Format("{0} x {0} world generated.", (WorldSizeInChunks * _options.Width)));
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
            for (int x = 0; x < newSize; x++)
            {
                for (int z = 0; z < newSize; z++)
                {
                    // Only add if beyond the old bounds
                    if (x >= oldSize || z >= oldSize)
                    {
                        Vector3Int chunkPos = new Vector3Int(x * Width, 0, z * Width);
                        if (!_chunks.ContainsKey(chunkPos))
                        {
                            AddChunk(chunkPos);
                        }
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
            DebugChunkVoxelValPersistence = DebugChunkVoxelValPersistence
        };
    }
}