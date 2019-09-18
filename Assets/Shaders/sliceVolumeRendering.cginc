// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
#ifndef SLICE_VOLUME_RENDERING_INCLUDED
#define SLICE_VOLUME_RENDERING_INCLUDED

sampler2D _TransferTex;
sampler3D _VolumeTex;
sampler3D _GradientTex ;
float3 _VolumeTex_TexelSize;
float _MaxDataValue;
float _MinDataValue;
float _SampleStep;
float3 _Tint;
float3 _VolumeLightDir;
matrix ObjectToLightClipPos;
float _UniformVoxelOpacity;

float4 transfer(float rawVoxelData) {
	float scaledData = (max(0, rawVoxelData - _MinDataValue)) / (_MaxDataValue - _MinDataValue);
	//below code makes the scale more "reasonable". 
	//without it, the scale will make data over maxDataValue much more larger, which is actually not bad if you're using simple transfer.
	//since opacity in those "large data" area will become much larger.
	scaledData = min(1, scaledData);
	float alpha = scaledData;
	float3 color = _Tint;
#ifdef TRANSFER_TEXTURE
	//color = tex2D(_TransferTex, float2(rawVoxelData, 0.5)).rgb * _Tint;
	color = _Tint;
	alpha = tex2D(_TransferTex, float2(rawVoxelData, 0.5)).a;
#endif
	return float4(color, alpha);
}

float alphaCorrection(float alpha) {
	return alpha * _SampleStep * _UniformVoxelOpacity;
}


float3 GetGradient(float3 pos) {
	float3 result;
	float3 delta = float3(_VolumeTex_TexelSize.x * 0.5, 0, 0);
	float h1 = tex3Dlod(_VolumeTex, float4(pos - delta,0));
	float h2 = tex3Dlod(_VolumeTex, float4(pos + delta,0));
	result.x = h2-h1;

	delta = float3(0, _VolumeTex_TexelSize.y * 0.5, 0);
	 h1 = tex3Dlod(_VolumeTex, float4(pos - delta, 0));
	 h2 = tex3Dlod(_VolumeTex, float4(pos + delta, 0));
	result.y = h2-h1;

	delta = float3(0, 0, _VolumeTex_TexelSize.y * 0.5);
	 h1 = tex3Dlod(_VolumeTex, float4(pos - delta, 0));
	 h2 = tex3Dlod(_VolumeTex, float4(pos + delta, 0));
	result.z = h2-h1;

	return result;
}

#endif