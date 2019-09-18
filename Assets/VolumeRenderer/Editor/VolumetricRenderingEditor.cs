using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class VolumetricRenderingEditor : ShaderGUI {

    Material target;
    MaterialEditor editor;
    MaterialProperty[] properties;

    void SetKeyword(string keyword, bool state) {
        if (state) {
            target.EnableKeyword(keyword);
        } else {
            target.DisableKeyword(keyword);
        }
    }
    bool IsKeywordEnabled(string keyword) {
        return target.IsKeywordEnabled(keyword);
    }
    MaterialProperty FindProperty(string name) {
        return FindProperty(name, properties);
    }
    static GUIContent staticLabel = new GUIContent();
    static GUIContent MakeLabel(string text, string tooltip = null) {
        staticLabel.text = text;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }
    static GUIContent MakeLabel(MaterialProperty property, string tooltip = null) {
        staticLabel.text = property.displayName;
        staticLabel.tooltip = tooltip;
        return staticLabel;
    }
    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties) {
        this.editor = materialEditor;
        this.properties = properties;
        this.target = editor.target as Material;
        DoMain();
    }

    private void DoTransfer() {
        editor.TexturePropertySingleLine(new GUIContent("Albedo"), FindProperty("_AlbedoIsoTransfer"));
        editor.TexturePropertySingleLine(new GUIContent("Metallic"), FindProperty("_PhysicsTransfer"), FindProperty("_Metallic"));
        editor.ShaderProperty(FindProperty("_Smoothness"), "Smoothness");

    }

    public enum UseTransferEnum {
        UseTransferTexture,
        UseConstantValue
    }
    private bool transferFold = false;
    private void DoMain() {
        EditorGUILayout.LabelField("Volume Configuration",new GUIStyle() { fontStyle= FontStyle.Bold});
        editor.TexturePropertySingleLine(
            new GUIContent("Volume Texture", "the volume texture(R)"),
            FindProperty("_VolumeTex"),
            FindProperty("_VisibleIsoValue"));

        EditorGUI.BeginChangeCheck();
        bool precalculategradient = EditorGUILayout.Toggle("Precalculate gradient",IsKeywordEnabled("GRADIENT_PRECALCULATED"));
        if (EditorGUI.EndChangeCheck()) {
            SetKeyword("GRADIENT_PRECALCULATED", precalculategradient);
        }
        if (precalculategradient) {
            editor.TexturePropertySingleLine(
                new GUIContent("Gradient Texture", "the gradient texture"),
                FindProperty("_GradientTex"),
                FindProperty("_GradientScale"));
        } else {
            editor.ShaderProperty(FindProperty("_GradientScale"),"Gradient Scale");
        }
        editor.ShaderProperty(FindProperty("_SplitPlane"), "Split Plane");

        EditorGUILayout.LabelField("Shading Configuration", new GUIStyle() { fontStyle = FontStyle.Bold });
        editor.ShaderProperty(FindProperty("_Albedo"), "Tint");
        editor.ShaderProperty(FindProperty("_AmbientColor"), "Ambient Light");
        EditorGUI.BeginChangeCheck();

        bool res = EditorGUILayout.Toggle(new GUIContent("Use Transfer Texture"), IsKeywordEnabled("ALL_USE_TRANSFER"));
        if (EditorGUI.EndChangeCheck()) {
            SetKeyword("ALL_USE_TRANSFER", res);
        }
        if (IsKeywordEnabled("ALL_USE_TRANSFER")) {
            DoTransfer();
        } else {
            DoNormal();
        }

        EditorGUI.BeginChangeCheck();
        var AOCheck = IsKeywordEnabled("AMBIENT_OCCULUSION_ON");
        AOCheck = EditorGUILayout.Toggle(new GUIContent("Enable AO"), AOCheck);
        if (EditorGUI.EndChangeCheck()) {
            SetKeyword("AMBIENT_OCCULUSION_ON", AOCheck);
        }
        EditorGUI.BeginChangeCheck();
        bool transcluency = EditorGUILayout.Toggle(new GUIContent("Enable Transmittance"), IsKeywordEnabled("TRANSCLUENCY_ON"));
        if (EditorGUI.EndChangeCheck()) {
            SetKeyword("TRANSCLUENCY_ON", transcluency);
        }
        if (IsKeywordEnabled("TRANSCLUENCY_ON")) {
            EditorGUI.indentLevel++;
            editor.ShaderProperty(FindProperty("_Transcluency"), new GUIContent("Transmittance"));
            EditorGUI.indentLevel--;
        }
    }

    private void DoNormal() {
        editor.ShaderProperty(FindProperty("_Metallic"), new GUIContent("Metallic"));
        editor.ShaderProperty(FindProperty("_Smoothness"), "Smoothness");
    }
}
