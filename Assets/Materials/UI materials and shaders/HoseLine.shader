Shader "Custom/HoseLineDoubleWall"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _InnerColor ("Inner Wall Color", Color) = (0.8,0.8,0.8,1)
        _OuterColor ("Outer Wall Color", Color) = (0.2,0.2,0.2,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.3
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Alpha ("Alpha", Range(0,1)) = 0.5
        _RimPower ("Rim Power", Range(1,8)) = 3.0
        _RimColor ("Rim Color", Color) = (1,1,1,1)
    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade

        sampler2D _MainTex;
        half _Glossiness;
        half _Metallic;
        half _Alpha;
        fixed4 _InnerColor;
        fixed4 _OuterColor;
        half _RimPower;
        fixed4 _RimColor;

        struct Input {
            float2 uv_MainTex;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o) {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

            // Simulate double wall: blend inner and outer color based on UV.y
            float wallBlend = smoothstep(0.3, 0.7, IN.uv_MainTex.y);
            fixed3 hoseColor = lerp(_InnerColor.rgb, _OuterColor.rgb, wallBlend);

            // Rim lighting for 3D effect
            float rim = pow(1.0 - saturate(dot(normalize(IN.viewDir), o.Normal)), _RimPower);
            hoseColor = lerp(hoseColor, _RimColor.rgb, rim * 0.5);

            o.Albedo = hoseColor * c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a * _Alpha;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}