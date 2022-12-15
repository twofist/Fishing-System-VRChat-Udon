Shader "MMMaellon/ToonLinkWater"
{

    Properties
    {
        [Toggle] _Lit ("Lit", Float) = 1
        _Color ("Color", Color) = (0,0.5,1,1)
        _FoamColor ("Foam Color", Color) = (1,1,1,1)
        _DarkFoamColor ("Dark Foam Color", Color) = (1,1,1,1)
        _FoamBias ("Foam Bias", Float) = 0.025
        _FoamFade ("Foam Distance Fade", Float) = 100
        _FoamBlur ("Foam Distance Blur", Float) = 50
        [Toggle] _Random ("Randomize", Float) = 1
        _Noise ("Noise", Float) = 1
        _Roundness ("Roundness", Float) = 0.2
        _CellScale ("Cell Scale", Float) = (4,4,4)
        _NoiseScale ("Noise Scale", Float) = (2,2,2)
        _CellDirection ("Cell Directional Animation", Float) = (0,0,0.05)
        _NoiseDirection ("Noise Directional Animation", Float) = (0.2,0,0.2)
        
        // Advanced options.
		[Header(System Render Flags)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 1                 // "One"
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Float) = 0            // "Zero"
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Operation", Float) = 0                 // "Add"
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("Depth Test", Float) = 4                // "LessEqual"
        [Toggle] _ZWrite("Depth Write", Float) = 1                                         // "On"
        [Enum(UnityEngine.Rendering.ColorWriteMask)] _ColorWriteMask("Color Write Mask", Float) = 15 // "All"
        [Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Float) = 2                     // "Back"
        _RenderQueueOverride("Render Queue Override", Range(-1.0, 5000)) = -1
        
        [IntRange] _Stencil ("Stencil ID [0;255]", Range(0,255)) = 0
	    _ReadMask ("ReadMask [0;255]", Int) = 255
	    _WriteMask ("WriteMask [0;255]", Int) = 255
	    [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 0
	    [Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Int) = 0
	    [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
	    [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0
        _ColorMask ("Color Mask", Float) = 15
	    [HideInInspector]__Baked ("Is this material referencing a baked shader?", Float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Blend[_SrcBlend][_DstBlend]
        BlendOp[_BlendOp]
        ZTest[_ZTest]
        ZWrite[_ZWrite]
        Cull[_CullMode]
        LOD 100
        
        Stencil
        {
            Ref [_Stencil]
            ReadMask [_ReadMask]
            WriteMask [_WriteMask]
            Comp [_StencilComp]
            Pass [_StencilOp]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
        }
        ColorMask [_ColorMask]
        CGPROGRAM
            #pragma surface surf Lambert finalcolor:mycolor vertex:myvert addshadow
            #pragma multi_compile_fog
            uniform float _Lit;
            uniform float _Random;
            uniform float4 _Color;
            uniform float4 _FoamColor;
            uniform float4 _DarkFoamColor;
            uniform float _FoamBias;
            uniform float _FoamFade;
            uniform float _FoamBlur;
            uniform float _Noise;
            uniform float3 _CellScale;
            uniform float3 _NoiseScale;
            uniform float3 _CellDirection;
            uniform float3 _NoiseDirection;
            uniform float _Looping;
            uniform float _Roundness;
            
            struct Input {
                float2 uv : TEXCOORD0;
                float4 objectPos;
                float3 worldPos;
                float4 color;
                float3 viewDir;
                float3 worldNormal;
            };
            
            
            
            
            // Description : Array and textureless GLSL 2D/3D/4D simplex
            //               noise functions.
            //      Author : Ian McEwan, Ashima Arts.
            //  Maintainer : ijm
            //     Lastmod : 20110822 (ijm)
            //     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
            //               Distributed under the MIT License. See LICENSE file.
            //               https://github.com/ashima/webgl-noise
            //
            
            float3 wglnoise_mod289(float3 x)
            {
                return x - floor(x / 289) * 289;
            }
            float4 wglnoise_mod289(float4 x)
            {
                return x - floor(x / 289) * 289;
            }
            float4 wglnoise_permute(float4 x)
            {
                return wglnoise_mod289((x * 34 + 1) * x);
            }
            float4 SimplexNoiseGrad(float3 v) {
                // First corner
                float3 i = floor(v + dot(v, 1.0 / 3));
                float3 x0 = v - i + dot(i, 1.0 / 6);

                // Other corners
                float3 g = x0.yzx <= x0.xyz;
                float3 l = 1 - g;
                float3 i1 = min(g.xyz, l.zxy);
                float3 i2 = max(g.xyz, l.zxy);

                float3 x1 = x0 - i1 + 1.0 / 6;
                float3 x2 = x0 - i2 + 1.0 / 3;
                float3 x3 = x0 - 0.5;

                // Permutations
                i = wglnoise_mod289(i); // Avoid truncation effects in permutation
                float4 p = wglnoise_permute(i.z + float4(0, i1.z, i2.z, 1));
                p = wglnoise_permute(p + i.y + float4(0, i1.y, i2.y, 1));
                p = wglnoise_permute(p + i.x + float4(0, i1.x, i2.x, 1));

                // Gradients: 7x7 points over a square, mapped onto an octahedron.
                // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
                float4 gx = lerp(-1, 1, frac(floor(p / 7) / 7));
                float4 gy = lerp(-1, 1, frac(floor(p % 7) / 7));
                float4 gz = 1 - abs(gx) - abs(gy);

                bool4 zn = gz < -0.01;
                gx += zn * (gx < -0.01 ? 1 : -1);
                gy += zn * (gy < -0.01 ? 1 : -1);

                float3 g0 = normalize(float3(gx.x, gy.x, gz.x));
                float3 g1 = normalize(float3(gx.y, gy.y, gz.y));
                float3 g2 = normalize(float3(gx.z, gy.z, gz.z));
                float3 g3 = normalize(float3(gx.w, gy.w, gz.w));

                // Compute noise and gradient at P
                float4 m = float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3));
                float4 px = float4(dot(g0, x0), dot(g1, x1), dot(g2, x2), dot(g3, x3));

                m = max(0.5 - m, 0);
                float4 m3 = m * m * m;
                float4 m4 = m * m3;

                float4 temp = -8 * m3 * px;
                float3 grad = m4.x * g0 + temp.x * x0 + m4.y * g1 + temp.y * x1 + m4.z * g2 +
                                temp.z * x2 + m4.w * g3 + temp.w * x3;

                return 107 * float4(grad, dot(m4, px));
            }
            
            //https://www.shadertoy.com/view/ll3GRM
            float3 hash33(float3 p, int seed) { 
                // Faster, but doesn't disperse things quite as nicely as other combinations. :)
                float n = sin(dot(p, float3(41 * _Random, 59 * _Random, 298 * _Random)));
                p = frac(float3(262144 + seed, 32768 + seed, 4875938 + seed)*n);
                return p;
            }
            // IQ's polynomial-based smooth minimum function.
            float smin( float a, float b, float k ){
                float h = k == 0 ? 0 : saturate(.5 + .5 * (b - a) / k);
                return lerp(b, a, h) - k*h*(1. - h);
            }

            //This is an original comment from Shadertoy:
            // 3D 3rd-order Voronoi: This is just a rehash of Fabrice Neyret's version, which is in
            // turn based on IQ's original. I've simplified it slightly, and tidied up the "if-statements,"
            // but the clever bit at the end came from Fabrice.
            //
            // Using a bit of science and art, Fabrice came up with the following formula to produce a more 
            // rounded, evenly distributed, cellular value:

            // d1, d2, d3 - First, second and third closest points (nodes).
            // val = 1./(1./(d2 - d1) + 1./(d3 - d1));
            //
            float Voronoi(in float3 p, int seed){
                
                float3 g = floor(p), o;
                p -= g;
                
                float3 d = float3(1,1,1); // 1.4, etc.
                
                float r = 0.;
                
                //scary looking 3-nested loops, but it's only evaulating 27 items
                for(int y = -1; y <= 1; y++){
                    for(int x = -1; x <= 1; x++){
                        for(int z = -1; z <= 1; z++){
                            o = float3(x, y, z);
                            o += hash33(g + o, seed) - p;
                            
                            r = dot(o, o);
                            
                            // 1st, 2nd and 3rd nearest squared distances.
                            d.z = max(d.x, max(d.y, min(d.z, r))); // 3rd.
                            d.y = max(d.x, min(d.y, r)); // 2nd.
                            d.x = min(d.x, r); // Closest.
                        }
                    }
                }
                
                d = sqrt(d); // Squared distance to distance.
                
                // Fabrice's formula.
                // return min(2./(1./max(d.y - d.x, .001) + 1./max(d.z - d.x, .001)), 1.);
                // Dr2's variation - See "Voronoi Of The Week": https://www.shadertoy.com/view/lsjBz1
                return min(smin(d.z, d.y, _Roundness) - d.x, 1.);
                
            }
            
            float divideByZeroSafe(float a, float b){
                return b ==0 ? 0 : a / b;
            }
            float3 divideByZeroSafe(float3 a, float3 b){
                return float3(divideByZeroSafe(a.x, b.x), divideByZeroSafe(a.y, b.y), divideByZeroSafe(a.z, b.z));
            }
            
            float3 waterColor (Input i, SurfaceOutput o) {
                float3 noise = _NoiseScale == float3(0,0,0) ? float3(0,0,0) : SimplexNoiseGrad(divideByZeroSafe(i.worldPos, _NoiseScale) + _Time.y * _NoiseDirection) * _Noise / 100;
                float cells = Voronoi(i.worldPos / _CellScale + noise + _Time.y * _CellDirection, 696969);
                float cells2 = Voronoi(i.worldPos / _CellScale + noise + _Time.y * _CellDirection, 1001);
                float pixelSize = fwidth(length(ObjSpaceViewDir(i.objectPos)));
                float3 blendedColor = lerp(lerp(_Color.rgb, _DarkFoamColor.rgb, _FoamBias), _FoamColor.rgb, _FoamBias);
                float3 waterColor = lerp(_Color.rgb, _DarkFoamColor.rgb, saturate((_FoamBias - cells2) * (_FoamBlur / 10000) / pixelSize));
                float3 waterWithFoam = lerp(waterColor, _FoamColor.rgb, saturate((_FoamBias - cells) * (_FoamBlur / 10000) / pixelSize));
                return lerp(blendedColor, waterWithFoam, saturate(_FoamFade / 10000 / pixelSize));
            }
            
            void surf (Input i, inout SurfaceOutput o) {
                o.Albedo = waterColor(i, o);
                o.Alpha = _Color.a;
            }
            
            
            void myvert (inout appdata_full v, out Input data){
                UNITY_INITIALIZE_OUTPUT(Input,data);
                data.objectPos = v.vertex;
            }
            
            void mycolor (Input i, SurfaceOutput o, inout float4 color) {
                if (_Lit <= 0){
                    color.rgb = waterColor(i, o);
                    color.a = _Color.a;
                }
            }
            
            
        ENDCG
    }
}
