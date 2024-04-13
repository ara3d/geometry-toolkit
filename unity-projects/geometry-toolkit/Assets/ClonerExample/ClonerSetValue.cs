using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerSetValue : ClonerJobComponent
    {
        public bool UseSelection = true;
        [Range(0, 1)] public float Strength = 1.0f;

        public bool ApplyTranslation;
        public Vector3 Translation;

        public bool ApplyRotation = true;
        public Quaternion Rotation = Quaternion.identity;

        public bool ApplyScaling = true;
        public Vector3 Scaling = Vector3.one;

        public bool ApplyColor = true;
        public Color Color;

        public bool ApplyMetallic = true;
        [Range(0, 1)] public float Metallic = 0.5f;

        public bool ApplySmoothness = true;
        [Range(0, 1)] public float Smoothness  = 0.5f;

        public override (CloneData, JobHandle) Schedule(CloneData cloneData, JobHandle h)
        {
            return (cloneData, new JobSetValue()
                {
                    Data = cloneData,
                    ApplyRotation = ApplyRotation,
                    ApplyScaling = ApplyScaling,
                    ApplyTranslation = ApplyTranslation,
                    UseSelection = UseSelection,
                    ApplyColor = ApplyColor,
                    Strength = Strength,
                    Translation = Translation,
                    Rotation = Rotation,
                    Scaling = Scaling,
                    ApplyMetallic = ApplyMetallic,
                    Metallic = Metallic,
                    ApplySmoothness = ApplySmoothness,
                    Smoothness = Smoothness,
                    Color = new float4(Color.r, Color.g, Color.b, Color.a),
                }
                .Schedule(cloneData.Count, 16, h));
        }
    }
}