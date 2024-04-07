using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Ara3D.ProceduralGeometry.Unity
{
    public struct GpuInstanceData
    {
        public float4x4 Matrix;
        public float4 Color;
        public static int Size => sizeof(float) * 4 * 5;
        public GpuInstanceData(float4x4 matrix, float4 color) => (Matrix, Color) = (matrix, color);
    }

    public struct CpuInstanceData
    {
        public float3 Position;
        public float Age;
        public float3 Scale;
        public quaternion Rotation;
        public float4 Color;
        public float3 Velocity;
        public quaternion RotationalVelocity;
        public float3 Acceleration;
        public bool Enabled;
    }

    // TODO: Set the Burst compile option.
    // [BurstCompile]
    public struct CloneData : IJobParallelFor
    {
        public NativeArray<CpuInstanceData> CpuData;
        public NativeArray<GpuInstanceData> GpuData;
        public ComputeBuffer ComputeBuffer;

        public int NumInstances;

        // Set this before updating 
        public float TimeStep;

        public void Resize(int size)
        {
            Release();
            CpuData = new NativeArray<CpuInstanceData>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            GpuData = new NativeArray<GpuInstanceData>(size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            ComputeBuffer = new ComputeBuffer(GpuData.Length, GpuInstanceData.Size);
            NumInstances = size;
        }

        public CpuInstanceData Update(CpuInstanceData inst)
        {
            inst.Age += TimeStep;
            //inst.Position += inst.Velocity * TimeStep;
            //var rotVel = math.slerp(quaternion.identity, inst.RotationalVelocity, TimeStep);
            //inst.Rotation = math.mul(inst.Rotation, rotVel);
            //inst.Velocity += inst.Acceleration * TimeStep;
            return inst;
        }

        public void Execute(int i)
        {
            if (CpuData[i].Enabled) CpuData[i] = Update(CpuData[i]);
            var matrix = float4x4.TRS(CpuData[i].Position, CpuData[i].Rotation, CpuData[i].Scale);
            GpuData[i] = new GpuInstanceData(matrix, CpuData[i].Color);
        }

        public ComputeBuffer UpdateAllInstances()
        {
            for (var i=0; i < NumInstances; i++) 
                Execute(i);
            return UpdateComputeBuffer();
        }

        public ComputeBuffer UpdateComputeBuffer()
        {
            ComputeBuffer.SetData(GpuData);
            return ComputeBuffer;
        }

        public void Release()
        {
            if (ComputeBuffer != null)
            {
                ComputeBuffer.Release();
                ComputeBuffer = null;
                CpuData.Dispose();
                GpuData.Dispose();
            }
        }
    }
}
