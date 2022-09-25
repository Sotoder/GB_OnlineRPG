Shader "Lit/Atmosfere"
  {
      Properties
      {
          _TextureOffset("TextureOffset", Range(0, 0.03)) = 0.015
          _CloudRotationSpeed("CloudRotationSpeed", Range(0, 3)) = 0.5
          _Color ("Color", Color) = (1,1,1,1)
          _MainTex ("Texture", 2D) = "white" {}
          _AlphaScale ("Alpha Scale", Range(0, 1)) = 1
      }
      SubShader
     {
         Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
         Cull Off
         Pass
         {
             ZWrite On
             ColorMask 0
         }

         Pass
         {
            Tags {"LightMode" = "CustomLit"}
            
             ZWrite Off
             Blend SrcAlpha OneMinusSrcAlpha
 
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             
             #include "UnityCG.cginc"
             #include "Lighting.cginc"
 
             struct a2v
             {
                 float4 vertex : POSITION;
                 float3 normal : NORMAL;
                 float4 texcoord : TEXCOORD0;
             };
 
             struct v2f
             {
                 float4 pos : SV_POSITION;
                 float2 uv : TEXCOORD0;
                 float3 worldNormal : TEXCOORD1;
                 float3 worldPos : TEXCOORD2;
             };
 
             sampler2D _MainTex;
             float4 _MainTex_ST;
             fixed4 _Color;
             fixed _AlphaScale;
             float _TextureOffset;
             float _CloudRotationSpeed;
             
             v2f vert (a2v v)
             {
                 v2f o;
 
                 v.vertex.xyz += v.normal * _TextureOffset;
                 o.pos = UnityObjectToClipPos(v.vertex);

                 v.texcoord.x += _CloudRotationSpeed * _Time;
                 o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                 o.worldNormal = UnityObjectToWorldNormal(v.normal);
                 o.worldPos = mul(unity_ObjectToWorld, v.vertex);
 
                 return o;
             }
             
             fixed4 frag (v2f i) : SV_Target
             {
                 fixed3 worldNormal = normalize(i.worldNormal);
                 fixed3 worldPos = normalize(i.worldPos);
                 fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(worldPos));
                 fixed4 texColor = tex2D(_MainTex, i.uv);
                 fixed3 albedo =  texColor.rgb * _Color.rgb;
                 fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * albedo;
                 fixed3 diffuse = _LightColor0.rgb * albedo * max(0, dot(worldNormal, worldLightDir));
                 return fixed4(ambient + diffuse, texColor.a * _AlphaScale);
             }
             ENDCG
         }
     }
 }
