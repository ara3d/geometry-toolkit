// https://toqoz.fyi/thousands-of-meshes.html
// https://github.com/ttvertex/Unity-InstancedIndirectExamples

Shader "GeometryToolkit/Cloner" 
{
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model
		#pragma surface surf Standard addshadow
		#pragma multi_compile_instancing
		#pragma instancing_options procedural:setup

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct GpuInstanceData {
                float4x4 mat;
                float4 color;
            };

            StructuredBuffer<GpuInstanceData> _InstanceData;		
		#endif
		

	void setup()
	{
	}

	half _Glossiness;
	half _Metallic;

	// https://forum.unity.com/threads/single-pass-instancing-surface-shader-with-vertex-modifier.1137136/
	void vert(inout appdata_full v) {
		DEFAULT_UNITY_SETUP_INSTANCE_ID(v);
#if defined(UNITY_INSTANCING_ENABLED) || defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) || defined(UNITY_STEREO_INSTANCING_ENABLED)
        float4 pos = mul(_InstanceData[unity_InstanceID].mat, v.vertex);
        v.vertex = pos;
#endif
    }

	void surf(Input IN, inout SurfaceOutputStandard o) 
	{
		float4 col = 1.0f;

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		col = _InstanceData[unity_InstanceID].color;
#else
		col = float4(0, 0, 1, 1);
#endif
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * col;
		o.Albedo = c.rgb;
		o.Metallic = _Metallic;
		o.Smoothness = _Glossiness;
		o.Alpha = c.a;
	}
	ENDCG
	}
	FallBack "Diffuse"
}
