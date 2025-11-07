Shader "Custom/StainedGlassBlockShader"
{
    Properties
    {
        _MainTex ("Sprite", 2D) = "white" {}
        _Tint ("Base Tint Color", Color) = (1,1,1,1)
        
        // Glass Properties
        _GlassThickness ("Glass Thickness", Range(0,1)) = 0.7
        _Translucency ("Light Pass-Through", Range(0,1)) = 0.4
        _InternalScatter ("Internal Light Scatter", Range(0,2)) = 0.8
        
        // Sunlight Rays
        _RayColor ("Sunlight Color", Color) = (1, 0.95, 0.8, 1)
        _RayCount ("Number of Rays", Range(1, 8)) = 3
        _RayWidth ("Ray Width", Range(0.01, 0.3)) = 0.08
        _RayIntensity ("Ray Intensity", Range(0, 2)) = 1.2
        _RaySpeed ("Ray Movement Speed", Range(0, 1)) = 0.15
        _RayAngle ("Ray Angle", Range(-90, 90)) = 30
        
        // Stained Glass Effects
        _ColorVariation ("Color Variation", Range(0, 1)) = 0.3
        _EdgeGlow ("Edge Glow", Range(0, 1)) = 0.4
        _EdgeGlowColor ("Edge Glow Color", Color) = (1, 0.9, 0.7, 1)
        
        // Surface Detail
        _SurfaceNoise ("Surface Imperfections", Range(0, 1)) = 0.15
        _NoiseScale ("Noise Scale", Range(1, 20)) = 8.0
        
        // Shimmer
        _ShimmerStrength ("Shimmer Strength", Range(0, 1)) = 0.2
        _ShimmerSpeed ("Shimmer Speed", Range(0, 5)) = 1.5
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
            float _GlassThickness;
            float _Translucency;
            float _InternalScatter;
            
            fixed4 _RayColor;
            float _RayCount;
            float _RayWidth;
            float _RayIntensity;
            float _RaySpeed;
            float _RayAngle;
            
            float _ColorVariation;
            float _EdgeGlow;
            fixed4 _EdgeGlowColor;
            
            float _SurfaceNoise;
            float _NoiseScale;
            
            float _ShimmerStrength;
            float _ShimmerSpeed;

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

            // Improved hash function for better randomness
            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.78));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // 2D noise function
            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
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
                // SURFACE IMPERFECTIONS (glass texture)
                //---------------------------------------------
                float surfaceNoise = Noise(i.uv * _NoiseScale + i.randSeed);
                float glassTexture = lerp(1.0, surfaceNoise, _SurfaceNoise);
                col.rgb *= glassTexture;
                
                //---------------------------------------------
                // COLOR VARIATION (stained glass effect)
                //---------------------------------------------
                float colorNoise = Noise(i.uv * 3.0 + float2(i.randSeed * 10.0, 0));
                float3 colorShift = lerp(float3(1,1,1), 
                                        float3(1.0 + colorNoise * 0.3, 
                                               1.0 - colorNoise * 0.2, 
                                               1.0 + colorNoise * 0.1), 
                                        _ColorVariation);
                col.rgb *= colorShift;
                
                //---------------------------------------------
                // MOVING SUNLIGHT RAYS
                //---------------------------------------------
                float rayAngleRad = radians(_RayAngle);
                float2 rayDir = float2(cos(rayAngleRad), sin(rayAngleRad));
                
                // Animate ray position over time (moves across screen)
                float timeOffset = _Time.y * _RaySpeed;
                float rayProjection = dot(i.worldPos.xy, rayDir) + timeOffset;
                
                float rayEffect = 0.0;
                for (float r = 0; r < _RayCount; r++)
                {
                    // Space out rays evenly with some randomness
                    float rayOffset = (r / _RayCount) * 3.0 + Hash21(float2(r, 17.3)) * 0.5;
                    float rayPos = frac((rayProjection + rayOffset) * 0.3);
                    
                    // Create soft ray shape
                    float rayDist = abs(rayPos - 0.5) * 2.0;
                    float ray = smoothstep(_RayWidth, 0.0, rayDist);
                    
                    // Add subtle variation to each ray
                    ray *= (0.8 + Hash21(float2(r, 42.1)) * 0.4);
                    
                    rayEffect += ray;
                }
                
                rayEffect = saturate(rayEffect * _RayIntensity);
                
                // Apply rays as light passing through glass
                col.rgb = lerp(col.rgb, col.rgb * _RayColor.rgb * 1.5, rayEffect * _Translucency);
                
                //---------------------------------------------
                // INTERNAL LIGHT SCATTER (thick glass glow)
                //---------------------------------------------
                float2 centeredUV = i.uv - 0.5;
                float distFromCenter = length(centeredUV);
                float scatter = exp(-distFromCenter * 2.0) * _InternalScatter;
                col.rgb += col.rgb * scatter * 0.3;
                
                //---------------------------------------------
                // EDGE GLOW (light catching edges)
                //---------------------------------------------
                float2 edgeDist = min(i.uv, 1.0 - i.uv);
                float edgeFactor = min(edgeDist.x, edgeDist.y);
                float edgeGlow = smoothstep(0.15, 0.0, edgeFactor);
                
                // Add ray influence to edge glow
                edgeGlow *= (1.0 + rayEffect * 0.5);
                
                col.rgb = lerp(col.rgb, _EdgeGlowColor.rgb * col.rgb * 1.8, edgeGlow * _EdgeGlow);
                
                //---------------------------------------------
                // SHIMMER (light dancing on surface)
                //---------------------------------------------
                float shimmer = sin((_Time.y * _ShimmerSpeed)
                                    + (i.uv.x * 8.0)
                                    + (i.uv.y * 12.0)
                                    + (i.randSeed * 6.28));
                shimmer = shimmer * 0.5 + 0.5;
                
                // Shimmer should be subtle and affected by rays
                shimmer *= (1.0 + rayEffect * 0.3);
                col.rgb += col.rgb * shimmer * _ShimmerStrength * 0.15;
                
                //---------------------------------------------
                // GLASS THICKNESS (opacity without full transparency)
                //---------------------------------------------
                // Make it look thick but still let light through
                col.a = lerp(0.85, 1.0, _GlassThickness);
                
                // Reduce alpha slightly where rays hit (light passes through)
                col.a *= lerp(1.0, 0.92, rayEffect * _Translucency);
                
                return col;
            }
            ENDCG
        }
    }
}