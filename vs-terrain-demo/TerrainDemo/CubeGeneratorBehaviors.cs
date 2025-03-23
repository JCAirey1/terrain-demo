using System;
using UnityEngine;

namespace TerrainDemo
{
    public class CubeGeneratorBehaviors
    {
        public Action<Vector3[], int[]> BuildMesh;
        public Func<float> GeneratePerlinNoiseDefault;
        public Func<float, float, int, float> GeneratePerlinNoise;
        public Action<Vector3, float> DrawGizmo;
        public Func<Vector3> GetTransformPosition;
    }
}
