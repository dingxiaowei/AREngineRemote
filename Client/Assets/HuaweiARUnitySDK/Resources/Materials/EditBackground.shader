Shader "Unlit/EditBackground"
{
    Properties
    {
        _MainTex ("Y_Texture", 2D) = "white" {}
        _UVTex ("UV_Texture",2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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

            sampler2D _UVTex;
            float4 _UVTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 ycol  = tex2D(_MainTex, i.uv);
                fixed4 uvcol = tex2D(_UVTex, i.uv);
                float r = ycol.a + 1.4022 * uvcol.g - 0.7011;
				float g = ycol.a - 0.3456 * uvcol.r - 0.7145 * uvcol.g + 0.53005;
				float b = ycol.a + 1.771 * uvcol.r - 0.8855;
                return  fixed4(r, g, b, 1);
            }
            ENDCG
        }
    }
}
