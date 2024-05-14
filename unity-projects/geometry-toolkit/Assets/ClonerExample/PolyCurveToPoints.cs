using System;
using Ara3D.Geometry;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class PolyCurveToPoints : MonoBehaviour, IPipelineComponent, IPoints
    {
        public int NumberOfSamples;
        private NativeArray<float3> _points;
        private ICurve3D _input;
        public bool InvalidCache;

        public void SetInput(object input)
        {
            _input = input as ICurve3D;
            InvalidCache = true;
        }

        public object GetOutput()
        {
            if (InvalidCache)
                Recompute();
            return this;
        }
        
        public Type GetInputType()
            => typeof(ICurve3D);

        public Type GetOutputType()
            => typeof(IPoints);

        public void Recompute()
        {
            var pts = NumberOfSamples.InterpolateInclusive();
            _points.Resize(pts.Count);
            for (var i = 0; i < pts.Count; i++)
            {
                var p = _input.Eval(pts[i]);
                _points[i] = new float3(p.X, p.Y, p.Z);
            }
            InvalidCache = false;
        }

        public ref NativeArray<float3> Points => ref _points;
    }
}