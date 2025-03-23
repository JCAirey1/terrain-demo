using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace TerrainDemo
{
    public class CubeGenerator
    {
        private CubeGenerator() { }

        public CubeGenerator(CubeGeneratorOptions options, CubeGeneratorBehaviors behaviors)
        {
            this.chunkSize = options.ChunkSize;
            this.cubeCornerVal0 = options.CubeCornerVal0;
            this.cubeCornerVal1 = options.CubeCornerVal1;
            this.cubeCornerVal2 = options.CubeCornerVal2;
            this.cubeCornerVal3 = options.CubeCornerVal3;
            this.cubeCornerVal4 = options.CubeCornerVal4;
            this.cubeCornerVal5 = options.CubeCornerVal5;
            this.cubeCornerVal6 = options.CubeCornerVal6;
            this.cubeCornerVal7 = options.CubeCornerVal7;
            this.generateChunk = options.GenerateChunk;
            this.iso_val = options.Iso_val;
            this.perlinScale = options.PerlinScale;
            this.smoothTerrain = options.SmoothTerrain;
            this.useLists = options.UseLists;
            this.useManualCornerValues = options.UseManualCornerValues;

            this.GeneratePerlinNoiseDefault = behaviors.GeneratePerlinNoiseDefault;
            this.GeneratePerlinNoise = behaviors.GeneratePerlinNoise;
            this.DrawGizmo = behaviors.DrawGizmo;
            this.GetTransformPosition = behaviors.GetTransformPosition;
            this.BuildMesh = behaviors.BuildMesh;
        }

        public void RefreshOptions(CubeGeneratorOptions options)
        {
            this.chunkSize = options.ChunkSize;
            this.cubeCornerVal0 = options.CubeCornerVal0;
            this.cubeCornerVal1 = options.CubeCornerVal1;
            this.cubeCornerVal2 = options.CubeCornerVal2;
            this.cubeCornerVal3 = options.CubeCornerVal3;
            this.cubeCornerVal4 = options.CubeCornerVal4;
            this.cubeCornerVal5 = options.CubeCornerVal5;
            this.cubeCornerVal6 = options.CubeCornerVal6;
            this.cubeCornerVal7 = options.CubeCornerVal7;
            this.generateChunk = options.GenerateChunk;
            this.iso_val = options.Iso_val;
            this.perlinScale = options.PerlinScale;
            this.smoothTerrain = options.SmoothTerrain;
            this.useLists = options.UseLists;
            this.useManualCornerValues = options.UseManualCornerValues;
        }

        //Delegates
        private readonly Func<float> GeneratePerlinNoiseDefault;
        private readonly Func<float, float, int, float> GeneratePerlinNoise;
        private readonly Func<Vector3> GetTransformPosition;
        private readonly Action<Vector3, float> DrawGizmo;
        private readonly Action<Vector3[], int[]> BuildMesh;

        //OPTIONS
        private float iso_val = 0.5f;              // Isosurface Value, surface that represents points of a constant value
        private bool useLists = true;              // Toggle between list-based and array-based storage
        private bool generateChunk = false;        // Toggle to generate a chunk of voxels
        private bool useManualCornerValues = true; // Toggle between manual corner values and Perlin noise
        private int chunkSize = 8;                 // Define chunk size
        private float perlinScale = 0.1f;          // Scale for Perlin noise terrain generation
        private bool smoothTerrain = false;        // Toggle for smoothing terrain
        private float cubeCornerVal0 = 0f;         //Float values for each corner of the cube, these are in the voxel grid
        private float cubeCornerVal1 = 0f;
        private float cubeCornerVal2 = 0f;
        private float cubeCornerVal3 = 0f;
        private float cubeCornerVal4 = 0f;
        private float cubeCornerVal5 = 0f;
        private float cubeCornerVal6 = 0f;
        private float cubeCornerVal7 = 0f;

        //INTERNALS
        private readonly List<Vector3> verticesList = new List<Vector3>();
        private readonly List<int> trianglesList = new List<int>();

        private Vector3[] verticesArray;
        private int[] trianglesArray;
        private int vertexCount = 0;
        private int triangleCount = 0;
        float[] cubeCornerVals = new float[8];

        public void Start()
        {
            ClearMeshData();
            cubeCornerVals = new float[8]; //Set to zero
        }

        // Update is called once per frame
        public void Update()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            ClearMeshData();

            if (generateChunk)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        float height = GeneratePerlinNoise(x, z, chunkSize);

                        for (int y = 0; y < chunkSize; y++)
                        {
                            if (useManualCornerValues)
                            {
                                cubeCornerVals = new float[] { cubeCornerVal0, cubeCornerVal1, cubeCornerVal2, cubeCornerVal3, cubeCornerVal4, cubeCornerVal5, cubeCornerVal6, cubeCornerVal7 };
                            }
                            else
                            {
                                float terrainValue = y < height ? 1f : 0f;
                                cubeCornerVals = new float[] { terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue };
                            }

                            GenerateVoxel(new Vector3(x, y, z), cubeCornerVals[0]);
                        }
                    }
                }
            }
            else
            {
                if (useManualCornerValues)
                {
                    cubeCornerVals = new float[] { cubeCornerVal0, cubeCornerVal1, cubeCornerVal2, cubeCornerVal3, cubeCornerVal4, cubeCornerVal5, cubeCornerVal6, cubeCornerVal7 };
                }
                else
                {
                    float terrainValue = GeneratePerlinNoiseDefault();

                    cubeCornerVals = new float[] { terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue, terrainValue };
                }

                GenerateVoxel(new Vector3(0, 0, 0), cubeCornerVals[0]);
            }

            BuildMesh(useLists ? verticesList.ToArray() : verticesArray, useLists ? trianglesList.ToArray() : trianglesArray);

            stopwatch.Stop();
        }

        void GenerateVoxel(Vector3 position, float terrainValue)
        {
            //cubeCornerVals = new float[] { cubeCornerVal0, cubeCornerVal1, cubeCornerVal2, cubeCornerVal3, cubeCornerVal4, cubeCornerVal5, cubeCornerVal6, cubeCornerVal7 };

            // Starting with a configuration of zero, loop through each point in the cube and check if it is below the terrain surface.
            int configurationIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                // If it is, use bit-magic to the set the corresponding bit to 1. So if only the 3rd point in the cube was below
                // the surface, the bit would look like 00100000, which represents the integer value 32.
                if (cubeCornerVals[i] > iso_val)
                    configurationIndex |= 1 << i;
            }
            //UnityEngine.Debug.Log(configurationIndex); //change this to a gui text at some point

            MarchCube(position, configurationIndex);
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

        void MarchCube(Vector3 position, int configIndex)
        {

            // If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
            if (configIndex == 0 || configIndex == 255)
                return;

            // Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
            int edgeIndex = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int p = 0; p < 3; p++)
                {
                    if (!useLists)
                        if (edgeIndex >= trianglesArray.Length || edgeIndex >= verticesArray.Length)
                            return; // Prevent out-of-bounds exception

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

                    if (smoothTerrain)
                    {
                        // Linear Interpolate to find the edge position
                        // Get the terrain values at either end of our current edge from the cube array created above.
                        float val1 = cubeCornerVals[CornerIndex(Constants.EdgeTable[indice, 0])];
                        float val2 = cubeCornerVals[CornerIndex(Constants.EdgeTable[indice, 1])];

                        // Calculate the difference between the terrain values.
                        float difference = val2 - val1;

                        // If the difference is 0, then the terrain passes through the middle.
                        // Can we delete this check?
                        /*
                        if (difference == 0)
                            difference = iso_val;
                        else
                        */
                        difference = (iso_val - val1) / difference;

                        // Calculate the point along the edge that passes through.
                        vertPosition = vert1 + ((vert2 - vert1) * difference);
                    }
                    else
                    {
                        // Default to center point of the edge
                        vertPosition = (vert1 + vert2) / 2f;
                    }
                    // Add to our vertices and triangles list and incremement the edgeIndex.
                    if (useLists)
                    {
                        verticesList.Add(vertPosition);
                        trianglesList.Add(verticesList.Count - 1);
                    }
                    else
                    {
                        if (vertexCount < verticesArray.Length && triangleCount < trianglesArray.Length)
                        {
                            verticesArray[vertexCount] = vertPosition;
                            trianglesArray[triangleCount] = vertexCount;
                            vertexCount++;
                            triangleCount++;
                        }
                    }

                    edgeIndex++;
                }
            }
        }

        void ClearMeshData()
        {
            if (useLists)
            {
                verticesList.Clear();
                trianglesList.Clear();
            }
            else
            {
                int maxVertices = chunkSize * chunkSize * chunkSize * 15;
                verticesArray = new Vector3[maxVertices]; // Adjust preallocation size
                trianglesArray = new int[maxVertices * 3];
                vertexCount = 0;
                triangleCount = 0;
            }
        }

        public void OnDrawGizmos()
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
                                DrawGizmo(cornerPosition, cubeCornerVals[i]);
                            }
                        }
                    }
                }
            }
            else
            {
                Vector3 position = GetTransformPosition();
                for (int i = 0; i < 8; i++)
                {
                    Vector3 cornerPosition = position + Constants.CornerTable[i];
                    DrawGizmo(cornerPosition, cubeCornerVals[i]);
                }
            }
        }
    }
}
