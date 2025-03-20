using System;
using System.Numerics;

namespace TerrainDemo
{
    public class CubeGeneratorBehaviors
    {
        public Action<Vector3[], int[]> BuildMesh;
        public Func<float> GeneratePerlinNoiseDefault;
        public Func<float, float, int, float> GeneratePerlinNoise;
    }
}
