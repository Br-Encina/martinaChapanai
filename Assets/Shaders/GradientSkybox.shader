Shader "Custom/GradientSkybox"
{
    Properties
    {
        _MainTex ("Gradient Texture (vertical)", 2D) = "white" {}
        _Exposure ("Exposure", Range(0, 4)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            half _Exposure;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 dir : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.dir = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.dir);
                // dir.y goes from -1 (looking down) to 1 (looking up).
                // Texture is authored black at the top, so sample it inverted:
                // looking up (dir.y = 1) -> v = 1 (top of texture).
                float v = saturate(dir.y * 0.5 + 0.5);
                fixed4 col = tex2D(_MainTex, float2(0.5, v));
                col.rgb *= _Exposure;
                return col;
            }
            ENDCG
        }
    }
    Fallback Off
}
