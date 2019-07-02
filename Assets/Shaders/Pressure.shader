Shader "FI/Pressure"
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
				float R = tex2D(_MainTex, i.rl.xy);
				float L = tex2D(_MainTex, i.rl.zw);
				float T = tex2D(_MainTex, i.tb.xy);
				float B = tex2D(_MainTex, i.tb.zw);
				float C = tex2D(_MainTex, i.uv);
								
				float div = tex2D(_DivergenceTex, i.uv);
				float press = 0.25 * (L + R + B + T - div);
				             
				return float4(press, 0.0, 0.0, 1.0);
            }
            ENDCG
        }
    }
}
