Shader "Custom/AsteroidUnfreeze"
{
	Properties
	{
		_FrozenAlbedo("Frozen Albedo", 2D) = "white" {}
		_FrozenNormal("Frozen Normal", 2D) = "bump" {}
		_FrozenRoughness("Frozen Roughness", 2D) = "white" {}

		_ThawedAlbedo("Thawed Albedo", 2D) = "white" {}
		_ThawedNormal("Thawed Normal", 2D) = "bump" {}
		_ThawedRoughness("Thawed Roughness", 2D) = "white" {}

		_Center("Reveal Center", Vector) = (0,0,0,0)
		_Radius("Reveal Radius", Float) = 0
		_EdgeWidth("Edge Width", Float) = 0.5
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 300

		CGPROGRAM
	#pragma surface surf Standard fullforwardshadows
	#pragma target 3.0

		sampler2D _FrozenAlbedo;
		sampler2D _FrozenNormal;
		sampler2D _FrozenRoughness;

		sampler2D _ThawedAlbedo;
		sampler2D _ThawedNormal;
		sampler2D _ThawedRoughness;

		float4 _Center;
		float _Radius;
		float _EdgeWidth;

		struct Input
		{
			float2 uv_FrozenAlbedo;
			float3 worldPos;
		};

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float dist = distance(IN.worldPos, _Center.xyz);
			float blend = smoothstep(_Radius - _EdgeWidth, _Radius, dist);

			// Albedo
			float3 frozenAlbedo = tex2D(_FrozenAlbedo, IN.uv_FrozenAlbedo).rgb;
			float3 thawedAlbedo = tex2D(_ThawedAlbedo, IN.uv_FrozenAlbedo).rgb;
			o.Albedo = lerp(thawedAlbedo, frozenAlbedo, blend);

			// Roughness (Unity uses Smoothness = 1 - Roughness)
			float frozenRough = tex2D(_FrozenRoughness, IN.uv_FrozenAlbedo).r;
			float thawedRough = tex2D(_ThawedRoughness, IN.uv_FrozenAlbedo).r;
			float rough = lerp(thawedRough, frozenRough, blend);
			o.Smoothness = 1 - rough;

			// Normal
			float3 frozenNormal = UnpackNormal(tex2D(_FrozenNormal, IN.uv_FrozenAlbedo));
			float3 thawedNormal = UnpackNormal(tex2D(_ThawedNormal, IN.uv_FrozenAlbedo));
			o.Normal = normalize(lerp(thawedNormal, frozenNormal, blend));

			o.Metallic = 0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}