using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialPanelView : MonoBehaviour {

    public void SetUseTransferTexture(bool val) {
        if (val) {
            SystemController.instance.mat.EnableKeyword("ALL_USE_TRANSFER");
        } else {
            SystemController.instance.mat.DisableKeyword("ALL_USE_TRANSFER");
        }
    }
    public void SetGradientScale(string str) {
        float res;
        if (float.TryParse(str,out res)) {
            SystemController.instance.mat.SetFloat("_GradientScale", res);
        }
    }

    public void SetIsoValue(float value) {
        SystemController.instance.mat.SetFloat("_VisibleIsoValue", value);
    }

    public void SetMetallic(float value) {
        SystemController.instance.mat.SetFloat("_Metallic", value);
    }

    public void SetSmoothenss(float value) {
        SystemController.instance.mat.SetFloat("_Smoothness", value);
    }

    public void SetAO(bool val) {
        if (val) {
            SystemController.instance.mat.EnableKeyword("AMBIENT_OCCULUSION_ON");
        } else {
            SystemController.instance.mat.DisableKeyword("AMBIENT_OCCULUSION_ON");
        }
    }
    public void SetGradientPrecalculated(bool val) {
        if (val) {
            SystemController.instance.mat.EnableKeyword("GRADIENT_PRECALCULATED");
        } else {
            SystemController.instance.mat.DisableKeyword("GRADIENT_PRECALCULATED");
        }
    }
    public void SetTransmittance(bool val) {
        if (val) {
            SystemController.instance.mat.EnableKeyword("TRANSCLUENCY_ON");
        } else {
            SystemController.instance.mat.DisableKeyword("TRANSCLUENCY_ON");
        }
    }
    public void SetTransmittanceFactor(float value) {
        SystemController.instance.mat.SetFloat("_Transcluency", value);
    }
}
