Shader "Custom/AsteroidUnfreeze_Unlit"
{
	Properties
	{
		_FrozenTex("Frozen Texture", 2D) = "white" {}
		_ThawedTex("Thawed Texture", 2D) = "white" {}

		_Center("Reveal Center", Vector) = (0,0,0,0)
		_Radius("Reveal Radius", Float) = 0
		_EdgeWidth("Edge Width", Float) = 0.5
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		sampler2D _FrozenTex;
		sampler2D _ThawedTex;

		float4 _Center;
		float _Radius;
		float _EdgeWidth;

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
			float3 worldPos : TEXCOORD1;
		};

		v2f vert(appdata v)
		{
			v2f o;

			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = v.uv;
			o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			float dist = distance(i.worldPos, _Center.xyz);

		float blend = smoothstep(
			_Radius - _EdgeWidth,
			_Radius,
			dist
		);

		fixed4 frozen = tex2D(_FrozenTex, i.uv);
		fixed4 thawed = tex2D(_ThawedTex, i.uv);

		return lerp(thawed, frozen, blend);
		}

			ENDCG
		}
	}
}