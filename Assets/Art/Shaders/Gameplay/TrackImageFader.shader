Shader "Custom/TrackImageFader"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {} // Main texture
        _DepthTex ("Depth Texture", 2D) = "white" {} // Depth texture
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _DepthTex;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Get the depth value from the depth texture
                float depth = tex2D(_DepthTex, i.uv).r;

                // Convert the depth value to linear 0-1 range using built-in macro
                float linearDepth = Linear01Depth(depth);
                
                return fixed4(linearDepth, linearDepth, linearDepth, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
