using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;

namespace Assets.ClonerExample
{
    [BurstCompile(CompileSynchronously = true)]
    public struct JobOffset : IJobParallelFor
    {
        public CloneData Data;
        public float3 Translation;
        public quaternion Rotation;
        public float Strength;
        public bool UseSelection; 

        public void Execute(int i)
        {
            var a = UseSelection 
                ? math.lerp(0, Data.CpuInstance(i).Selection, Strength) 
                : Strength;
            var p0 = Data.GpuInstance(i).Pos;
            var r0 = Data.GpuInstance(i).Rot;
            var r1 = math.mul(r0, Rotation);
            var r2 = math.slerp(r0, r1, a);
            Data.GpuInstance(i).Rot = r2;
            var t1 = math.mul(r0, Translation);
            var p1 = p0 + t1;
            var p2 = math.lerp(p0, p1, a);
            Data.GpuInstance(i).Pos = p2;
        }
    }
}