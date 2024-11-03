using System;
using System.Collections.Generic;
using Ara3D.Geometry;
using Ara3D.Studio.RenderData;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using Bounds = UnityEngine.Bounds;
using Mesh = UnityEngine.Mesh;

namespace Assets.ClonerExample
{
    [ExecuteAlways]
    public class ClonerFromProcess : MonoBehaviour
    {
        public Material Material;
        private Material _material;
        public ShadowCastingMode ShadowCasting = ShadowCastingMode.Off;
        public bool ReceiveShadows = false;

        GraphicsBuffer _commandBuf;
        GraphicsBuffer.IndirectDrawIndexedArgs[] _commandData;
        ComputeBuffer _gpuBuffer;

        public void OnValidate()
        {
            // This is required ... to force the creation of a new material when the material is changed
            _material = null;
        }

        public void OnDestroy()
        {
            ReleaseData();
        }

        public void ReleaseData()
        {
            _commandBuf?.Release();
            _commandBuf = null;
            _gpuBuffer?.Release();
            _gpuBuffer = null;
        }

        public void Update()
        {
            if (Material == null)
            {
                Debug.Log("Material is null: assign a cloner material (with a cloner shader)");
                return;
            }

            if (_material == null)
                _material = new Material(Material);

            using (var reader = new RenderDataReader())
            {
                reader.Read(Render);
            }
        }

        public unsafe void Render(RenderDataLayout layout, RenderDataPointers pointers)
        {
            var safetyHandle = AtomicSafetyHandle.GetTempMemoryHandle();

            var vertexArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Ara3D.Studio.RenderData.Vertex>(
                pointers.VerticesPtr, pointers.NumVertices, Allocator.None);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref vertexArray, safetyHandle);

            var indexArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(
                pointers.IndicesPtr, pointers.NumIndices, Allocator.None);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref indexArray, safetyHandle);

            var meshArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<MeshOffsets>(
                pointers.MeshesPtr, pointers.NumMeshes, Allocator.None);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref meshArray, safetyHandle);

            var instanceArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Instance>(
                pointers.InstancesPtr, pointers.NumInstances, Allocator.None);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref instanceArray, safetyHandle);

            var instancedMeshArray = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<InstancedMesh>(
                pointers.InstancedMeshesPtr, pointers.NumInstancedMeshes, Allocator.None);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref instancedMeshArray, safetyHandle);

            var mesh = new Mesh();
            var vertexLayout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UInt8, 4),
            };

            // Set the vertex and index buffer layouts and data 
            mesh.SetVertexBufferParams(vertexArray.Length, vertexLayout);
            mesh.SetVertexBufferData(vertexArray, 0, 0, vertexArray.Length, 0, MeshUpdateFlags.Default);
            mesh.SetIndexBufferParams(indexArray.Length, IndexFormat.UInt32);
            mesh.SetIndexBufferData(indexArray, 0, 0, indexArray.Length, MeshUpdateFlags.Default);

            // Define submesh (NOTE: GPT suggested, not sure if necessary)
            // TODO: maybe trry different submeshes 
            /*
            for (var i=0; i < meshArray.Length; ++i)
            {
                var meshOffsets = meshArray[i];
                var subMeshDescriptor = new SubMeshDescriptor(meshOffsets.IndexStart, meshOffsets.IndexCount);
                mesh.SetSubMesh(i, subMeshDescriptor);
            }
            */
            var subMeshDescriptor = new SubMeshDescriptor(0, indexArray.Length);
            mesh.SetSubMesh(0, subMeshDescriptor);

            /*
            // Create ComputeBuffers and set data
            argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            gpuBuffer = new ComputeBuffer(instanceArray.Length, UnsafeUtility.SizeOf<Instance>());
            gpuBuffer.SetData(instanceArray);
            _material.SetBuffer("instanceBuffer", gpuBuffer);
            var bounds = new Bounds(Vector3.zero, new Vector3(100000.0f, 100000.0f, 100000.0f));

            for (var i = 0; i < instancedMeshArray.Length; i++)
            {
                var instMeshData = instancedMeshArray[i];
                var meshData = meshArray[instMeshData.MeshIndex];

                var args = new uint[5] { 0, 0, 0, 0, 0 };
                var indexCountPerInstance = args[0] = (uint)meshData.IndexCount;
                var instanceCount = args[1] = (uint)instMeshData.NumInstances;
                var startIndexLocation = args[2] = (uint)meshData.IndexStart;
                var baseVertexLocation = args[3] = (uint)meshData.VertexOffset;
                var startInstanceLocation = args[4] = (uint)instMeshData.InstanceStart;
                argsBuffer.SetData(args);

                Graphics.DrawMeshInstancedIndirect(mesh, 0, _material, bounds, argsBuffer, 0, null, ShadowCasting, ReceiveShadows);
            }
            */
            
            ReleaseData();

            _gpuBuffer = new ComputeBuffer(instanceArray.Length, UnsafeUtility.SizeOf<Instance>());
            _gpuBuffer.SetData(instanceArray);
            _material.SetBuffer("instanceBuffer", _gpuBuffer);

            var rp = new RenderParams(_material);
            rp.worldBounds = new Bounds(Vector3.zero, 10000*Vector3.one); // use tighter bounds for better FOV culling
            rp.matProps = new MaterialPropertyBlock();  
            //rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));

            var ninst = instancedMeshArray.Length;
            _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[ninst];
            for (var i=0; i < ninst; i++)
            {
                var instMeshData = instancedMeshArray[i];
                var meshData = meshArray[instMeshData.MeshIndex];
                _commandData[i] = new GraphicsBuffer.IndirectDrawIndexedArgs
                {
                    indexCountPerInstance = (uint)meshData.IndexCount,
                    instanceCount = (uint)instMeshData.NumInstances,
                    baseVertexIndex = (uint)meshData.VertexOffset,
                    startIndex = (uint)meshData.IndexStart,
                    startInstance = (uint)instMeshData.InstanceStart
                };
            }

            _commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, ninst, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            _commandBuf.SetData(_commandData);

            // Issue separate draw calls for each command
            for (int i = 0; i < ninst; i++)
            {
                var matProps = new MaterialPropertyBlock();
                matProps.SetInt("_CommandIndex", i);
                var rpInstance = new RenderParams
                {
                    material = _material,
                    worldBounds = rp.worldBounds,
                    matProps = matProps,
                };

                // Issue the draw call
                Graphics.RenderMeshIndirect(rpInstance, mesh, _commandBuf, 1, i);

            }
            
            //Graphics.RenderMeshIndirect(rp, mesh, _commandBuf, ninst);

            // TEMP: try to draw just one mesh
            //Graphics.DrawMesh(mesh, Matrix4x4.identity, _material, 0, null, 0);
            //Graphics.DrawMesh(mesh, Matrix4x4.Translate(Vector3.up * 10), _material, 0, null, 1);
        }
    }
}