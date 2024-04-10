using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.ClonerExample
{
    /// <summary>
    /// Manages important information for rendering instance data efficiently.
    /// Contains an array of CpuInstanceData, and GpuInstanceData. 
    /// </summary>
    public class CloneRenderData : IDisposable
    {
        public uint NumIndices => args[0];
        public uint NumInstances => args[1];
        public Mesh Mesh;
        public Material Material;
        public Bounds bounds;

        private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        private ComputeBuffer argsBuffer;
        private ComputeBuffer gpuBuffer;

        public float elapsed;
        public float maxElapsed;

        public void UpdateGpuData(Mesh mesh, NativeArray<GpuInstanceData> gpuInstances, Material material)
        {
            Mesh = mesh;

            if (argsBuffer == null) argsBuffer =
                new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

            var numIndices = (Mesh != null) ? Mesh.GetIndexCount(0) : 0u;
            var numInstances = (uint)gpuInstances.Length;

            if (numInstances != NumInstances || gpuBuffer == null)
            {
                if (gpuBuffer != null)
                    gpuBuffer.Release();
                gpuBuffer = null;
                if (gpuInstances.Length != 0)
                {
                    gpuBuffer = new ComputeBuffer(gpuInstances.Length, GpuInstanceData.Size);
                }
            }

            if (gpuBuffer != null)
            {
                gpuBuffer.SetData(gpuInstances);
            }

            if (numIndices != NumIndices || numInstances != NumInstances)
            {
                args[0] = numIndices;
                args[1] = numInstances;
                argsBuffer.SetData(args);
            }

            Material = material;
            Material.SetBuffer("instanceBuffer", gpuBuffer);
        }

        public void Render(ShadowCastingMode shadowCasting, bool receiveShadows)
        {
            if (Material == null)
            {
                Debug.Log("No material present");
                return;
            }
            
            if (NumInstances == 0)
            {
                Debug.Log("No instances present");
                return;
            }
            
            if (NumIndices == 0)
            {
                Debug.Log("No mesh indices present");
                return;
            }

            bounds = new Bounds(Vector3.zero, new Vector3(10000.0f, 10000.0f, 10000.0f));

            Graphics.DrawMeshInstancedIndirect(Mesh, 0, Material, bounds,
                argsBuffer, 0, null, shadowCasting, receiveShadows);
        }

        public void Dispose()
        {
            if (gpuBuffer != null) gpuBuffer.Dispose();
            gpuBuffer = null;
            if (argsBuffer != null) argsBuffer.Dispose();
            argsBuffer = null;
        }
    }
}