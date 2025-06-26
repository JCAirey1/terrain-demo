namespace TerrainDemo
{
    public class CubeGeneratorOptions
    {
        public float Iso_val { get; set; } = 0.5f;
        public bool UseLists { get; set; } = true;
        public bool GenerateChunk { get; set; } = false;
        public bool UseManualCornerValues { get; set; } = true;
        public int ChunkSize { get; set; } = 8;
        public float PerlinScale { get; set; } = 0.1f;
        public bool SmoothTerrain { get; set; } = false;
        public float CubeCornerVal0 { get; set; } = 0f;
        public float CubeCornerVal1 { get; set; } = 0f;
        public float CubeCornerVal2 { get; set; } = 0f;
        public float CubeCornerVal3 { get; set; } = 0f;
        public float CubeCornerVal4 { get; set; } = 0f;
        public float CubeCornerVal5 { get; set; } = 0f;
        public float CubeCornerVal6 { get; set; } = 0f;
        public float CubeCornerVal7 { get; set; } = 0f;
    }
}
