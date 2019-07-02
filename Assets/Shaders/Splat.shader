Shader "FI/Splat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "Vertex.cginc"
			

            float4 frag (v2f i) : SV_Target
            {
				float4 col = tex2D(_MainTex, i.uv);

				float2 p = i.uv - _Pos;
				float3 splat = exp(-dot(p, p) / _Radius) * _Color;
				splat *= 1 - tex2D(_ObstaclesTex, i.uv);
             
				return float4(splat + col.rgb, 1.0);
            }
            ENDCG
        }
    }
}
