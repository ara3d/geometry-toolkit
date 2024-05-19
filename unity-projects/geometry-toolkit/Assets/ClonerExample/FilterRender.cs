using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Collections;
using Ara3D.Geometry;
using Ara3D.Mathematics;
using Ara3D.UnityBridge;
using UnityEngine;
using IPoints = Ara3D.Geometry.IPoints;
using Matrix4x4 = Ara3D.Mathematics.Matrix4x4;
using Vector3 = Ara3D.Mathematics.Vector3;


namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class FilterRender : MonoBehaviour
    {
        public Material Material;
        public Mesh InstanceMesh;
        public bool ZUp;
        public bool FlipTriangles;
        public bool DoubleSided;
        
        // Maybe?
        // public float Scale; 
        // public useCubesOrCylindersForLines;
        // public useBoxesOrSpheresForPoints; 

        public class TransformedMesh
        {
            public Mesh Mesh;
            public UnityEngine.Matrix4x4 Matrix;
            public TransformedMesh(Mesh mesh)
                : this (mesh, UnityEngine.Matrix4x4.identity)
            { }
            public TransformedMesh(Mesh mesh, UnityEngine.Matrix4x4 matrix)
            {
                Mesh = mesh;
                Matrix = matrix;
            }
        }

        public void Update()
        {
            var comps = gameObject.GetComponents<MonoBehaviour>();

            if (comps.Length == 0)
                return;

            object val = default;

            for (var i = 0; i < comps.Length; ++i)
            {
                if (comps[i] == this)
                    break;
                if (!comps[i].enabled)
                    continue;
                if (comps[i] is IFilter filter)
                    val = EvalFilter(filter, val, 1);
            }

            var meshes = ToUnityMeshes(val);

            var rp = new RenderParams(Material);
            foreach (var tm in meshes)
            {
                Graphics.RenderMesh(rp, tm.Mesh, 0, tm.Matrix * transform.localToWorldMatrix);
            }
        }

        public object EvalFilter(IFilter f, object input, float strength)
        {
            var expectedType = f.Input;
            if (input == null) 
                return f.Eval(input);

            var inputType = input.GetType();
            if (expectedType.IsAssignableFrom(inputType))
                return f.Eval(input);

            if (input is List<object> list)
            {
                // Auto-mapping 
                return list
                    .Select((item, i) 
                        => EvalFilter(f, item, (float)i / (list.Count - 1)))
                    .ToList();
            }

            if (expectedType == typeof(Vector3))
            {
                var deformer = input.GetType().GetMethod("Deform");
                if (deformer != null)
                {
                    Func<Vector3, Vector3> func 
                        = v => v.Lerp((Vector3)f.Eval(v), strength);
                    
                    return deformer.Invoke(input, new object[] { func });
                }
            }

            throw new Exception($"Cannot cast from {inputType} to {expectedType}");
        }

        public TransformedMesh ToUnityMesh(object obj)
        {
            if (obj is ITriMesh m)
                return new TransformedMesh(
                    m.ToUnity(ZUp, FlipTriangles, DoubleSided));
            if (obj is Matrix4x4 mat)
                return new TransformedMesh(InstanceMesh, mat.ToUnityRaw());
            if (obj is Vector3 vec)
                return new TransformedMesh(InstanceMesh,
                    UnityEngine.Matrix4x4.Translate(vec.ToUnity()));
            throw new Exception($"Could not convert {obj} to TransformedMesh");
        }

        public IEnumerable<TransformedMesh> ToUnityMeshes(object obj)
        {
            if (obj is List<object> list)
                return list.SelectMany(ToUnityMeshes);
            if (obj is IPoints points && !(obj is ITriMesh))
                return points.Points.ToEnumerable().Select(p => ToUnityMesh(p));
 
            // TODO: handle, lines, points, transforms, strands, curves. 
            return new[] { ToUnityMesh(obj) };
        }
    }
}