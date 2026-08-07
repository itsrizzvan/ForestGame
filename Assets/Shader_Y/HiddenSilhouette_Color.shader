Shader "Custom/HiddenSilhouette_Color"
{
    Properties
    {
        _Color("Color", Color) = (1,0,0,1)
    }
    SubShader
    {
        // Renders last
        Tags { "RenderType"="Opaque" "Queue"="Transparent" }
        LOD 100

        ZWrite Off
        ZTest Greater
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref 1
            Comp NotEqual // Only draw if the Mask ISN'T here
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            float4 _Color;

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : COLOR { return _Color; }
            ENDCG
        }
    }
}