Shader "FI/Advection"
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
				float2 vel = tex2D(_VelocityTex, i.uv);
				float2 uv = i.uv - unity_DeltaTime.x * _SpeedMultiplier * vel * _TexelSize;
             
				return float4((_Dissipation * tex2D(_MainTex, uv)).rgb, 1.0);
            }

            ENDCG
        }
    }
}
