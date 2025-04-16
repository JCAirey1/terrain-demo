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
    public int ChunkWidth = 16;
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
                Vector3Int chunkPos = new Vector3Int(x * ChunkWidth, 0, z * ChunkWidth);
                var chunk = new Chunk(chunkPos, _options);
                chunk.Render();
                _chunks.Add(chunkPos, chunk);
                _chunks[chunkPos].chunkObject.transform.SetParent(transform); //put chunks under transform of the World Generator object
            }
        }
        Debug.Log(string.Format("{0} x {0} world generated.", (WorldSizeInChunks * ChunkWidth)));
    }

    void ReGenerate()
    {
        foreach(var chunk in _chunks)
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

        _options = newOptions;
        ReGenerate();
    }

    private ChunkOptions GetOptions() {
        return new ChunkOptions
        {
            WorldSizeInChunks = WorldSizeInChunks,
            WorldSeed = WorldSeed,
            ChunkWidth = ChunkWidth,
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