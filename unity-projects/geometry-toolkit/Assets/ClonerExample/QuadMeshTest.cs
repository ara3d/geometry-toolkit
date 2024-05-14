using System.Collections.Generic;
using Ara3D.Collections;
using Ara3D.Geometry;
using Ara3D.UnityBridge;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class QuadMeshTest : MonoBehaviour
    {
        public Material Material;
        public Mesh Mesh;
        public List<Vector3> Points = new List<Vector3>()
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(1, 0, 2),
        };
        public bool ClosedX;
        public bool ClosedY;
        public int Rows = 3;
        public Vector3 Offset;

        public void Update()
        {
            if (Mesh == null)
                Mesh = new Mesh();

            var points0 = Points.ToIArray().Select(p => p.ToAra3D());
            var points1 = points0.Select(v => v + Offset.ToAra3D());
            var points = LinqArray.Create(points0, points1).ToArray2D();
            var gridMesh = new GridMesh(points, ClosedX, ClosedY);
            Mesh.UpdateMesh(gridMesh.Triangulate());
            var rparams = new RenderParams(Material);
            Graphics.RenderMesh(rparams, Mesh, 0, transform.localToWorldMatrix);
        }
    }
}