using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerSelectRange : ClonerJobComponent
    {
        public int From = 0;
        public int Count = 1000 * 1000 * 1000;
        public int Stride = 1;
        [Range(0, 1)] public float Strength = 1f;
        
        public override (CloneData, JobHandle) Schedule(CloneData previousData, JobHandle previousHandle)
        {
            return (previousData, new SelectRangeJob()
            {
                Data = previousData,
                From = From,
                Count = Count,
                Stride = Stride,
                Strength = Strength
            }
                .Schedule(previousData.Count, 16, previousHandle));
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct SelectRangeJob : IJobParallelFor
    {
        public CloneData Data;
        public int From;
        public int Count;
        public int Stride;
        public float Strength;

        public void Execute(int i)
        {
            var n = i - From;
            Data.CpuInstance(i).Selection = n >= 0 && n < Count && (n % Stride == 0) ? Strength : 0;
        }
    }
}