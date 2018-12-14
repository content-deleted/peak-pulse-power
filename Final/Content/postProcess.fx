#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
bool toggle;
bool noisy;
float time;
float darkenFactor;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float rand(float2 co)
{
    float2 temp = sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453;
    return temp - floor(temp);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float dist = distance(input.TextureCoordinates.xy, float2(0.5, 0.5));
    float4 color = (1 - dist / (2 * darkenFactor)) * input.Color;

    float4 noise = float4(rand(input.TextureCoordinates.xy + float2(time, 1/time)) * float3(1, 1, 1), 1);

    return (noisy) ? noise
    : tex2D(SpriteTextureSampler, input.TextureCoordinates.xy) * (toggle ? color : input.Color);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};