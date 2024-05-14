using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;


namespace Assets.ClonerExample
{
    public interface ICloneJob
    {
        JobHandle Schedule(ICloneJob previous);
        JobHandle Handle { get; }
        ref CloneData CloneData { get; }
        int Count { get; }
    }

    public interface IPolyCurveComponent
    {
        IPolyCurve CurveData { get; }
    }

    public interface IJobData
    { }

    public interface IPolyCurve : IJobData
    {
        public NativeArray<float3> Points { get; }
    }

    public interface IJobDataArray<out T> where T : IJobData
    {
        public int Count { get; }
        public T this[int index] { get; }
    }

    public interface INoData : IJobData
    { }

    public class NoData : INoData
    {
        public static NoData Instance = new NoData();
    }

    public interface IPoints : IJobData
    {
        ref NativeArray<float3> Points { get; }
    }

    public interface IVoxels : IJobData
    {
        ref VoxelData<float> Voxels { get; }
    }

    public interface IClones : IJobData
    {
        ref CloneData CloneData { get; }
    }

    public interface IMesh : IJobData
    {
    }

    public interface IJobResult 
    {
        JobHandle Handle { get; }
    }

    public interface IJobResult<out TOutput>
        : IJobResult
        where TOutput : IJobData
    {
        TOutput Result { get; }
    }

    public class JobResult<TOutput> : IJobResult<TOutput>
        where TOutput : IJobData
    {
        public JobResult(TOutput result, JobHandle handle)
        {
            Result = result;
            Handle = handle;
        }

        public TOutput Result { get; }
        public JobHandle Handle { get; }
    }

    public interface IJobScheduler 
    {
    }

    public interface IJobScheduler<out TOutput>
        : IJobScheduler
        where TOutput : IJobData
    {
        IJobResult<TOutput> Schedule();
    }

    public abstract class JobScheduler<TInput, TOutput> :
        MonoBehaviour,
        IJobScheduler<TOutput>
        where TInput : IJobData
        where TOutput : IJobData
    {
        public abstract JobHandle ScheduleJob(TInput inputData, JobHandle previousHandle);

        public abstract TOutput Result { get; }

        public IJobResult<TOutput> Schedule()
        {
            var previous = this.GetPreviousComponent<IJobScheduler<TInput>>();
            var previousResult = previous != null ? previous.Schedule() : null;
            var h = previousResult != null
                ? ScheduleJob(previousResult.Result, previousResult.Handle)
                : ScheduleJob(default, default);
            return new JobResult<TOutput>(Result, h);
        }
    }

    public static class JobSchedulerExtensions
    {

        public static TOutput ScheduleNow<TOutput>(this IJobScheduler<TOutput> self)
            where TOutput : IJobData
        {
            var r = self.Schedule();
            r.Handle.Complete();
            return r.Result;
        }
    }
}