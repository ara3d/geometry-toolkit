using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Assets.ClonerExample
{
    public static class NativeArrayRefExtensions
    {
        public static ref T GetRef<T>(this NativeArray<T> array, int index)
            where T : struct
        {
            // You might want to validate the index first, as the unsafe method won't do that.
            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            unsafe
            {
                return ref UnsafeUtility.ArrayElementAsRef<T>(array.GetUnsafePtr(), index);
            }
        }
    }
}