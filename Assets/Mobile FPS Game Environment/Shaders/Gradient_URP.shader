Shader "Custom/Gradient_3Colors_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorTop ("Top Color", Color) = (1,1,1,1)
        _ColorMid ("Mid Color", Color) = (1,1,1,1)
        _ColorBot ("Bot Color", Color) = (1,1,1,1)
        _Middle ("Middle", Range(0.001, 0.999)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode" = "SRPDefaultUnlit"}
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                uniform float4 _MainTex_ST;
                uniform float4 _ColorTop;
                uniform float4 _ColorMid;
                uniform float4 _ColorBot;
                float _Middle;
            CBUFFER_END

            Varyings vert (Attributes attr)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(attr.positionOS);
                output.uv = TRANSFORM_TEX(attr.uv, _MainTex);
                return output;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float4 c = lerp(_ColorBot, _ColorMid, i.uv.y / _Middle) * step(i.uv.y, _Middle);
                c += lerp(_ColorMid, _ColorTop, (i.uv.y - _Middle) / (1 - _Middle)) * step(_Middle, i.uv.y);
                c.a = 1;
                return c;
            }
            ENDHLSL
        }
    }
}