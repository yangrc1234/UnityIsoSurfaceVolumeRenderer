using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TotalControl : MonoBehaviour {
    public enum ControllingEnum {
        Volume,
        Camera,
        Light,
        SlicePlane
    }

    public void ResetCamera() {
        camera.transform.position = Vector3.back * 2;
        camera.transform.rotation = Quaternion.identity;
    }

    public ControllingEnum controlling { get { return _controllingEnum; }set {
            _controllingEnum = value;
            switch (_controllingEnum) {
                case ControllingEnum.Volume:
                    _controlling = volume;
                    break;
                case ControllingEnum.Camera:
                    _controlling = camera;
                    break;
                case ControllingEnum.Light:
                    _controlling = lightC;
                    break;
                case ControllingEnum.SlicePlane:
                    _controlling = slicePlane;
                    break;
                default:
                    break;
            }
        }
    }
    public UnityEngine.EventSystems.EventSystem eventSystem;
    public GameObject volume;
    public GameObject lightC;
    public GameObject camera;
    public GameObject slicePlane;
	// Use this for initialization
	void Start () {
        controlling =  ControllingEnum.Volume;
	}
    private ControllingEnum _controllingEnum;
    private GameObject _controlling;
    public float lightDistance;
    public bool autoRotate;
    public float rotateSpeed = 30;
    public bool autoRotateCamera;
    public float cameraRotateSpeed = 30;
    // Update is called once per frame
    void Update () {

        var horizontal = Camera.main.transform.right;
        var vertical = Vector3.up;

        if (Input.GetMouseButton(0) && !eventSystem.IsPointerOverGameObject()) {

            _controlling.transform.RotateAround(Vector3.zero,
                horizontal, 3 * Input.GetAxis("Mouse Y"));

            _controlling.transform.RotateAround(Vector3.zero,
                vertical, -3 * Input.GetAxis("Mouse X"));

        }
        if (autoRotate) {
            volume.transform.Rotate(rotateSpeed * Time.deltaTime * new Vector3(0, -1,0), Space.World);
        }

        if (autoRotateCamera) {
            camera.transform.RotateAround(volume.transform.position, Vector3.up, cameraRotateSpeed * Time.deltaTime);

        }

        if (Input.GetMouseButton(1)) {
            var nearest = volume.transform.position + (camera.transform.position - volume.transform.position).normalized * 0.5f;
            var dir = volume.transform.position - camera.transform.position;
            var currDistance = dir.magnitude;
            camera.transform.position =
                camera.transform.position + dir * Mathf.Min(currDistance - 0.2f, 5.0f * Time.deltaTime * Input.GetAxis("Mouse Y"));
            
        }
	}
}
