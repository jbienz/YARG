Shader "Hidden/YARG/DistanceFade"
{
	Properties 
	{
	    _MainTex ("Main Texture", 2D) = "white" {}
        _NearClip("Near Clip", Float) = 0.1
        _FarClip("Far Clip", Float) = 0.3
        _NearFade("Near Fade", Float) = 0.01
        _FarFade("Far Fade", Float) = 0.1
        [Toggle] _PreviewMask("Preview Mask", Float) = 0.0
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

            float _NearClip = 0.1;
            float _FarClip = 0.3;
            float _NearFade = 0.01;
            float _FarFade = 0.1;
            float _PreviewMask = 0.0;

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

                // Convert to linear 0 - 1 value where
                // NEAR = 0.0
                // FAR = 1.0
                float lin = Linear01Depth(depth, _ZBufferParams);

                // return float4(lin, lin, lin, 1);

                // Check to see if the linear value is between our virtual clipping planes
                if (lin >= _NearClip && lin <= _FarClip)
                {
                    // Within clipping plane, force it to near
                    lin = 0.0;
                }
                // Fade based on NearClip and NearFade
                else if (lin < _NearClip)
                {
                    float fadeFactor = (_NearClip - lin) / _NearFade;
                    lin = fadeFactor;

                    // Ensure lin doesn't exceed 1.0
                    lin = min(lin, 1.0);
                }
                // Fade based on FarClip and FarFade
                else if (lin > _FarClip)
                {
                    float fadeFactor = (lin - _FarClip) / _FarFade;
                    lin = fadeFactor;

                    // Ensure lin doesn't exceed 1.0
                    lin = min(lin, 1.0);
                }

                // Invert the depth buffer so that depth becomes opacity
                // NEAR becomes White (= )1.0
                // FAR = Black = 0.0
                lin = 1.0 - lin;

                // Are we previewing mask or doing alpha?
                if (_PreviewMask > 0)
                {
                    // Return grayscale color value
                    return float4(lin, lin, lin, 1);
                }
                else
                {
                    // Sample the color
                    float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);

                    // Change the alpha
                    color.a = lin;

                    // Return the color
                    return color;
                }
            }
            
			ENDHLSL
		}
	} 
	FallBack "Diffuse"
}