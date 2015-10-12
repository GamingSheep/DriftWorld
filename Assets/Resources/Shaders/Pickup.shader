Shader "Custom/Color Rim Halo" 
{
	Properties 
	{
		_RimColor ("Rim Color (RGB) Rim Power (A)", Color) = (0.26,0.19,0.16,0.0)
		_RimMultiplier ("Rim Multiplier", Range (0, 10)) = 1
	}

	SubShader 
	{
		Tags { "RenderType" = "Transparent" }
		CGPROGRAM
		
		#pragma surface surf SimpleSpecular alpha
		
		half4 LightingSimpleSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) 
		{
			half3 h = normalize (lightDir + viewDir);
			half diff = max (0, dot (s.Normal, lightDir));
			float nh = max (0, dot (s.Normal, h));
			float spec = pow (nh, 48.0);
			half4 c;
			c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec * s.Alpha) * (atten * 2);
			c.a = s.Alpha;
			return c;
		}
		
		struct Input 
		{
			float2 uv_MainTex;
			float3 viewDir;
		};
		
		float4 _RimColor;
		float _RimMultiplier;
		
		void surf (Input IN, inout SurfaceOutput o) 
		{
			o.Albedo = (0.0,0.0,0.0,0.0);
			half rim = 1.0 - saturate(dot (IN.viewDir, o.Normal));
			o.Emission = _RimColor.rgb * pow (rim, _RimColor.a * _RimMultiplier);
			o.Alpha = 0.0 + rim;
		}
		ENDCG
	} 
	Fallback "Diffuse"
}