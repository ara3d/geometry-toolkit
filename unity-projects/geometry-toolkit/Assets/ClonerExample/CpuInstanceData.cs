using UnityEngine;

namespace Assets.ClonerExample
{
    public class CpuInstanceData
    {
        public Vector3 Position;
        public float Age;
        public Vector3 Scale;
        public Quaternion Rotation;
        public Vector3 Color;
        public float Alpha;
        public Vector3 Velocity;
        public Vector3 Acceleration;
        public bool Enabled;
        public uint Id;
        public Vector2 Uv;
        public float Metallic;
        public float Smoothness;
        public float Mass;

        public Matrix4x4 ToMatrix => Matrix4x4.TRS(Position, Rotation, Scale);

        public void Update(float timeDelta)
        {
            Age += timeDelta;
            Position += Velocity * timeDelta;
            Velocity += Acceleration * timeDelta;
        }

        public GpuInstanceData ToGpuStruct() =>
            new GpuInstanceData()
            {
                Color = new Vector4(Color.x, Color.y, Color.z, Enabled ? Alpha : 0),
                Id = Id,
                Metallic = Metallic,
                Smoothness = Smoothness,
                Matrix = Matrix4x4.TRS(Position, Quaternion.identity, Scale),
            };
    }
}