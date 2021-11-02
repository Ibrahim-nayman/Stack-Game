Shader "Vertex Color" {
    
    properties {
        
        _Maintex (" Base (RGB(", 2D) = "white"{}
        
}
    SubShader {

        Pass {
            Lighting On
            ColorMaterial AmbientAndDiffuse
            SetTexture [_MainTex] {
            combine texture * primary DOUBLE 
                }

            }

        }

    }
