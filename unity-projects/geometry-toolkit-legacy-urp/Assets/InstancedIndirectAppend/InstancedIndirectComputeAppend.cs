using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// This demo shows the use of Compute Shaders to update the object's positions. 
/// The buffer is stored and updated directly in GPU.
/// The append buffer can be used when there is an unknown output buffer size.
/// </summary>
public class InstancedIndirectComputeAppend : MonoBehaviour
{
	public int instanceCount = 100000;
	public Mesh instanceMesh;
	public Material instanceMaterial;

	public ShadowCastingMode castShadows = ShadowCastingMode.Off;
	public bool receiveShadows = false;

	public ComputeShader positionComputeShader;
	private int positionComputeKernelId;

	private ComputeBuffer positionAppendBuffer;
	private ComputeBuffer argsBuffer;

	void Start()
	{
		/// it's 5 args: index count per instance, instance count, start index location, base vertex location, start instance location.
		argsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
		CreateBuffers();
	}

	void Update()
	{
		// Update position buffer
		UpdateBuffers();

		// Render - same old, only now we'll have argsBuffer with the count set from the append result.
		Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, instanceMesh.bounds, argsBuffer, 0, null, castShadows, receiveShadows);
	}

	void UpdateBuffers()
	{
		/// reset the append buffer counter,
		/// this is important! otherwise we'll keep appending to a buffer indefinitely!
		positionAppendBuffer.SetCounterValue(0);

		/// TODO this only works with POT, integral sqrt vals
		int bs = instanceCount / 64;
		positionComputeShader.Dispatch(positionComputeKernelId, bs, 1, 1);
		positionComputeShader.SetBuffer(positionComputeKernelId, "positionBuffer", positionAppendBuffer);
		positionComputeShader.SetFloat("_Dim", Mathf.Sqrt(instanceCount));

		/// as we don't know exactly how many positions were output, we use this function
		/// to copy the count from positionAppendBuffer to argsBuffer, which will be used for rendering.
		/// The offset 4 is because the instance count is placed in args[1] for the DrawMeshInstancedIndirect
		/// + info https://docs.unity3d.com/ScriptReference/ComputeBuffer.CopyCount.html
		ComputeBuffer.CopyCount(positionAppendBuffer, argsBuffer, 4);
	}

	void CreateBuffers()
	{
		if (instanceCount < 1)
			instanceCount = 1;

		instanceCount = Mathf.ClosestPowerOfTwo(instanceCount);

		positionComputeKernelId = positionComputeShader.FindKernel("CSPositionKernel");
		instanceMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);

		if (positionAppendBuffer != null)
			positionAppendBuffer.Release();

		/// note the compute buffer append type!
		positionAppendBuffer = new ComputeBuffer(instanceCount, 16, ComputeBufferType.Append);
		positionAppendBuffer.SetCounterValue(0);
		instanceMaterial.SetBuffer("positionBuffer", positionAppendBuffer);

		// indirect args
		uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
		argsBuffer.SetData(new uint[5] { numIndices, (uint)instanceCount, 0, 0, 0 });
	}

	void OnDisable()
	{
		if (positionAppendBuffer != null)
			positionAppendBuffer.Release();
		positionAppendBuffer = null;

		if (argsBuffer != null)
			argsBuffer.Release();
		argsBuffer = null;
	}

	void OnGUI()
	{
		GUI.Label(new Rect(265, 12, 200, 30), "Instance Count: " + instanceCount.ToString("N0"));
	}
}
