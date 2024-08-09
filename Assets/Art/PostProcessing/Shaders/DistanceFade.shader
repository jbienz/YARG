Shader "Hidden/YARG/DistanceFade"
{
	Properties 
	{
	    _MainTex ("Main Texture", 2D) = "white" {}
        _NearClip("Near Clip", Float) = 0
        _FarClip("Far Clip", Float) = 1
        _NearFade("Near Fade", Float) = 0.1
        _FarFade("Far Fade", Float) = 0.1
        [Toggle] _PreviewMask("Preview Mask", Float) = 0
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

            struct Attributes
            {
                float4 positionOS       : POSITION;
                float2 uv               : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv        : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.vertex = vertexInput.positionCS;
                output.uv = input.uv;
                
                return output;
            }
            
            float4 frag (Varyings input) : SV_Target 
            {
                // Get Depth
                float depth = SampleSceneDepth(input.uv);

                // Convert to linear 0 - 1 value
                float lin = Linear01Depth(depth, _ZBufferParams);

                // Return grayscale color value
                return float4(lin, lin, lin, 1);
            }
            
			ENDHLSL
		}
	} 
	FallBack "Diffuse"
}