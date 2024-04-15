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
            return (previousData, new JobSelectRange()
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
}