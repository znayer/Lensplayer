// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Basic Transparent" {
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_Color("Main Color", Color) = (1,1,1,1) // Lets you set a color in addition to the texture
		_ScrollXSpeed("X Scroll Speed",Range(0,1)) = 0.0
		_ScrollYSpeed("Y Scroll Speed",Range(0,1)) = 0.0
	}

		SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		LOD 100

		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_fog

#include "UnityCG.cginc"

		struct appdata_t {
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
	};

	struct v2f {
		float4 vertex : SV_POSITION;
		half2 texcoord : TEXCOORD0;
		float2 texcoord1 : TEXCOORD1;
		UNITY_FOG_COORDS(1)
	};

	fixed4 _Color; // This is the color that you set

	sampler2D _MainTex;
	float4 _MainTex_ST;
	fixed _ScrollXSpeed;
	fixed _ScrollYSpeed;

	v2f vert(appdata_t v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
		o.texcoord1.x = frac(_ScrollXSpeed * _Time.y);
		o.texcoord1.y = frac(_ScrollYSpeed * _Time.y);
		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{


		// sample the texture
		//				fixed4 col = tex2D(_MainTex, i.uv);
		// sample the texture
		float2 scrolledUV = i.texcoord;
//		float xScrollValue = frac(_ScrollXSpeed * _Time.y);
//		float yScrollValue = frac(_ScrollYSpeed * _Time.y);
		scrolledUV += i.texcoord1;
		fixed4 col = tex2D(_MainTex, scrolledUV) * _Color;


		// apply fog
		UNITY_APPLY_FOG(i.fogCoord, col);
		return col;






//		fixed4 col = tex2D(_MainTex, i.texcoord) * _Color; // Just multiply the color with the texture
//	UNITY_APPLY_FOG(i.fogCoord, col);
//	return col;

	}
		ENDCG
	}
	}

}
