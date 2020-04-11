// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/RayShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform float4x4 _FrustumCornersES;
            uniform sampler2D _MainTex;
            uniform float4 _MainTex_TexelSize;
            uniform float4x4 _CameraInvViewMatrix;
            uniform float3 _LightDir;
            uniform float3 _CameraWS;

            uniform float4 _Object1;
            uniform float4 _Object2;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 ray : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
    
                // Index passed via custom blit function in RaymarchGeneric.cs
                half index = v.vertex.z;
                v.vertex.z = 0.1;
    
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy;
    
                #if UNITY_UV_STARTS_AT_TOP
                if (_MainTex_TexelSize.y < 0)
                o.uv.y = 1 - o.uv.y;
                #endif

                // Get the eyespace view ray (normalized)
                o.ray = _FrustumCornersES[(int)index].xyz;

                // Transform the ray from eyespace to worldspace
                // Note: _CameraInvViewMatrix was provided by the script
                o.ray = mul(_CameraInvViewMatrix, o.ray);
                return o;
            }

            float sdTorus(float3 p, float2 t)
            {
                float2 q = float2(length(p.xz) - t.x, p.y);
                return length(q) - t.y;
            }

            float sdSphere(float4 s, float3 p)
            {
                return length(p - s.xyz) - s.w;
            }

            float sdCube(float4 s, float3 p)
            {
                float3 q = abs(p - s.xyz) - s.w;
                return length(max(q, 0)) + min(max(q.x, max(q.y, q.z)), 0);
            }

            float map(float3 p)
            {
                float dist1 = sdCube(float4(_Object1), p);
                float dist2 = sdSphere(float4(_Object2), p);
                
                // sdTorus(p, float2(1, 0.2));
                float final =  max(dist1, dist2);
                return final;
            }

            float3 calcNormal(in float3 pos)
            {
                // epsilon - used to approximate dx when taking the derivative
                const float2 eps = float2(0.001, 0.0);

                // The idea here is to find the "gradient" of the distance field at pos
                // Remember, the distance field is not boolean - even if you are inside an object
                // the number is negative, so this calculation still works.
                // Essentially you are approximating the derivative of the distance field at this point.
                float3 nor = float3(
                    map(pos + eps.xyy).x - map(pos - eps.xyy).x,
                    map(pos + eps.yxy).x - map(pos - eps.yxy).x,
                    map(pos + eps.yyx).x - map(pos - eps.yyx).x);
                return normalize(nor);
            }

            fixed4 raymarch(float3 ro, float3 rd)
            {
                fixed4 ret = fixed4(0,0,0,0);

                const int maxstep = 100;
                float t = 0; // current distance traveled along ray
                for (int i = 0; i < maxstep; ++i) {
                    float3 p = ro + rd * t; // World space position of sample
                    float d = map(p);       // Sample of distance field (see map())

                    // If the sample <= 0, we have hit something (see map()).
                    if (d < 0.001) {
                        // Lambertian Lighting
                        float3 n = calcNormal(p);
                        ret = fixed4(dot(-_LightDir.xyz, n).rrr, 1);
                        break;
                    }

                    // If the sample > 0, we haven't hit anything yet so we should march forward
                    // We step forward by distance d, because d is the minimum distance possible to intersect
                    // an object (see map()).
                    t += d;
                }
                return ret;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                float3 rd = normalize(i.ray.xyz);
                float3 ro = _CameraWS;

                // Color of the scene before this shader was run
                fixed3 col = tex2D(_MainTex,i.uv);
                // Color of raymarch
                fixed4 add = raymarch(ro, rd);

                // Returns final color using alpha blending
                return fixed4(col*(1.0 - add.w) + add.xyz * add.w,1.0);
            }

            ENDCG
        }
    }
}
