Shader "Unlit/DisplacementShader"
{
    Properties
    {
        _MainTex ("Color", 2D) = "white" {}
        //_DepthTex("Depth", 2D) = "white" {}
        _DepthScale("Depth Scale", Float) = 1
        _NearPlane("Near Plane", Float) = 1
        _FarPlane("Far Plane", Float) = 1
        _FocalPoint("Focal Point", Vector) = (0,0,0)

        _GradThresh("Gradient Threshold", Float) = 0.15
        _MaxDepth("Max Depth", Float) = 0.996



    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
            Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;

                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float isCut : TEXCOORD2;
                float depth : TEXCOORD3;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            //sampler2D _DepthTex;
            //float4 _DepthTex_ST;

            float _DepthScale;
            float _GradThresh;
            float _MaxDepth;
            float _NearPlane;
            float _FarPlane;
            float3 _FocalPoint;


            float Remap(float dataValue, float from0, float to0, float from1, float to1)
            {
                return from1 + (dataValue - from0) * (to1 - from1) / (to0 - from0);
            }

           
            v2f vert (appdata v)
            {
                v2f o;
                fixed4 texcol = tex2Dlod(_MainTex, float4(v.uv, 0, 0));
                float4 vert = v.vertex;
                float depth = texcol.a;
                              
                o.depth = depth;
                //float3 focal = float3(0, 0, -_FarPlane);
 


                //float depthToFocal = Remap(depth, 0, _FarPlane, 0, 1);
                //float3 transformed =  lerp(focal, vert.xyz, depth);

                //vert.x = transformed.x;
                //vert.y = transformed.y;
                //vert.z = transformed.z;
                o.vertex = UnityObjectToClipPos(vert);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = TRANSFORM_TEX(v.uv1, _MainTex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float depth = col.a;
                col.a = 1;

                if (abs(i.uv1.x - depth) > _GradThresh || depth > _MaxDepth) discard;
                return col;

                return depth*255%2;
            }
            ENDCG
        }
    }
}
