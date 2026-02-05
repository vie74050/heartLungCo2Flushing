Shader "Custom/Fill_Y"
{
    Properties
    {
        _Color("Base Color", Color) = (0.2, 0.6, 1, 0.8)
        _GlowColor("Glow Color", Color) = (0.4, 0.9, 1, 1)
        _GlowStrength("Glow Strength", Range(0, 10)) = 3.0
        _GlowPower("Glow Sharpness", Range(1, 8)) = 4.0

        _ClipStartY("Clip Start (World Y)", Float) = 0
        _ClipEndY("Clip End (World Y)", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
            };

            float4 _Color;
            float4 _GlowColor;
            float _GlowStrength;
            float _GlowPower;

            float _ClipStartY;
            float _ClipEndY;

            v2f vert (appdata v)
            {
                v2f o;
                float3 world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldPos = world;

                o.worldNormal = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Vertical clipping
                if (i.worldPos.y < _ClipStartY) discard;
                if (i.worldPos.y > _ClipEndY) discard;

                // Fresnel-based glow
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1.0 - saturate(dot(i.worldNormal, viewDir)), _GlowPower);

                float glow = fresnel * _GlowStrength;

                float4 finalColor = _Color + _GlowColor * glow;

                // Keep transparency from base color
                finalColor.a = _Color.a;

                return finalColor;
            }
            ENDCG
        }
    }
}