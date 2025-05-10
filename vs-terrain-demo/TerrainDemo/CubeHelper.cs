//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace TerrainDemo
//{
//    public class CubeHelper
//    {
//        public CubeHelper()
//        {
////aa
//        }

//        public bool UseLists { get; set; }
//        public bool SmoothTerrain { get; set; }
//        public int UnitSize { get; set; }
//        public int UnitScale { get; set; }

//        public void MarchCube(Vector3 position, int configIndex, Vector3[] verticesArray, int[] trianglesArray)
//        {
//            // If the configuration of this cube is 0 or 255 (completely inside the terrain or completely outside of it) we don't need to do anything.
//            if (configIndex == 0 || configIndex == 255)
//                return;

//            // Loop through the triangles. There are never more than 5 triangles to a cube and only three vertices to a triangle.
//            int edgeIndex = 0;
//            for (int i = 0; i < 5; i++)
//            {
//                for (int p = 0; p < 3; p++)
//                {
//                    if (!UseLists)
//                        if (edgeIndex >= trianglesArray.Length || edgeIndex >= verticesArray.Length)
//                            return; // Prevent out-of-bounds exception

//                    // Get the current indice. We increment triangleIndex through each loop.
//                    int indice = Constants.TriangleTable[configIndex, edgeIndex];

//                    // If the current edgeIndex is -1, there are no more indices and we can exit the function.
//                    if (indice == -1)
//                        return;

//                    // Get the vertices for the start and end of this edge.
//                    Vector3 vert1 = position + (Constants.EdgeTable[indice, 0] * UnitSize * UnitScale);
//                    Vector3 vert2 = position + (Constants.EdgeTable[indice, 1] * UnitSize * UnitScale);

//                    // Get the midpoint of this edge.
//                    Vector3 vertPosition;
//                    if (SmoothTerrain)
//                    {
//                        // Linear Interpolate to find the edge position
//                        // Get the terrain values at either end of our current edge from the cube array created above.
//                        float val1 = cube_corner_vals[CornerIndex(Constants.EdgeTable[indice, 0])];
//                        float val2 = cube_corner_vals[CornerIndex(Constants.EdgeTable[indice, 1])];

//                        // Calculate the difference between the terrain values.
//                        float difference = val2 - val1;

//                        // If the difference is 0, then the terrain passes through the middle.
//                        // Can we delete this check?
//                        /*
//                        if (difference == 0)
//                            difference = iso_val;
//                        else
//                        */
//                        difference = (iso_val - val1) / difference;

//                        // Calculate the point along the edge that passes through.
//                        vertPosition = vert1 + ((vert2 - vert1) * difference);
//                    }
//                    else
//                    {
//                        // Default to center point of the edge
//                        vertPosition = (vert1 + vert2) / 2f;
//                    }
//                    // Add to our vertices and triangles list and incremement the edgeIndex.
//                    if (useLists)
//                    {
//                        verticesList.Add(vertPosition);
//                        trianglesList.Add(verticesList.Count - 1);
//                    }
//                    else
//                    {
//                        if (vertexCount < verticesArray.Length && triangleCount < trianglesArray.Length)
//                        {
//                            verticesArray[vertexCount] = vertPosition;
//                            trianglesArray[triangleCount] = vertexCount;
//                            vertexCount++;
//                            triangleCount++;
//                        }
//                    }

//                    edgeIndex++;


//                }
//            }
//        }
//    }
//}
