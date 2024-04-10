using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerComponent : MonoBehaviour
    {
        public NativeArray<CpuInstanceData> CpuArray;
        public NativeArray<GpuInstanceData> GpuArray;

        public int Rows = 5;
        public int Columns = 5;
        public int Layers = 5;
        public float Spacing = 1.5f;
        public int Count => Rows * Columns * Layers;
        public Color Color = Color.blue;
        public Quaternion Rotation = Quaternion.identity;
        public Vector3 Scale = Vector3.one;

        public void OnEnable()
        {
            CreateNativeArrays();
        }

        public void CreateNativeArrays()
        {
            if (Count != CpuArray.Length || !CpuArray.IsCreated)
            {
                if (CpuArray.IsCreated) CpuArray.Dispose();
                CpuArray = new NativeArray<CpuInstanceData>(Count, Allocator.Persistent);
            }

            if (Count != GpuArray.Length || !GpuArray.IsCreated)
            {
                if (GpuArray.IsCreated) GpuArray.Dispose();
                GpuArray = new NativeArray<GpuInstanceData>(Count, Allocator.Persistent);
            }

        }

        public void Update()
        {
            CreateNativeArrays();

            for (var i = 0; i < Count; i++)
            {
                var col = i % Columns;
                var row = (i / Columns) % Rows;
                var layer = (i / (Columns * Rows));

                var position = col * Vector3.right * Spacing
                               + row * Vector3.forward * Spacing
                               + layer * Vector3.up * Spacing;
                var rotation = Rotation;
                var scale = Scale;

                var mat = Matrix4x4.TRS(position, rotation, scale);
                ;
                GpuArray[i] = new GpuInstanceData()
                {
                    Pos = position,
                    Rot = rotation,
                    Scl = scale,
                    Color = new Vector4(Color.r, Color.g, Color.b, 1),
                };

                CpuArray[i] = new CpuInstanceData()
                {
                    Id = (uint)i
                };
            }
        }

        public void OnDisable()
        {
            CpuArray.Dispose();
            GpuArray.Dispose();
        }
    }
}   