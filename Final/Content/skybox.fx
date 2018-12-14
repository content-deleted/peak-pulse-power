// Parameters that should be set from the program
float4x4 World; // World Matrix
float4x4 View; // View Matrix
float4x4 Projection; // Projection Matrix
texture Texture;

sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexInput
{
    float4 Position : SV_POSITION;
	float2 UV: TEXCOORD0;
    float3 Normal : NORMAL0;
};


struct StandardVertexOutput
{
    float4 Position : SV_POSITION;
	float2 UV: TEXCOORD0;
	float4 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

StandardVertexOutput PhongVertex(VertexInput input)
{
	StandardVertexOutput output;

	// Pass Through
	output.WorldPosition = mul(input.Position, World);
	float4 viewPosition = mul(output.WorldPosition, View);
	output.Position = mul(viewPosition, Projection);
    output.WorldNormal = mul(output.WorldPosition, input.Normal);
	output.UV = input.UV;
	return output;
}

float4 PhongPixel(StandardVertexOutput input) : COLOR0
{
    return tex2D(TextureSampler, input.UV);
}

technique Phong
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 PhongVertex();
		PixelShader = compile ps_4_0 PhongPixel();
	}
}