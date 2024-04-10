using UnityEngine;

namespace Assets.ClonerExample
{
    public struct GpuInstanceData
    {
        public Vector3 Pos;
        public Quaternion Rot;
        public Vector3 Scl;
        public Vector4 Color;
        public float Smoothness;
        public float Metallic;
        public uint Id;
        public static int Size => 17 * 4;
    }
}