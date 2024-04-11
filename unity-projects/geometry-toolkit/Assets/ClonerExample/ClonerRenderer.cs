using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    [RequireComponent(typeof(ClonerComponent))]
    public class ClonerRenderer : MonoBehaviour
    {
        public Mesh mesh; 
        public Material material;
        public CloneRenderData CloneRenderData;
        public ShadowCastingMode shadowCasting = ShadowCastingMode.Off;
        public bool receiveShadows = false;

        public float elasped = 0;
        public float maxElapsed = 0.2f;

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

            //if ((elasped += Time.deltaTime) > maxElapsed)
            {
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
                CloneRenderData.UpdateGpuData(mesh, data.GpuArray, material);
                elasped = 0;
            }

            CloneRenderData.Render(shadowCasting, receiveShadows);
        }
    }
}