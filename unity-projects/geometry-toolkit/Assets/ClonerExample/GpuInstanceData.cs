using Unity.Mathematics;

namespace Assets.ClonerExample
{
    public struct GpuInstanceData
    {
        public float3 Pos;
        public quaternion Rot;
        public float3 Scl;
        public float4 Color;
        public float Smoothness;
        public float Metallic;
        public uint Id;
        public static int Size => 17 * 4;
    }
}