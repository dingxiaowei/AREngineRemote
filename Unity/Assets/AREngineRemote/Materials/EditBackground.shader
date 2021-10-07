Shader "Unlit/EditBackground"
{
    Properties
    {
        _MainTex ("MainTexture", 2D) = "white" {}
        _YTex ("Y_Texture", 2D) = "white" {}
        _UVTex ("UV_Texture",2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
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
            float4 _MainTex_ST;

            sampler2D _YTex;
            float4 _YTex_ST;

            sampler2D _UVTex;
            float4 _UVTex_ST;

            float4 uv_st;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex).yx;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                //float2 uv2 = float2(1, 1) - i.uv;
                // float2 uv2 = float2(i.uv.x, 1 - i.uv.y);
                float2 uv2 = uv_st.xy + uv_st.zw * i.uv;
                fixed4 ycol = tex2D(_YTex, uv2);
                fixed4 uvcol = tex2D(_UVTex, uv2);
                fixed4 col = tex2D(_MainTex, i.uv.yx);
                float r = ycol.a + 1.771 * uvcol.r - 0.8855;
                float g = ycol.a - 0.3456 * uvcol.r - 0.7145 * uvcol.g + 0.53005;
                float b = ycol.a + 1.4022 * uvcol.g - 0.7011;
                return fixed4(r, g, b, 1) + col;
            }
            ENDCG
        }
    }
}