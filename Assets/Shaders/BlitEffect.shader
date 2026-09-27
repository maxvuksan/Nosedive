Shader "Custom/CustomBlit"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _SourceTexSize ("SourceTexSize", Vector) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _SourceTexSize; // x = width, y = height

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float2 texelSize = float2(1.0 / _SourceTexSize.x, 1.0 / _SourceTexSize.y);

                // 3x3 neighborhood
                fixed3 c[9];
                c[0] = tex2D(_MainTex, uv + texelSize * float2(-1,  1)).rgb;
                c[1] = tex2D(_MainTex, uv + texelSize * float2( 0,  1)).rgb;
                c[2] = tex2D(_MainTex, uv + texelSize * float2( 1,  1)).rgb;
                c[3] = tex2D(_MainTex, uv + texelSize * float2(-1,  0)).rgb;
                c[4] = tex2D(_MainTex, uv).rgb;
                c[5] = tex2D(_MainTex, uv + texelSize * float2( 1,  0)).rgb;
                c[6] = tex2D(_MainTex, uv + texelSize * float2(-1, -1)).rgb;
                c[7] = tex2D(_MainTex, uv + texelSize * float2( 0, -1)).rgb;
                c[8] = tex2D(_MainTex, uv + texelSize * float2( 1, -1)).rgb;

                // Edge strengths
                float dH = length(c[3] - c[5]);
                float dV = length(c[1] - c[7]);
                float dD1 = length(c[0] - c[8]);
                float dD2 = length(c[2] - c[6]);

                // Edge threshold — tweak for sharper or smoother look
                float threshold = 0.2;

                // Direction selection: pick from cleanest direction
                fixed3 result = c[4]; // default: center

                if (dD1 < threshold && dD1 < dD2 && dD1 < dH && dD1 < dV)
                    result = c[0]; // top-left → bottom-right
                else if (dD2 < threshold && dD2 < dD1 && dD2 < dH && dD2 < dV)
                    result = c[2]; // top-right → bottom-left
                else if (dH < threshold && dH < dV)
                    result = c[3]; // horizontal
                else if (dV < threshold)
                    result = c[1]; // vertical

                return fixed4(result, tex2D(_MainTex, uv).a);
            }
            ENDCG
        }
    }
}
