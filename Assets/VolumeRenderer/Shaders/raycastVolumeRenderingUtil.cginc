#ifndef RAYCAST_VOLUME_RENDERING_UTIL_INCLUDED
#define RAYCAST_VOLUME_RENDERING_UTIL_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"
#include "UnityStandardBRDF.cginc"
#include "UnityPBSLighting.cginc"

sampler3D _VolumeTex;
sampler3D _GradientTex;
float3 _VolumeTex_TexelSize;
matrix VolumeClipToObject;
float _VisibleIsoValue;
float3 _Albedo;
float _Metallic;
float _Smoothness;
float _Transcluency;
float _GradientScale;
float3 _AmbientColor;
float4 _SplitPlane;
sampler2D _AlbedoIsoTransfer;
sampler2D _PhysicsTransfer;

half3 GetGradientSobel(float3 pos) {
#ifdef GRADIENT_PRECALCULATED
	return _GradientScale * tex3Dlod(_GradientTex, float4(pos, 0));
#else
	float4 delta = 1.0 / 128.0;
	delta.w = 0.0;
	float3 res = (0.0).xxx;
	res.x = tex3Dlod(_VolumeTex, float4(pos + delta.xww, 0)) - tex3Dlod(_VolumeTex, float4(pos - delta.xww, 0));
	res.y = tex3Dlod(_VolumeTex, float4(pos + delta.wxw, 0)) - tex3Dlod(_VolumeTex, float4(pos - delta.wxw, 0));
	res.z = tex3Dlod(_VolumeTex, float4(pos + delta.wwx, 0)) - tex3Dlod(_VolumeTex, float4(pos - delta.wwx, 0));
	return _GradientScale * res;
#endif
	/*#else
	float3 result;
	float kernal[2][3][3] = {
		{
			{ -1,-2,-1 },
			{ -2,-4,-2 },
			{ -1,-2,-1 }

		},{
			{ 1,2,1 },
			{ 2,4,2 },
			{ 1,2,1 }
		}
	};
	int i;
	for (i = 0; i < 2; i++) {
		for (int j = 0; j < 3; j++) {
			for (int k = 0; k < 3; k++) {
				float3 delta = float3(
					2 * (i - 1) * _VolumeTex_TexelSize.x,
					(j - 1) * _VolumeTex_TexelSize.y,
					(k - 1) * _VolumeTex_TexelSize.y
					);
				result.x += kernal[i][j][k] * tex3Dlod(_VolumeTex, float4(pos + delta, 0));
			}
		}
	}


	for (i = 0; i < 2; i++) {
		for (int j = 0; j < 3; j++) {
			for (int k = 0; k < 3; k++) {
				float3 delta = float3(
					(j - 1) * _VolumeTex_TexelSize.x,
					2 * (i - 1) * _VolumeTex_TexelSize.y,
					(k - 1) * _VolumeTex_TexelSize.y
					);
				result.y += kernal[i][j][k] * tex3Dlod(_VolumeTex, float4(pos + delta, 0));
			}
		}
	}

	for (i = 0; i < 2; i++) {
		for (int j = 0; j < 3; j++) {
			for (int k = 0; k < 3; k++) {
				float3 delta = float3(
					(k - 1) * _VolumeTex_TexelSize.x,
					(j - 1) * _VolumeTex_TexelSize.y,
					2 * (i - 1) * _VolumeTex_TexelSize.y
					);
				result.z += kernal[i][j][k] * tex3Dlod(_VolumeTex, float4(pos + delta, 0));
			}
		}
	}

	return _GradientScale * result / 32;
#endif*/
}

half3 GetAlbedo(float2 index) {
#if defined(ALBEDO_USE_TRANSFER) || defined (ALL_USE_TRANSFER)
	return _Albedo * tex2Dlod(_AlbedoIsoTransfer, float4(index, 0, 0)).rgb;
#else
	return _Albedo;
#endif
}

half GetIsoValue(float2 index) {
#if defined(ISOVALUE_USE_TRANSFER) || defined (ALL_USE_TRANSFER)		//this will hit performance badly on large dataset
	return tex2Dlod(_AlbedoIsoTransfer, float4(index, 0, 0)).a;
#else
	return index.r;
#endif
}

half GetMetallic(float2 index) {
#if defined(METALLIC_USE_TRANSFER) || defined (ALL_USE_TRANSFER)
	return _Metallic * tex2Dlod(_PhysicsTransfer, float4(index, 0, 0)).r;
#else
	return _Metallic;
#endif
}

half GetSmoothness(float2 index) {
#if defined(SMOOTHNESS_USE_TRANSFER) || defined (ALL_USE_TRANSFER)
	return _Smoothness * tex2Dlod(_PhysicsTransfer, float4(index, 0, 0)).g;
#else
	return _Smoothness;
#endif
}

fixed GetTranscluency(float2 index) {
#if 0//defined(TRANSCLUENCY_USE_TRANSFER) || defined (ALL_USE_TRANSFER)
	return _Transcluency * tex2Dlod(_PhysicsTransfer, float4(index, 0, 0)).b;
#else
	return _Transcluency;
#endif
}

fixed rand(fixed3 co) {
	return frac(sin(dot(co.xyz, fixed3(12.9898, 78.233, 45.5432))) * 43758.5453);
}

float PointPlaneDistance(float3 pt, float4 plane) {
	plane.xyz = normalize(plane.xyz);
	return dot(plane, pt) + plane.w;
}
float SplitedSample(float3 uvw) {
	float origisample = tex3Dlod(_VolumeTex, float4(uvw, 0)).r;
	if (PointPlaneDistance(uvw, _SplitPlane) < 0)
		return 0;
	else
		return origisample;
}
#define MAX_SAMPLE_COUNT 248
#define SAMPLE_STEP_SIZE (1.7/MAX_SAMPLE_COUNT)
float4 GetFirstHit(float3 startPosition, float3 objViewDir, out float2 index) {

	for (int j = 0; j < MAX_SAMPLE_COUNT; j++) {
		float3 uvw = startPosition + 0.5;
		if (uvw.x < -0.001 || uvw.y < -0.001 || uvw.z < -0.001 || uvw.x > 1.001 || uvw.y > 1.001f || uvw.z > 1.001) {
			return float4(uvw, 0);
		}

		float planeDistance;
		float samp = SplitedSample(uvw);
		float grad = length(GetGradientSobel(uvw));
		float distance = PointPlaneDistance(uvw - 0.5, _SplitPlane);
		index = float2(samp, grad);
		float isoValue = GetIsoValue(index);
		if (isoValue > _VisibleIsoValue && distance > 0) {

			float3 start = startPosition - objViewDir * SAMPLE_STEP_SIZE;
			float3 end = startPosition;
			for (int b = 0; b < 10; b++) {
				float3 testPos = (start + end) / 2.0;
				uvw = testPos + 0.5;
				samp = SplitedSample(uvw);
				grad = length(GetGradientSobel(uvw));
				index = float2(samp, grad);
				isoValue = GetIsoValue(index);
				distance = PointPlaneDistance(uvw - 0.5, _SplitPlane);
				if (isoValue < _VisibleIsoValue || distance < 0) {
					start = testPos;
				}
				else {
					end = testPos;
				}
			}
			return float4(uvw, 1);
		}

		startPosition += objViewDir * SAMPLE_STEP_SIZE;
	}
	return 0;
}

#define MONTE_CARLO_SPECULAR_SAMPLE_COUNT 64
half3 MonteCarloSpecular(float3 N, float3 R, float smoothness) {

#if MONTECARLO_SPECULAR		//try the cool monte-carlo!
	float3 randSeed = R.xyz;
	float3 specular = 0;
	for (int i = 0; i < MONTE_CARLO_SPECULAR_SAMPLE_COUNT; i++) {
		randSeed += R;
		float3 randDir = normalize(float3(rand(randSeed.xyz) - 0.5, rand(randSeed.xzy) - 0.5, rand(randSeed.yzx) - 0.5));

		float cosTheta = dot(randDir, N);
		if (cosTheta < 0.0) {
			randDir = -randDir;
			cosTheta = -cosTheta;
		}
		randDir = normalize(lerp(randDir, R, smoothness));

		float4 samp = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, randDir);
		specular += DecodeHDR(samp, unity_SpecCube0_HDR) * cosTheta;
	}

	specular /= MONTE_CARLO_SPECULAR_SAMPLE_COUNT;
#else
	float roughness = 1 - smoothness;
	roughness *= 1.7 - 0.7 * roughness;
	float4 samp = UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, R, roughness * UNITY_SPECCUBE_LOD_STEPS);
	float3 specular = DecodeHDR(samp, unity_SpecCube0_HDR);
#endif
	return specular;
}

#define OCCLUDE_SAMPLE_COUNT 128
#define OCCLUDE_SAMPLE_STEP_SIZE 0.01
float4 GetOcclude(float3 startPosition, float3 raycastDir) {
	float3 result = 0.0001;
	float alpha = 0.0001;
	for (int j = 0; j < OCCLUDE_SAMPLE_COUNT; j++) {
		float3 uvw = startPosition + 0.5;
		if (uvw.x < -0.0001 || uvw.y < -0.0001 || uvw.z < -0.0001 || uvw.x > 1.0001 || uvw.y > 1.0001 || uvw.z > 1.0001) {
			return float4(result / alpha, alpha);
		}
		startPosition += OCCLUDE_SAMPLE_STEP_SIZE * raycastDir;
		float samp = tex3Dlod(_VolumeTex, float4(uvw, 0));
		float grad = length(GetGradientSobel(uvw));
		float distance = PointPlaneDistance(uvw - 0.5, _SplitPlane);
		float2 index = float2(samp, grad);
		//float occlude = _Transcluency*5 * max(GetIsoValue(index) - _VisibleIsoValue,0.001) * OCCLUDE_SAMPLE_STEP_SIZE * 10;
		float occlude = GetIsoValue(index) - _VisibleIsoValue > 0 ? 10 * _Transcluency * OCCLUDE_SAMPLE_STEP_SIZE : 0;
		occlude = distance < 0 ? 0 : occlude;
		result = (1 - occlude) * result + GetAlbedo(index) * occlude;
		alpha += (1 - alpha) * occlude;
		if (alpha > 0.99) {
			return float4(result,1);
		}
	}
	return float4(result / alpha , alpha);
}

#define AO_NUM_SAMPLES 32.0
#define FACTOR_RAYSTEP 3.0
#define MAX_NUM_RAY_STEPS 3
#define STEPSIZE 0.01
float AmbientOcclusion(float3 pos, float3 localN) {

	float amb = 0;
#ifdef AMBIENT_OCCULUSION_ON
	float3 randSeed = 0;

	for (int i = 0; i < AO_NUM_SAMPLES; i++) {
		randSeed += float3(0.1, 0.2, 0.3);
		float3 randDir = normalize(float3(rand(randSeed.xyz) - 0.5, rand(randSeed.xzy) - 0.5, rand(randSeed.yzx) - 0.5));

		if (dot(randDir, localN) < 0.0) randDir = -randDir;

		randDir = lerp(randDir, localN, 0.1);
		float3 dirStep = randDir * FACTOR_RAYSTEP * STEPSIZE;
		float3 samplepos = pos + 0.001*normalize(localN)+ dirStep;

		for (int s = 0; s < MAX_NUM_RAY_STEPS; s++) {
			float value = tex3Dlod(_VolumeTex, float4(samplepos, 0)).r;
			float grad = GetGradientSobel(samplepos);
			float2 index = float2(value, grad);
			float distance = PointPlaneDistance(samplepos - 0.5, _SplitPlane);

			if (GetIsoValue(index) > _VisibleIsoValue && distance > 0) {
				amb += 1.0f;
				break;
			}
			samplepos += dirStep;
		}
	}
	amb /= AO_NUM_SAMPLES;
#endif
	return amb;
}
#endif