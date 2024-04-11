namespace Assets.ClonerExample
{
    public unsafe struct Rng
    {
        public uint Seed;
        public uint Index;

        public Rng(uint seed, uint index)
            => (Seed, Index) = (seed, index);

        public Rng Next => new Rng(Hash(Seed, Index), Index + 1);

        public static uint Hash(uint h1, uint h2)
        {
            unchecked
            {
                // RyuJIT optimizes this to use the ROL instruction
                // Related GitHub pull request: dotnet/coreclr#1830
                var rol5 = (h1 << 5) | (h1 >> 27);
                return (rol5 + h1) ^ h2;
            }
        }

        public float Float 
            => ToFloat(Seed);

        public int Int 
            => ToInt(Seed);

        public static float ToFloat(uint val)
            => *(float*)(&val);

        public static int ToInt(uint val)
            => *(int*)(&val);

        public (Rng, float) NextFloat()
            => (Next, Float);

        public (Rng, int) NextInt()
            => (Next, Int);

        public (Rng, float[]) NextFloats(int n)
        {
            var array = new float[n];
            var rng = this;
            for (var i = 0; i < n; i++)
            {
                array[i] = rng.Float;
                rng = rng.Next;
            }
            return (rng, array);
        }
    }
}