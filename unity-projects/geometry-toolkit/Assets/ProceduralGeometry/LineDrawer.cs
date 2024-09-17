using UnityEngine;
using System.Collections.Generic;

public class LineDrawer : MonoBehaviour
{
    public Material lineMaterial;  // Assign a material in the Unity Inspector
    public List<LineRenderer> lineRenderers = new List<LineRenderer>();

    public void AddLineRenderer()
    {
        // Create a new GameObject to hold the LineRenderer
        var lineObj = new GameObject("LineRendererObject");
        lineObj.transform.parent = transform;
        var lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderers.Add(lineRenderer);
    }

    public void RemoveLineRenderer()
    {
        var go = lineRenderers[lineRenderers.Count - 1].gameObject;
        lineRenderers.RemoveAt(lineRenderers.Count - 1);
        DestroyImmediate(go);
    }

    public void DrawLineSegments(List<List<Vector3>> lines, float width = 0.1f)
    {
        while (lineRenderers.Count > lines.Count)
            RemoveLineRenderer();

        while (lineRenderers.Count < lines.Count)
            AddLineRenderer();

        for (var i=0; i < lines.Count; i++)
        {
            var line = lines[i];
            var lineRenderer = lineRenderers[i];
            lineRenderer.sharedMaterial = lineMaterial;
            lineRenderer.positionCount = line.Count;  // Number of points in the line
            lineRenderer.startWidth = width;  // Set the width of the line
            lineRenderer.endWidth = width;
            lineRenderer.useWorldSpace = true;  // Use world coordinates

            var j = 0;
            foreach (var p in line)
                lineRenderer.SetPosition(j++, p);
        }
    }
}