		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		struct InstanceData 
		{
			float4x4 mat;
			float4 col;
			float smoothness;
			float metallic;
			float padding1;
			float padding2;
		};

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
		StructuredBuffer<InstanceData> instanceBuffer;
#endif

		void setup()
		{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			InstanceData inst = instanceBuffer[unity_InstanceID];
			float4x4 mat = inst.mat;
			unity_ObjectToWorld = inst.col.w == 0 ? 0 : mat;
#endif
		}

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			InstanceData inst = instanceBuffer[unity_InstanceID];
			float4 col = inst.col;
			o.Smoothness = inst.smoothness;
			o.Metallic = inst.metallic;
#else
			float4 col = float4(0, 0, 1, 1);
#endif

			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * col;
			o.Albedo = c.rgb;
			o.Alpha = col.w;
		}