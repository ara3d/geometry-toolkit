Shader "GeometryToolkit/ClonerTransparent" 
{
	Properties
	{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags 
		{ 
			"RenderType" = "Transparent"
			"Queue" = "Transparent" 
		}
		LOD 200

		Blend SrcAlpha OneMinusSrcAlpha		
		
		CGPROGRAM

#pragma surface surf Standard addshadow keepalpha
#pragma multi_compile_instancing
#pragma instancing_options procedural:setup

#include "ClonerCommon.cginc"

	ENDCG
	}
	FallBack "Diffuse"
}
