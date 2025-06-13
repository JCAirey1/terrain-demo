namespace TerrainDemo
{
    public class ChunkOptions
    {
        
        public int WorldSizeInChunks { get; set; } = 2;
        public int WorldSeed { get; set; } = 1;
        public int Width { get; set; } = 16;                                //for terrain generation
        public int Octaves { get; set; } = 4;                               //for terrain generation
        public int Height { get; set; } = 32;                               //for terrain generation
        public float TerrainHeightRange { get; set; } = 12f;                //the max height above baseterrainheight our terrain will generate to
        public float Scale { get; set; } = 16f;                             //for terrain generation
        public float Persistance { get; set; } = 0.3f;                      //for terrain generation
        public float Lacunarity { get; set; } = 2.3f;                       //for terrain generation                                                 
        public float IsoVal { get; set; } = 0.5f;                           //isosurface value, surface that represents points of a constant value
        public float BaseTerrainHeight { get; set; } = 16f;                 //minimum height of terrain before modification (i.e sea level)
        public bool UseLists { get; set; } = true;                          //toggle between list-based and array-based storage
        public bool SmoothTerrain { get; set; } = false;                    //toggle for smoothing terrain
        public bool Noise2D { get; set; } = true;                           //perlin noise switch
        public bool FlatShaded { get; set; } = true;                        //toggle for triangles sharing points for rendering
        public bool DebugChunkWireframePersistence { get; set; } = false;   //draw chunk bounding wireframe even when chunk is not selected
        public bool DebugChunkWireframe { get; set; } = true;               //draw chunk bounding wireframe when chunk is selected
        public bool DebugChunkVoxelValPersistence { get; set; } = false;    //draw grayscale sphere gizmos even when chunk is notselected
        public bool DebugChunkVoxelVal { get; set; } = true;                //draw grayscale sphere gizmos when chunk is selected
        public bool DebugUseSplineShaping { get; set; } = true;             //Uses spline shaping noise maps for C, E, P&V
        public bool DebugSaveNoiseMaps { get; set; } = false;               //writes noise maps to a local png file for inspection

        public override bool Equals(object obj)
        {
            if (!(obj is ChunkOptions other))
                return false;

            return WorldSizeInChunks == other.WorldSizeInChunks &&
                   WorldSeed == other.WorldSeed &&
                   Width == other.Width &&
                   Octaves == other.Octaves &&
                   Height == other.Height &&
                   TerrainHeightRange == other.TerrainHeightRange &&
                   Scale == other.Scale &&
                   Persistance == other.Persistance &&
                   Lacunarity == other.Lacunarity &&
                   IsoVal == other.IsoVal &&
                   BaseTerrainHeight == other.BaseTerrainHeight &&
                   UseLists == other.UseLists &&
                   SmoothTerrain == other.SmoothTerrain &&
                   Noise2D == other.Noise2D &&
                   FlatShaded == other.FlatShaded &&
                   DebugChunkWireframePersistence == other.DebugChunkWireframePersistence &&
                   DebugChunkWireframe == other.DebugChunkWireframe &&
                   DebugChunkVoxelValPersistence == other.DebugChunkVoxelValPersistence &&
                   DebugChunkVoxelVal == other.DebugChunkVoxelVal;
        }

        public override int GetHashCode()
        {
            unchecked // Allow arithmetic overflow without exceptions
            {
                int hash = 17;
                hash = hash * 23 + WorldSizeInChunks.GetHashCode();
                hash = hash * 23 + WorldSeed.GetHashCode();
                hash = hash * 23 + Width.GetHashCode();
                hash = hash * 23 + Octaves.GetHashCode();
                hash = hash * 23 + Height.GetHashCode();
                hash = hash * 23 + TerrainHeightRange.GetHashCode();
                hash = hash * 23 + Scale.GetHashCode();
                hash = hash * 23 + Persistance.GetHashCode();
                hash = hash * 23 + Lacunarity.GetHashCode();
                hash = hash * 23 + IsoVal.GetHashCode();
                hash = hash * 23 + BaseTerrainHeight.GetHashCode();
                hash = hash * 23 + UseLists.GetHashCode();
                hash = hash * 23 + SmoothTerrain.GetHashCode();
                hash = hash * 23 + Noise2D.GetHashCode();
                hash = hash * 23 + FlatShaded.GetHashCode();
                hash = hash * 23 + DebugChunkWireframePersistence.GetHashCode();
                hash = hash * 23 + DebugChunkWireframe.GetHashCode();
                hash = hash * 23 + DebugChunkVoxelValPersistence.GetHashCode();
                hash = hash * 23 + DebugChunkVoxelVal.GetHashCode();
                return hash;
            }
        }
    }
}
