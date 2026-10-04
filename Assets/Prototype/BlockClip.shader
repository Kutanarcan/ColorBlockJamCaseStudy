// Standard-like block shader with one world-space clip plane.
// _ClipPlane = (normal.xyz, distance): pixels where dot(worldPos, normal) > distance are discarded.
// Default (0,0,0,0) clips nothing. Set per block with a MaterialPropertyBlock.
Shader "Prototype/BlockClip"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _ClipPlane ("Clip Plane (xyz normal, w distance)", Vector) = (0,0,0,0)
        _CapShade ("Cut Face Shade", Range(0,1)) = 0.6
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        // Back faces are drawn too: through the cut you see the inside, painted flat, so the block reads as solid.
        Cull Off

        CGPROGRAM
        // addshadow: the shadow caster pass runs surf too, so clipped parts cast no shadow.
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float4 _ClipPlane;
        half _CapShade;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float facing : VFACE;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            clip(_ClipPlane.w - dot(IN.worldPos, _ClipPlane.xyz));

            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            // Back face = inside of the block seen through the cut: flat color, no lighting
            // (its normal points the wrong way, so lit shading would look broken).
            if (IN.facing < 0)
            {
                o.Albedo = 0;
                o.Metallic = 0;
                o.Smoothness = 0;
                o.Emission = c.rgb * _CapShade;
                o.Alpha = c.a;
                return;
            }

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
