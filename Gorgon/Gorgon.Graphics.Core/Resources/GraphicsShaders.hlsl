// Texture and sampler for blitting a texture.
Texture2D _bltTexture : register(t0);
SamplerState _bltSampler : register(s0);

// Our default blitting vertex.
struct GorgonBltVertex
{
   float4 position : SV_POSITION;
   float4 color : COLOR;
   float2 uv : TEXCOORD;
};

// The transformation matrices (for vertex shader).
cbuffer GorgonBltWorldViewProjection : register(b0)
{
	float4x4 WorldViewProjection;
}

// Our vertex shader for blitting textures.
GorgonBltVertex GorgonBltVertexShader(GorgonBltVertex vertex)
{
	GorgonBltVertex output = vertex;

	output.position = mul(WorldViewProjection, output.position);

	return output;
}

// Our pixel shader for blitting textures.
float4 GorgonBltPixelShader(GorgonBltVertex vertex) : SV_Target
{
	return _bltTexture.Sample(_bltSampler, vertex.uv) * vertex.color;	
}