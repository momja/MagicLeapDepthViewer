Shader "IVLabUtilities/RGBD"{
	//show values to edit in inspector
	Properties{
		[HideInInspector]_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader{
		// markers that specify that we don't need culling 
		// or comparing/writing to the depth buffer
		Cull Off
		ZWrite Off 
		ZTest Always

		Pass{
			CGPROGRAM
			//include useful shader functions
			#include "UnityCG.cginc"

			//define vertex and fragment shader
			#pragma vertex vert
			#pragma fragment frag

			//the rendered screen so far
			sampler2D _MainTex;

			//the depth texture
			sampler2D _CameraDepthTexture;


			//the object data that's put into the vertex shader
			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			//the data that's used to generate fragments and can be read by the fragment shader
			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 projPos : TEXCOORD1; //Screen position of pos

			};

			//the vertex shader
			v2f vert(appdata v){
				v2f o;
				//convert the vertex positions from object space to clip space so they can be rendered
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.projPos = ComputeScreenPos(o.position);

				return o;
			}

			float CorrectDepth(float rawDepth)
			{
				float persp = LinearEyeDepth(rawDepth);
				float ortho = (_ProjectionParams.z - _ProjectionParams.y) * (1 - rawDepth) + _ProjectionParams.y;
				return lerp(persp, ortho, unity_OrthoParams.w);
			}
			//the fragment shader
			fixed4 frag(v2f i) : SV_TARGET{
				//get depth from depth texture
				float depth = tex2D(_CameraDepthTexture, i.uv).r;
#ifdef UNITY_REVERSED_Z
			//depth = 1 - depth;
#endif
			depth = Linear01Depth(depth);
				//return depth;
				//linear depth between camera and far clipping plane
				//depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r;
				//depth as distance from camera in units 
				//depth = depth * _ProjectionParams.z/4;
			//depth = Linear01Depth(depth);
			//depth = CorrectDepth(depth)* _ProjectionParams.w;

				//get source color
				fixed4 source = tex2D(_MainTex, i.uv);
				//skip wave and return source color if we're at the skybox
				if(depth >= _ProjectionParams.z)
					return source;

				//calculate wave


				//mix wave into source color
				fixed4 col = source;// lerp(source, _WaveColor, wave);
				col = fixed4(source.xyz, depth);
				return col;
			}
			ENDCG
		}
	}
}

