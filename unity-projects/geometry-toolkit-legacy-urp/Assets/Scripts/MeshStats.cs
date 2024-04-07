using UnityEngine;
using Ara3D.UnityBridge;

[ExecuteAlways]
public class MeshStats : MonoBehaviour
{
    public int NumVertices;
    public int NumTriangles;
    public bool HasFilter;
    public bool HasRenderer;
    public int NumSubMeshes;
    public string IndexFormat;
    public bool IsReadable;
    public string Name;

    public void Update()
    {
        OnValidate();
    }

    public void OnValidate()
    {
        var mesh = this.GetMesh();
        if (mesh != null)
        {
            NumVertices = mesh.vertexCount;
            NumSubMeshes = mesh.subMeshCount;
            IndexFormat = mesh.indexFormat.ToString();
            HasFilter = GetComponent<MeshFilter>() != null;
            HasRenderer = GetComponent<MeshRenderer>() != null;
            IsReadable = mesh.isReadable;
            Name = mesh.name;
            NumTriangles = mesh.triangles.Length;
        }
        else
        {
            NumVertices = 0;
            NumSubMeshes = 0;
            IndexFormat = "";
            HasFilter = GetComponent<MeshFilter>() != null;
            HasRenderer = GetComponent<MeshRenderer>() != null;
            IsReadable = false;
            Name = "";
            NumTriangles = 0;
        }
    }
}
