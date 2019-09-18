using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
[ExecuteInEditMode]
public class DOFAdjuster : MonoBehaviour {
    private DepthOfField dof;
	// Use this for initialization
	void Start () {
        dof = GetComponent<DepthOfField>();
	}
	
	// Update is called once per frame
	void Update () {
        dof.focalLength = transform.position.magnitude;
	}
}
