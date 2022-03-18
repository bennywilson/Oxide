Shader "SpeedBoost_S"
{
    Properties
    {
        [NoScaleOffset]Text("Text", 2D) = "white" { }
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" { }
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" { }
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" { }
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue" = "Transparent"
        }
        Pass
        {
            Name "Pass"
            Tags
            {
                // LightMode: <None>
            }

            // Render State
            Cull Off
        Blend one one, one one
        ZTest Off
        ZWrite Off

            // Debug
            // <None>

            // --------------------------------------------------
            // Pass

            HLSLPROGRAM

            // Pragmas
#pragma target 2.0
#pragma only_renderers gles gles3 glcore d3d11
#pragma multi_compile_instancing
#pragma multi_compile_fog
#pragma vertex vert
#pragma fragment frag

            // DotsInstancingOptions: <None>
            // HybridV1InjectedBuiltinProperties: <None>

            // Keywords
#pragma multi_compile _ LIGHTMAP_ON
#pragma multi_compile _ DIRLIGHTMAP_COMBINED
#pragma shader_feature _ _SAMPLE_GI
            // GraphKeywords: <None>

            // Defines
#define _SURFACE_TYPE_TRANSPARENT 1
#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TANGENT
#define ATTRIBUTES_NEED_TEXCOORD0
#define VARYINGS_NEED_TEXCOORD0
#define FEATURES_GRAPH_VERTEX
            /* WARNING: $splice Could not find named fragment 'PassInstancing' */
#define SHADERPASS SHADERPASS_UNLIT
            /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

            // Includes
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

            // --------------------------------------------------
            // Structs and Packing

struct Attributes
{
    float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
struct Varyings
{
    float4 positionCS : SV_POSITION;
            float4 texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
#endif
};
struct SurfaceDescriptionInputs
{
    float4 uv0;
};
struct VertexDescriptionInputs
{
    float3 ObjectSpaceNormal;
    float3 ObjectSpaceTangent;
    float3 ObjectSpacePosition;
};
struct PackedVaryings
{
    float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

PackedVaryings PackVaryings(Varyings input)
{
    PackedVaryings output;
    output.positionCS = input.positionCS;
    output.interp0.xyzw = input.texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}
Varyings UnpackVaryings(PackedVaryings input)
{
    Varyings output;
    output.positionCS = input.positionCS;
    output.texCoord0 = input.interp0.xyzw;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}

// --------------------------------------------------
// Graph

// Graph Properties
CBUFFER_START(UnityPerMaterial)
        float4 Text_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Text);
SAMPLER(samplerText);

// Graph Functions
// GraphFunctions: <None>

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 BaseColor;
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0 = UnityBuildTexture2DStructNoScale(Text);
    float4 _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.tex, _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_R_4 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.r;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_G_5 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.g;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_B_6 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.b;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.a;
    surface.BaseColor = (_SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.xyz) * 0.1f;
    surface.Alpha = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

// --------------------------------------------------
// Main

# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

ENDHLSL
        }
        Pass
{
    Name "ShadowCaster"
            Tags
            {
        "LightMode" = "ShadowCaster"
            }

    // Render State
    Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

    // --------------------------------------------------
    // Pass

    HLSLPROGRAM

            // Pragmas
#pragma target 2.0
#pragma only_renderers gles gles3 glcore d3d11
#pragma multi_compile_instancing
#pragma vertex vert
#pragma fragment frag

    // DotsInstancingOptions: <None>
    // HybridV1InjectedBuiltinProperties: <None>

    // Keywords
#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
    // GraphKeywords: <None>

    // Defines
#define _SURFACE_TYPE_TRANSPARENT 1
#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TANGENT
#define ATTRIBUTES_NEED_TEXCOORD0
#define VARYINGS_NEED_TEXCOORD0
#define FEATURES_GRAPH_VERTEX
    /* WARNING: $splice Could not find named fragment 'PassInstancing' */
#define SHADERPASS SHADERPASS_SHADOWCASTER
    /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

    // Includes
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

    // --------------------------------------------------
    // Structs and Packing

struct Attributes
{
    float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
struct Varyings
{
    float4 positionCS : SV_POSITION;
            float4 texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
#endif
};
struct SurfaceDescriptionInputs
{
    float4 uv0;
};
struct VertexDescriptionInputs
{
    float3 ObjectSpaceNormal;
    float3 ObjectSpaceTangent;
    float3 ObjectSpacePosition;
};
struct PackedVaryings
{
    float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

PackedVaryings PackVaryings(Varyings input)
{
    PackedVaryings output;
    output.positionCS = input.positionCS;
    output.interp0.xyzw = input.texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}
Varyings UnpackVaryings(PackedVaryings input)
{
    Varyings output;
    output.positionCS = input.positionCS;
    output.texCoord0 = input.interp0.xyzw;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}

// --------------------------------------------------
// Graph

// Graph Properties
CBUFFER_START(UnityPerMaterial)
        float4 Text_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Text);
SAMPLER(samplerText);

// Graph Functions
// GraphFunctions: <None>

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0 = UnityBuildTexture2DStructNoScale(Text);
    float4 _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.tex, _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_R_4 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.r;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_G_5 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.g;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_B_6 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.b;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.a;
    surface.Alpha = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

// --------------------------------------------------
// Main

# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

ENDHLSL
        }
        Pass
{
    Name "DepthOnly"
            Tags
            {
        "LightMode" = "DepthOnly"
            }

    // Render State
    Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

    // --------------------------------------------------
    // Pass

    HLSLPROGRAM

            // Pragmas
#pragma target 2.0
#pragma only_renderers gles gles3 glcore d3d11
#pragma multi_compile_instancing
#pragma vertex vert
#pragma fragment frag

    // DotsInstancingOptions: <None>
    // HybridV1InjectedBuiltinProperties: <None>

    // Keywords
    // PassKeywords: <None>
    // GraphKeywords: <None>

    // Defines
#define _SURFACE_TYPE_TRANSPARENT 1
#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TANGENT
#define ATTRIBUTES_NEED_TEXCOORD0
#define VARYINGS_NEED_TEXCOORD0
#define FEATURES_GRAPH_VERTEX
    /* WARNING: $splice Could not find named fragment 'PassInstancing' */
#define SHADERPASS SHADERPASS_DEPTHONLY
    /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

    // Includes
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

    // --------------------------------------------------
    // Structs and Packing

struct Attributes
{
    float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
struct Varyings
{
    float4 positionCS : SV_POSITION;
            float4 texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
#endif
};
struct SurfaceDescriptionInputs
{
    float4 uv0;
};
struct VertexDescriptionInputs
{
    float3 ObjectSpaceNormal;
    float3 ObjectSpaceTangent;
    float3 ObjectSpacePosition;
};
struct PackedVaryings
{
    float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

PackedVaryings PackVaryings(Varyings input)
{
    PackedVaryings output;
    output.positionCS = input.positionCS;
    output.interp0.xyzw = input.texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}
Varyings UnpackVaryings(PackedVaryings input)
{
    Varyings output;
    output.positionCS = input.positionCS;
    output.texCoord0 = input.interp0.xyzw;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}

// --------------------------------------------------
// Graph

// Graph Properties
CBUFFER_START(UnityPerMaterial)
        float4 Text_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Text);
SAMPLER(samplerText);

// Graph Functions
// GraphFunctions: <None>

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0 = UnityBuildTexture2DStructNoScale(Text);
    float4 _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.tex, _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_R_4 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.r;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_G_5 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.g;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_B_6 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.b;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.a;
    surface.Alpha = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

// --------------------------------------------------
// Main

# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

ENDHLSL
        }
    }
    SubShader
{
    Tags
        {
        "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue" = "Transparent"
        }
    Pass
        {
        Name "Pass"
            Tags
            {
            // LightMode: <None>
        }

        // Render State
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off

            // Debug
            // <None>

        // --------------------------------------------------
        // Pass

        HLSLPROGRAM

            // Pragmas
#pragma target 4.5
#pragma exclude_renderers gles gles3 glcore
#pragma multi_compile_instancing
#pragma multi_compile_fog
#pragma multi_compile _ DOTS_INSTANCING_ON
#pragma vertex vert
#pragma fragment frag

        // DotsInstancingOptions: <None>
        // HybridV1InjectedBuiltinProperties: <None>

        // Keywords
#pragma multi_compile _ LIGHTMAP_ON
#pragma multi_compile _ DIRLIGHTMAP_COMBINED
#pragma shader_feature _ _SAMPLE_GI
        // GraphKeywords: <None>

        // Defines
#define _SURFACE_TYPE_TRANSPARENT 1
#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TANGENT
#define ATTRIBUTES_NEED_TEXCOORD0
#define VARYINGS_NEED_TEXCOORD0
#define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
#define SHADERPASS SHADERPASS_UNLIT
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

        // Includes
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

        // --------------------------------------------------
        // Structs and Packing

struct Attributes
{
    float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
struct Varyings
{
    float4 positionCS : SV_POSITION;
            float4 texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
#endif
};
struct SurfaceDescriptionInputs
{
    float4 uv0;
};
struct VertexDescriptionInputs
{
    float3 ObjectSpaceNormal;
    float3 ObjectSpaceTangent;
    float3 ObjectSpacePosition;
};
struct PackedVaryings
{
    float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

PackedVaryings PackVaryings(Varyings input)
{
    PackedVaryings output;
    output.positionCS = input.positionCS;
    output.interp0.xyzw = input.texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}
Varyings UnpackVaryings(PackedVaryings input)
{
    Varyings output;
    output.positionCS = input.positionCS;
    output.texCoord0 = input.interp0.xyzw;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}

// --------------------------------------------------
// Graph

// Graph Properties
CBUFFER_START(UnityPerMaterial)
        float4 Text_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Text);
SAMPLER(samplerText);

// Graph Functions
// GraphFunctions: <None>

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float3 BaseColor;
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0 = UnityBuildTexture2DStructNoScale(Text);
    float4 _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.tex, _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_R_4 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.r;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_G_5 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.g;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_B_6 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.b;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.a;
    surface.BaseColor = (_SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.xyz);
    surface.Alpha = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

// --------------------------------------------------
// Main

# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"

ENDHLSL
        }
        Pass
{
    Name "ShadowCaster"
            Tags
            {
        "LightMode" = "ShadowCaster"
            }

    // Render State
    Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

    // --------------------------------------------------
    // Pass

    HLSLPROGRAM

            // Pragmas
#pragma target 4.5
#pragma exclude_renderers gles gles3 glcore
#pragma multi_compile_instancing
#pragma multi_compile _ DOTS_INSTANCING_ON
#pragma vertex vert
#pragma fragment frag

    // DotsInstancingOptions: <None>
    // HybridV1InjectedBuiltinProperties: <None>

    // Keywords
#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
    // GraphKeywords: <None>

    // Defines
#define _SURFACE_TYPE_TRANSPARENT 1
#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TANGENT
#define ATTRIBUTES_NEED_TEXCOORD0
#define VARYINGS_NEED_TEXCOORD0
#define FEATURES_GRAPH_VERTEX
    /* WARNING: $splice Could not find named fragment 'PassInstancing' */
#define SHADERPASS SHADERPASS_SHADOWCASTER
    /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

    // Includes
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

    // --------------------------------------------------
    // Structs and Packing

struct Attributes
{
    float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
struct Varyings
{
    float4 positionCS : SV_POSITION;
            float4 texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
#endif
};
struct SurfaceDescriptionInputs
{
    float4 uv0;
};
struct VertexDescriptionInputs
{
    float3 ObjectSpaceNormal;
    float3 ObjectSpaceTangent;
    float3 ObjectSpacePosition;
};
struct PackedVaryings
{
    float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

PackedVaryings PackVaryings(Varyings input)
{
    PackedVaryings output;
    output.positionCS = input.positionCS;
    output.interp0.xyzw = input.texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}
Varyings UnpackVaryings(PackedVaryings input)
{
    Varyings output;
    output.positionCS = input.positionCS;
    output.texCoord0 = input.interp0.xyzw;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}

// --------------------------------------------------
// Graph

// Graph Properties
CBUFFER_START(UnityPerMaterial)
        float4 Text_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Text);
SAMPLER(samplerText);

// Graph Functions
// GraphFunctions: <None>

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0 = UnityBuildTexture2DStructNoScale(Text);
    float4 _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.tex, _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_R_4 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.r;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_G_5 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.g;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_B_6 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.b;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.a;
    surface.Alpha = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

// --------------------------------------------------
// Main

# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"

ENDHLSL
        }
        Pass
{
    Name "DepthOnly"
            Tags
            {
        "LightMode" = "DepthOnly"
            }

    // Render State
    Cull Off
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        ColorMask 0

            // Debug
            // <None>

    // --------------------------------------------------
    // Pass

    HLSLPROGRAM

            // Pragmas
#pragma target 4.5
#pragma exclude_renderers gles gles3 glcore
#pragma multi_compile_instancing
#pragma multi_compile _ DOTS_INSTANCING_ON
#pragma vertex vert
#pragma fragment frag

    // DotsInstancingOptions: <None>
    // HybridV1InjectedBuiltinProperties: <None>

    // Keywords
    // PassKeywords: <None>
    // GraphKeywords: <None>

    // Defines
#define _SURFACE_TYPE_TRANSPARENT 1
#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TANGENT
#define ATTRIBUTES_NEED_TEXCOORD0
#define VARYINGS_NEED_TEXCOORD0
#define FEATURES_GRAPH_VERTEX
    /* WARNING: $splice Could not find named fragment 'PassInstancing' */
#define SHADERPASS SHADERPASS_DEPTHONLY
    /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */

    // Includes
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
# include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
# include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"

    // --------------------------------------------------
    // Structs and Packing

struct Attributes
{
    float3 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float4 tangentOS : TANGENT;
            float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
struct Varyings
{
    float4 positionCS : SV_POSITION;
            float4 texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
#endif
};
struct SurfaceDescriptionInputs
{
    float4 uv0;
};
struct VertexDescriptionInputs
{
    float3 ObjectSpaceNormal;
    float3 ObjectSpaceTangent;
    float3 ObjectSpacePosition;
};
struct PackedVaryings
{
    float4 positionCS : SV_POSITION;
            float4 interp0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
            uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };

PackedVaryings PackVaryings(Varyings input)
{
    PackedVaryings output;
    output.positionCS = input.positionCS;
    output.interp0.xyzw = input.texCoord0;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}
Varyings UnpackVaryings(PackedVaryings input)
{
    Varyings output;
    output.positionCS = input.positionCS;
    output.texCoord0 = input.interp0.xyzw;
#if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
#endif
#if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
#endif
#if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
#endif
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
#endif
    return output;
}

// --------------------------------------------------
// Graph

// Graph Properties
CBUFFER_START(UnityPerMaterial)
        float4 Text_TexelSize;
CBUFFER_END

// Object and Global properties
SAMPLER(SamplerState_Linear_Repeat);
TEXTURE2D(Text);
SAMPLER(samplerText);

// Graph Functions
// GraphFunctions: <None>

// Graph Vertex
struct VertexDescription
{
    float3 Position;
    float3 Normal;
    float3 Tangent;
};

VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
{
    VertexDescription description = (VertexDescription)0;
    description.Position = IN.ObjectSpacePosition;
    description.Normal = IN.ObjectSpaceNormal;
    description.Tangent = IN.ObjectSpaceTangent;
    return description;
}

// Graph Pixel
struct SurfaceDescription
{
    float Alpha;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
    SurfaceDescription surface = (SurfaceDescription)0;
    UnityTexture2D _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0 = UnityBuildTexture2DStructNoScale(Text);
    float4 _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0 = SAMPLE_TEXTURE2D(_Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.tex, _Property_2cfd0a4815b044f0a5cc646fb10f1f6c_Out_0.samplerstate, IN.uv0.xy);
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_R_4 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.r;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_G_5 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.g;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_B_6 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.b;
    float _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7 = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_RGBA_0.a;
    surface.Alpha = _SampleTexture2D_24821690a0084e3c84f9f7463ca44b5b_A_7;
    return surface;
}

// --------------------------------------------------
// Build Graph Inputs

VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
{
    VertexDescriptionInputs output;
    ZERO_INITIALIZE(VertexDescriptionInputs, output);

    output.ObjectSpaceNormal = input.normalOS;
    output.ObjectSpaceTangent = input.tangentOS.xyz;
    output.ObjectSpacePosition = input.positionOS;

    return output;
}
SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
{
    SurfaceDescriptionInputs output;
    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);





    output.uv0 = input.texCoord0;
#if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
#else
#define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
#endif
#undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN

    return output;
}

// --------------------------------------------------
// Main

# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
# include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"

ENDHLSL
        }
    }
    FallBack "Hidden/Shader Graph/FallbackError"
}