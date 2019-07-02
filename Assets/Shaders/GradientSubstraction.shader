Shader "FI/GradientSubstraction"
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
				float R = tex2D(_PressureTex, i.rl.xy);
				float L = tex2D(_PressureTex, i.rl.zw);
				float T = tex2D(_PressureTex, i.tb.xy);
				float B = tex2D(_PressureTex, i.tb.zw);
				float C = tex2D(_PressureTex, i.uv);
				
				float2 vel = tex2D(_VelocityTex, i.uv);
				vel -= 0.5 * float2(R - L, T - B);

				vel *= 1 - tex2D(_ObstaclesTex, i.uv);

				return float4(vel, 0.0, 1.0);
            }

            ENDCG
        }
    }
}
