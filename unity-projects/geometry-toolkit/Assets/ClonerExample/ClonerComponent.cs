using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;

namespace Assets.ClonerExample
{

    [BurstCompile(CompileSynchronously = true)]
    public struct InitializeDataJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<CpuInstanceData> CpuArray;
        [WriteOnly] public NativeArray<GpuInstanceData> GpuArray;

        public int Rows;
        public int Columns;
        public float Spacing;
        
        public float4 Color;
        public quaternion Rotation;
        public float3 Scale;

        public void Execute(int i)
        {
            var col = i % Columns;
            var row = (i / Columns) % Rows;
            var layer = (i / (Columns * Rows));

            var position = col * new float3(1, 0, 0) * Spacing
                           + row * new float3(0, 0, 1) * Spacing
                           + layer * new float3(0, 1, 0) * Spacing;
            var rotation = Rotation;
            var scale = Scale;

            GpuArray[i] = new GpuInstanceData()
            {
                Pos = position,
                Rot = rotation,
                Scl = scale,
                Color = Color,
            };

            CpuArray[i] = new CpuInstanceData()
            {
                Id = (uint)i
            };
        }
    }

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

            var job = new InitializeDataJob
            {
                Rows = Rows,
                Columns = Columns,
                Spacing = Spacing,
                Color = new float4(Color.r, Color.g, Color.b, Color.a),
                Rotation = Rotation,
                Scale = Scale,
                CpuArray = CpuArray,
                GpuArray = GpuArray
            };

            var handle = job.Schedule(Count, 1);
            handle.Complete();
        }

        public void OnDisable()
        {
            CpuArray.Dispose();
            GpuArray.Dispose();
        }

    }
}   