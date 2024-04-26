using System;
using System.Collections.Generic;
using Ara3D.UnityBridge;
using UnityEngine;

namespace Ara3D.ProceduralGeometry.Unity
{
    public class InstancedMeshDrawer
    {
        public Mesh Mesh;
        public Material Material;
        public IReadOnlyList<Matrix4x4> Matrices;
        public Color Color;

        public void Draw()
        {
            throw new NotImplementedException();
        }
    }

    [ExecuteAlways]
    public class SceneRenderer : MonoBehaviour
    {
        public Material Material;

        public UnityMeshScene Scene;
        
        private List<InstancedMeshDrawer> drawers = new List<InstancedMeshDrawer>();
        
        public void Update()
        {
            foreach (var drawer in drawers)
            {
                drawer.Draw();
            }
        }

        public void Init(UnityMeshScene scene)
        {
            drawers.Clear();
            foreach (var set in scene.InstanceSets)
            {
                // If there are no instances, skip it
                if (set.Matrices.Count <= 0)
                    continue;

                // If there are no faces, skip it
                if (set.TriMesh.FaceIndices.Count == 0)
                    continue;

                var mesh = new Mesh();
                set.TriMesh.AssignToMesh(mesh);

                var drawer = new InstancedMeshDrawer()
                {
                    Color = set.Color,
                    Material = Material,
                    Mesh = mesh,
                    Matrices = set.Matrices,
                };

                drawers.Add(drawer);
            }
            Scene = scene;
        }
    }
}