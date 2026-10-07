// Block surface (P6.1, FINDINGS exit): Standard-like, with one world-space clip plane for the exit cut.
// _ClipPlane = (normal.xyz, distance): pixels where dot(worldPos, normal) > distance are discarded.
// The default (0,0,0,0) clips nothing; an exiting block gets its plane through a MaterialPropertyBlock.
Shader "Game/Block"
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
        // addshadow: the shadow caster runs surf too, so the clipped part casts no shadow.
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma target 3.0
        // Every block shares one material; the color is per instance (D108), so pieces of one mesh draw together
        // whatever their color. An exiting block's clip plane is not per instance: it takes only that block out.
        #pragma multi_compile_instancing

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        float4 _ClipPlane;
        half _CapShade;

        UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float facing : VFACE;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            clip(_ClipPlane.w - dot(IN.worldPos, _ClipPlane.xyz));

            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

            // Back face = the inside seen through the cut: flat and unlit (its normal points the wrong way).
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
