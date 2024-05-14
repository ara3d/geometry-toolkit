using System;
using System.Collections.Generic;
using Ara3D.Collections;
using Ara3D.Geometry;
using Ara3D.UnityBridge;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class QuadMeshFromArray : MonoBehaviour
    {
        public Material Material;
        public Mesh Mesh;

        public void Update()
        {
            if (Mesh == null)
                Mesh = new Mesh();
            var triMesh = ComputeGeometry();
            Mesh.UpdateMesh(triMesh);
            var rparams = new RenderParams(Material);
            Graphics.RenderMesh(rparams, Mesh, 0, transform.localToWorldMatrix);
        }

        public static IArray<Ara3D.Mathematics.Vector3> ToIArray(IPoints points)
            => points.Points.Length.Select(i =>
            {
                var p = points.Points[i];
                return new Ara3D.Mathematics.Vector3(p.x, p.y, p.z);
            });

        public ITriMesh ComputeGeometry()
        {
            var comps = gameObject.GetComponents<MonoBehaviour>();

            if (comps.Length == 0)
                return null;

            object val = default;

            for (var i = 0; i < comps.Length; ++i)
            {
                if (comps[i] == this)
                    break;
                if (!comps[i].enabled)
                    continue;
                if (comps[i] is IPipelineComponent ipc)
                {
                    ipc.SetInput(val);
                    val = ipc.GetOutput();
                    Debug.Log($"Value = {val}");
                }
            }

            if (val is List<object> list)
            {
                var listArray = list.ToIArray().Select(o => (IPoints)o);
                var points = listArray.Select((p,i) => ToIArray(p).Select(v => v + (0,i,0))).ToArray2D();
                var gridMesh = new GridMesh(points, false, true);
                return gridMesh.Triangulate();
            }
            else
            {
                throw new Exception($"{val} of type {val?.GetType()} is not recognized");
            }
        }
    }
}