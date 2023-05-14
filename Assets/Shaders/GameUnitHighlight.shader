Shader "Unlit/GameUnitHighlight"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _HighlightMode("HighlightMode", Int) = 0 // 0 => Normal, 1 => Highlight
        _UnitCategory("UnitType", Int) = 0 // 0 => Infantry, 1 => Cavalry
        _Old("Old", Int) = 0 // 0 => New, 1 => Old
    }
    SubShader
    {
        // Tags { "RenderType"="Opaque" }
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent"}
        LOD 100
        Cull Off // double edge
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            // sampler2D _MainTex;
            float4 _Color;
            float4 _MainTex_ST;
            int _HighlightMode;
            int _UnitCategory;
            int _Old;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = _Color;
                
                if (_HighlightMode == 1) {
                    col = col / 2 + fixed4(0.5, 0.5, 0.5, 0.5);
                }
                if (_UnitCategory == 1 && i.uv.x < i.uv.y)
                {
                    col = fixed4(0.9, 0.9, 0.9, 1);
                }
                /*
                if (_HighlightMode == 1 && (i.uv.x < 0.05 || i.uv.x > 0.95 || i.uv.y < 0.05 || i.uv.y > 0.95)) {
                    col = fixed4(0.5,0.5,0,1);
                }
                */
                if (_Old == 1)
                {
                    col.a = 0.1;
                    // col = fixed4(col.r, col.g, col.b, 0.1);
                }
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
