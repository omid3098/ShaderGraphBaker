Shader "Hidden/ObjectToTangentNormal"
{
	Properties {
		_MainTex ("", 2D) = "white" {}
		_ObjectNormal ("", 2D) = "bump" {}
	}

	SubShader {
		Pass {
			Name "Object to Tangent Space"
			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_Position;
				float2 uv : TEXCOORD0;
				float4 tangent : TEXCOORD1;
				float3 bitangent : TEXCOORD2;
				float3 normal : TEXCOORD3;
			};

			v2f vert(appdata_full v) 
			{
				v2f o;
				o.pos = float4(v.texcoord.xy * 2.0 - 1.0, 0.5, 1.0);
				o.pos.y = -o.pos.y;

				o.uv = v.texcoord;

				float3 worldNormal = v.normal;
			    float3 worldTangent = v.tangent.xyz;
			    float tangentSign = v.tangent.w;// * unity_WorldTransformParams.w;
			    float3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;
			    o.tangent = float4(worldTangent, tangentSign);
			    o.bitangent = worldBitangent;
			    o.normal = worldNormal;

			    return o;
			}

			sampler2D _ObjectNormal;
			float4x4 _ObjectSpaceCorrection;
			bool _PerPixelBitangent;

			float3x3 inverseTBN(float3 t, float3 b, float3 n)
			{
	            half3x3 w2tRotation;
	            w2tRotation[0] = b.yzx * n.zxy - b.zxy * n.yzx;
	            w2tRotation[1] = t.zxy * n.yzx - t.yzx * n.zxy;
	            w2tRotation[2] = t.yzx * b.zxy - t.zxy * b.yzx;
 
	            half det = dot(t.xyz, w2tRotation[0]);
 
    	        w2tRotation *= rcp(det);

    	        return w2tRotation;
			}

			float4 frag(v2f i) : SV_Target
			{
				float3 objectNormal = tex2D(_ObjectNormal, i.uv).xyz * 2.0 - 1.0;

				objectNormal = normalize(mul((float3x3)_ObjectSpaceCorrection, objectNormal));

				float3 tangent = i.tangent.xyz;
				float3 bitangent = i.bitangent;
				float3 normal = i.normal;

				if (_PerPixelBitangent)
					bitangent = (cross(normal, tangent)) * i.tangent.w;

				float3x3 worldToTangent = inverseTBN(tangent, bitangent, normal);
				float3 tangentNormal = normalize(mul(worldToTangent, objectNormal));

				return float4(tangentNormal * 0.5 + 0.5, 1.0);
			}

			ENDCG
		}
		Pass
		{
			Name "Dilate"
			Cull Off

			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			float4 frag(v2f_img i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				if (col.a == 0)
				{
					float2 offsets[4];
					offsets[0] = float2(-_MainTex_TexelSize.x, 0) + i.uv;
					offsets[1] = float2( _MainTex_TexelSize.x, 0) + i.uv;
					offsets[2] = float2(0,-_MainTex_TexelSize.y) + i.uv;
					offsets[3] = float2(0, _MainTex_TexelSize.y) + i.uv;

					UNITY_UNROLL
					for (int p=0; p<4; p++)
					{
						float4 offsetCol = tex2D(_MainTex, offsets[p]);
						if (offsetCol.a > 0)
						{
							col = offsetCol;
							break;
						}
					}
				}
				return col;
			}
			ENDCG
		}
	}
}