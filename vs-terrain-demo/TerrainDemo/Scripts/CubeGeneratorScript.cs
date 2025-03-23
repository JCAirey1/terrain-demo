using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
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
    TerrainDemo.CubeGenerator cubeGeneratorGameObject;

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
            GeneratePerlinNoiseDefault = () => Mathf.PerlinNoise(0, 0),
            DrawGizmo = (cornerPosition, cubeCornerValue) =>
            {
                float intensity = Mathf.Clamp01(cubeCornerValue);
                Gizmos.color = new Color(intensity, intensity, intensity);
                Gizmos.DrawSphere(cornerPosition, 0.1f);
            },
            GetTransformPosition = () => transform.position
        };

        cubeGeneratorGameObject = new TerrainDemo.CubeGenerator(options, behaviors);
        cubeGeneratorGameObject.Start();
    }

    void Update()
    {
        cubeGeneratorGameObject.Update();
    }

    void OnDrawGizmos()
    {
        cubeGeneratorGameObject.OnDrawGizmos();
        return;
    }
}