Shader "ClonerIndirect"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"
            #define UNITY_INDIRECT_DRAW_ARGS IndirectDrawIndexedArgs
            #include "UnityIndirect.cginc"

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR0;
            };

            struct InstanceData 
            {
                float3 pos;
                float4 rot;
                float3 scl;
                float4 col;
                float smoothness;
                float metallic;
                uint id;
            };

            StructuredBuffer<InstanceData> instanceBuffer;
            uniform float4x4 _ObjectToWorld;
            uniform int _CommandIndex;

            float4x4 QuatToMatrix(float4 q)
            {
                float4x4 rotMat = float4x4
                (
                    float4(1 - 2 * q.y * q.y - 2 * q.z * q.z, 2 * q.x * q.y + 2 * q.w * q.z, 2 * q.x * q.z - 2 * q.w * q.y, 0),
                    float4(2 * q.x * q.y - 2 * q.w * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z, 2 * q.y * q.z + 2 * q.w * q.x, 0),
                    float4(2 * q.x * q.z + 2 * q.w * q.y, 2 * q.y * q.z - 2 * q.w * q.x, 1 - 2 * q.x * q.x - 2 * q.y * q.y, 0),
                    float4(0, 0, 0, 1)
                );
                return rotMat;
            }

            float4x4 MakeTRSMatrix(float3 pos, float4 rotQuat, float3 scale)
            {
                float4x4 rotPart = QuatToMatrix(rotQuat);
                float4x4 sclPart = float4x4(float4(scale.x, 0, 0, 0), float4(0, scale.y, 0, 0), float4(0, 0, scale.z, 0), float4(0, 0, 0, 1));
                float4x4 trPart = float4x4(float4(1, 0, 0, 0), float4(0, 1, 0, 0), float4(0, 0, 1, 0), float4(pos, 1));
                return mul(mul(transpose(trPart), transpose(rotPart)), transpose(sclPart));
            }

            v2f vert(appdata_base v, uint svInstanceID : SV_InstanceID)
            {
                //InitIndirectDrawArgs(_CommandIndex);
                //uint cmdID = GetCommandID(_CommandIndex);
                //uint instanceID = GetIndirectInstanceID(svInstanceID);
                InstanceData inst = instanceBuffer[svInstanceID + 1000000 * _CommandIndex];
                float4x4 mat = (inst.col.a > 0) ? MakeTRSMatrix(inst.pos, inst.rot, inst.scl) : 0;
                float4 wpos = mul(mat, v.vertex);
                v2f o;
                o.pos = mul(UNITY_MATRIX_VP, wpos);
                o.color = inst.col;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
