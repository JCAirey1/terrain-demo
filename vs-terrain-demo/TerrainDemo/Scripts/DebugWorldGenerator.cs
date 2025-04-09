using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerrainDemo;

public class DebugWorldGenerator : MonoBehaviour
{
    public int WorldSizeInChunks = 2;
    public int WorldSeed = 1;
    public int chunkWidth = 16;
    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>(); //Storage for currently loaded Chunks

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        for (int x = 0; x < WorldSizeInChunks; x++)
        {
            for (int z = 0; z < WorldSizeInChunks; z++)
            {
                Vector3Int chunkPos = new Vector3Int(x * chunkWidth, 0, z * chunkWidth);
                var chunk = new Chunk(chunkPos, WorldSeed, WorldSizeInChunks);
                chunk.smoothTerrain = true;
                chunk.Render();
                chunks.Add(chunkPos, chunk);
                chunks[chunkPos].chunkObject.transform.SetParent(transform); //put chunks under transform of the World Generator object
            }
        }
        Debug.Log(string.Format("{0} x {0} world generated.", (WorldSizeInChunks * chunkWidth)));
    }
}