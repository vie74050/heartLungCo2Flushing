Shader "Outlined/OutlineHollowDiffuse"
{
    Properties
    {
        _Color        ("Main Color", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _Outline      ("Outline width", Range (0.0, 0.3)) = 0.02
        _MainTex      ("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Cull Back
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        CGINCLUDE
        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float2 uv     : TEXCOORD0;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv  : TEXCOORD0;
        };

        struct v2fOutline
        {
            float4 pos : SV_POSITION;
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;
        float4 _Color;
        float4 _OutlineColor;
        float  _Outline;

        // Base vertex
        v2f vertBase (appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv  = TRANSFORM_TEX(v.uv, _MainTex);
            return o;
        }

        // Outline vertex: expand along view-space normal
        v2fOutline vertOutline (appdata v)
        {
            v2fOutline o;

            float4 pos = UnityObjectToClipPos(v.vertex);

            float3 viewNormal = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
            float2 offset = TransformViewToProjection(viewNormal.xy);

            pos.xy += offset * pos.z * _Outline;
            o.pos = pos;
            return o;
        }

        // Base fragment: fades with _Color.a but ALWAYS writes depth
        fixed4 fragBase (v2f i) : SV_Target
        {
            fixed4 tex = tex2D(_MainTex, i.uv);
            fixed4 col = tex * _Color;
            // When _Color.a = 0, this is fully transparent but still writes depth
            return col;
        }

        // Outline fragment: solid outline color
        fixed4 fragOutline (v2fOutline i) : SV_Target
        {
            return _OutlineColor;
        }
        ENDCG

        // -------- BASE PASS (writes depth even when transparent) --------
        Pass
        {
            Name "BASE"
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex   vertBase
            #pragma fragment fragBase
            ENDCG
        }

        // -------- OUTLINE PASS (hollow border) --------
        Pass
        {
            Name "OUTLINE"
            // Cull front faces of the expanded mesh so we see only the rim
            Cull Front
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex   vertOutline
            #pragma fragment fragOutline
            ENDCG
        }
    }

    Fallback "Transparent/Diffuse"
}