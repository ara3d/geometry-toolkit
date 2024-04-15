using System;

namespace Assets.ClonerExample
{
    public static class Rng
    {
        public static ulong GetNth(ulong seed, ulong index) => Hasher.Hash(seed + index);
        public static float HashToFloat(ulong hash) => ((uint)hash) / (float)uint.MaxValue;
        public static float GetNthFloat(ulong seed, ulong index) => HashToFloat(GetNth(seed, index));
        public static int GetNthInt(ulong seed, int index, int maxValue = int.MaxValue) => Math.Abs((int)GetNth(seed, (ulong)index) % maxValue);
    }
}