using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerSelection : ClonerJobComponent
    {
        [Range(0, 1)] public float Center = 0.5f;
        [Range(0, 1)] public float Extent = 1f;
        public bool Invert;
        public bool Randomize;
        public uint Seed = 428749;
        [Range(0, 1)] public float MinStrength = 0f;
        [Range(0, 1)] public float MaxStrength = 1f;

        public override (CloneData, JobHandle) Schedule(CloneData previousData, JobHandle previousHandle)
        {
            return (previousData, new SelectJob()
                {
                    CloneData = previousData,
                    Center = Center,
                    Extent = Extent,
                    Invert = Invert,
                    Randomize = Randomize,
                    Seed = Seed,
                    MinStrength = MinStrength,
                    MaxStrength = MaxStrength,
                }
                .Schedule(previousData.Count, 1, previousHandle));
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct SelectJob : IJobParallelFor
    {
        public CloneData CloneData;
        public float Center;
        public float Extent;
        public bool Invert;
        public bool Randomize;
        public uint Seed;
        public float MinStrength;
        public float MaxStrength;

        public void Execute(int i)
        {
            if (Randomize)
            {
                var rng = new Rng(Seed, (uint)(i + 35000));
                CloneData.CpuInstance(i).Selection = rng.Float;
                return;
            }

            var relIndex = (float)i / CloneData.Count;
            var halfExtent = Extent / 2;
            var dist = math.abs(Center - relIndex);
            if (dist > halfExtent) return;
            var amount = dist / halfExtent;

            var sel = math.lerp(MaxStrength, MinStrength, amount);
            if (Invert) sel = 1.0f - sel;
            CloneData.CpuInstance(i).Selection = sel;
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