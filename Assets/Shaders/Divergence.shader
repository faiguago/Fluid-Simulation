Shader "FI/Divergence"
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
				float R = tex2D(_VelocityTex, i.rl.xy).x;
				float L = tex2D(_VelocityTex, i.rl.zw).x;
				float T = tex2D(_VelocityTex, i.tb.xy).y;
				float B = tex2D(_VelocityTex, i.tb.zw).y;
				float2 C = tex2D(_VelocityTex, i.uv);
				
				float MR = tex2D(_ObstaclesTex, i.rl.xy);
				float ML = tex2D(_ObstaclesTex, i.rl.zw);
				float MT = tex2D(_ObstaclesTex, i.tb.xy);
				float MB = tex2D(_ObstaclesTex, i.tb.zw);

				R = lerp(R, -C.x, MR);
				L = lerp(L, -C.x, ML);
				T = lerp(T, -C.y, MT);
				B = lerp(B, -C.y, MB);
												
				float div = 0.5 * (R - L + T - B);
				return float4(div, 0.0, 0.0, 1.0);
            }

            ENDCG
        }
    }
}
