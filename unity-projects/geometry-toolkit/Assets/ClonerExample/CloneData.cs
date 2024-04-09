using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.ClonerExample
{
    /// <summary>
    /// Manages important information for rendering instance data efficiently.
    /// Converts an array of "CpuInstanceData" into an array of "GpuInstanceData"
    /// and passes that to the renderer for efficient instancing. 
    /// </summary>
    public class CloneData : IDisposable
    {
        public uint NumIndices => args[0];
        public uint NumInstances => args[1];
        public Mesh Mesh;
        public Material Material;
        public GpuInstanceData[] _gpuInstances = Array.Empty<GpuInstanceData>();
        public Bounds bounds;

        private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        private ComputeBuffer argsBuffer;
        private ComputeBuffer gpuBuffer;

        public float elapsed;
        public float maxElapsed;

        public void UpdateGpuData(IReadOnlyList<CpuInstanceData> cpuInstances, Material material)
        {
            // Resize the GPU instance array, if needed 
            if (_gpuInstances.Length != cpuInstances.Count)
            {
                Array.Resize(ref _gpuInstances, cpuInstances.Count);
            }

            // Compute the initial bounds (which is the bounds of all points 
            bounds = cpuInstances.Count > 0 
                ? new Bounds(cpuInstances[0].Position, Vector3.zero) 
                : new Bounds();
            
            for (var i = 0; i < cpuInstances.Count; i++)
            {
                var inst = cpuInstances[i];
                bounds.Encapsulate(inst.Position);
                _gpuInstances[i] = inst.ToGpuStruct();
            }

            if (argsBuffer == null) argsBuffer =
                new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

            var numIndices = (Mesh != null) ? Mesh.GetIndexCount(0) : 0u;
            var numInstances = (uint)_gpuInstances.Length;

            if (numInstances != NumInstances || gpuBuffer == null)
            {
                if (gpuBuffer != null)
                    gpuBuffer.Release();
                gpuBuffer = null;
                if (_gpuInstances.Length != 0)
                {
                    gpuBuffer = new ComputeBuffer(_gpuInstances.Length, GpuInstanceData.Size);
                }
            }

            if (_gpuInstances.Length != 0)
            {
                gpuBuffer.SetData(_gpuInstances);
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

        public void Render(Mesh mesh, ShadowCastingMode shadowCasting, bool receiveShadows)
        {
            Mesh = mesh;
            var meshBounds = Mesh.bounds;
            var totalBounds = new Bounds(bounds.center, bounds.size);
            totalBounds.Expand(meshBounds.size);
            if (mesh == null)
            {
                Debug.Log("No mesh present");
                return;
            }
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
            Graphics.DrawMeshInstancedIndirect(Mesh, 0, Material, totalBounds,
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