using Ara3D.Geometry;
using UnityEngine;

namespace Ara3D.UnityBridge
{
    [ExecuteAlways]
    public abstract class ProceduralGeometryObject : MonoBehaviour
    {
        public Material Material;
        public bool ZUp;
        public bool FlipTriangles;

        public void Update()
        {
            var mesh = ComputeGeometry().ToUnity(ZUp, FlipTriangles);
            var rp = new RenderParams(Material);
            UnityEngine.Graphics.RenderMesh(rp, mesh, 0, transform.localToWorldMatrix);
        }

        public abstract ITriMesh ComputeGeometry();
    }
}
