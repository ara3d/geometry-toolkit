using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerComponent : MonoBehaviour
    {
        public CpuInstanceData[] Instances = Array.Empty<CpuInstanceData>();

        public int Rows = 5;
        public int Columns = 5;
        public int Layers = 5;
        public float Spacing = 1.5f;
        public int Count => Rows * Columns * Layers;
        public Color Color = Color.blue;
        public Quaternion Rotation = Quaternion.identity;
        public Vector3 Scale = Vector3.one;
        
        public void Update()
        {
            if (Count != Instances.Length)
                Array.Resize(ref Instances, Count);
            for (var i = 0; i < Count; i++)
            {
                var col = i % Columns;
                var row = (i / Columns) % Rows;
                var layer = (i / (Columns * Rows));
                if (Instances[i] == null)
                    Instances[i] = new CpuInstanceData();
                var inst = Instances[i];
                inst.Age = 0;
                inst.Position = col * Vector3.right * Spacing
                                + row * Vector3.forward * Spacing
                                + layer * Vector3.up * Spacing;
                inst.Rotation = Rotation;
                inst.Scale = Scale;
                inst.Color = new Vector3(Color.r, Color.g, Color.b);
                inst.Alpha = Color.a;
                inst.Enabled = true;
            }
        }
    }
}   