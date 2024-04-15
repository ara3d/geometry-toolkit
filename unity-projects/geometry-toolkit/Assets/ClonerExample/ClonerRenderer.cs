using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    [RequireComponent(typeof(ClonerInitialize))]
    public class ClonerRenderer : MonoBehaviour
    {
        public Mesh mesh; 
        public Material material;
        public CloneRenderData CloneRenderData;
        public ShadowCastingMode shadowCasting = ShadowCastingMode.Off;
        public bool receiveShadows = false;

       public void OnEnable()
        {
            CloneRenderData = new CloneRenderData();
        }

        public void OnDisable()
        {
            CloneRenderData.Dispose();
            CloneRenderData = null;
        }

        public void Update()
        {
            Debug.Assert(mesh != null);
            Debug.Assert(material != null);
            Debug.Assert(CloneRenderData != null);

            CloneData data = default;
            JobHandle handle = default;
            var jobComponents = gameObject.GetComponents<ClonerJobComponent>();
            foreach (var jc in jobComponents)
            {
                if (jc.enabled)
                {
                    (data, handle) = jc.Schedule(data, handle);
                }
            }
            handle.Complete();
            if (CloneRenderData == null)
            {
                Debug.LogError("Clone render data is not created");
                return;
            }
            CloneRenderData.UpdateGpuData(mesh, data.GpuArray, material);
            CloneRenderData.Render(shadowCasting, receiveShadows);
        }
    }
}