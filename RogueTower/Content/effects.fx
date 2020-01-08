#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

static const float PI = 3.14159265f;
static const float TAU = 6.28318530f;

matrix WorldViewProjection;

texture2D texture_main : register(t0);
texture2D texture_map : register(t1);
SamplerState sampler_main;

float4x4 map_transform;

float4 gradient_topleft;
float4 gradient_topright;
float4 gradient_bottomleft;
float4 gradient_bottomright;

float angle_target;
float angle_spread;

float4x4 color_matrix;
float4 color_add;

float2 distort_offset;

float threshold;
float4x4 threshold_color_matrix;
float4 threshold_color_add;

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 ScreenCoords : TEXCOORD1;
    float2 TextureCoordinates : TEXCOORD0;
};

float2 transformTexCoord(float4x4 x, float2 texcoord)
{
	return mul(x, float4(texcoord.x,texcoord.y,0,0)).xy;
}

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
	output.ScreenCoords = input.Position.xy;
	
	return output;
}

float4 GradientPS(VertexShaderOutput input) : COLOR
{
    float4 color = lerp(lerp(gradient_topleft, gradient_topright, input.ScreenCoords.xxxx), lerp(gradient_bottomleft, gradient_bottomright, input.ScreenCoords.xxxx), input.ScreenCoords.yyyy);
	return input.Color * color;
}

float4 ColorMatrixPS(VertexShaderOutput input) : COLOR
{
	float4 color = texture_main.Sample(sampler_main, input.TextureCoordinates) * input.Color;
	if (color.a <= 0)
		return color;
	return mul(color_matrix, color) + color_add;
}

float4 ColorMatrixThresholdPS(VertexShaderOutput input) : COLOR
{
	float4 color = texture_main.Sample(sampler_main, input.TextureCoordinates) * input.Color;
	float4 mask = texture_map.Sample(sampler_main, transformTexCoord(map_transform, input.TextureCoordinates));
	if (color.a <= 0)
		return color;
	if (mask.r < threshold)
		return mul(threshold_color_matrix, color) + threshold_color_add;
	else
		return mul(color_matrix, color) + color_add;
}

float4 ClockPS(VertexShaderOutput input) : COLOR
{
	float4 color = texture_main.Sample(sampler_main, input.TextureCoordinates) * input.Color;
	float dx = input.TextureCoordinates.x - 0.5;
	float dy = input.TextureCoordinates.y - 0.5;
	float angle = atan2(dy, dx);
	float da = (angle - angle_target) % TAU;
	float anglediff = abs((2 * da) % TAU - da);
	if (anglediff > angle_spread)
		return float4(0, 0, 0, 0);
	return color;
}

float4 DistortPS(VertexShaderOutput input) : COLOR
{
	float4 mask = texture_map.Sample(sampler_main, transformTexCoord(map_transform, input.TextureCoordinates));
	float2 offset = mask.r * distort_offset;
	float4 color = texture_main.Sample(sampler_main, input.TextureCoordinates + offset) * input.Color;
	return color;
}

float4 CirclePS(VertexShaderOutput input) : COLOR
{
	float dx = input.TextureCoordinates.x - 0.5;
	float dy = input.TextureCoordinates.y - 0.5;
	float angle = atan2(dy, dx);
	float dist = sqrt(dx * dx + dy * dy) * 2;
	float4 color = texture_main.Sample(sampler_main, transformTexCoord(map_transform, float2(dist, angle / TAU + 0.5))) * input.Color;
	if (dist > 1)
		return float4(0, 0, 0, 0);
	return color;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return input.Color;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
technique Gradient
{
    pass P0
    {
        //VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL GradientPS();
    }
};
technique ColorMatrix
{
	pass P0
	{
        VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL ColorMatrixPS();
	}
};
technique ColorMatrixThreshold
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL ColorMatrixThresholdPS();
	}
};
technique Circle
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL CirclePS();
	}
};
technique Clock
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL ClockPS();
	}
};
technique Distort
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL DistortPS();
	}
};