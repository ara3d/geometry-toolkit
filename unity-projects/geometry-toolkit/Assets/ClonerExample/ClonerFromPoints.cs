using System;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerFromPoints : ClonerJobComponent
    {
        public CloneData CloneData;
        public Color Color = Color.blue;
        [Range(0, 1)] public float Metallic = 0.5f;
        [Range(0, 1)] public float Smoothness = 0.5f;

        public override (CloneData, JobHandle) Schedule(CloneData previousData, JobHandle previousHandle, int batchSize)
        {
            var previousJob = this.GetPreviousComponent<IJobScheduler<IPoints>>();
            if (previousJob == null)
                throw new Exception("No points generator found");

            var points = previousJob.ScheduleNow().Points;
            var count = points.Length;

            if (count == 0)
            {
                Debug.Log("No points");
                return (CloneData, previousHandle);
            }

            CloneData.Resize(count);
            var job = new JobInitializeFromPoints(CloneData,
                points,
                new float4(Color.r, Color.g, Color.b, Color.a),
                Metallic,
                Smoothness,
                math.float3(1,1,1));
            var h = job.Schedule(count, batchSize, previousHandle);
            return (CloneData, h);
        }

        void OnDisable()
        {
            CloneData.Dispose();
        }
    }
}