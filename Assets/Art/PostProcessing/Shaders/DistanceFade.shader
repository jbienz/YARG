Shader "Hidden/YARG/DistanceFade"
{
	Properties 
	{
	    _MainTex ("Main Texture", 2D) = "white" {}
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
		
		Pass
		{
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
			#pragma vertex vert
			#pragma fragment frag
			
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
			float _Intensity;
            float4 _OverlayColor;
            
            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv        : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
            };
            
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.vertex);
                
                return output;
            }
            
            float4 frag (Varyings input) : SV_Target 
            {
                // Color Mode WITHOUT Overlay
				float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
				return color;


                // Color Mode WITH Overlay
				// float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
				// return lerp(color, _OverlayColor, _Intensity);

                // Depth Direct
                // float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.screenPos.xy);
                // float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.uv.xy);
                // return float4(depth, 0, 0, 1);
                // float grayscale = Linear01Depth(depth);
                // return float4(grayscale, grayscale, grayscale, 1);

                // // Sample the depth value
                // float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.screenPos.xy).r;

                // // Convert depth to grayscale
                // float grayscale = Linear01Depth(depth);

                // return float4(grayscale, grayscale, grayscale, 1.0);

                // #if UNITY_REVERSED_Z
                //     real depth = SampleSceneDepth(input.uv);
                // #else
                //     // Adjust z to match NDC for OpenGL
                //     real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(input.screenPos.xy));
                // #endif
                // return float4(depth, 0, 0, 1);
            }
            
			ENDHLSL
		}
	} 
	FallBack "Diffuse"
}