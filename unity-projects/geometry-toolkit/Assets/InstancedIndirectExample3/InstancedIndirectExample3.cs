// https://docs.unity3d.com/560/Documentation/ScriptReference/Graphics.DrawMeshInstancedIndirect.html

using UnityEngine;
using UnityEngine.Rendering;

public class InstancedIndirectExample3 : MonoBehaviour
{ 
    public int instanceCount = 100000;
    public Mesh opaqueMesh;
    public Mesh transparentMesh;
    public Material opaqueMaterial;
    public Material transparentMaterial;

    public ShadowCastingMode castShadows = ShadowCastingMode.Off;
    public bool receiveShadows = false;

    private int cachedInstanceCount = -1;
    private ComputeBuffer positionBuffer;
	private ComputeBuffer colorBuffer;

    private ComputeBuffer opaqueArgsBuffer;
    private ComputeBuffer transparentArgsBuffer;

    private uint[] opaqueArgs = new uint[5] { 0, 0, 0, 0, 0 };
    private uint[] transparentArgs = new uint[5] { 0, 0, 0, 0, 0 };

    void Start()
	{
        opaqueArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
        transparentArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
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
        Graphics.DrawMeshInstancedIndirect(transparentMesh, 0, transparentMaterial, bounds, transparentArgsBuffer, 0, null, castShadows, receiveShadows);
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
        if (positionBuffer != null) positionBuffer.Release();
		if (colorBuffer != null) colorBuffer.Release();

        positionBuffer	= new ComputeBuffer(instanceCount, 16);
		colorBuffer		= new ComputeBuffer(instanceCount, 4*4);

        var positions = new Vector4[instanceCount];
		var colors	= new Vector4[instanceCount];

        for (var i=0; i < instanceCount; i++)
		{
            var angle = Random.Range(0.0f, Mathf.PI * 2.0f);
            var distance = Random.Range(20.0f, 100.0f);
            var height = Random.Range(-2.0f, 2.0f);
            var size = Random.Range(0.05f, 0.25f);
            positions[i]	= new Vector4(Mathf.Sin(angle) * distance, height, Mathf.Cos(angle) * distance, size);
			colors[i]		= new Vector4( Random.value, Random.value, Random.value, 1f );
        }

        positionBuffer.SetData(positions);
		colorBuffer.SetData(colors);

        opaqueMaterial.SetBuffer("positionBuffer", positionBuffer);
        opaqueMaterial.SetBuffer("colorBuffer", colorBuffer);

        transparentMaterial.SetBuffer("positionBuffer", positionBuffer);
        transparentMaterial.SetBuffer("colorBuffer", colorBuffer);

        // indirect args
        {
            var numIndices = (opaqueMesh != null) ? (uint)opaqueMesh.GetIndexCount(0) : 0;
            opaqueArgs[0] = numIndices;
            opaqueArgs[1] = (uint)instanceCount;
            opaqueArgsBuffer.SetData(opaqueArgs);
        }
        {
            var numIndices = (transparentMesh != null) ? (uint)transparentMesh.GetIndexCount(0) : 0;
            transparentArgs[0] = numIndices;
            transparentArgs[1] = (uint)instanceCount;
            transparentArgsBuffer.SetData(transparentArgs);
        }

        cachedInstanceCount = instanceCount;
    }

    void OnDisable()
	{
        if (positionBuffer != null) positionBuffer.Release();
        positionBuffer = null;

		if (colorBuffer != null) colorBuffer.Release();
        colorBuffer = null;

        if (opaqueArgsBuffer != null) opaqueArgsBuffer.Release();
        opaqueArgsBuffer = null;

        if (transparentArgsBuffer != null) transparentArgsBuffer.Release();
        transparentArgsBuffer = null;
    }
}
