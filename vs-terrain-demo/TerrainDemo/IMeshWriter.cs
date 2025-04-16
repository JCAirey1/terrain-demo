using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TerrainDemo
{
    public interface IMeshWriter
    {
        void AddVertex(Vector3 position);
    }

    public class ListMeshWriter : IMeshWriter
    {
        public List<Vector3> Vertices { get; } = new List<Vector3>();
        public List<int> Triangles { get; } = new List<int>();

        public void AddVertex(Vector3 pos)
        {
            Vertices.Add(pos);
            Triangles.Add(Vertices.Count - 1);
        }
    }
}
