Shader "Unlit/PiecewiseLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Percent ("Percent", float) = 0
    }
    SubShader
    {
        // https://stackoverflow.com/questions/33322167/how-to-write-a-unity-shader-that-respects-sorting-layers
        // Tags { "RenderType"="Opaque" }
        Tags{"Queue" = "Transparent"}
        LOD 100

        Pass
        {
            ZTest Off

            Blend SrcAlpha
            OneMinusSrcAlpha
            
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

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Percent;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                
                if (i.uv.x < _Percent) {
                    col = col * 0.8;
                    // col = sqrt(col);
                }
                
                // col = i.uv.x;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
