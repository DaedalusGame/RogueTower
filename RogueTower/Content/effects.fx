#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix WorldViewProjection;

sampler s0;

float4 gradient_topleft;
float4 gradient_topright;
float4 gradient_bottomleft;
float4 gradient_bottomright;

float4x4 color_matrix;
float4 color_add;

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
	float4 color = tex2D(s0, input.TextureCoordinates) * input.Color;
	if (color.a <= 0)
		return color;
	return mul(color_matrix, color) + color_add;
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