Shader "Melenitas Dev/SG_Triplanar"
{
    Properties
    {
        _TopTex ("Top (Y) Texture", 2D) = "white" {}
        _SideXTex ("Side X Texture", 2D) = "white" {}
        _SideZTex ("Side Z Texture", 2D) = "white" {}

        _TintColor ("Tint Color", Color) = (1,1,1,1)
        _Scale ("Texture Scale", Float) = 1.0

        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _TopTex;
        sampler2D _SideXTex;
        sampler2D _SideZTex;

        fixed4 _TintColor;
        float _Scale;
        half _Metallic;
        half _Glossiness;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Normal en mundo normalizada
            float3 n = normalize(IN.worldNormal);
            float3 an = abs(n);

            // Pesos de mezcla normalizados
            float sum = an.x + an.y + an.z + 1e-5;
            an /= sum;

            // UVs triplanares (espacio mundo, sin tiling por objeto)
            float2 uvX = IN.worldPos.zy * _Scale; // Proyección en plano YZ (para caras mirando +/-X)
            float2 uvY = IN.worldPos.xz * _Scale; // Proyección en plano XZ (para caras mirando +/-Y)
            float2 uvZ = IN.worldPos.xy * _Scale; // Proyección en plano XY (para caras mirando +/-Z)

            fixed4 colX = tex2D(_SideXTex, uvX);
            fixed4 colY = tex2D(_TopTex,   uvY);
            fixed4 colZ = tex2D(_SideZTex, uvZ);

            // Mezcla triplanar
            fixed4 c = colX * an.x + colY * an.y + colZ * an.z;

            // Tint
            c.rgb *= _TintColor.rgb;
            c.a   *= _TintColor.a;

            o.Albedo = c.rgb;
            o.Alpha  = c.a;

            o.Metallic   = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }

    FallBack "Diffuse"
}