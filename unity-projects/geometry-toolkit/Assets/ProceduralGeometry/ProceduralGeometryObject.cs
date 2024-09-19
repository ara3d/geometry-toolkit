using Ara3D.Geometry;
using Assets.ClonerExample;
using UnityEngine;

namespace Ara3D.UnityBridge
{
    [ExecuteAlways]
    public abstract class ProceduralGeometryObject : FilterComponent<object, ITriMesh>
    {
        public bool Render = true;
        public Material Material;
        public bool FlipTriangles;
        public bool DoubleSided;
        public bool ApplyTransform = true;

        public void Update()
        {
            if (Render)
            {
                var mesh = ComputeGeometry().ToUnity(FlipTriangles, DoubleSided);
                var rp = new RenderParams(Material);
                UnityEngine.Graphics.RenderMesh(rp, mesh, 0, ApplyTransform ? transform.localToWorldMatrix : Matrix4x4.identity);
            }
        }

        public override ITriMesh EvalImpl(object input)
        {
            return ComputeGeometry();
        }

        public abstract ITriMesh ComputeGeometry();
    }
}
