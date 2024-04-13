using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerSetValue : ClonerJobComponent
    {
        public bool UseSelection = false;
        [Range(0, 1)] public float Strength = 1.0f;

        public bool SetPosition;
        public Vector3 Position;

        public bool SetRotation = true;
        public Quaternion Rotation = Quaternion.identity;

        public bool SetScaling = true;
        public Vector3 Scaling = Vector3.one;

        public bool SetColor = true;
        public Color Color;

        public bool SetMetallic = true;
        [Range(0, 1)] public float Metallic = 0.5f;

        public bool SetSmoothness = true;
        [Range(0, 1)] public float Smoothness  = 0.5f;

        public override (CloneData, JobHandle) Schedule(CloneData cloneData, JobHandle h)
        {
            return (cloneData, new JobSetValue()
                {
                    Data = cloneData,
                    SetRotation = SetRotation,
                    SetScaling = SetScaling,
                    SetTranslation = SetPosition,
                    UseSelection = UseSelection,
                    SetColor = SetColor,
                    Strength = Strength,
                    Translation = Position,
                    Rotation = Rotation,
                    Scaling = Scaling,
                    SetMetallic = SetMetallic,
                    Metallic = Metallic,
                    SetSmoothness = SetSmoothness,
                    Smoothness = Smoothness,
                    Color = new float4(Color.r, Color.g, Color.b, Color.a),
                }
                .Schedule(cloneData.Count, 16, h));
        }
    }
}