Shader "Custom/GlassBlockShader"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Tint ("Tint Color", Color) = (1,1,1,1)
        _ShineColor ("Shine Color", Color) = (1,1,1,1)
        _ShineStrength ("Shine Strength", Range(0,2)) = 0.5

        _WorldLightPos ("World Light Position", Vector) = (0, 5, 0, 0)
        _DistanceFalloff ("Distance Falloff", Range(0.0, 5.0)) = 1.5

        _ShimmerStrength ("Shimmer Strength", Range(0, 1)) = 0.15
        _ShimmerSpeed ("Shimmer Speed", Range(0, 5)) = 1.0

        _EdgeDarkness ("Edge Darkness", Range(0, 1)) = 0.25
        _EdgeWidth ("Edge Width", Range(0, 0.5)) = 0.15

        _GlassTransparency ("Glass Transparency", Range(0,1)) = 0.3


        // NEW: Colored internal reflections
        _InternalTintColor ("Internal Tint Color", Color) = (1, 0.8, 1, 1)
        _InternalTintStrength ("Internal Tint Strength", Range(0,1)) = 0.25
        _InternalAngleSharpness ("Internal Angle Sharpness", Range(0,5)) = 1.5
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Tint;
            fixed4 _ShineColor;
            float _ShineStrength;
            float3 _WorldLightPos;
            float _DistanceFalloff;

            float _ShimmerStrength;
            float _ShimmerSpeed;

            float _EdgeDarkness;
            float _EdgeWidth;

            fixed4 _InternalTintColor;
            float _InternalTintStrength;
            float _InternalAngleSharpness;

            float _GlassTransparency;


            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                fixed4 color : COLOR;
                float randSeed : TEXCOORD2;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.color = v.color;
                o.randSeed = Hash21(o.worldPos.xy);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Tint * i.color;

                //---------------------------------------------
                // WORLD-AWARE SHINE
                //---------------------------------------------
                float3 toLight = _WorldLightPos - i.worldPos.xyz;
                float dist = length(toLight);
                float3 dir = normalize(toLight);

                float2 centeredUV = i.uv - 0.5;
                float3 fakeNormal = normalize(float3(centeredUV.x, centeredUV.y, 1));

                float shine = max(0, dot(fakeNormal, dir));
                shine *= exp(-dist * _DistanceFalloff);

                //---------------------------------------------
                // SHIMMER
                //---------------------------------------------
                float shimmer = sin((_Time.y * _ShimmerSpeed)
                                    + (i.uv.x * 10.0)
                                    + (i.uv.y * 10.0)
                                    + (i.randSeed * 6.2831));

                shimmer = shimmer * 0.5 + 0.5;
                shine += shimmer * _ShimmerStrength;

                col.rgb += _ShineColor.rgb * shine * _ShineStrength;

                //---------------------------------------------
                // EDGE DARKENING
                //---------------------------------------------
                float2 edgeDist = min(i.uv, 1.0 - i.uv);
                float edgeFactor = saturate(edgeDist.x / _EdgeWidth);
                edgeFactor = min(edgeFactor, saturate(edgeDist.y / _EdgeWidth));
                float edgeDark = 1.0 - edgeFactor;
                col.rgb *= (1.0 - edgeDark * _EdgeDarkness);

                //---------------------------------------------
                // COLORED INTERNAL REFLECTIONS (NEW)
                //---------------------------------------------
                // Angle-based tint: stronger when light hits at grazing angles
                float angleTint = pow(1.0 - abs(dot(fakeNormal, dir)), _InternalAngleSharpness);

                // Add randomness so each block’s tint is subtly different
                float smallNoise = sin(i.randSeed * 12.789 + _Time.y * 0.2) * 0.05;

                float tintIntensity = saturate(angleTint + smallNoise) * _InternalTintStrength;

                col.rgb = lerp(col.rgb, col.rgb * _InternalTintColor.rgb, tintIntensity);

                //---------------------------------------------
                // OUTPUT
                //---------------------------------------------
                col.a *= (1.0 - _GlassTransparency);

                return col;
            }
            ENDCG
        }
    }
}
