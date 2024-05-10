// https://forum.unity.com/threads/gpu-frustum-culling-tips.1102627/
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;

public class DrawMeshInstancedIndirectDemo : MonoBehaviour
{
    public static System.Random random;

    public uint COMPUTE_SHADER_CULLING_AABB_THREAD_GROUP_SIZE_X = 10;
    public uint COMPUTE_SHADER_CULLING_AABB_THREAD_GROUP_SIZE_Y = 10;
    public uint COMPUTE_SHADER_CULLING_AABB_THREAD_GROUP_SIZE_Z = 10;

    //every Gameobject in BatchPrefabs will contain Childs instances
    public GameObject[] BatchPrefabs;

    //AABB Culling ComputeShader
    public ComputeShader CullingShader;

    private Color[] m_Colors;
    private Mesh[] m_Meshs;
    private Material[] m_Materials;
    private uint[] m_InstancesCount;

    private ComputeBuffer[] m_VisibleIndicesCB;
    private ComputeBuffer[] m_PropertiesBuffer;
    private ComputeBuffer[] m_BoundsBuffer;


    private uint[] m_GlobalArgsBufferArray;
    private ComputeBuffer m_GlobalArgsBuffer;


    private uint[] m_DispatchArgsBufferArray; //Used to fill m_DispatchArgsBuffer
    private ComputeBuffer m_DispatchArgsBuffer; //CullingShader will used this buffer to DispatchIndirect


    private MaterialPropertyBlock propertyBlock;
    private int FrustumCullingKarnelID;


    private Bounds bounds;

    private void OnDisable()
    {

        for (var i = 0; i < m_VisibleIndicesCB.Length; i++)
        {
            m_VisibleIndicesCB[i].Dispose();
            m_VisibleIndicesCB[i].Release();
            m_VisibleIndicesCB[i] = null;
        }

        for (var i = 0; i < m_PropertiesBuffer.Length; i++)
        {
            m_PropertiesBuffer[i].Dispose();
            m_PropertiesBuffer[i].Release();
            m_PropertiesBuffer[i] = null;
        }

        for (var i = 0; i < m_BoundsBuffer.Length; i++)
        {
            m_BoundsBuffer[i].Dispose();
            m_BoundsBuffer[i].Release();
            m_BoundsBuffer[i] = null;
        }


        if (m_GlobalArgsBuffer != null)
        {
            m_GlobalArgsBuffer.Dispose();
            m_GlobalArgsBuffer.Release();
            m_GlobalArgsBuffer = null;
        }


        if (m_DispatchArgsBuffer != null)
        {
            m_DispatchArgsBuffer.Dispose();
            m_DispatchArgsBuffer.Release();
            m_DispatchArgsBuffer = null;
        }

    }


    // Mesh Properties struct to be read from the GPU.
    // Size() is a convenience funciton which returns the stride of the struct.
    private struct TestMeshProperties
    {
        public float4x4 Matrix;
        public float4 Color;
        public static int Cpu_Size()
        {
            return UnsafeUtility.SizeOf<TestMeshProperties>();
        }
    }


    private void SetupChildAtIndex(int parentIndex)
    {
        var parent = BatchPrefabs[parentIndex];
        var parentTransform = parent.transform;

        if (parentTransform.childCount == 0)
        {
            Debug.LogWarning($"Parent at Index {parentIndex} has no childs");
            return;
        }

        var childs = new Transform[parentTransform.childCount];

        for (var i = 0; i < parentTransform.childCount; i++)
        {
            childs[i] = parentTransform.GetChild(i);
        }



        //Set Color to Index
        m_Colors[parentIndex] = Color.blue;

        //Set up Material
        m_Materials[parentIndex] = childs[0].GetComponent<MeshRenderer>().sharedMaterial;

        //Set up Material
        var currentMesh = childs[0].GetComponent<MeshFilter>().sharedMesh;
        m_Meshs[parentIndex] = currentMesh;


        //Set Instances Count
        m_InstancesCount[parentIndex] = (uint)childs.Length;


        //Set Args for ComputeShader.DispatchIndirect
        var dispatchArgsStartIndex = parentIndex * 3;
        m_DispatchArgsBufferArray[dispatchArgsStartIndex] = math.max((uint)childs.Length / COMPUTE_SHADER_CULLING_AABB_THREAD_GROUP_SIZE_X, 1);

        //Putting the argsStartIndex++ in [] will fail for an unknow reason
        dispatchArgsStartIndex++;
        m_DispatchArgsBufferArray[dispatchArgsStartIndex] = COMPUTE_SHADER_CULLING_AABB_THREAD_GROUP_SIZE_Y;

        dispatchArgsStartIndex++;
        m_DispatchArgsBufferArray[dispatchArgsStartIndex] = COMPUTE_SHADER_CULLING_AABB_THREAD_GROUP_SIZE_Z;





        //Setup ComputeBuffers
        var propertiesByteSize = TestMeshProperties.Cpu_Size();
        var propertiesTotalByteSize = propertiesByteSize * childs.Length;

        var boundsSize = 24; // WorldBounds.Cpu_Size();
        var boundsDataBytesize = boundsSize * childs.Length;

        //Init Compute Buffers
        m_PropertiesBuffer[parentIndex] = new ComputeBuffer(childs.Length, TestMeshProperties.Cpu_Size(), ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
        m_BoundsBuffer[parentIndex] = new ComputeBuffer(childs.Length, boundsSize, ComputeBufferType.Default, ComputeBufferMode.SubUpdates);
        m_VisibleIndicesCB[parentIndex] = new ComputeBuffer(childs.Length, sizeof(uint), ComputeBufferType.Append);

        //Setting Args Buffer
        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.

        var globalArgsArrayStartIndex = parentIndex * 5;
        m_GlobalArgsBufferArray[globalArgsArrayStartIndex] = currentMesh.GetIndexCount(0);
        globalArgsArrayStartIndex++;
        m_GlobalArgsBufferArray[globalArgsArrayStartIndex] = (uint)childs.Length;
        globalArgsArrayStartIndex++;
        m_GlobalArgsBufferArray[globalArgsArrayStartIndex] = currentMesh.GetIndexStart(0);
        globalArgsArrayStartIndex++;
        m_GlobalArgsBufferArray[globalArgsArrayStartIndex] = currentMesh.GetBaseVertex(0);
        globalArgsArrayStartIndex++;
        m_GlobalArgsBufferArray[globalArgsArrayStartIndex] = 0;

        var propertiesComputeBufferGPUNativeArray = m_PropertiesBuffer[parentIndex].BeginWrite<byte>(0, propertiesTotalByteSize);
        var boundsComputeBufferGPUNativeArray = m_BoundsBuffer[parentIndex].BeginWrite<byte>(0, boundsDataBytesize);

        var meshLocalBounds = currentMesh.bounds;

        unsafe
        {
            var ComputeBufferBytePtr = propertiesComputeBufferGPUNativeArray.GetUnsafePtr();
            var m_BoundsBufferPtr = boundsComputeBufferGPUNativeArray.GetUnsafePtr();
            // Initialize buffer with the given population.
            for (int i = 0; i < childs.Length; i++)
            {

                var matrix = childs[i].localToWorldMatrix;

                //var matrix = Matrix4x4.TRS(Vector3.zero, quaternion.identity, new Vector3(100,100,100));
                var color = m_Colors[parentIndex];

                //Write Properties
                var newBufferPosition = matrix.WriteToBuffer(((byte*)ComputeBufferBytePtr) + (propertiesByteSize * i));
                color.WriteToBuffer(newBufferPosition);

                //Write Bounds
                var worldBounds = WorldBounds.From(AABB.Transform(matrix, meshLocalBounds));
                worldBounds.WriteToBuffer(((byte*)m_BoundsBufferPtr) + boundsSize * i);
            }

        }
        m_PropertiesBuffer[parentIndex].EndWrite<byte>(propertiesTotalByteSize);
        m_BoundsBuffer[parentIndex].EndWrite<byte>(boundsDataBytesize);


    }

    private void Setup()
    {
        m_MainCamera = Camera.main;

        m_VisibleIndicesCB = new ComputeBuffer[BatchPrefabs.Length];
        m_BoundsBuffer = new ComputeBuffer[BatchPrefabs.Length];
        m_PropertiesBuffer = new ComputeBuffer[BatchPrefabs.Length];




        m_Meshs = new Mesh[BatchPrefabs.Length];
        m_Materials = new Material[BatchPrefabs.Length];
        m_Colors = new Color[BatchPrefabs.Length];
        m_InstancesCount = new uint[BatchPrefabs.Length];
        m_DispatchArgsBufferArray = new uint[BatchPrefabs.Length * 3];
        m_GlobalArgsBufferArray = new uint[BatchPrefabs.Length * 5];


        FrustumCullingKarnelID = CullingShader.FindKernel(CustomHybridRanderConstants.ComputeShaderCulling.COMPUTE_SHADER_CULLING_AABB_KERNEL_NAME);
        propertyBlock = new MaterialPropertyBlock();


        //setup All Childs and buffers
        for (var i = 0; i < BatchPrefabs.Length; i++)
        {
            SetupChildAtIndex(i);
        }

        m_GlobalArgsBuffer = new ComputeBuffer(BatchPrefabs.Length, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        m_GlobalArgsBuffer.SetData(m_GlobalArgsBufferArray);

        m_DispatchArgsBuffer = new ComputeBuffer(BatchPrefabs.Length, 3 * sizeof(int), ComputeBufferType.IndirectArguments);
        m_DispatchArgsBuffer.SetData(m_DispatchArgsBufferArray);
    }

    Camera m_MainCamera;
    private void Start()
    {
        bounds = new Bounds(Vector3.zero, Vector3.one * 1000);
        Setup();
    }

    private void Update()
    {

        var cameraFrustumPlanes = math.mul(m_MainCamera.projectionMatrix, m_MainCamera.worldToCameraMatrix);

        Profiler.BeginSample("Setting Global Matrix");
        //TODO: Set Camera Frustum VPMatrix once for all Compute shaders as it has the same value
        //Set Camera Frustum VPMatrix
        CullingShader.SetMatrix("_VP_MATRIX", cameraFrustumPlanes);
        Profiler.EndSample();

        Profiler.BeginSample("Dispatch Culling Compute shaders");
        //Dispatch Culling Compute shaders
        for (var i = 0; i < m_Meshs.Length; i++)
        {
            propertyBlock.Clear();

            //Reset Visible Indices the zero every frame
            m_VisibleIndicesCB[i].SetCounterValue(0);


            //Set Bounds Buffer
            CullingShader.SetBuffer(FrustumCullingKarnelID, CustomHybridRanderConstants.ComputeShaderCulling.BoundsBufferPropertyID, m_BoundsBuffer[i]);
            CullingShader.SetBuffer(FrustumCullingKarnelID, CustomHybridRanderConstants.VisibleIndicesPropertyID, m_VisibleIndicesCB[i]);


            //Start Frustum Culling
            var dispatchArgsStartIndex = i * 3 * 4;
            CullingShader.DispatchIndirect(FrustumCullingKarnelID, m_DispatchArgsBuffer, (uint)dispatchArgsStartIndex);

        }
        Profiler.EndSample();




        Profiler.BeginSample("Execute DrawCalls");
        //Execute DrawCalls
        for (var i = 0; i < m_Meshs.Length; i++)
        {
            var argsStartIndex = i * 5 * 4;

            //ArgsBuffer current Batch Start Offset
            Profiler.BeginSample("ComputeBuffer.CopyCount Operation");
            //Copy Visible Entities Count to the argsBuffer
            ComputeBuffer.CopyCount(m_VisibleIndicesCB[i], m_GlobalArgsBuffer, argsStartIndex + 4); //By removing this Line everthing works correctly
            Profiler.EndSample();

            Profiler.BeginSample("propertyBlock.SetBuffer Operation");
            //Set Buffers to MPB
            //propertyBlock.SetBuffer(CustomHybridRanderConstants.VisibleIndicesPropertyID, m_VisibleIndicesCB[i]);
            propertyBlock.SetBuffer(CustomHybridRanderConstants.PropertiesBufferPropertyID, m_PropertiesBuffer[i]);
            Profiler.EndSample();

#if UNITY_EDITOR
            //Debug.Log($"argsStartIndex: {argsStartIndex}");
            // Execute Rendering Command
            Graphics.DrawMeshInstancedIndirect(m_Meshs[i], 0, m_Materials[i], bounds, bufferWithArgs: m_GlobalArgsBuffer, argsOffset: argsStartIndex, propertyBlock);

#else
            // Execute Rendering Command For the Main Camera Only
            Graphics.DrawMeshInstancedIndirect(m_Meshs[i], 0, m_Materials[i], bounds, bufferWithArgs: m_GlobalArgsBuffer, argsOffset: argsStartIndex, propertyBlock, camera: m_MainCamera);
#endif
        }

        Profiler.EndSample();
    }
}