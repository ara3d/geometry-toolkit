using UnityEngine;

namespace Assets.ClonerExample
{
    public struct CpuInstanceData
    {
        public float Age;
        public Vector3 Velocity;
        public Vector3 Acceleration;
        public uint Id;
        public Vector2 Uv;
        public float Mass;

        public void Update(float timeDelta)
        {
            Age += timeDelta;
            Velocity += Acceleration * timeDelta;
        }
    }
}