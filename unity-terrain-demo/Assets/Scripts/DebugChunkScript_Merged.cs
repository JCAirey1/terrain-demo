using System;
using TerrainDemo;
using UnityEngine;

public class DebugChunkScript_Merged : MonoBehaviour
{
    public int worldSeed = 1;

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
    public float BaseTerrainHeight = 16f; //minimum height of terrain before modification (i.e sea level)
    public float TerrainHeightRange = 12f; //the max height above BaseTerrainHeight our terrain will generate to

    //Marching Cube Settings
    public bool smoothTerrain; //Toggle for smoothing terrain
    public bool flatShaded = true; //Toggle for triangles sharing points for rendering
    public bool useLists = true; //Toggle between list-based and array-based storage
    public bool debugChunkWireframe = true; //Draw chunk bounding wireframe when chunk is selected
    public bool debugChunkWireframePersistence = false; //Draw chunk bounding wireframe even when chunk is not selected
    public bool debugChunkVoxelVal = true; //Draw grayscale sphere gizmos when chunk is selected
    public bool debugChunkVoxelValPersistence = false; //Draw grayscale sphere gizmos even when chunk is notselected

    private Chunk _chunk;
    private Vector3Int _position = new(0, 0, 0);

    public TerrainLogger Logger { get; } = new TerrainLogger();

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            TerrainLogger.Log("Starting DebugChunkScript");

            var options = GetOptions();

            _chunk = new Chunk(_position, options);
            _chunk.Render();
        }
        catch (Exception ex)
        {
            TerrainLogger.Error(ex.ToString());
        }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            var options = GetOptions();

            _chunk.ReRender(options);
        }
        catch (Exception ex)
        {
            TerrainLogger.Error(ex.ToString());
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

    void DrawChunkVoxelVal()
    {
        Gizmos.color = Color.white;

        if(_chunk == null || _chunk.TerrainMap == null)
        {
            TerrainLogger.Warn("Chunk or TerrainMap is null, skipping voxel value drawing.");
            return;
        }

        for (int x = 0; x < width + 1; x++)
        {
            for (int y = 0; y < height + 1; y++)
            {
                for (int z = 0; z < width + 1; z++)
                {
                    float val = _chunk.TerrainMap[x, y, z] / 255f;

                    // Convert the scalar value to a grayscale color.
                    float intensity = Mathf.Clamp01(val); // assumes values roughly between 0–1
                    Gizmos.color = new Color(intensity, intensity, intensity);

                    // Offset by the chunk's world position
                    var position = new Vector3(x, y, z) + transform.position;
                    Gizmos.DrawSphere(position, 0.1f); // Small sphere at the grid point
                }
            }
        }
    }

    private void DrawChunkBounds()
    {
        Gizmos.color = Color.yellow;

        // Define the size and position of the bounding box
        var center = new Vector3(width / 2f, height / 2f, width / 2f) + transform.position;
        var size = new Vector3(width, height, width);

        Gizmos.DrawWireCube(center, size);
    }

    private ChunkOptions GetOptions()
    {
        return new ChunkOptions
        {
            BaseTerrainHeight = BaseTerrainHeight,
            DebugChunkVoxelVal = debugChunkVoxelVal,
            DebugChunkVoxelValPersistence = debugChunkVoxelValPersistence,
            DebugChunkWireframe = debugChunkWireframe,
            DebugChunkWireframePersistence = debugChunkWireframePersistence,
            DebugSaveNoiseMaps = false, // Set to true if you want to save noise maps
            DebugUseSplineShaping = false,  // Set to true if you want to use spline shaping noise maps
            FlatShaded = flatShaded,
            Height = height,
            IsoVal = iso_val,
            Lacunarity = lacunarity,
            Noise2D = noise2D,
            Octaves = octaves,
            Persistance = persistance,
            Scale = scale,
            SmoothTerrain = smoothTerrain,
            TerrainHeightRange = TerrainHeightRange,
            UseLists = useLists,
            Width = width,
            WorldSeed = worldSeed,
            WorldSizeInChunks = 1, // Assuming a single chunk for debugging    
        };
    }
}