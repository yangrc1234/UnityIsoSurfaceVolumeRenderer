using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ControlPanelView : MonoBehaviour {
    public void setSlicePlaneDistance(float val) {
            SystemController.instance.slicePlaneBehaviour.distance = val;
    }
    public void resetCamera() {
        SystemController.instance.totalControl.ResetCamera();
    }
    public void setControlling(int val) {
        SystemController.instance.totalControl.controlling = (TotalControl.ControllingEnum)val;
    }
    public void setVolumeRotateSpeed(string str) {
        float res;
        if (float.TryParse(str,out res)) {
            SystemController.instance.totalControl.rotateSpeed = res;
        }
    }
    public void setCameraRotateSpeed(string str) {
        float res;
        if (float.TryParse(str, out res)) {
            SystemController.instance.totalControl.cameraRotateSpeed= res;
        }
    }
    public void setCameraRotate(bool val) {
        SystemController.instance.totalControl.autoRotateCamera = val;
    }
    public void setVolumeRotate(bool val) {
        SystemController.instance.totalControl.autoRotate = val;
    }
    public void setLightIntensity(string str) {
        float res;
        if (float.TryParse(str, out res)) {
            SystemController.instance.totalControl.lightC.GetComponent<Light>().intensity = res;
        }
    }
}
