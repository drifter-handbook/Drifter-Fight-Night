Shader "Custom/PixelTrail"
{
	Properties
	{
		// Color property for material inspector, default to white
		_InsideFarColor("Inside Far Color", Color) = (0.2,0.2,1,1)
		_InsideNearColor("Inside Near Color", Color) = (0.5,1,1,1)
		_OutsideColor("Outside Color", Color) = (1,1,1,1)
		_MainTex("pixel", 2D) = "white" {}
	}
		SubShader
	{
	Tags
	{
	"Queue" = "Transparent"
	}
	Blend SrcAlpha OneMinusSrcAlpha
	Pass
	{
	CGPROGRAM
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"
	fixed4 _OutsideColor;
	fixed4 _InsideFarColor;
	fixed4 _InsideNearColor;
	sampler2D _MainTex;
	float4 _MainTex_ST;
	struct appdata
	{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
	float4 color : COLOR;
	};
	struct v2f
	{
	float2 uv : TEXCOORD0;
	float4 vertex : SV_POSITION;
	float4 color : COLOR;
	};
	v2f vert(appdata v)
	{
	v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = float4(v.uv.xy, 0, 0);
	o.color = float4(0, 0, v.uv.y, 1);
	return o;
	}
	// vertex shader
	float nrand(float2 uv)
	{
	return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
	}
	// pixel shader
	fixed4 frag(v2f i) : SV_Target
	{
	float distY = abs(i.uv.y - .5) * 2;
	fixed4 c;
	if (distY > .7)
	{
	c = _OutsideColor;
	if (nrand(i.vertex) > 1 - (i.uv.x * i.uv.x * i.uv.x))
	c = float4(0, 0, 0, 0);
	}
	else
	{
	if (i.uv.x > 0.8)
	c = _InsideFarColor;
	else if (0.8 >= i.uv.x && i.uv.x > 0.6)
	{
	c = _InsideFarColor * ((i.uv.x - 0.6) * 5) + _InsideNearColor * (1 - (i.uv.x - 0.6) * 5);
	}
	else
	{
	c = _InsideNearColor;
	}
	if (distY <= .7 && distY > 0.5)
	{
	c = _OutsideColor * ((distY - 0.5) * 5) + c * (1 - (distY - 0.5) * 5);
	}
	if (nrand(i.vertex) > 1 - (i.uv.x * i.uv.x))
	c = float4(0, 0, 0, 0);
	}
	return c;
	}
	ENDCG
	}
	}
}