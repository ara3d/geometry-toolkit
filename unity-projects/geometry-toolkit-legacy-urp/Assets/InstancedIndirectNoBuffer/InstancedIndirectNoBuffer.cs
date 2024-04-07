using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This demo shows the use of the procedural instancing features to render objects
/// without need of any position buffer. The values are calculated direclty inside the 
/// shader. 
/// The color buffer is used for debug only.
/// </summary>
public class InstancedIndirectNoBuffer : MonoBehaviour
{
    public int gridDim = 1000;
    public int instanceCount = 0;
    public Mesh instanceMesh;
    public Material instanceMaterial;

    public ShadowCastingMode castShadows = ShadowCastingMode.Off;
    public bool receiveShadows = false;

    private ComputeBuffer argsBuffer;
    private ComputeBuffer colorBuffer;

    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
 
    void Start()
	{
        instanceCount = gridDim * gridDim;
        argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        CreateBuffers();
    }

    void Update()
	{ 
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, instanceMesh.bounds, argsBuffer, 0, null, castShadows, receiveShadows);
    }

    void CreateBuffers()
	{ 
		if ( instanceCount < 1 ) instanceCount = 1;

        //instanceCount = Mathf.ClosestPowerOfTwo(instanceCount);
        instanceMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f); //avoid culling
        
		/// Colors - for debug only
        if (colorBuffer != null) colorBuffer.Release();

        colorBuffer = new ComputeBuffer(instanceCount, 16);

		Vector4[] colors = new Vector4[instanceCount];
        for (int i = 0; i < instanceCount; i++)
            colors[i] = Random.ColorHSV();

        colorBuffer.SetData(colors);

        instanceMaterial.SetBuffer("colorBuffer", colorBuffer);

        // indirect args
        uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)instanceCount;
        argsBuffer.SetData(args);

		Shader.SetGlobalFloat("_Dim", gridDim);
    }

    void OnDisable()
	{
        if (colorBuffer != null) colorBuffer.Release();
        colorBuffer = null;

        if (argsBuffer != null) argsBuffer.Release();
        argsBuffer = null;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(265, 12, 200, 30), "Instance Count: " + instanceCount.ToString("N0"));
    }
}
