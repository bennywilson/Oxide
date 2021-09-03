Shader "Oxide\Backdrop"
{
  Properties
  {
      _StartBackdropTex("Start Backdrop Tex", 2D) = "white" {}
      _EndBackdropTex("End Backdrop Tex", 2D) = "white" {}
      _ScaleBias("Scale And Bias", Vector) = (1,1,0,0)
      _FinalScaleBias("Final Scale And Bias", Vector) = (1,1,0,0)
      _ScrollMagnitude("Scroll Magnitude", Float) = 50
      _UseTint("Use Tint", Int) = 0
      _BlendScaleBiasWithTimeOfDay("Blend Scale/Bias with Time Of Day", Int) = 0
      _BlendScaleBiasWithDistance("Blend Scale/Bias with Distance", Int) = 0
      _DistanceBlendSmoothStep("Distance Blend Smooth Step", Vector) = (0,1,0,0)
  }
    SubShader
    {
      Tags { "RenderType" = "Transparent" }
      LOD 100
      Blend SrcAlpha OneMinusSrcAlpha

      Pass
      {
          cull off

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

          sampler2D _StartBackdropTex;
          sampler2D _EndBackdropTex;
          float _TimeOfDay;
          float _DistanceTravelled;
          float4 _ScaleBias;
          float4 _FinalScaleBias;
          float4 _PlayerCameraRight;
          float _ScrollMagnitude;
          float4 _BackgroundTintColor;
          float4 _DistanceBlendSmoothStep;
          int _UseTint;
          int _BlendScaleBiasWithTimeOfDay;
          int _BlendScaleBiasWithDistance;

          v2f vert(appdata v)
          {
              float4 vertPos = v.vertex;
              vertPos.y *= -1.0f;

              float4 targetScaleAndBias = _ScaleBias;
              if (_BlendScaleBiasWithTimeOfDay)
              {
                targetScaleAndBias = lerp(_ScaleBias, _FinalScaleBias, _TimeOfDay);
              }
              else if (_BlendScaleBiasWithDistance)
              {
                  if (_DistanceTravelled < _DistanceBlendSmoothStep.x)
                  {
                      targetScaleAndBias = 0;
                  }
                  else
                  {
                      float dist = smoothstep(_DistanceBlendSmoothStep.x, _DistanceBlendSmoothStep.y, _DistanceTravelled);
                      targetScaleAndBias = lerp(_ScaleBias, _FinalScaleBias, dist);
                  }
              }
              vertPos.xy = vertPos.xy * targetScaleAndBias.xy + targetScaleAndBias.zw;

              float theDot = dot(float3(0.0f, 0.0f, 1.0f), _PlayerCameraRight);
              vertPos.x += theDot * _ScrollMagnitude;
              vertPos.z = -200.0f;

              v2f o;
              o.vertex = UnityViewToClipPos(vertPos);
              o.uv = v.uv;

              return o;
          }

          fixed4 frag(v2f i) : SV_Target
          {
            float4 startCol = tex2D(_StartBackdropTex, i.uv);
            float4 endCol = tex2D(_EndBackdropTex, i.uv);
            float4 finalCol = lerp(startCol, endCol, _TimeOfDay);

            if (_UseTint)
            {
              finalCol.xyz *= _BackgroundTintColor.xyz;
            }
            return finalCol;
      }
      ENDCG
    }
  }
}
