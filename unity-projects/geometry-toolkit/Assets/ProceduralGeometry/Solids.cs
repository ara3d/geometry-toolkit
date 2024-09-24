using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Collections;
using Ara3D.Mathematics;
using Ara3D.UnityBridge;
using UnityEngine;
using Matrix4x4 = Ara3D.Mathematics.Matrix4x4;
using Quaternion = Ara3D.Mathematics.Quaternion;
using Vector2 = Ara3D.Mathematics.Vector2;
using Vector3 = Ara3D.Mathematics.Vector3;

namespace Ara3D.Geometry
{

    public interface IPolyline
    {
        IReadOnlyList<Vector2> Points { get; }
    }

    public interface ISolid
    {
        SurfacePoint Eval(Vector2 uv);
        Vector2 Uv(Vector3 point);
    }

    public readonly struct SurfacePoint
    {
        public readonly Vector3 Position;
        public readonly Vector3 Normal;

        public SurfacePoint(Vector3 position, Vector3 normal) =>
            (Position, Normal) = (position, normal);

        public static implicit operator (Vector3, Vector3)(SurfacePoint p) =>
            (p.Position, p.Normal);

        public static implicit operator SurfacePoint((Vector3, Vector3) t) =>
            new SurfacePoint(t.Item1, t.Item2);

        public SurfacePoint Scale(float f)
            => (Position * f, Normal);

        public SurfacePoint Translate(Vector3 v)
            => (Position + v, Normal);

        public SurfacePoint Rotate(Quaternion q)
            => Transform(q.ToMatrix());

        public SurfacePoint Transform(Matrix4x4 mat)
            => (Position.Transform(mat), Normal.TransformNormal(mat));
    }

    public class Solid : ISolid
    {
        public Func<Vector2, SurfacePoint> UvToSurface { get; }
        public Func<Vector3, Vector2> PointToUv { get; }
        public SurfacePoint Eval(Vector2 uv) => UvToSurface(uv);
        public Vector2 Uv(Vector3 point) => PointToUv(point);
        public Solid(Func<Vector2, SurfacePoint> f, Func<Vector3, Vector2> g) => (UvToSurface, PointToUv) = (f, g);
    }

    public static class Solids
    {
        public static float eps = 1e-7f;
        
        public static Solid Sphere => new Solid(SpherePoint, PointToSphereUv);
        public static Solid Cylinder => new Solid(CylinderPoint, PointToCylinderUv);
        public static Solid Cube => new Solid(BoxPoint, PointToBoxUV);

        public static SurfacePoint SpherePoint(Vector2 uv)
        {
            // Spherical coordinates from UV
            var theta = uv.X * 2 * (float)Math.PI; // Longitude
            var phi = uv.Y * (float)Math.PI; // Latitude

            // Convert spherical coordinates to Cartesian coordinates (position on the sphere)
            var position = new Vector3(
                (float)Math.Sin(phi) * (float)Math.Cos(theta),
                (float)Math.Sin(phi) * (float)Math.Sin(theta),
                (float)Math.Cos(phi)
            );

            // The normal is the same as the position vector (since it's a unit sphere)
            var normal = position.Normalize();

            return (position, normal);
        }

        public static SurfacePoint CylinderPoint(Vector2 uv)
        {
            var theta = uv.X * 2 * (float)Math.PI; // Angle around the cylinder

            if (uv.Y <= 0.25f) // Bottom face (v = [0, 0.25])
            {
                var r = uv.Y / 0.25f; // Maps v from [0, 0.25] to radius 0-1
                var position = new Vector3(r * (float)Math.Cos(theta), r * (float)Math.Sin(theta), 0);
                var normal = -Vector3.UnitZ; // Normal points downwards for the bottom face
                return (position, normal);
            }
            else if (uv.Y <= 0.75f) // Side of the cylinder (v = [0.25, 0.75])
            {
                var z = (uv.Y - 0.25f) * 2.0f; 
                var position = new Vector3((float)Math.Cos(theta), (float)Math.Sin(theta), z);
                var normal = new Vector3((float)Math.Cos(theta), (float)Math.Sin(theta), 0);
                return (position, normal);
            }
            else // Top face (v = [0.75, 1.0])
            {
                var r = (1.0f - uv.Y) / 0.25f; // Maps v from [0.75, 1.0] to radius 1-0
                var position = new Vector3(r * (float)Math.Cos(theta), r * (float)Math.Sin(theta), 1);
                var normal = Vector3.UnitZ; // Normal points upwards for the top face
                return (position, normal);
            }
        }

        /// <summary>
        /// Computes the closest UV coordinates on a sphere for a given 3D point.
        /// </summary>
        public static Vector2 PointToSphereUv(Vector3 p)
        {
            // Calculate the length of the vector
            var length = p.Length();
            if (length < eps)
            {
                // If the vector is zero, return the center of the texture
                return (0.5f, 0.5f);
            }

            // Normalize the vector to lie on the sphere
            var n = p.Normalize();

            // Compute the spherical coordinates from the normalized Cartesian coordinates
            var theta = Math.Atan2(n.Y, n.X); // Longitude
            var phi = Math.Acos(n.Z); // Latitude

            // Convert spherical coordinates to UV
            var u = theta / (2 * Math.PI); // Convert longitude to [0, 1] range
            var v = phi / Math.PI; // Convert latitude to [0, 1] range

            // Ensure u and v are within [0, 1]
            u %= 1.0;
            if (u < 0) u += 1.0; // Handle negative values

            return ((float)u, (float)v);
        }

        /// <summary>
        /// Computes the closest UV coordinates on a cylinder for a given 3D point.
        /// </summary>
        public static Vector2 PointToCylinderUv(Vector3 p)
        {
            var theta = MathF.Atan2(p.Z, p.X);
            if (theta < 0) theta += 2 * MathF.PI; // Ensure theta is in the range [0, 2π]

            var u = theta / (2 * MathF.PI); // Maps theta to u in [0, 1]

            var r = MathF.Sqrt(p.X * p.X + p.Z * p.Z); // Radial distance

            if (Math.Abs(p.Y + 1.0f) < eps) // Bottom face (z ≈ -1)
            {
                var v = Math.Clamp(r, 0, 1) * 0.25f; // Maps radial distance to v in [0, 0.25]
                return new Vector2(u, v);
            }
            else if (Math.Abs(p.Y - 1.0f) < eps) // Top face (z ≈ 1)
            {
                var v = 0.75f + Math.Clamp(r, 0, 1) * 0.25f; // Maps radial distance to v in [0.75, 1.0]
                return new Vector2(u, v);
            }
            else // Side surface
            {
                var v = (p.Y + 1.0f) / 2.0f * 0.5f + 0.25f; // Maps height y to v in [0.25, 0.75]
                return new Vector2(u, Math.Clamp(v, 0.25f, 0.75f));
            }
        }

        // Maps UV (Vector2) to 3D point (Vector3) on the box surface
        public static SurfacePoint BoxPoint(Vector2 uv)
        {
            var u = uv.X;
            var v = uv.Y;
            var size = 1f;
            var halfSize = size / 2.0f;

            // Front face (z = halfSize)
            if (u >= 0.25f && u <= 0.5f && v >= 0.25f && v <= 0.5f)
            {
                var localU = (u - 0.25f) / 0.25f;
                var localV = (v - 0.25f) / 0.25f;
                var position = new Vector3(localU * size - halfSize, localV * size - halfSize, halfSize);
                var normal = new Vector3(0, 0, 1);
                return (position, normal);
            }

            // Back face (z = -halfSize)
            if (u >= 0.5f && u <= 0.75f && v >= 0.25f && v <= 0.5f)
            {
                var localU = (u - 0.5f) / 0.25f;
                var localV = (v - 0.25f) / 0.25f;
                var position = new Vector3(localU * size - halfSize, localV * size - halfSize, -halfSize);
                var normal = new Vector3(0, 0, -1);
                return (position, normal);
            }

            // Left face (x = -halfSize)
            if (u <= 0.25f && v >= 0.25f && v <= 0.5f)
            {
                var localU = u / 0.25f;
                var localV = (v - 0.25f) / 0.25f;
                var position = new Vector3(-halfSize, localV * size - halfSize, localU * size - halfSize);
                var normal = new Vector3(-1, 0, 0);
                return (position, normal);
            }

            // Right face (x = halfSize)
            if (u >= 0.75f && v >= 0.25f && v <= 0.5f)
            {
                var localU = (u - 0.75f) / 0.25f;
                var localV = (v - 0.25f) / 0.25f;
                var position = new Vector3(halfSize, localV * size - halfSize, localU * size - halfSize);
                var normal = new Vector3(1, 0, 0);
                return (position, normal);
            }

            // Top face (y = halfSize)
            if (u >= 0.25f && u <= 0.5f && v >= 0.5f && v <= 0.75f)
            {
                var localU = (u - 0.25f) / 0.25f;
                var localV = (v - 0.5f) / 0.25f;
                var position = new Vector3(localU * size - halfSize, halfSize, localV * size - halfSize);
                var normal = new Vector3(0, 1, 0);
                return (position, normal);
            }

            // Bottom face (y = -halfSize)
            if (u >= 0.25f && u <= 0.5f && v <= 0.25f)
            {
                var localU = (u - 0.25f) / 0.25f;
                var localV = v / 0.25f;
                var position = new Vector3(localU * size - halfSize, -halfSize, localV * size - halfSize);
                var normal = new Vector3(0, -1, 0);
                return (position, normal);
            }

            throw new Exception("Not expected");
        }

        // Maps a 3D point (Vector3) to UV coordinates on the nearest face of the box
        public static Vector2 PointToBoxUV(Vector3 point)
        {
            var size = 1f;
            float halfSize = size / 2.0f;

            // Front face (z = halfSize)
            if (Math.Abs(point.Z - halfSize) < eps)
            {
                float u = (point.X + halfSize) / size * 0.25f + 0.25f;
                float v = (point.Y + halfSize) / size * 0.25f + 0.25f;
                return new Vector2(u, v);
            }

            // Back face (z = -halfSize)
            if (Math.Abs(point.Z + halfSize) < eps)
            {
                float u = (point.X + halfSize) / size * 0.25f + 0.5f;
                float v = (point.Y + halfSize) / size * 0.25f + 0.25f;
                return new Vector2(u, v);
            }

            // Left face (x = -halfSize)
            if (Math.Abs(point.X + halfSize) < eps)
            {
                float u = (point.Z + halfSize) / size * 0.25f;
                float v = (point.Y + halfSize) / size * 0.25f + 0.25f;
                return new Vector2(u, v);
            }

            // Right face (x = halfSize)
            if (Math.Abs(point.X - halfSize) < eps)
            {
                float u = (point.Z + halfSize) / size * 0.25f + 0.75f;
                float v = (point.Y + halfSize) / size * 0.25f + 0.25f;
                return new Vector2(u, v);
            }

            // Top face (y = halfSize)
            if (Math.Abs(point.Y - halfSize) < 0.0001f)
            {
                float u = (point.X + halfSize) / size * 0.25f + 0.25f;
                float v = (point.Z + halfSize) / size * 0.25f + 0.5f;
                return new Vector2(u, v);
            }

            // Bottom face (y = -halfSize)
            if (Math.Abs(point.Y + halfSize) < 0.0001f)
            {
                float u = (point.X + halfSize) / size * 0.25f + 0.25f;
                float v = (point.Z + halfSize) / size * 0.25f;
                return new Vector2(u, v);
            }

            throw new ArgumentException("Point is not on the surface of the box.");
        }

        public static Vector2 ClosestUVOnTorus(Vector3 point, float r)
        {
            // Project the 3D point onto the XY plane (ignore Z)
            var x = point.X;
            var y = point.Y;

            // Step 1: Find the closest point on the major circle (radius 1)
            var u = Math.Atan2(y, x); // Angle around the major circle
            if (u < 0) u += 2 * Math.PI; // Ensure u is in the range [0, 2 * PI]

            // Step 2: Calculate the closest point on the minor circle (radius r)
            // First, find the distance from the projected point to the center of the torus's tube
            var majorCircleX = Math.Cos(u); // X coordinate of the point on the major circle
            var majorCircleY = Math.Sin(u); // Y coordinate of the point on the major circle

            // Calculate the distance from the point to the torus's major circle
            var projectedPoint = new Vector2(x, y);
            var majorCirclePoint = new Vector2((float)majorCircleX, (float)majorCircleY);
            var distToMajorCircle = projectedPoint.Distance(majorCirclePoint);

            // Step 3: Find the closest angle on the minor circle (v)
            // The distance from the major circle center to the point gives the radial distance
            // Use the distance to figure out the angle on the minor circle
            var v = Math.Acos(Math.Clamp((distToMajorCircle - 1) / r, -1f, 1f));

            // Step 4: Return UV coordinates (normalized)
            var x1 = u / (2 * Math.PI);
            var y1 = v / (2 * Math.PI);
            return new Vector2((float)x1, (float)y1);
        }
    }

    public class Polyline
    {
        public IReadOnlyList<Vector2> Points { get; } = new List<Vector2>();
        public Polyline(IReadOnlyList<Vector2> points) => Points = points;
    }

    public static class ParametricSurfaceExtensions
    {
        public static Vector3 Point(this ISolid s, Vector2 uv)
            => s.Eval(uv).Position;
        public static Vector3 SurfacePosition(this ISolid s, Vector3 p)
            => s.Point(s.Uv(p));

        public static Vector3 SurfaceNormal(this ISolid s, Vector3 p)
            => s.Eval(s.Uv(p)).Normal;

        public static float Distance(this ISolid s, Vector3 p)
        {
            // Get the UV coordinates of the point projected onto the surface
            var uv = s.Uv(p);

            // Evaluate the surface point and its normal at those coordinates
            var surfacePoint = s.Eval(uv);

            // Vector from the surface point to the given point
            var surfaceToPoint = p - surfacePoint.Position;

            // Compute the distance from the surface point to the given point
            var distance = surfaceToPoint.Magnitude();

            // Early return if the point is already on the surface (within some tolerance)
            const float epsilon = 1e-8f;
            if (distance < epsilon)
                return 0;

            // Compute the dot product to determine if the point is inside or outside
            var dotProduct = surfacePoint.Normal.Dot(surfaceToPoint);

            // Adjust for inverted normals
            return dotProduct > 0 ? distance : -distance;
        }

        public static bool IsInside(this ISolid s, Vector3 p)
            => s.Distance(p) < 0;

        public static Vector3 SurfaceToPoint(this ISolid s, Vector3 p)
            => s.SurfacePosition(p) - p;

        public static bool Inside(this ISolid solid, Vector3 point)
        {
            var uv = solid.Uv(point);
            var surfacePoint = solid.Eval(uv);
            var surfaceToPoint = point - surfacePoint.Position;
            var dotProduct = surfacePoint.Normal.Dot(surfaceToPoint);
            return dotProduct < 0;
        }

        public static Vector2 FindNearestUV(
            Func<Vector2, Vector3> surfaceFunction,
            Vector3 point,
            Func<Vector2, Vector3[]> surfaceGradientFunction = null)
        {
            // Initial guess (could be improved by prior knowledge)
            var uv = new Vector2(0.5f, 0.5f);

            // Optimization parameters
            const float tolerance = 1e-6f;
            const int maxIterations = 100;
            var stepSize = 0.01f;

            for (var i = 0; i < maxIterations; i++)
            {
                var f_uv = surfaceFunction(uv);
                var diff = f_uv - point;
                var distanceSquared = diff.LengthSquared();

                // Check for convergence
                if (distanceSquared < tolerance)
                    break;

                Vector2 gradient;

                if (surfaceGradientFunction != null)
                {
                    // Use provided gradients
                    var grad_f_uv = surfaceGradientFunction(uv); // [df/du, df/dv]
                    var grad_u = 2 * diff.Dot(grad_f_uv[0]);
                    var grad_v = 2 * diff.Dot(grad_f_uv[1]);           
                    gradient = (grad_u, grad_v);
                }
                else
                {
                    // Approximate gradients using finite differences
                    var h = 1e-5f;
                    var uv_u = uv + (h, 0);
                    var uv_v = uv + (0, h);

                    var f_uv_u = surfaceFunction(uv_u);
                    var f_uv_v = surfaceFunction(uv_v);

                    var grad_u_vec = (f_uv_u - f_uv) / h;
                    var grad_v_vec = (f_uv_v - f_uv) / h;

                    var grad_u = 2 * diff.Dot(grad_u_vec);
                    var grad_v = 2 * diff.Dot(grad_v_vec);
                    gradient = (grad_u, grad_v);
                }

                // Update UV coordinates
                uv -= stepSize * gradient;

                // Clamp UV to [0,1]
                //uv = new Vector2(Matf.Clamp01(uv.x), Mathf.Clamp01(uv.y));
            }

            return uv;
        }

        /// <summary>
        /// Determines if a point is inside a polygon using the ray casting algorithm.
        /// </summary>
        /// <param name="point">The point to test.</param>
        /// <param name="polygon">The list of vertices defining the polygon.</param>
        /// <returns>True if the point is inside the polygon; otherwise, false.</returns>
        public static bool IsInsidePolygon(this IPolyline polygon, Vector2 point)
        {
            var crossings = 0;
            var count = polygon.Points.Count;

            for (var i = 0; i < count; i++)
            {
                var a = polygon.Points[i];
                var b = polygon.Points[(i + 1) % count];

                if (((a.Y > point.Y) != (b.Y > point.Y)) &&
                    (point.X < (b.X - a.X) * (point.Y - a.Y) / (b.Y - a.Y + 1e-8f) + a.X))
                {
                    crossings++;
                }
            }

            return (crossings % 2) != 0;
        }

        public static List<List<Vector2>> ComputeIntersectionCurves(ISolid solid1, ISolid solid2, int gridSize = 100)
        {
            // Initialize data structures
            var distances = new float[gridSize + 1, gridSize + 1];
            var uvGrid = new Vector2[gridSize + 1, gridSize + 1];

            // UV ranges (assuming [0,1] for both U and V)
            var du = 1.0f / gridSize;
            var dv = 1.0f / gridSize;

            // Precompute surface points and distances
            for (var i = 0; i <= gridSize; i++)
            {
                var u = i * du;
                for (var j = 0; j <= gridSize; j++)
                {
                    var v = j * dv;
                    var uv = new Vector2(u, v);
                    uvGrid[i, j] = uv;

                    // Evaluate the surface point on the first solid and compute signed distance to the second solid
                    var p = solid1.Eval(uv).Position;
                    distances[i, j] = solid2.Distance(p);
                }
            }

            // List to store the intersection polylines
            var polylines = new List<List<Vector2>>();

            // Apply Marching Squares algorithm
            for (var i = 0; i < gridSize; i++)
            {
                for (var j = 0; j < gridSize; j++)
                {
                    // Get distances at the four corners of the cell
                    var d00 = distances[i, j];
                    var d10 = distances[i + 1, j];
                    var d11 = distances[i + 1, j + 1];
                    var d01 = distances[i, j + 1];

                    // Determine if there's an intersection by checking sign changes
                    var caseIndex = ((d00 >= 0) ? 1 : 0) 
                                    | ((d10 >= 0) ? 2 : 0) 
                                    | ((d11 >= 0) ? 4 : 0) 
                                    | ((d01 >= 0) ? 8 : 0);

                    if (caseIndex == 0 || caseIndex == 15) continue;

                    var intersections = GetIntersectionsForCell(caseIndex, uvGrid, distances, i, j);

                    // Add the intersections as polylines if valid
                    if (intersections.Count >= 2)
                    {
                        polylines.Add(intersections);
                    }
                }
            }

            return polylines;
        }

        // Helper method to compute intersections for a given cell
        private static List<Vector2> GetIntersectionsForCell(int caseIndex, Vector2[,] uvGrid, float[,] distances, int i, int j)
        {
            var intersections = new List<Vector2>();

            // Edge interpolation based on caseIndex
            if (SignChanged(distances[i, j], distances[i + 1, j]))
                intersections.Add(InterpolateEdge(uvGrid[i, j], uvGrid[i + 1, j], distances[i, j], distances[i + 1, j]));

            if (SignChanged(distances[i + 1, j], distances[i + 1, j + 1]))
                intersections.Add(InterpolateEdge(uvGrid[i + 1, j], uvGrid[i + 1, j + 1], distances[i + 1, j], distances[i + 1, j + 1]));

            if (SignChanged(distances[i + 1, j + 1], distances[i, j + 1]))
                intersections.Add(InterpolateEdge(uvGrid[i + 1, j + 1], uvGrid[i, j + 1], distances[i + 1, j + 1], distances[i, j + 1]));

            if (SignChanged(distances[i, j + 1], distances[i, j]))
                intersections.Add(InterpolateEdge(uvGrid[i, j + 1], uvGrid[i, j], distances[i, j + 1], distances[i, j]));

            return intersections;
        }

        // Helper function to check if a sign change occurred between two distances
        private static bool SignChanged(float d1, float d2)
        {
            return (d1 >= 0 && d2 < 0) || (d1 < 0 && d2 >= 0);
        }

        // Helper function to interpolate intersection points based on distance values
        private static Vector2 InterpolateEdge(Vector2 uv1, Vector2 uv2, float d1, float d2)
        {
            var t = d1 / (d1 - d2);  // Linear interpolation factor
            return uv1 + t * (uv2 - uv1);
        }

        /// <summary>
        /// Interpolates the zero crossing point between two values.
        /// </summary>
        /// <param name="v1">First value.</param>
        /// <param name="v2">Second value.</param>
        /// <returns>Interpolation factor between 0 and 1.</returns>
        public static float InterpolateZeroCrossing(float v1, float v2)
        {
            return v1 / (v1 - v2);
        }

        public static float SmoothStep(float edge0, float edge1, float x)
        {
            x = (x - edge0) / (edge1 - edge0);
            x = Math.Max(x, 0.0f);
            x = Math.Min(x, 1.0f); 
            return x * x * (3 - 2 * x);
        }

        /// <summary>
        /// Creates a mesh by sampling the solid at regular intervals 
        /// </summary>
        public static TessellatedMesh ToMesh(this ISolid solid, int cols, int rows)
        {
            var discreteSurface = new SurfaceDiscretization(cols, rows, true, true);
            var vertices = discreteSurface.Uvs.Select(uv => solid.Point(uv)).Evaluate();
            var faceVertices = discreteSurface.QuadIndices.Evaluate();
            return new TessellatedMesh(vertices, faceVertices);
        }

        public static ISolid Scale(this ISolid solid, float scale)
            => new Solid(
                uv => solid.Eval(uv).Scale(scale),
                p => solid.Uv(p / scale));

        public static ISolid Translate(this ISolid solid, Vector3 offset)
            => new Solid(
                uv => solid.Eval(uv).Translate(offset),
                p => solid.Uv(p - offset));

        public static ISolid Transform(this ISolid solid, Ara3D.Mathematics.Matrix4x4 m)
        {
            var im = m.Inverse(); 
            return new Solid(
                uv => solid.Eval(uv).Transform(m),
                p => solid.Uv(p.Transform(im)));
        }
    }
}

