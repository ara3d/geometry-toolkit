using UnityEngine;

namespace Assets.ClonerExample
{
    public struct GpuInstanceData
    {
        public Matrix4x4 Matrix;
        public Vector4 Color;
        public float Smoothness;
        public float Metallic;
        public uint Id;
        public uint Padding;
        public static int Size => 96;
    }
}