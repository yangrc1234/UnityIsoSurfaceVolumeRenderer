using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
[ExecuteInEditMode]
public class RaycastVolumeRenderer : MonoBehaviour {

    private void OnWillRenderObject() {
        var PVM = (Camera.current.projectionMatrix * Camera.current.worldToCameraMatrix * transform.localToWorldMatrix).inverse;
        Shader.SetGlobalMatrix("VolumeClipToObject", PVM);
    }
}
