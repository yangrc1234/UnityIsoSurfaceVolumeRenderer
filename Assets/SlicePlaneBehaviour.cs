using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Specialized;
public class SlicePlaneBehaviour : MonoBehaviour {
    public Material mat;
    private int id;
    public float distance;
    private void Start() {
        id = Shader.PropertyToID("_SplitPlane");        //don't mind the name, it's same.
        this.transform.forward = mat.GetVector(id);
        this.distance = mat.GetVector(id).w;
    }
    private void Update() {
        var normal =  transform.forward;
        mat.SetVector(id, new Vector4(normal.x, normal.y, normal.z, distance));
    }
}
