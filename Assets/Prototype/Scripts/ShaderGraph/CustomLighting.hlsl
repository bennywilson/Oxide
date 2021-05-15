#ifndef CUSTOMLIGHTING_HLSL_
#define CUSTOMLIGHTING_HLSL_

void Custom_Light_float(float3 WorldPos, float3 Normal, float3 AmbientLight, float LightScale, float LightBias, out half3 Color)
{
	Color = half3(0,0,0);
#if defined(SHADERGRAPH_PREVIEW)
  //Direction = half3(0.5, 0.5, 0);
  Color = 1;
 // DistanceAtten = 1;
  //ShadowAtten = 1;
#else
  #if SHADOWS_SCREEN
    half4 clipPos = TransformWorldToHClip(WorldPos);
    half4 shadowCoord = ComputeScreenPos(clipPos);
  #else
    half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
  #endif
  Light mainLight = GetMainLight(shadowCoord);
  float3 Direction = mainLight.direction;
  //Direction = half3(0,1,0);
  const float LightIntensity = (saturate(dot(Normal, Direction))) * LightScale + LightBias;// * 0.35f) + 0.65f;
  Color = mainLight.color * LightIntensity + AmbientLight;
 // DistanceAtten = mainLight.distanceAttenuation;
  //ShadowAtten = mainLight.shadowAttenuation;
#endif
}

#endif