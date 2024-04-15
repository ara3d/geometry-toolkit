using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerColorize : ClonerJobComponent
    {
        [Range(0, 1)] public float Hue;
        [Range(0, 1)] public float Saturation;
        public Color[] Colors;
        [Range(0, 1)] public float Strength = 1.0f;
        private NativeArray<float4> cachedColors = new NativeArray<float4>();
        public void OnValidate()
        {
            Update();
        }

        public void Update()
        {
            Colors = ColorPaletteGenerator.GenerateTriadColors(Hue * 360f, Saturation);
        }

        public override (CloneData, JobHandle) Schedule(CloneData previousData, JobHandle previousHandle)
        {
            Colors = ColorPaletteGenerator.GenerateTriadColors(Hue * 360f, Saturation);
            cachedColors.Assign(Colors, ToFloat4);

            return (previousData, new JobAssignColors()
            {
                Colors = cachedColors,
                Data = previousData,
                Strength = Strength,
            }
            .Schedule(previousData.Count, 32, previousHandle));
        }

        public static float4 ToFloat4(Color c)
            => new float4(c.r, c.g, c.b, c.a);
    }
}