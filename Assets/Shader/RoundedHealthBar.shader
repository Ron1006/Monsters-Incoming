Shader "UI/RoundedHealthBar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} // Ö÷ÌùÍ¼
        _Color ("Color", Color) = (1,1,1,1)  // ÑÕÉ«
        _Radius ("Corner Radius", Range(0, 0.5)) = 0.2 // Ô²½Ç°ë¾¶
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Radius; // Ô²½Ç°ë¾¶

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 texColor = tex2D(_MainTex, uv) * _Color;

                // ¼ÆËãÔ²½Ç
                float2 corner = min(uv, 1 - uv);
                float dist = length(corner);
                float alpha = smoothstep(_Radius, _Radius + 0.01, dist);

                texColor.a *= alpha; // ÈÃ±ßÔµ±äÍ¸Ã÷
                return texColor;
            }
            ENDCG
        }
    }
}
