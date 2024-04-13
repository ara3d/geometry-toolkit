using Unity.Jobs;
using UnityEngine;

namespace Assets.ClonerExample
{
    public abstract class ClonerJobComponent : MonoBehaviour
    {
        public abstract (CloneData, JobHandle) Schedule(CloneData previousData, JobHandle previousHandle);

        /// <summary>
        /// Called once per-frame, the presence of an empty function assures that the user is provided
        /// an enable/disable checkbox on the component. 
        /// </summary>
        public void Update()
        {
        }
    }
}