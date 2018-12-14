/*#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
*/
float3 CameraPosition; // in world space
float4x4 World; // World Matrix
float4x4 View; // View Matrix
float4x4 Projection; // Projection Matrix

float rotation;

struct VertexShaderInput
{
    float4 Position : SV_Position0;
    float3 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
    float4 WorldPosition : TEXCOORD1;
    float dist : TEXCOORD2;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    output.WorldPosition = mul(input.Position + float4(sin(rotation) * input.Normal, 0), World);
    float4 viewPosition = mul(output.WorldPosition, View);
    output.Position = mul(viewPosition, Projection);

    
	// Send normal in world space
    //output.WorldNormal = mul(input.Normal, World);
	
    output.dist = distance(input.Normal, input.Position);
    output.UV = input.UV;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    //float edge = dot( (CameraPosition - input.WorldPosition.xyz), input.WorldNormal );
    //float4 color = (1, 1, 1, ((int) edge));
    //float dist = abs(1 / (distance(CameraPosition, input.WorldPosition.xyz) / Offset));
    //edge = clamp(edge, 0.01, 1);
    //clip(dist - 0.05);
    //float d = 1 / distance(World._41_42_43, input.WorldNormal.xyz);
    
    return float4(1 / input.dist, 0, 1 / input.dist, 1); //float4(Color, min(dist, AlphaMax)); //input.Color;
}

technique hoopDrawing
{
	pass P0
	{
        VertexShader = compile vs_4_0 MainVS();
        PixelShader = compile ps_4_0 MainPS();
    }
};

/*
texture wire;

sampler WireSampler = sampler_state
{
    Texture = <wire>;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : SV_Position0;
    float3 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
    //float4 Color : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	//float4 Color : COLOR0;
    float4 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
    float2 UV : TEXCOORD0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    output.WorldPosition = mul(input.Position + float4(0, HeightOffset, 0,0), World);
    float4 viewPosition = mul(output.WorldPosition, View);
    output.Position = mul(viewPosition, Projection);

    //output.Color = input.Color; //olor : COLOR0;
	// Send normal in world space
    output.WorldNormal = mul(input.Normal, World);
	
    output.UV = input.UV;
	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float edge = dot( (CameraPosition - input.WorldPosition.xyz), input.WorldNormal );
    //float4 color = (1, 1, 1, ((int) edge));
    float dist = abs(1 / (distance(CameraPosition, input.WorldPosition.xyz) / Offset));
    //edge = clamp(edge, 0.01, 1);
    clip(dist - 0.05);
    float4 tex = tex2D(WireSampler, input.UV); //min(dist, AlphaMax)
    tex += float4(0, 1 / edge, 0, 0);
    return tex; //input.Color;

*/