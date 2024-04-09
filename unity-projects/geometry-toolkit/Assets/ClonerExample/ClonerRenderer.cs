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
        public CloneData cloneData;
        public ShadowCastingMode shadowCasting = ShadowCastingMode.Off;
        public bool receiveShadows = false;

        public float elaspedSinceRecompute = 0;
        public float maxElapsed = 0.1f;

        public void OnEnable()
        {
            cloneData = new CloneData();
        }

        public void OnDisable()
        {
            cloneData.Dispose();
            cloneData = null;
        }

        public void Update()
        {
            Debug.Assert(mesh != null);
            Debug.Assert(material != null);
            Debug.Assert(cloneData != null);

            if ((elaspedSinceRecompute += Time.deltaTime) > maxElapsed)
            {
                var component = GetComponent<ClonerComponent>();
                cloneData.UpdateGpuData(component.Instances, material);
                elaspedSinceRecompute = 0;
            }

            cloneData.Render(mesh, shadowCasting, receiveShadows);
        }
    }
}