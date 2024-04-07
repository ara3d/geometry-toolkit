using Ara3D.ProceduralGeometry.Unity;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Efficient rendering of multiple meshes using GPU instancing
/// https://toqoz.fyi/thousands-of-meshes.html
/// https://docs.unity3d.com/ScriptReference/Graphics.RenderMeshIndirect.html
/// https://github.com/keijiro/StickShow/blob/main/Assets/Scripts/StickShow.cs
/// https://github.com/GarrettGunnell/Grass/blob/main/Assets/Scripts/ModelGrass.cs
/// https://github.com/ttvertex/Unity-InstancedIndirectExamples
/// </summary>
[ExecuteAlways]
public class Cloner : MonoBehaviour
{
    /// <summary>
    /// Contains attribute information for each clone instance
    /// </summary>
    public CloneData CloneData = new();

    /// <summary>
    /// The instanced mesh.  
    /// </summary>
    public Mesh Mesh;

    /// <summary>
    /// A GPU instanced material 
    /// </summary>
    public Material Material;

    /// <summary>
    /// Set to true to receive shadows
    /// </summary>
    public bool RecieveShadows = false;

    /// <summary>
    /// Control how shadows are cast. 
    /// </summary>
    public ShadowCastingMode CastShadows = ShadowCastingMode.Off;

    /// <summary>
    /// The number of instances the user wants 
    /// </summary>
    public int NumInstances = 100;

    /// <summary>
    /// Render parameters 
    /// </summary>
    private RenderParams renderParams;

    /// <summary>
    /// The command buffer 
    /// </summary>
    private GraphicsBuffer commandBuffer;
    private GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;

    // Start is called before the first frame update
    public void Start()
    {
        CreateCommandBuffer();
        CloneData.Resize(100);
    }

    public void CreateCommandBuffer()
    {
        const int CommandCount = 1;
        commandBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, CommandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[CommandCount];
    }

    public int NumRows = 10;
    public int NumColumns = 10;
    public int NumPlanes = 10;
    public int LayerSize => NumRows * NumColumns;
    public int RowSize => NumColumns;
    public float Spacing = 1.5f;

    public void UpdateNumInstances(int numInstances)
    {
        var oldNumInstances = CloneData.NumInstances;
        CloneData.Resize(NumInstances);
        for (var i = 0; i < CloneData.NumInstances; i++)
        {
            var column = i % NumColumns;
            var row = (i / RowSize) % NumRows;
            var layer = i / LayerSize;

            var cloneInstance = CloneData.CpuData[i];
            cloneInstance.Acceleration = float3.zero;
            cloneInstance.Color = new float4(0.5f, 0.6f, 0.7f, 0.8f);
            cloneInstance.Enabled = true;
            cloneInstance.Position = new float3(column * Spacing, row * Spacing, layer * Spacing);
            cloneInstance.Rotation = Quaternion.identity;
            cloneInstance.Scale = Vector3.one;

            CloneData.CpuData[i] = cloneInstance;
        }
    }

    // Update is called once per frame
    // TODO: this needs to move to a CloneRenderer 
    public void Update()
    {
        if (Mesh == null)
            return;
        if (NumInstances == 0)
            return;
        if (commandData == null)
            CreateCommandBuffer();

        if (NumInstances != CloneData.NumInstances)
        {
            UpdateNumInstances(NumInstances);
        }

        // TODO: use the JOB system to update the clone data. 
        // For now, we just ask it to loop through all instances and update them. 
        // It will return a compute buffer that we can pass to the material 
        CloneData.TimeStep = Time.deltaTime;
        var buffer = CloneData.UpdateAllInstances();
        
        renderParams = new RenderParams(Material)
        {   
            // TODO: fix this temporary hack, by estimating the bounds using the bounds of all positions 
            // to define bounds to cull and sort the geometry rendered with the method as a single entity..
            worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one), // use tighter bounds for better FOV culling

            receiveShadows = RecieveShadows,
            shadowCastingMode = CastShadows,
            //matProps = new MaterialPropertyBlock()
        };
        // rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
        
        // We make the instance data available to the shader 
        Material.SetBuffer("_InstanceData", buffer);

        /*
        // This is the magical incantation needed to say: hey Unity, render NumInstances 
        commandData[0].indexCountPerInstance = Mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)CloneData.NumInstances;
        commandBuffer.SetData(commandData);
        */

        //Graphics.RenderMeshIndirect(renderParams, Mesh, commandBuffer, commandData.Length);

        Graphics.RenderMeshInstanced(renderParams, Mesh, 0, CloneData.GpuData);
    }
}
