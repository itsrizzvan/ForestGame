Shader "Custom/HiddenSilhouette_Mask"
{
    SubShader
    {
        // Renders just before the color pass
        Tags { "RenderType"="Opaque" "Queue"="Transparent-1" }
        LOD 100

        ZWrite Off
        ZTest LEqual
        ColorMask 0 // Invisible

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 pos : SV_POSITION; };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target { return half4(0,0,0,0); }
            ENDCG
        }
    }
}