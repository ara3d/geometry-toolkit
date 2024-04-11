using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.ClonerExample
{

    [BurstCompile(CompileSynchronously = true)]
    public struct InitializeDataJob : IJobParallelFor
    {
        public CloneData CloneData;

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

            var position = col * new float3(1, 0, 0) * Spacing
                           + row * new float3(0, 0, 1) * Spacing;
            var rotation = Rotation;
            var scale = Scale;

            CloneData.GpuArray[i] = new GpuInstanceData()
            {
                Pos = position,
                Rot = rotation,
                Scl = scale,
                Color = Color,
                Id = (uint)i
            };
        }
    }
}