/*#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_3 
	#define PS_SHADERMODEL ps_4_0_level_9_3 
#endif
*/
float3 CameraPosition; // in world space
float4x4 World; // World Matrix
float4x4 View; // View Matrix
float4x4 Projection; // Projection Matrix
float Offset;
float AlphaMax;
float3 Color;
float HeightOffset;
Texture2D songData;
float songPos;
float avgE;
//<float>
sampler songSampler = sampler_state
{
    Texture = <songData>;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

struct VertexShaderInput
{
    float4 Position : SV_POSITION0;
    float3 Normal : NORMAL0;
    float2 UV : TEXCOORD0;
    //float4 Color : COLOR0;
    //float2 UV: TEXCOORD0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	//float4 Color : COLOR0;
    float4 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
    float normalClipCheck : TEXCOORD3;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
    //dot(tex2D(songSampler, float2(input.Position.x, currentFrame)), float4(1, 1, 1, 1));
    float samp = songData.SampleLevel(songSampler, float2(input.Position.y / 1000, songPos), 1);
    float samp2 = songData.SampleLevel(songSampler, float2(input.Position.y / 1000, songPos-0.001), 1);
    float avg = (samp + samp2) / 2;
    float h = HeightOffset + samp + avgE;
    output.WorldPosition = mul(input.Position + float4(0, h, 0,0), World);
    float4 viewPosition = mul(output.WorldPosition, View);
    output.Position = mul(viewPosition, Projection);

    //output.Color = input.Color; //olor : COLOR0;
	// Send normal in world space
    output.WorldNormal = mul(input.Normal, World);
	
    output.normalClipCheck = input.Normal.y;

	return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    //float edge = dot( (CameraPosition - input.WorldPosition.xyz), input.WorldNormal );
    //float4 color = (1, 1, 1, ((int) edge));
    float dist = abs(1 / (distance(CameraPosition, input.WorldPosition.xyz) / (Offset + 35)));
    dist = dist;
    //edge = clamp(edge, 0.01, 1);
    clip(dist - 0.05); 
    clip(input.normalClipCheck); // flag for verticies attached at draw edges
    clip(input.WorldPosition.z - (CameraPosition.z - 5) ); // dont bother with anything behind

    float3 altColor = float3(0.6, 0 , 0.6);

    return float4(Color + clamp(altColor / (dist) -0.15, float3(0, 0, 0), float3(1, 1, 1)), AlphaMax); //input.Color;

}

technique BasicColorDrawing
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