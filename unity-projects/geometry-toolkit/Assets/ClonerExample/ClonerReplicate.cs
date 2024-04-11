using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerReplicate : ClonerJobComponent
    {
        public int Count;
        public Vector3 Translation;
        public Quaternion Rotation;

        public CloneData CloneData;

        public override (CloneData, JobHandle) Schedule(CloneData previousData, JobHandle previousHandle)
        {
            CloneData.Replicate(previousData, Count);
            if (CloneData.Count == 0)
                return (CloneData, default);

            return (CloneData, new ReplicateJob()
                {
                    Count = Count,
                    Translation = Translation,
                    Rotation = Rotation,
                    DataSource = previousData,
                    DataSink = CloneData,
                }
                .Schedule(previousData.Count, 1, previousHandle));
        }

        public void OnDisable()
        {
            {
                CloneData.Dispose();
            }
        }
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct ReplicateJob : IJobParallelFor
    {
        public CloneData DataSource;
        public CloneData DataSink;
        public int Count;
        public float3 Translation;
        public Quaternion Rotation;

        public void Execute(int index)
        {
            Debug.Assert(DataSink.Count == DataSource.Count * Count);
            var pos = DataSource.GpuInstance(index).Pos;
            var rot = DataSource.GpuInstance(index).Rot;
            for (var i = 0; i < Count; ++i)
            {
                var j = index * Count + i;
                DataSink.GpuArray[j] = DataSource.GpuArray[i];
                DataSink.CpuArray[j] = DataSource.CpuArray[i];
                DataSink.GpuInstance(j).Pos = pos;
                DataSink.GpuInstance(j).Rot = rot;
                pos += Translation;
                rot *= Rotation;
            }
        }
    }
}