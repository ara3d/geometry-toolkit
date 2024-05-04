using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerRenderer : MonoBehaviour
    {
        public Mesh mesh; 
        public Material material;
        private Material _material;
        public CloneRenderData _cloneRenderData;
        public ShadowCastingMode shadowCasting = ShadowCastingMode.Off;
        public bool receiveShadows = false;

        public void OnEnable()
        {
            _cloneRenderData = new CloneRenderData();
        }

        public void OnValidate()
        {
            _material = null;
        }

        public void OnDisable()
        {
            _cloneRenderData.Dispose();
            _cloneRenderData = null;
        }

        public void Update()
        {
            Debug.Assert(mesh != null);
            Debug.Assert(material != null);
            Debug.Assert(_cloneRenderData != null);

            if (_material == null)
            {
                _material = new Material(material);
            }

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
            if (_cloneRenderData == null)
            {
                Debug.LogError("Clone render data is not created");
                return;
            }
            _cloneRenderData.UpdateGpuData(mesh, data.GpuArray, _material);
            _cloneRenderData.Render(shadowCasting, receiveShadows);
        }
    }
}