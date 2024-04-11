using Unity.Collections;
using UnityEngine;

namespace Assets.ClonerExample
{
    public struct CloneData
    {
        public int Count => CpuArray.Length;

        public NativeArray<CpuInstanceData> CpuArray;
        public NativeArray<GpuInstanceData> GpuArray;

        public ref CpuInstanceData CpuInstance(int i) => ref CpuArray.GetRef(i);
        public ref GpuInstanceData GpuInstance(int i) => ref GpuArray.GetRef(i);

        public void Resize(int n)
        {
            if (n == Count && CpuArray.IsCreated && GpuArray.IsCreated)
                return;

            if (CpuArray.IsCreated) CpuArray.Dispose();
            if (GpuArray.IsCreated) GpuArray.Dispose();

            CpuArray = new NativeArray<CpuInstanceData>(n, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            GpuArray = new NativeArray<GpuInstanceData>(n, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        }

        public void Replicate(CloneData other, int count)
        {
            var n = other.Count * count;
            Resize(n);
        }

        public void Dispose()
        {
            CpuArray.Dispose();
            GpuArray.Dispose();
        }
    }
}