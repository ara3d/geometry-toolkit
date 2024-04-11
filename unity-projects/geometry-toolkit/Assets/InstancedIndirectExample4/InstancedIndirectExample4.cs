// https://docs.unity3d.com/560/Documentation/ScriptReference/Graphics.DrawMeshInstancedIndirect.html

using UnityEngine;
using UnityEngine.Rendering;

public class InstancedIndirectExample4 : MonoBehaviour
{ 
    public struct GpuInstanceData
    {
        public Matrix4x4 Matrix;
        public Vector4 Color;
    }

    public int instanceCount = 100000;
    public GpuInstanceData[] gpuInstanceData;
    public Mesh opaqueMesh;
    public Material opaqueMaterial;

    public ShadowCastingMode castShadows = ShadowCastingMode.Off;
    public bool receiveShadows = false;

    private int cachedInstanceCount = -1;

    private ComputeBuffer gpuBuffer;
    private ComputeBuffer opaqueArgsBuffer;

    private uint[] opaqueArgs = new uint[5] { 0, 0, 0, 0, 0 };
    
    void Start()
	{
        opaqueArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    void Update()
	{ 
        // UpdateGpuData starting position buffer
        if (cachedInstanceCount != instanceCount) UpdateBuffers();

        // Pad input
        if (Input.GetAxisRaw("Horizontal") != 0.0f) instanceCount = (int)Mathf.Clamp(instanceCount + Input.GetAxis("Horizontal") * 40000, 1.0f, 5000000.0f);

        var bounds = new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f));

        Graphics.DrawMeshInstancedIndirect(opaqueMesh, 0, opaqueMaterial, bounds, opaqueArgsBuffer, 0, null, castShadows, receiveShadows);
    }

    void OnGUI()
	{ 
        GUI.Label(new Rect(265, 12, 200, 30), "Instance Count: " + instanceCount.ToString("N0"));
        instanceCount = (int)GUI.HorizontalSlider(new Rect(25, 20, 200, 30), (float)instanceCount, 1.0f, 5000000.0f);
    }
 
    void UpdateBuffers()
	{ 
		if ( instanceCount < 1 ) instanceCount = 1;

        // Positions & Colors
        if (gpuBuffer != null) gpuBuffer.Release();
		
        gpuInstanceData = new GpuInstanceData[instanceCount];
        
        for (int i=0; i < instanceCount; i++)
		{
            var angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            var distance = Random.Range(20.0f, 100.0f);
            var height = Random.Range(-2.0f, 2.0f);
            var pos = new Vector3(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance);
            var matrix = Matrix4x4.Translate(pos);
            var color = new Vector4(Random.value, Random.value, Random.value, 1f);
            gpuInstanceData[i] = new GpuInstanceData() { Color = color, Matrix = matrix };
        }

        gpuBuffer = new ComputeBuffer(instanceCount, 20 * 4);
        gpuBuffer.SetData(gpuInstanceData);

        opaqueMaterial.SetBuffer("instanceBuffer", gpuBuffer);

        // indirect args
        {
            uint numIndices = (opaqueMesh != null) ? (uint)opaqueMesh.GetIndexCount(0) : 0;
            opaqueArgs[0] = numIndices;
            opaqueArgs[1] = (uint)instanceCount;
            opaqueArgsBuffer.SetData(opaqueArgs);
        }
        cachedInstanceCount = instanceCount;
    }

    void OnDisable()
	{
        if (gpuBuffer != null) gpuBuffer.Release();
        gpuBuffer = null;

        if (opaqueArgsBuffer != null) opaqueArgsBuffer.Release();
        opaqueArgsBuffer = null;
    }
}
