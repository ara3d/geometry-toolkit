 using System;
 using System.Collections.Generic;
 using Ara3D.Geometry;
 using Unity.Mathematics;
 using UnityEngine;
 using Vector3 = UnityEngine.Vector3;

 namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class PolyCurve : MonoBehaviour, ICurve3D, IPipelineComponent
    {
        public bool Closed => true;

        public List<Vector3> Points = new()
        {
            new Vector3(0, 0, 0),
            new Vector3(0.5f, 0, 0.5f),
            new Vector3(1f, 0, 0.5f),
        };

        public Vector3 EvalBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
            => (1 - t) * (1 - t) * (1 - t) * p0 + 3 * (1 - t) * (1 - t) * t * p1 + 3 * (1 - t) * t * t * p2 + t * t * t * p3;

        public Ara3D.Mathematics.Vector3 Eval(float x)
        {
            if (Points.Count < 3) return (0,0,0);
            var segs = Points.Count - 2;
            var i = (int)math.floor(segs * x);
            var t = (segs * x) - i;
            var p0 = Points[i];
            var p1 = Points[(i + 1) % Points.Count];
            var p2 = Points[(i + 2) % Points.Count];
            var p3 = Points[(i + 3) % Points.Count];
            var r = EvalBezier(t, p0, p1, p2, p3);
            return (r.x, r.y, r.z);
        }

        public void SetInput(object _) 
        {}

        public object GetOutput()
            => this;

        public Type GetInputType()
            => typeof(void);

        public Type GetOutputType()
            => typeof(ICurve3D);
    }


    /*
     * What can flow?
     
     * Discrete stuff 
     * - Clones
     * - Points
     * - Voxels 
     * - Tri Mesh
     * - Quad Mesh
     * - Poly Curve
     * - Bitmap
     * - Particles
     * - Lines
     * - Strands
     * - Rigid bodies
     * - Constraints
     * - Settings
     * - Scenes
     * - Tabular Data
     * - Agents (goals)
     *
     * Continuous stuff
     * - Surface
     * - 3D Vector Field
     * - 3D Distance Field
     * - 2D Vector Field
     * - 2D Distance Field
     * - Procedural map
     * - Curve 
     *
     * Generic Stuff 
     * - Array of X 
     * - Array of Array of X
     *
     * Things you can do:
     * - Transform
     * - Deform
     * - Select (Choose 1, Choose X, Soft Select) 
     * - Repeat
     * - Animate
     * - Convert
     * - Render
     * - Smooth
     * - Sample
     * - Generate
     * - Distance
     * - Packing
     * - Scatter
     * - Edit (Move, Add, Delete: one, some, all)
     * - Tessellate / Subdivide
     * - Distance
     * - Redistribute
     * - Randomize / Apply noise
     * - Use curves to drive things 
     *
     * Everything needs a default rendering mode.
     * Things can refer to other things ... 
     */
}