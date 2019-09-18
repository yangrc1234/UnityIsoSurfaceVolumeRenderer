Shader "Custom/IsosurfaceRendering" {
	Properties{
		_VolumeTex("VolumeDataTexture",3D) = "black"{}
		_GradientTex("GradientTex",3D) = "gray"{}
		_VisibleIsoValue("VisibleIsoValue",Range(0,1)) = 0.5
		_SplitPlane("SplitPlane",Vector) = (0,0,0,0)
		_Albedo("Albedo",Color) = (0,0,0,1)
		_Metallic("Metallic",Range(0,1)) = 0.5
		_Smoothness("Smoothness",Range(0,1)) = 0.5
		_Transcluency("Transcluency",Range(0.1,5)) = 1
		_AmbientColor("AmbientColor",Color) = (0,0,0,1)
		_AlbedoIsoTransfer("Albedo(RGB)ISO(A)Transfer",2D) = "gray"{}
		_PhysicsTransfer("Metallic(R)Smoothness(G)Transfer",2D) = "gray"{}
		_GradientScale("Gradient Scale",float) = 1
	}
	SubShader{
			Tags{
				"Queue" = "Transparent+1"
			}

		Pass{		//base pass.
			Tags{
				"LightMode" = "ForwardBase"
			}
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ SHADOWS_SCREEN
			#pragma multi_compile __ AMBIENT_OCCULUSION_ON  
			#pragma multi_compile __ TRANSCLUENCY_ON  
			#pragma multi_compile __ GRADIENT_PRECALCULATED 
			#pragma multi_compile __ MONTECARLO_SPECULAR 
			#pragma multi_compile __ ALL_USE_TRANSFER
		/*	#pragma multi_compile __ ISOVALUE_USE_TRANSFER
			#pragma multi_compile __ ALBEDO_USE_TRANSFER
			#pragma multi_compile __ SMOOTHNESS_USE_TRANSFER
			#pragma multi_compile __ METALLIC_USE_TRANSFER*/
			#include "Assets/VolumeRenderer/Shaders/raycastVolumeRendering.cginc"
			
			ENDCG
		}
		Pass{		//render the near clip.

			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ SHADOWS_SCREEN
			#pragma multi_compile __ AMBIENT_OCCULUSION_ON 
			#pragma multi_compile __ TRANSCLUENCY_ON  
			#pragma multi_compile __ GRADIENT_PRECALCULATED 
			#pragma multi_compile __ MONTECARLO_SPECULAR 
			#pragma multi_compile __ ALL_USE_TRANSFER
		/*	#pragma multi_compile __ ISOVALUE_USE_TRANSFER
			#pragma multi_compile __ ALBEDO_USE_TRANSFER
			#pragma multi_compile __ SMOOTHNESS_USE_TRANSFER
			#pragma multi_compile __ METALLIC_USE_TRANSFER*/
			#define VOLUME_BACKFACE
			#include "Assets/VolumeRenderer/Shaders/raycastVolumeRendering.cginc"
			ENDCG
		}
		Pass{
			Tags{
				"LightMode" = "ShadowCaster"
			}

			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ GRADIENT_PRECALCULATED 
			#pragma multi_compile __ ALL_USE_TRANSFER
			/*
			#pragma multi_compile __ ISOVALUE_USE_TRANSFERR*/
			#include "Assets/VolumeRenderer/Shaders/raycastVolumeRenderingShadow.cginc"

			ENDCG
		}
	}
	CustomEditor "VolumetricRenderingEditor"
}