using Ara3D.Geometry;
using UnityEngine;

namespace Ara3D.UnityBridge
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter))]
    public abstract class ProceduralGeometryObject : MonoBehaviour
    {
        private ITriMesh _newGeometry;
        private ITriMesh _oldGeometry;
        private Mesh Mesh;

        public virtual void Reset()
        {
            _newGeometry = null;
            _oldGeometry = null;
            Start();
        }

        public virtual void Update()
        {
            if (Mesh == null)
                Mesh = new Mesh();

            if (_newGeometry == null)
                _newGeometry = ComputeGeometry();

            if (_newGeometry != _oldGeometry)
            {
                _oldGeometry = _newGeometry;
                Mesh.UpdateMesh(_newGeometry);
                GetComponent<MeshFilter>().mesh = Mesh;
            }
        }

        public virtual void OnEnable()
        {
            this.CreateMesh();
        }

        public virtual void Start()
        {
            Update();
        }

        public void OnValidate()
        {
            _newGeometry = ComputeGeometry();
        }

        public abstract ITriMesh ComputeGeometry();
    }
}
