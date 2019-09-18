using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideUIButtonView : MonoBehaviour {
    public CanvasGroup cg;
    public GameObject[] toset;
    public bool currentStatus = true;
    public void onClick() {
        if (currentStatus) {
            cg.alpha = 0.1f;
            foreach (var item in toset) {
                item.SetActive(false);
            }
        } else {
            cg.alpha = 1.0f;
            foreach (var item in toset) {
                item.SetActive(true);
            }
        }
        currentStatus = !currentStatus;
    }
}
