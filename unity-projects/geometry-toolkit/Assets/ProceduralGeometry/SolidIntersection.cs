using System.Collections.Generic;
using Ara3D.Geometry;
using Ara3D.UnityBridge;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(LineDrawer))]
public class SolidIntersection : MonoBehaviour
{
    public Solid A;
    public Solid B;
    
    public int MarchingCubeGrid = 20;

    public void Update()
    {
        var tmp = ParametricSurfaceExtensions
            .ComputeIntersectionCurves(A.Interface, B.Interface, MarchingCubeGrid);

        // draw segments
        var lineDrawer = GetComponent<LineDrawer>();
        if (lineDrawer != null)
        {
            var lineSegments = new List<List<Vector3>>();
            foreach (var uvList in tmp)
            {
                var points = new List<Vector3>();
                foreach (var uv in uvList)
                {
                    var p = A.Interface.Eval(uv).Position.ToUnity();
                    points.Add(p);
                }

                lineSegments.Add(points);
            }

            lineDrawer.DrawLineSegments(lineSegments);
        }

    }
}