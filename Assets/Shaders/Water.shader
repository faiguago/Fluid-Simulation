Shader "FI/Water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo_1 (RGB)", 2D) = "white" {}
		_SecondaryTex("Albedo_2 (RGB)", 2D) = "white" {}
		_MainNormal("Normal_1", 2D) = "bump" {}
		_SecondaryNormal("Normal_2", 2D) = "bump" {}
		_MaskTex("Mask (R)", 2D) = "black" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _SecondaryTex, _MaskTex, _MainNormal, _SecondaryNormal, _ObstaclesTex;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv_MaskTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			float obstaclesMask = tex2D(_ObstaclesTex, IN.uv_MaskTex);

			float mask = tex2D(_MaskTex, IN.uv_MaskTex);
			mask = saturate(mask);

            float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			float4 c2 = tex2D(_SecondaryTex, IN.uv_MainTex) * _Color;
			o.Albedo = lerp(c, c2, mask);

			float3 mainNormal = UnpackNormal(tex2D(_MainNormal, IN.uv_MainTex));
			float3 secondaryNormal = UnpackNormal(tex2D(_SecondaryNormal, IN.uv_MainTex));

			o.Normal = lerp(mainNormal, secondaryNormal, mask);

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
