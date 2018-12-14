#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float offset;
float height;
float tile;

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

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float2 p = input.TextureCoordinates.xy; 
    p = 2.0 * p - 1;
    float a = atan2(p.y, p.x ) / (2 * 3.1416);
    float r = sqrt(dot(p, p)) / sqrt(2);
    float2 uv;
    uv.x = (r + offset) % 1;
    uv.y = a + r;

    return tex2D(SpriteTextureSampler, uv) * input.Color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};