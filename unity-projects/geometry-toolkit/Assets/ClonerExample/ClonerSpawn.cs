using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = System.Random;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerSpawn : ClonerJobComponent, ICloneJob
    {
        public CloneData _cloneData;
        public CloneData _previousCloneData;
        public Bounds SpawningRegion = new Bounds(Vector3.zero, Vector3.one * 5);
        public Bounds InitialVelocities = new Bounds(Vector3.one, Vector3.one / 2);
        public float Lifetime;
        public int Seed = 367;
        public int ParticlesPerSecond = 300;
        public NativeList<int> ReclaimList;
        public JobHandle Handle { get; set; }
        public JobHandle ReclaimHandle { get; set; }
        public bool Reset;

        public JobHandle Schedule(ICloneJob previous)
        {
            var t = Time.deltaTime;
            var particleSpawnCount = ParticlesPerSecond * t;
            ReclaimHandle.Complete();
            if (!ReclaimList.IsCreated)
                ReclaimList = new NativeList<int>(Allocator.Persistent);
            
            var newSpaceRequired = (int)(particleSpawnCount - ReclaimList.Length);

            _previousCloneData.Dispose();
            _previousCloneData = _cloneData;
            
            var oldSize = _previousCloneData.Count;
            var newSize = oldSize + newSpaceRequired;

            _cloneData = new CloneData();
            _cloneData.Resize(newSize);

            var minSize = Math.Min(newSize, oldSize);

            Handle = previous?.Handle ?? default;
            if (minSize > 0)
            {
                var jobCopy = new JobCopy(_previousCloneData, _cloneData, 0, 0);
                Debug.Log($"Copying {minSize} items where old was {oldSize} and new is {newSize}");
                Handle = jobCopy.Schedule(minSize, 256, Handle);
            }

            var cpuInst = new CpuInstanceData(Time.time);

            var gpuInst = new GpuInstanceData
            {
                Color = new float4(0.5f, 0.5f, 1, 1),
                Id = 0,
                Metallic = 0.5f,
                Orientation = quaternion.identity,
                Pos = float3.zero,
                Scl = new float3(1, 1, 1),
                Smoothness = 0.5f,
            };

            var jobSpawn = new JobSpawn(_cloneData,
                new float3x2(SpawningRegion.min, SpawningRegion.max),
                new float3x2(InitialVelocities.min, InitialVelocities.max),
                (ulong)Seed,
                cpuInst,
                gpuInst);

            int delta = newSize - oldSize;
            if (delta > 0)
            {
                var jobSlice = new JobSlice<JobSpawn>(jobSpawn, oldSize);
                Debug.Log($"Spawning {delta} objects");
                Handle = jobSlice.Schedule(delta, 256, Handle);
            }

            var jobUpdate = new JobUpdate(_cloneData, Time.time);
            Debug.Log($"Updating {newSize} objects. Previous is {_previousCloneData.Count}, and Current is {_cloneData.Count}");
            Handle = jobUpdate.Schedule(newSize, 256, Handle);
            return Handle;
        }
        
        public ref CloneData CloneData => ref _cloneData;
        public int Count => CloneData.Count;

        public override (CloneData, JobHandle) Schedule(CloneData previousData, JobHandle previousHandle, int batchSize)
        {
            throw new NotSupportedException();
        }

        public void Update()
        {
            if (Reset)
            {
                _previousCloneData.Resize(0);
                _cloneData.Resize(0);
                Reset = false;
            }
        }
    }

    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, Debug = false, DisableSafetyChecks = true)]
    public struct JobGatherReclaimed : IJobFilter
    {
        public JobGatherReclaimed(CloneData data, float currentTime, float maxAge)
            => (Data, CurrentTime, MaxAge) = (data, currentTime, maxAge);

        private CloneData Data;
        private readonly float CurrentTime;
        private readonly float MaxAge;

        public bool Execute(int i)
        {
            return Data.Expired(i, MaxAge, CurrentTime);
        }
    }

    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, Debug = false, DisableSafetyChecks = true)]
    public struct JobSetData : IJobParallelFor
    {
        private CloneData Data;
        private CpuInstanceData CpuInst;
        private GpuInstanceData GpuInst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobSetData(in CloneData cloneData, in CpuInstanceData cpuInst, in GpuInstanceData gpuInst)
        {
            Data = cloneData;
            CpuInst = cpuInst;
            GpuInst = gpuInst;
        }

        [SkipLocalsInit, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(int i)
        {
            Data.GpuInstance(i) = GpuInst;
            Data.CpuInstance(i) = CpuInst;
        }
    }

    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, Debug = false, DisableSafetyChecks = true)]
    public struct JobCopy : IJobParallelFor
    {
        private CloneData OldData;
        private CloneData NewData;
        private int OffsetSource;
        private int OffsetDest;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobCopy(in CloneData oldData, in CloneData newData, int offsetSource, int offsetDest)
        {
            OldData = oldData;
            NewData = newData;
            OffsetSource = offsetSource;
            OffsetDest = offsetDest;
        }

        [SkipLocalsInit, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(int i)
        {
            NewData.GpuInstance(i + OffsetDest) = OldData.GpuInstance(i + OffsetSource);
            NewData.CpuInstance(i + OffsetDest) = OldData.CpuInstance(i + OffsetSource);
        }
    }

    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, Debug = false, DisableSafetyChecks = true)]
    public struct JobSpawnReclaim : IJobParallelFor
    {
        public NativeArray<int> ReclaimList;
        public JobSpawn SpawnJob;

        [SkipLocalsInit, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(int i)
        {
            SpawnJob.Execute(ReclaimList[i]);
        }
    }

    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, Debug = false, DisableSafetyChecks = true)]
    public struct JobSpawn : IJobParallelFor
    {
        private CloneData CloneData;
        private readonly float3x2 PositionRange;
        private readonly float3x2 VelocityRange;
        private readonly ulong Seed;
        private readonly GpuInstanceData GpuInst;
        private readonly CpuInstanceData CpuInst;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobSpawn(in CloneData cloneData, in float3x2 posRange, in float3x2 velRange, ulong seed,             
            in CpuInstanceData cpuInst, in GpuInstanceData gpuInst)
        {
            CloneData = cloneData;
            PositionRange = posRange;
            VelocityRange = velRange;
            Seed = seed;
            GpuInst = gpuInst;
            CpuInst = cpuInst;
        }

        [SkipLocalsInit, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(int i)
        {
            var pos = Rng.GetNthFloat3(Seed, (ulong)i * 2, PositionRange.c0, PositionRange.c1);
            var vel = Rng.GetNthFloat3(Seed, (uint)i * 2 + 1, VelocityRange.c0, VelocityRange.c1);
            CloneData.CpuInstance(i) = CpuInst;
            CloneData.GpuInstance(i) = GpuInst;
            CloneData.CpuInstance(i).Velocity = vel;
            CloneData.GpuInstance(i).Pos = pos;
        }
    }

    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, Debug = false, DisableSafetyChecks = true)]
    public struct JobIndirect<T> : IJobParallelForDefer where T: struct, IJobParallelFor
    {
        private T _job;
        private NativeArray<int> _indices;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobIndirect(in T job, NativeList<int> indices)
        {
            _job = job;
            _indices = indices.AsDeferredJobArray();
        }

        [SkipLocalsInit, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(int i)
        {
            _job.Execute(_indices[i]);
        }
    }

    [BurstCompile(CompileSynchronously = true, FloatMode = FloatMode.Fast, OptimizeFor = OptimizeFor.Performance, Debug = false, DisableSafetyChecks = true)]
    public struct JobSlice<T> : IJobParallelFor where T : struct, IJobParallelFor
    {
        private T _job;
        private int _offset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobSlice(in T job, int offset)
        {
            _job = job;
            _offset = offset;
        }

        [SkipLocalsInit, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Execute(int i)
        {
            _job.Execute(i + _offset);
        }
    }

    public static class CloneDataJobExtensions
    {
        public static JobHandle ScheduleResize(this ref CloneData dest, ref CloneData previous, int newSize,
            in CpuInstanceData cpuInst, in GpuInstanceData gpuInst, JobHandle previousJob = default)
        {
            dest.Resize(newSize);
            var oldSize = previous.Count;
            var minSize = Math.Min(newSize, oldSize);
            if (minSize > 0)
            {
                var jobCopy = new JobCopy(previous, dest, 0, 0);
                previousJob = jobCopy.Schedule(minSize, 256, previousJob);
            }

            if (newSize > oldSize)
            {
                var jobSlice = new JobSlice<JobSetData>(new JobSetData(dest, cpuInst, gpuInst), oldSize);
                previousJob = jobSlice.Schedule(newSize - oldSize, 256, previousJob);
            }

            return previousJob;
        }
    }
}