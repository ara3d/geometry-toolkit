using Unity.Jobs;
using UnityEngine;

namespace Assets.ClonerExample
{
    public abstract class ClonerJobComponent : MonoBehaviour
    {
        public abstract (CloneData, JobHandle) Schedule(CloneData previousData, JobHandle previousHandle);
    }
}