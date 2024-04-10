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

        public float elaspedSinceRecompute = 0;
        public float maxElapsed = 0.1f;

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

            var component = GetComponent<ClonerComponent>();
            
            if (component == null || component.enabled == false)
                return;

            CloneRenderData.UpdateGpuData(mesh, component.GpuArray, material);
            CloneRenderData.Render(shadowCasting, receiveShadows);
        }
    }
}