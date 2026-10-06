// Frozen block surface (D102). Lit, opaque, so the BlockParts studs keep their shading.
// Color comes from two fixed ice colors; the ice texture only blends between them, so every block looks the same.
// The texture is projected in world space from three axes (triplanar): no UVs needed, same density on every block.
// World space is safe here: a frozen block cannot move, so the texture never slides on it.
Shader "Game/IceBlock"
{
    Properties
    {
        [Header(Color)]
        _DeepColor ("Deep Color", Color) = (0.22, 0.56, 0.92, 1)
        _SurfaceColor ("Surface Color", Color) = (0.70, 0.90, 1.0, 1)

        [Header(Ice Texture)]
        _IceTex ("Ice Texture", 2D) = "gray" {}
        _IceScale ("Tiles Per Unit", Float) = 0.25
        _PatchContrast ("Patch Contrast", Range(0, 3)) = 1.2
        _PatchBlur ("Patch Blur (mip bias)", Range(0, 8)) = 4

        [Header(Cracks)]
        _CrackColor ("Crack Color", Color) = (0.90, 0.98, 1.0, 1)
        _CrackStrength ("Crack Strength", Range(0, 10)) = 3

        [Header(Rim)]
        _RimColor ("Rim Color", Color) = (0.85, 0.97, 1.0, 1)
        _RimPower ("Rim Falloff", Range(0.5, 8)) = 3
        _RimStrength ("Rim Strength", Range(0, 2)) = 0.6

        [Header(Surface)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.85
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _IceTex;
        fixed4 _DeepColor, _SurfaceColor, _CrackColor, _RimColor;
        half _IceScale, _PatchContrast, _PatchBlur, _CrackStrength;
        half _RimPower, _RimStrength, _Smoothness;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float3 viewDir;
        };

        half Luma(half3 color) { return dot(color, half3(0.299, 0.587, 0.114)); }

        // Sharp blend toward the axis the surface faces most: top faces read the XZ projection.
        half3 TriplanarWeights(float3 normal)
        {
            half3 weights = pow(abs(normal), 4);
            return weights / (weights.x + weights.y + weights.z);
        }

        half SampleIce(float3 position, half3 weights, half mipBias)
        {
            float3 p = position * _IceScale;
            half x = Luma(tex2Dbias(_IceTex, float4(p.zy, 0, mipBias)).rgb);
            half y = Luma(tex2Dbias(_IceTex, float4(p.xz, 0, mipBias)).rgb);
            half z = Luma(tex2Dbias(_IceTex, float4(p.xy, 0, mipBias)).rgb);
            return x * weights.x + y * weights.y + z * weights.z;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 normal = normalize(IN.worldNormal);
            half3 weights = TriplanarWeights(normal);

            // Large soft patches pick the color between deep and surface ice.
            half patches = SampleIce(IN.worldPos, weights, _PatchBlur);
            half patchBlend = saturate(0.5 + (patches - 0.5) * _PatchContrast);
            half3 albedo = lerp(_DeepColor.rgb, _SurfaceColor.rgb, patchBlend);

            // Fine bright detail (texture minus its blurred self) becomes white cracks.
            half detail = SampleIce(IN.worldPos, weights, 0) - patches;
            albedo = lerp(albedo, _CrackColor.rgb, saturate(detail * _CrackStrength));

            // Edges and stud sides turn away from the camera and catch a frosty rim.
            half facing = saturate(dot(normalize(IN.viewDir), normal));
            half rim = pow(1 - facing, _RimPower) * _RimStrength;

            o.Albedo = albedo;
            o.Emission = _RimColor.rgb * rim;
            o.Metallic = 0;
            o.Smoothness = _Smoothness;
        }
        ENDCG
    }

    Fallback "Standard"
}
