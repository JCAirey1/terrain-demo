using System;
using TerrainDemo;
using UnityEngine;
using UnityEngine.UIElements;

public class DebugCubeScript_Merged : MonoBehaviour
{
    [Range(0f, 1f)] public float iso_val = 0.5f; // Isosurface Value, surface that represents points of a constant value
    public bool useLists = true; // Toggle between list-based and array-based storage
    public bool useManualCornerValues = true; // Toggle between manual corner values and Perlin noise
    public float perlinScale = 0.1f; // Scale for Perlin noise terrain generation
    public bool smoothTerrain = false; // Toggle for smoothing terrain
    [Range(1, 10)] public int unitSize = 1; // Define unit size
    [Range(1, 10)] public int unitScale = 1; // Define unit scale

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
    readonly float[] cube_corner_vals = new float[8];

    private Chunk _chunk;
    private Vector3Int _position;

    public TerrainLogger Logger { get; } = new TerrainLogger();

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            TerrainLogger.Log("Starting DebugChunkScript");
            // Replace this line:
            // _position = transform.position;

            // With this line:
            _position = Vector3Int.RoundToInt(transform.position);

            var options = GetOptions();
            _chunk = new Chunk(_position, options);

            if (useManualCornerValues)
            {
                cube_corner_vals[0] = cube_corner_val0;
                cube_corner_vals[1] = cube_corner_val1;
                cube_corner_vals[2] = cube_corner_val2;
                cube_corner_vals[3] = cube_corner_val3;
                cube_corner_vals[4] = cube_corner_val4;
                cube_corner_vals[5] = cube_corner_val5;
                cube_corner_vals[6] = cube_corner_val6;
                cube_corner_vals[7] = cube_corner_val7;
            }
            else
            {
                float terrainValue = Mathf.PerlinNoise(0, 0); // Default Perlin noise for single voxel
                
                cube_corner_vals[0] = terrainValue;
                cube_corner_vals[1] = terrainValue;
                cube_corner_vals[2] = terrainValue;
                cube_corner_vals[3] = terrainValue;
                cube_corner_vals[4] = terrainValue;
                cube_corner_vals[5] = terrainValue;
                cube_corner_vals[6] = terrainValue;
                cube_corner_vals[7] = terrainValue;
            }

            var point = new Vector3Int(0, 0, 0);
            _chunk.SetCustomVoxelValues(point, cube_corner_vals);

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

    private ChunkOptions GetOptions()
    {
        return new ChunkOptions
        {
            BaseTerrainHeight = 1, // Assuming a base terrain height of 1 for debugging purposes
            DebugChunkVoxelVal = false, // Assuming voxel value debugging is not used for this script
            DebugChunkVoxelValPersistence = false, // Assuming voxel value persistence is not used for debugging purposes
            DebugChunkWireframe = false, // Assuming wireframe is not used for debugging purposes
            DebugChunkWireframePersistence = false, // Assuming wireframe persistence is not used for debugging purposes
            DebugSaveNoiseMaps = false, // Set to true if you want to save noise maps
            DebugUseSplineShaping = false,  // Set to true if you want to use spline shaping noise maps
            FlatShaded = false, // Assuming flat shading is not used for debugging purposes
            Height = 1, // Assuming a height of 1 for debugging purposes
            IsoVal = iso_val,
            Lacunarity = 1, // Assuming a lacunarity of 1 for debugging purposes
            Noise2D = false, // Assuming 2D noise is not used for debugging purposes
            Octaves = 1, // Assuming a single octave for debugging purposes
            Persistance = 1, // Assuming a persistence of 1 for debugging purposes
            Scale = 1, // Assuming a scale of 1 for debugging purposes
            SmoothTerrain = smoothTerrain,
            TerrainHeightRange = 1, // Assuming a range of 1 for debugging purposes
            UseLists = useLists,
            Width = 1, // Assuming a single unit for debugging
            WorldSeed = 1, // Assumuing 1 for seed,
            WorldSizeInChunks = 1, // Assuming a single chunk for debugging    
        };
    }

    //Draw Gizmos for each cube corner with grayscale based on value
    void OnDrawGizmos()
    {
        if (cube_corner_vals == null || Constants.CornerTable == null)
            return;

        Vector3 position = transform.position;

        for (int i = 0; i < 8; i++)
        {
            Vector3 cornerPosition = position + (unitScale * unitSize * Constants.CornerTable[i]);
            float intensity = Mathf.Clamp01(cube_corner_vals[i]);
            Gizmos.color = new Color(intensity, intensity, intensity);
            Gizmos.DrawSphere(cornerPosition, 0.1f);
        }
    }
}
