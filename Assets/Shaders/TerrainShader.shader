Shader "Custom/TerrainShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _WallTex ("Wall Texture", 2D) = "white" {}

        _TexScale ("Scale", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        float _TexScale;

        sampler2D _MainTex;
        sampler2D _WallTex;

        struct Input
        {
            float2 uv_MainTex;

            float3 worldPos;
            float3 worldNormal;
        };


        UNITY_INSTANCING_BUFFER_START(Props)
            // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            // #pragma instancing_options assumeuniformscaling
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            
            float3 scaleWorldPos = IN.worldPos / _TexScale;
            float3 pWeight = abs(IN.worldNormal);
            pWeight /= pWeight.x + pWeight.y + pWeight.z;

            float3 xProj = tex2D(_WallTex, scaleWorldPos.yz) * pWeight.x;
            float3 yProj = tex2D(_MainTex, scaleWorldPos.xz) * pWeight.y;
            float3 zProj = tex2D(_WallTex, scaleWorldPos.xy) * pWeight.z;

            o.Albedo = xProj + yProj + zProj;

        }
        ENDCG
    }
    FallBack "Diffuse"
}
