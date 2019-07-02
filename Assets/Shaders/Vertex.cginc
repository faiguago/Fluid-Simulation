#ifndef SEED_VELOCITY_INCLUDED
#define SEED_VELOCITY_INCLUDED


#include "UnityCG.cginc"

float _Radius, _TexelSize, _Dissipation = 1.0, _SpeedMultiplier = 1.0;

float2 _Pos;

float3 _Color;

sampler2D _MainTex, _VelocityTex, _PressureTex, _DivergenceTex, _ObstaclesTex;

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 rl : TEXCOORD1;
    float4 tb : TEXCOORD2;

    float4 vertex : SV_POSITION;
};

v2f vert(appdata v)
{
    v2f o;
    o.vertex = UnityObjectToClipPos(v.vertex);
    
    o.uv = v.uv;
    o.rl.xy = v.uv + float2(_TexelSize, 0.0);
    o.rl.zw = v.uv + float2(-_TexelSize, 0.0);
    o.tb.xy = v.uv + float2(0.0, _TexelSize);
    o.tb.zw = v.uv + float2(0.0, -_TexelSize);

    return o;
}


#endif