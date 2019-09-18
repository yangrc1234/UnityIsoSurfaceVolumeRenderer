#ifndef RAYCAST_VOLUME_RENDERING_SHADOW_INCLUDED
#define RAYCAST_VOLUME_RENDERING_SHADOW_INCLUDED
#include "./raycastVolumeRenderingUtil.cginc"

struct vertexData {
	float3 vertex : POSITION;
};

struct Interpolator {
	float4 svpos : SV_POSITION;
	float3 localPos : TEXCOORD0;
	float3 worldPos : TEXCOORD1;
};

Interpolator vert(vertexData i) {
	Interpolator result;
	result.svpos = UnityObjectToClipPos(i.vertex);
	result.localPos = i.vertex;
	result.worldPos = mul(unity_ObjectToWorld, float4(i.vertex, 1));
	return result;
}

struct Out {
	half4 col:SV_TARGET; //should be useless here.
	float depth : SV_DEPTH;
};

Out frag(Interpolator i){
	float4 firstHit = 0;
	float4 gradient = 0;
	float3 worldPos = i.worldPos;
	float4 localPos = float4(i.localPos,1);
	float3
		viewDir =
		UNITY_MATRIX_P[3][3] * -UNITY_MATRIX_V[2].xyz
		+
		(1- UNITY_MATRIX_P[3][3]) * (worldPos - _WorldSpaceCameraPos);
	
	//this works only for directional light. 
	float3 objViewDir = normalize(UnityWorldToObjectDir(viewDir));
	float3 startPosition = localPos;
	float4 splitPlane = _SplitPlane;
	splitPlane.xyz = normalize(_SplitPlane.xyz);

	float2 index;	// (dataValue,gradient), will be calculated by GetFirstHit().
	firstHit = GetFirstHit(startPosition, objViewDir, index);
	float3 localFirstHit = firstHit - 0.5;
	float3 worldFirstHit = mul(UNITY_MATRIX_M, float4(localFirstHit, 1));

	float3 grad = GetGradientSobel(firstHit.xyz);
	gradient.w = length(grad);
	gradient.xyz = normalize(grad.xyz);

	float3 normal = UnityObjectToWorldNormal(-gradient.xyz);
	if (firstHit.a < 0.0001)
		discard;

	float4 opos = float4(localFirstHit, 1.0);
	opos = UnityClipSpaceShadowCasterPos(opos, normal);
	opos = UnityApplyLinearShadowBias(opos);

	Out res;
	res.col = res.depth = opos.z / opos.w;
	return res;
}
#endif