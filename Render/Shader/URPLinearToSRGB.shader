Shader "Custom/URPLinearToSRGB"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float3 LinearToSRGB(float3 linearColor)
            {
                linearColor = max(linearColor, 0);
                return pow(linearColor, 1.0 / 2.2);
            }


            half4 frag(Varyings IN) : SV_Target
            {
                float4 linearColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                linearColor.rgb = LinearToSRGB(linearColor.rgb);
                return linearColor;
            }

            ENDHLSL
        }
    }
    FallBack "Diffuse"
}
