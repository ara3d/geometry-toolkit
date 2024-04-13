using Unity.Jobs;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerOffset : ClonerJobComponent
    {
        public Vector3 Translation;
        public Quaternion Rotation;
        [Range(0, 1)] public float Strength = 1;
        public bool UseSelection;

        public override (CloneData, JobHandle) Schedule(CloneData previousData, JobHandle previousHandle)
        {
            return (previousData,
                new JobOffset() { Data = previousData, Translation = Translation, Rotation = Rotation, Strength = Strength, UseSelection = UseSelection }
                    .Schedule(previousData.Count, 32, previousHandle));
        }
    }
}