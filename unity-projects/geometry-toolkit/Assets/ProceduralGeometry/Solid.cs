using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry;
using Ara3D.UnityBridge;
using UnityEngine;
using Vector2 = Ara3D.Mathematics.Vector2;
using Vector3 = Ara3D.Mathematics.Vector3;

[ExecuteAlways]
[RequireComponent(typeof(LineDrawer))]
public class Solid : ProceduralGeometryObject
{
    public int USegments = 20;
    public int VSegments = 20;

    public enum GeometryType
    {
        Sphere,
        Cylinder
    }

    public Transform Other;

    public int MarchingCubes = 20;

    public float Distance;
    public UnityEngine.Vector2 UvOnSurface;
    public UnityEngine.Vector3 PointOnSurface;
    public UnityEngine.Vector3 NormalOnSurface;

    public List<UnityEngine.Vector2> IntersectionPoints0 = new List<UnityEngine.Vector2>();
    public List<UnityEngine.Vector2> IntersectionPoints1 = new List<UnityEngine.Vector2>();
    public List<UnityEngine.Vector2> IntersectionPoints2 = new List<UnityEngine.Vector2>();

    public GeometryType TypeA = GeometryType.Sphere;
    public GeometryType TypeB = GeometryType.Sphere;

    public static ISolid ToSolid(GeometryType type)
    {
        switch (type)
        {
            case GeometryType.Sphere: return Solids.Sphere;
            case GeometryType.Cylinder: return Solids.Cylinder;
            default: return Solids.Sphere;
        }
    }

    // TODO: move this to a utility class
    public static Ara3D.Mathematics.Matrix4x4 ToMatrix(Transform t)
    {
        var p = t.position;
        var r = t.rotation;
        var s = t.localScale;
        return Ara3D.Mathematics.Matrix4x4.CreateTRS(p.ToAra3D(), r.ToAra3D(), s.ToAra3DScale());
    }

    public static UnityEngine.Vector2 ToUnity(Vector2 v) 
        => new UnityEngine.Vector2(v.X, v.Y);

    public static List<UnityEngine.Vector2> ToPoints(List<List<Vector2>> araPointsList, int i)
        => i < araPointsList.Count ? ToPoints(araPointsList[i]) : new List<UnityEngine.Vector2>();

    public static List<UnityEngine.Vector2> ToPoints(IEnumerable<Vector2> araPoints)
        => araPoints.Select(ToUnity).ToList();

    public override ITriMesh ComputeGeometry()
    {
        var ma = ToMatrix(gameObject.transform);
        var mb = ToMatrix(Other);

        var solidA = ToSolid(TypeA).Transform(ma);
        var solidB = ToSolid(TypeB).Transform(mb);

        UvOnSurface = ToUnity(solidA.Uv(mb.Translation));
        PointOnSurface = solidA.SurfacePosition(mb.Translation).ToUnity();
        NormalOnSurface = solidA.SurfaceNormal(mb.Translation).ToUnity();
        Distance = solidA.Distance(mb.Translation);

        var tmp = ParametricSurfaceExtensions.ComputeIntersectionCurves(solidA, solidB, MarchingCubes, MarchingCubes);
        IntersectionPoints0 = ToPoints(tmp, 0);
        IntersectionPoints1 = ToPoints(tmp, 1);
        IntersectionPoints2 = ToPoints(tmp, 2);

        // draw segments
        var lineDrawer = GetComponent<LineDrawer>();
        if (lineDrawer != null)
        {
            var lineSegments = new List<List<UnityEngine.Vector3>>();
            foreach (var uvList in tmp)
            {
                var points = new List<UnityEngine.Vector3>();
                foreach (var uv in uvList)
                {
                    var p = solidA.Eval(uv).Position.ToUnity();
                    points.Add(p);
                }
                lineSegments.Add(points);
            }
            lineDrawer.DrawLineSegments(lineSegments);
        }

        return solidA.ToMesh(USegments, VSegments).Triangulate();
    }
}