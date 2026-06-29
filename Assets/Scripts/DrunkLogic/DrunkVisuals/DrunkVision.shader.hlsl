Shader "Custom/DrunkVision"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveAmplitude ("Wave Amplitude", Float) = 0.01
        _WaveFrequency ("Wave Frequency", Float) = 2.0
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _Time ("Time", Float) = 0
        _Vignette ("Vignette", Float) = 0.3
        _TrailStrength ("Trail Strength", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _MainTex;
            float _WaveAmplitude;
            float _WaveFrequency;
            float _WaveSpeed;
            float _Time;
            float _Vignette;
            float _TrailStrength;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Wave distortion
                uv.x += sin(uv.y * _WaveFrequency + _Time * _WaveSpeed)
                    * _WaveAmplitude;
                uv.y += cos(uv.x * _WaveFrequency * 0.7 + _Time * _WaveSpeed * 0.8)
                    * _WaveAmplitude * 0.6;

                fixed4 col = tex2D(_MainTex, uv);

                // Slight amber tint when drunk
                col.r = min(1.0, col.r + 0.05 * _WaveAmplitude * 100);
                col.b = max(0.0, col.b - 0.03 * _WaveAmplitude * 100);

                // Vignette
                float2 center = uv - 0.5;
                float vignette = 1.0 - dot(center, center) * _Vignette * 4.0;
                col.a = (1.0 - vignette) * _WaveAmplitude * 80;

                return col;
            }
            ENDCG
        }
    }
}