using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
public class ReadFileButton : MonoBehaviour {
    public InputField text;
    public void onClick() {
        SystemController.instance.ReadVolumeFromDescriptionFile(text.text);
    }
}
