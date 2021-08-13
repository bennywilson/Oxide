Shader "Oxide/BackDrop_S"
{
    Properties
    {
        _AfternoonBackdropTex ("Afternoon Backdrop", 2D) = "white" {}
        _SunsetBackdropTex ("Sunset Backdrop", 2D) = "white" {}
        _HorizonClipSpaceY ("HorizonClipSpaceY", float) = 1
        _TextureWidth( "TextureWidth", float) = 1024
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off ZWrite Off
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

            sampler2D _AfternoonBackdropTex;
            sampler2D _SunsetBackdropTex;
            float4 _MainTex_ST;
            half _HorizonClipSpaceY;
            float _TimeOfDay;
            float _TextureWidth;

            v2f vert (appdata v)
            {
                v2f o;

                float YScale = (1.0f - _HorizonClipSpaceY) * 0.5f;
                float YBias = 1.0f - YScale;
                
                const float XScale = YScale * 2.0f * ((_TextureWidth / _ScreenParams.x) * 2.0f - 1.0f);

                float3 vertPos = normalize(v.vertex.xyz);
                o.vertex.x = sign(vertPos.x) *  XScale;
                o.vertex.y = v.vertex.y * -YScale + YBias;
                o.vertex.z = 0.0f;
                o.vertex.w = 1.0f;

                o.uv = v.uv;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 afternoonColor = tex2D(_AfternoonBackdropTex, i.uv);
                float4 sunsetColor = tex2D(_SunsetBackdropTex, i.uv);
                float3 col = lerp(afternoonColor, sunsetColor, _TimeOfDay);

                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);
                return float4(col.xyz,1);
            }
            ENDCG
        }
    }
}
