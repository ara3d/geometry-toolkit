using Ara3D.Geometry;
using Ara3D.UnityBridge;
using UnityEngine;

[ExecuteAlways]
public class Solid : ProceduralGeometryObject
{
    public int Segments = 20;

    public ISolid Interface 
        => ToSolid(Type).Transform(gameObject.transform.ToAraMatrix()); 

    public enum GeometryType
    {
        Sphere,
        Cylinder,
        Cube
    }

    public GeometryType Type = GeometryType.Sphere;

    public static ISolid ToSolid(GeometryType type)
    {
        switch (type)
        {
            case GeometryType.Sphere: return Solids.Sphere;
            case GeometryType.Cylinder: return Solids.Cylinder;
            case GeometryType.Cube: return Solids.Cube;
            default: return Solids.Sphere;
        }
    }

    public override ITriMesh ComputeGeometry()
    {
        return Interface.ToMesh(Segments, Segments).Triangulate();
    }
}