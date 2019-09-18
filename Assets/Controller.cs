using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Controller : MonoBehaviour {
    public Vector3 lookingAt;
    public float distance = 40;
    // Update is called once per frame
	void Update () {
	}

    private void LateUpdate() {
        var lookVector = transform.forward;
        var currentPosition = lookingAt - lookVector * distance;
        transform.position = currentPosition;
        if (Input.GetMouseButton(0)) {
            transform.rotation *= Quaternion.Euler(new Vector3(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")));
        }
    }
}
