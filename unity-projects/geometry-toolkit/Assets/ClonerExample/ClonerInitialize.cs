using System;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerInitialize : ClonerJobComponent
    {
        public CloneData CloneData;

        public int Rows = 5;
        public int Columns = 5;
        public float Spacing = 1.5f;
        public int Count => Rows * Columns;
        public Color Color = Color.blue;
        public Quaternion Rotation = Quaternion.identity;
        public Vector3 Scale = Vector3.one;
        [Range(0, 1)] public float Metallic = 0.5f;
        [Range(0, 1)] public float Smoothness = 0.5f;

        public void OnValidate()
        {
            Rows = Math.Max(1, Rows);
            Columns = Math.Max(1, Columns);
        }

        public override (CloneData, JobHandle) Schedule(CloneData previousData, JobHandle previousHandle)
        {
            CloneData.Resize(Count);
            return (CloneData, new JobInitializeData
            {
                CloneData = CloneData,
                Rows = Rows,
                Columns = Columns,
                Spacing = Spacing,
                Color = new float4(Color.r, Color.g, Color.b, Color.a),
                Rotation = Rotation,
                Scale = Scale,
                Metallic = Metallic,
                Smoothness = Smoothness,
            }
            .Schedule(Count, 16, previousHandle));
        }

        void OnDisable()
        {
            CloneData.Dispose();
        }
    }
}   