using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Numerics;
using TerrainDemo;
using System.Runtime.InteropServices;
using System;

public class CubeGeneratorScript : MonoBehaviour
{
    MeshFilter meshFilter;
    [Range(0f, 1f)] public float iso_val = 0.5f; // Isosurface Value, surface that represents points of a constant value
    public bool useLists = true; // Toggle between list-based and array-based storage
    public bool generateChunk = false; // Toggle to generate a chunk of voxels
    public bool useManualCornerValues = true; // Toggle between manual corner values and Perlin noise
    [Range(1, 16)] public int chunkSize = 8; // Define chunk size
    public float perlinScale = 0.1f; // Scale for Perlin noise terrain generation
    public bool smoothTerrain = false; // Toggle for smoothing terrain

    List<Vector3> verticesList = new List<Vector3>();
    List<int> trianglesList = new List<int>();

    Vector3[] verticesArray;
    int[] trianglesArray;
    int vertexCount = 0;
    int triangleCount = 0;

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
    float[] cube_corner_vals = new float[8];
    CubeGenerator gameObject;


    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        var options = new CubeGeneratorOptions()
        {
            ChunkSize = chunkSize,
            CubeCornerVal0 = cube_corner_val0,
            CubeCornerVal1 = cube_corner_val1,
            CubeCornerVal2 = cube_corner_val2,
            CubeCornerVal3 = cube_corner_val3,
            CubeCornerVal4 = cube_corner_val4,
            CubeCornerVal5 = cube_corner_val5,
            CubeCornerVal6 = cube_corner_val6,
            CubeCornerVal7 = cube_corner_val6,
            GenerateChunk = generateChunk,
            Iso_val = iso_val,
            PerlinScale = perlinScale,
            SmoothTerrain = smoothTerrain,
            UseLists = useLists,
            UseManualCornerValues = useManualCornerValues,
        };

        var behaviors = new CubeGeneratorBehaviors()
        {
            BuildMesh = (vertices, triangles) =>
            {
                Mesh mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.triangles = triangles;
                mesh.RecalculateNormals();
                meshFilter.mesh = mesh;
            },
            GeneratePerlinNoise = (x, z, chunkSize) => {
                return Mathf.PerlinNoise(x * perlinScale, z * perlinScale) * chunkSize;
            },
            GeneratePerlinNoiseDefault = () => Mathf.PerlinNoise(0, 0)
        };

        //public Action<float> DrawGizmo;
        //public Action<Vector3[], int[]> BuildMesh;
        //public Func<Vector3> GetTransformPosition;
        //public Func<float, float, float> GeneratePerlinNoiseDefault;
        //public Func<float, float, int, float> GeneratePerlinNoise;

        gameObject = new TerrainDemo.CubeGenerator(options, behaviors);
        gameObject.Start();
    }

    void Update()
    {
        gameObject.Update();
    }

    void OnDrawGizmos()
    {
        if (cubeCornerVals == null || Constants.CornerTable == null)
            return;

        if (generateChunk)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        Vector3 voxelPosition = new Vector3(x, y, z);
                        for (int i = 0; i < 8; i++)
                        {
                            Vector3 cornerPosition = voxelPosition + Constants.CornerTable[i];
                            DrawGizmo(cubeCornerVals[i]);
                            float intensity = Mathf.Clamp01(cubeCornerVals[i]);
                            Gizmos.color = new Color(intensity, intensity, intensity);
                            Gizmos.DrawSphere(cornerPosition, 0.1f);
                    }
                    }
                }
            }
        }
        else
        {
            Vector3 position = transform.position;
            for (int i = 0; i < 8; i++)
            {
                Vector3 cornerPosition = position + Constants.CornerTable[i];
                DrawGizmo(cubeCornerVals[i]);
                float intensity = Mathf.Clamp01(cubeCornerVals[i]);
                Gizmos.color = new Color(intensity, intensity, intensity);
                Gizmos.DrawSphere(cornerPosition, 0.1f);
            }
        }
    }
}