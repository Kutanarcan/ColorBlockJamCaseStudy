// Screen-space selection outline, used by SelectionOutline.cs through a camera CommandBuffer.
// Pass 0 (mask):      the selected block's renderers drawn as solid white into a screen-size mask.
// Pass 1 (composite): blitted over the camera image; a pixel outside the mask within _Width pixels of it gets _Color.
Shader "Prototype/SelectionOutline"
{
    Properties
    {
        _MainTex ("Mask", 2D) = "black" {}
        _Color ("Color", Color) = (1, 0.78, 0.2, 1)
        _Width ("Width (px)", Range(1, 6)) = 4
    }
    SubShader
    {
        // Pass 0: mask
        Pass
        {
            ZTest Always ZWrite Off Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 vert (float4 vertex : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(vertex);
            }

            fixed4 frag () : SV_Target
            {
                return 1;
            }
            ENDCG
        }

        // Pass 1: composite
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            #define MAX_WIDTH 6

            sampler2D _MainTex;
            fixed4 _Color;
            float _Width;

            fixed4 frag (v2f_img i) : SV_Target
            {
                // Inside the block: no outline.
                if (tex2D(_MainTex, i.uv).r > 0.5) return 0;

                // Distance (px) to the nearest mask pixel, searched in a disc of radius _Width.
                float2 texel = 1.0 / _ScreenParams.xy;
                float nearest = 1e5;
                for (int x = -MAX_WIDTH; x <= MAX_WIDTH; x++)
                for (int y = -MAX_WIDTH; y <= MAX_WIDTH; y++)
                {
                    float d = length(float2(x, y));
                    if (d > _Width) continue;
                    if (tex2D(_MainTex, i.uv + float2(x, y) * texel).r > 0.5) nearest = min(nearest, d);
                }

                // Soft last pixel so the outer edge is not jagged.
                fixed4 c = _Color;
                c.a *= saturate(_Width + 0.5 - nearest);
                return c;
            }
            ENDCG
        }
    }
}
