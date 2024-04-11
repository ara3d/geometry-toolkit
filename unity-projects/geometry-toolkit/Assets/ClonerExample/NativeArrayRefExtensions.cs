using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Assets.ClonerExample
{
    public static class NativeArrayRefExtensions
    {
        public static unsafe ref T GetRef<T>(this NativeArray<T> array, int index)
            where T : struct
            => ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
    }
}