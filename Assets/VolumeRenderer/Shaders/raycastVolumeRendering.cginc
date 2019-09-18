// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef RAYCAST_VOLUME_RENDERING_INCLUDED
#define RAYCAST_VOLUME_RENDERING_INCLUDED

#include "./raycastVolumeRenderingUtil.cginc"
#include "AutoLight.cginc"
struct vertexData {
	float3 vertex : POSITION;
};

struct Interpolator {
	float4 pos : SV_POSITION;
#ifdef VOLUME_BACKFACE
	float4 clipPos : TEXCOORD0;
#else
	float3 localPos : TEXCOORD0;
	float3 worldPos : TEXCOORD1;
#endif
	SHADOW_COORDS(5)
};

Interpolator vert(vertexData i) {
	Interpolator result;
	result.pos = UnityObjectToClipPos(i.vertex);
#ifdef VOLUME_BACKFACE
	result.clipPos = result.pos;
#else
	result.localPos = i.vertex;
	result.worldPos = mul(unity_ObjectToWorld, float4(i.vertex, 1));
#endif

	TRANSFER_SHADOW(result);
	return result;
}
float4 frag(Interpolator i,out float depth:SV_DEPTH) :SV_TARGET{
	float4 firstHit = 0;
	float4 gradient = 0;
#ifdef VOLUME_BACKFACE
	//i.clipPos.y = - i.clipPos.y;
	float3 nearClipPlanePos = (i.clipPos / i.clipPos.w).xyz;
	nearClipPlanePos.z = -1;
	float4 localPos = mul(VolumeClipToObject, float4(nearClipPlanePos, 1));
	localPos /= localPos.w;
	float3 worldPos = mul(unity_ObjectToWorld, localPos);
#else
	float3 worldPos = i.worldPos;
	float4 localPos = float4(i.localPos,1);
#endif
	float3 viewDir = normalize(lerp(worldPos - _WorldSpaceCameraPos, -UNITY_MATRIX_V[2].xyz, UNITY_MATRIX_P[3][3]));
	float3 objViewDir = normalize(UnityWorldToObjectDir(viewDir));
	float3 startPosition = localPos;
	float4 splitPlane = _SplitPlane;
	splitPlane.xyz = normalize(_SplitPlane.xyz);

	float2 index;	// (dataValue,gradient), will be calculated by GetFirstHit().
	firstHit = GetFirstHit(startPosition, objViewDir, index);		//this is uvw pos.
	float3 localFirstHit = firstHit - 0.5;
	float3 worldFirstHit = mul(UNITY_MATRIX_M, float4(localFirstHit, 1));

	if (firstHit.a < 0.0001)
		discard;
	float3 grad = GetGradientSobel(firstHit.xyz);
	gradient.w = length(grad);
	gradient.xyz = normalize(grad.xyz);
	
	float3 V = viewDir;
	float3 N = UnityObjectToWorldNormal(-gradient.xyz);
	float3 R = reflect(V, N);

	float3 specular = 0.0;

	specular = MonteCarloSpecular( N, R, GetSmoothness(index));

	float oneMinusReflectivity;
	float3 specularTint;
	float3 albedo = DiffuseAndSpecularFromMetallic(
		GetAlbedo(index), GetMetallic(index), specularTint, oneMinusReflectivity
	);

	float3 lightDir = _WorldSpaceLightPos0.xyz;

	UnityLight light;

	UNITY_LIGHT_ATTENUATION(attenuation, i, worldFirstHit);	//this is simplified by unity's built-in shadow map. the shadow map render is done in "ShadowCaster" pass.

	float amb = 0;
	amb = AmbientOcclusion(firstHit.xyz, -gradient.xyz);		//don't use N, it should be a local normal.
	light.color =  _LightColor0.rgb * attenuation;
	light.dir = lightDir;
	light.ndotl = DotClamped(N, lightDir) ;

	UnityIndirect indirectLight;
#ifdef TRANSCLUENCY_ON
	float4 occlude = 1;
	occlude = GetOcclude(localFirstHit, normalize(UnityWorldToObjectDir(lightDir)));
	indirectLight.diffuse = _LightColor0.rgb * occlude.rgb * (1 - occlude.a) * (1 - light.ndotl);
#else
	indirectLight.diffuse = 0;
#endif
	indirectLight.specular = specular;

	float4 opos = UnityObjectToClipPos(float4(localFirstHit,1));
	depth = opos.z / opos.w;

	float4 col = UNITY_BRDF_PBS(
		albedo, specularTint,
		oneMinusReflectivity, GetSmoothness(index),
		N, V,
		light, indirectLight
	) + float4(_AmbientColor,0);
	return float4(1 - amb.xxx,1) *col;
}
#endif