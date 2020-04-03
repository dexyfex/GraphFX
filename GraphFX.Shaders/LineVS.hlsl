#include "Common.hlsli"

cbuffer VSSceneVars : register(b0)
{
	float4x4 ViewProj;
	float4 CameraPos;
}

struct PathShaderVertex
{
	float3 Position;
	uint Colour;
};

StructuredBuffer<PathShaderVertex> Vertices : register(t0);


struct VS_INPUT
{
	uint id : SV_VertexID;
};

struct VS_OUTPUT
{
	float4 Position  : SV_POSITION;
	float4 Colour    : COLOR0;
};


VS_OUTPUT main(VS_INPUT input)
{
	VS_OUTPUT output;

	float3 pos;
	float4 col;

	PathShaderVertex vert = Vertices[input.id];
	pos = vert.Position;
	col = Unpack4x8UNF(vert.Colour).abgr;

	float3 opos = pos - CameraPos.xyz;
	float4 cpos = mul(float4(opos, 1), ViewProj);
	output.Position = cpos;
	output.Colour.rgb = col.rgb;
	output.Colour.a = col.a;

	return output;
}

