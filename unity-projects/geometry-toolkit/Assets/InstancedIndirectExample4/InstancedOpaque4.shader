Shader "Instanced/InstancedOpaque4" 
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader
	{
		Tags 
		{ 
			"RenderType" = "Opaque"
		}
		LOD 200

		CGPROGRAM

#pragma surface surf Standard addshadow 
#pragma multi_compile_instancing
#pragma instancing_options procedural:setup

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		struct InstanceData 
		{
			float4x4 mat;
			float4 col;
		};

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		StructuredBuffer<InstanceData> instanceBuffer;
#endif

		void setup()
		{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			//unity_ObjectToWorld = unity_InstanceID % 100 != 0 ? 0 : instanceBuffer[unity_InstanceID].mat;
			unity_ObjectToWorld = instanceBuffer[unity_InstanceID].mat;
			//unity_WorldToObject = math.inverse(unity_ObjectToWorld);
			//if (unity_InstanceID % 10 != 0) unity_ObjectToWorld = float4x4.zero;
#endif
		}

		half _Glossiness;
		half _Metallic;

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			//float4 col = float4(0, 0, 1, 1);
			float4 col = instanceBuffer[unity_InstanceID].col;
#else
			float4 col = float4(0, 0, 1, 1);
#endif

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * col;
			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
		FallBack "Diffuse"
}
