using Assets.ClonerExample;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[ExecuteAlways]
public class VoxelsToPoints : JobScheduler<IVoxels, IPoints>, IPoints
{
    public float Threshold;
    private NativeQueue<float3> _queue;
    private VoxelsToQueueJob _job;

    public ref NativeArray<float3> Points => ref _points;
    public NativeArray<float3> _points;
    public override IPoints Result => this;

    public void OnDisable()
    {
        _queue.SafeDispose();
        _points.SafeDispose();
    }

    public override JobHandle ScheduleJob(IVoxels inputData, JobHandle previous)
    {
        var voxels = inputData.Voxels;
        _queue = new NativeQueue<float3>(Allocator.Persistent);

        _job.Voxels = voxels;
        _job.PointWriter = _queue.AsParallelWriter();
        _job.Threshold = Threshold;

        var h = _job.Schedule(voxels.VoxelCount, 64, previous);
        h.Complete();

        _points = _queue.ToArray(Allocator.Persistent);
        return h;
    }
}

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
public struct VoxelsToQueueJob : IJobParallelFor
{
    [ReadOnly] public VoxelData<float> Voxels;
    [WriteOnly] public NativeQueue<float3>.ParallelWriter PointWriter;
    [ReadOnly] public float Threshold;

    public void Execute(int i)
    {
        var f = Voxels.GetValue(i);
        if (f > Threshold)
        {
            PointWriter.Enqueue(Voxels.GetCenter(i));
        }
    }
}

