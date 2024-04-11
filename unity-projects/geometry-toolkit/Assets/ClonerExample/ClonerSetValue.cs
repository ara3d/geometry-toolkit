using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerSetValue : ClonerJobComponent
    {
        public bool UseSelection = true;
        public bool HardOrSoftSelection = false;

        public bool ApplyTranslation;
        public Vector3 Translation;

        public bool ApplyRotation = true;
        public Quaternion Rotation = Quaternion.identity;

        public bool ApplyScaling = true;
        public Vector3 Scaling = Vector3.one;

        public bool ApplyColor = true;
        public Color Color;

        public bool ApplyMetallic = true;
        public float Metallic = 0.5f;

        public bool ApplySmoothness = true;
        public float Smoothness  = 0.5f;

        public override (CloneData, JobHandle) Schedule(CloneData cloneData, JobHandle h)
        {
            return (cloneData, new SetValueJob()
                {
                    CloneData = cloneData,
                    ApplyRotation = ApplyRotation,
                    ApplyScaling = ApplyScaling,
                    ApplyTranslation = ApplyTranslation,
                    UseSelection = UseSelection,
                    ApplyColor = ApplyColor,
                    HardOrSoftSelection = HardOrSoftSelection,
                    Translation = Translation,
                    Rotation = Rotation,
                    Scaling = Scaling,
                    ApplyMetallic = ApplyMetallic,
                    Metallic = Metallic,
                    ApplySmoothness = ApplySmoothness,
                    Smoothness = Smoothness,
                    Color = new float4(Color.r, Color.g, Color.b, Color.a),
                }
                .Schedule(cloneData.Count, 1, h));
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct SetValueJob : IJobParallelFor
    {
        public CloneData CloneData;

        public bool UseSelection;
        public bool HardOrSoftSelection;

        public bool ApplyTranslation;
        public float3 Translation;

        public bool ApplyRotation;
        public quaternion Rotation;

        public bool ApplyScaling;
        public float3 Scaling;

        public bool ApplyColor;
        public float4 Color;

        public bool ApplySmoothness;
        public float Smoothness;

        public bool ApplyMetallic;
        public float Metallic;

        public void Execute(int i)
        {
            var sel = CloneData.CpuInstance(i).Selection;
            if (HardOrSoftSelection)
                sel = sel <= 0.5 ? 0f : 1f;
            if (!UseSelection)
                sel = 1f;

            if (ApplyColor)
                CloneData.GpuInstance(i).Color = math.lerp(CloneData.GpuInstance(i).Color, Color, sel);
            if (ApplyTranslation)
                CloneData.GpuInstance(i).Pos = math.lerp(CloneData.GpuInstance(i).Pos, Translation, sel);
            if (ApplyScaling)
                CloneData.GpuInstance(i).Scl = math.lerp(CloneData.GpuInstance(i).Scl, Scaling, sel);
            if (ApplyRotation)
                CloneData.GpuInstance(i).Rot = math.slerp(CloneData.GpuInstance(i).Rot, Rotation, sel);
            if (ApplySmoothness)
                CloneData.GpuInstance(i).Smoothness = math.lerp(CloneData.GpuInstance(i).Smoothness, Smoothness, sel);
            if (ApplyMetallic)
                CloneData.GpuInstance(i).Metallic = math.lerp(CloneData.GpuInstance(i).Metallic, Metallic, sel);
        }

        /*
         * double gauss(double x, double a, double b, double c)
        {
            var v1 = (x - b);
            var v2 = (v1 * v1) / (2 * (c*c));
            var v3 = a * Math.Exp(-v2);
            return v3;
        }
         */
    }

}