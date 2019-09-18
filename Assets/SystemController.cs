using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class SystemController : MonoBehaviour {
    public TotalControl totalControl;
    public HintView hint;
    public SlicePlaneBehaviour slicePlaneBehaviour;
    [HideInInspector]
    public bool isLoadingFile;
    private static SystemController _instance;
    public DescriptionFileReader reader;
    public Material mat;
    public static SystemController instance {
        get {
            
            if (_instance == null) {
                _instance = FindObjectOfType<SystemController>();
            }
            return _instance;
        }
    }
    public void ReadVolumeFromDescriptionFile(string desctiptionFilePath) {
        if (isLoadingFile) {
            hint.hintText = "Please wait until current loading progress finish!";
            return;
        }
        try {
            reader._headFilePath = desctiptionFilePath;
            reader.ImportTexture();
            isLoadingFile = true;
            StartCoroutine(WaitForReadFinish());
        } catch (UnityException e) {
            hint.hintText = e.Message;
            throw;
        }
    }

    private IEnumerator WaitForReadFinish() {
        yield return new WaitUntil(() => reader.finishedReading);
        hint.hintText = "File Loading Finished";
        var tex = reader.tex;
        //try set albedo tex
        var albedoTex = reader.GetAlbedoTransfer();
        if (albedoTex != null)
            mat.SetTexture("_AlbedoIsoTransfer", albedoTex);
        //try set metallic tex
        var metallicTex = reader.GetMetallicTransfer();
        if (metallicTex!= null)
            mat.SetTexture("_PhysicsTransfer", metallicTex);
        mat.SetFloat("_GradientScale", reader.desc.gradientScale);
        mat.SetTexture("_VolumeTex", tex);
        yield return new WaitUntil(() => reader.finishedGradientCalculating);
        hint.hintText = "Gradient Calculation Finished";
        var gtex = reader.gradientTex;
        mat.SetTexture("_GradientTex", gtex);
        isLoadingFile = false;
    }

#if UNITY_EDITOR
 /*   private void OnGUI() {
        if (GUILayout.Button("Save Volume")) {
            SaveVolumeTex();
        }
        if (GUILayout.Button("Save Gradient")) {
            SaveGradient();
        }
    }
    public void SaveVolumeTex() {
        var t = reader.tex;
        AssetDatabase.CreateAsset(t, "Assets/Volume.asset");
    }
    public void SaveGradient() {
        var t = reader.gradientTex;
        AssetDatabase.CreateAsset(t, "Assets/Gradient.asset");
    }*/
#endif
}