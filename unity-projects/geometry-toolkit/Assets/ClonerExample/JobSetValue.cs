using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.ClonerExample
{
    [BurstCompile(CompileSynchronously = true)]
    public struct JobSetValue : IJobParallelFor
    {
        public CloneData Data;

        public bool UseSelection;
        public float Strength;
        
        public bool SetTranslation;
        public float3 Translation;

        public bool SetRotation;
        public quaternion Rotation;

        public bool SetScaling;
        public float3 Scaling;

        public bool SetColor;
        public float4 Color;

        public bool SetSmoothness;
        public float Smoothness;

        public bool SetMetallic;
        public float Metallic;

        public void Execute(int i)
        {
            var sel = UseSelection 
                ? math.lerp(0, Data.CpuInstance(i).Selection, Strength) 
                : Strength;
            if (SetColor)
                Data.GpuInstance(i).Color = math.lerp(Data.GpuInstance(i).Color, Color, sel);
            if (SetTranslation)
                Data.GpuInstance(i).Pos = math.lerp(Data.GpuInstance(i).Pos, Translation, sel);
            if (SetScaling)
                Data.GpuInstance(i).Scl = math.lerp(Data.GpuInstance(i).Scl, Scaling, sel);
            if (SetRotation)
                Data.GpuInstance(i).Rot = math.slerp(Data.GpuInstance(i).Rot, Rotation, sel);
            if (SetSmoothness)
                Data.GpuInstance(i).Smoothness = math.lerp(Data.GpuInstance(i).Smoothness, Smoothness, sel);
            if (SetMetallic)
                Data.GpuInstance(i).Metallic = math.lerp(Data.GpuInstance(i).Metallic, Metallic, sel);
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