using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class HintView : MonoBehaviour {
    public UnityEngine.UI.Text text;
    private float lastTime;
    [SerializeField]
    private string _hintText;
    public Vector3 showPosition;
    public Vector3 hidePosition;
    public string hintText { set {
            lastTime = 3.0f;
            text.text = value;
        } }
    private void OnValidate() {
        hintText = _hintText;
    }
    // Update is called once per frame
    void Update () {
        lastTime -= Time.deltaTime;
        var trans = transform as RectTransform;
        if (lastTime > 0) {
            trans.anchoredPosition = Vector3.Lerp(trans.anchoredPosition, showPosition, 5 * Time.deltaTime);
        } else {
            trans.anchoredPosition = Vector3.Lerp(trans.anchoredPosition, hidePosition, 5 * Time.deltaTime);
        }
	}
}
