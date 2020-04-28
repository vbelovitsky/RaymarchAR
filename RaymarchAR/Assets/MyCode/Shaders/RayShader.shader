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

            uniform float4 _positions[4];
            uniform float4 _colors[4];
            uniform float4 _sizes[4];
            uniform float _types[4];
            uniform float _operations[4];

            uniform int _numShapes;

            static const float maxDst = 50;
            static const float epsilon = 0.01f;

            struct Shape
            {
                float3 position;
                float3 color;
                float3 size;
                int type;
                int operation;
            };
            

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
                half index = v.vertex.z;
                v.vertex.z = 0.1;
    
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy;
    
                // #if UNITY_UV_STARTS_AT_TOP
                // if (_MainTex_TexelSize.y < 0)
                // o.uv.y = 1 - o.uv.y;
                // #endif

                o.ray = _FrustumCornersES[(int)index].xyz;

                o.ray = mul(_CameraInvViewMatrix, o.ray);
                return o;
            }

            Shape GetShape(int index)
            {
                Shape shape;
                shape.position = _positions[index].xyz;
                shape.color = _colors[index].xyz;
                shape.size = _sizes[index].xyz;
                shape.type = (int)_types[index];
                shape.operation = (int)_operations[index];
                return shape;
            }

            // float sdTorus(float3 p, float2 t)
            // {
            //     float2 q = float2(length(p.xz) - t.x, p.y);
            //     return length(q) - t.y;
            // }

            float sdSphere(float3 p, float3 center, float3 size)
            {
                return length(p - center) - size.x;
            }

            float sdCube(float3 p, float3 center, float3 size)
            {
                float3 q = abs(p - center) - size.x;
                return length(max(q, 0)) + min(max(q.x, max(q.y, q.z)), 0);
            }

            // Union
            float opU(float d1, float d2)
            {
                return min(d1, d2);
            }

            // Subtraction
            float opS(float d1, float d2)
            {
                return max(-d1, d2);
            }

            // Intersection
            float opI(float d1, float d2)
            {
                return max(d1, d2);
            }

            float GetShapeDistance(float3 p, Shape shape)
            {
                if (shape.type == 0)
                {
                    return sdCube(p, shape.position, shape.size);
                }
                else if (shape.type == 1)
                {
                    return sdSphere(p, shape.position, shape.size);
                }
                return maxDst;
            }

            float4 Combine(float dstA, float dstB, float3 colorA, float3 colorB, int operation)
            {
                float dst = dstA;
                float3 color = colorA;

                if (operation == 0)
                {
                    dst = opU(dstA, dstB);
                }
                else if (operation == 1)
                {
                    dst = opS(dstA, dstB);
                }
                else if (operation == 2)
                {
                     dst = opI(dstA, dstB);
                }

                return float4(color, dst);
            }

            float4 map(float3 p)
            {
                float globalDst = maxDst;
                float3 globalColor = 1;

                float localDst = 0;
                float3 localColor = 1;

                for(int i = 0; i < _numShapes; i++)
                {
                    Shape shape = GetShape(i);
                    localDst = GetShapeDistance(p, shape);
                    localColor = shape.color;

                    float4 globalCombined = Combine(globalDst, localDst, globalColor, localColor, shape.operation);
                    globalColor = globalCombined.xyz;
                    globalDst = globalCombined.w;
                }

                return float4(globalColor, globalDst);
            }

            float3 calcNormal(in float3 p) {
                float x = map(float3(p.x + epsilon, p.y, p.z)).w - map(float3(p.x - epsilon, p.y, p.z)).w;
                float y = map(float3(p.x, p.y + epsilon, p.z)).w - map(float3(p.x, p.y-epsilon, p.z)).w;
                float z = map(float3(p.x, p.y, p.z + epsilon)).w - map(float3(p.x, p.y, p.z - epsilon)).w;
                return normalize(float3(x,y,z));
            }

            fixed4 raymarch(float3 ro, float3 rd)
            {
                fixed4 ret = fixed4(0, 0, 0, 0);

                const int maxstep = 100;

                float t = 0; // current distance traveled along ray
                for (int i = 0; i < maxstep; i++) {
                    float3 p = ro + rd * t;   // World space position of sample
                    float4 scene = map(p);
                    float d = scene.w;       // Sample of distance field (see map())
                    
                    // If the sample <= 0, we have hit something (see map()).
                    if (d < epsilon) {
                        // Lambertian Lighting
                        float3 n = calcNormal(p);
                        ret = fixed4(dot(-_LightDir.xyz, n).rrr, 1);
                        //ret = float4(scene.xyz * lighting * shadow, 1);
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
                fixed3 col = tex2D(_MainTex, i.uv);

                // Color of raymarch
                fixed4 add = raymarch(ro, rd);

                // Returns final color using alpha blending
                return fixed4(col * (1.0 - add.w) + add.xyz * add.w, 1.0);
            }

            ENDCG
        }
    }
}
