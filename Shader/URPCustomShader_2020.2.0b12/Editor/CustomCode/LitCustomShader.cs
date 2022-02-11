using System;
using UnityEngine;

namespace UnityEditor.Rendering.Universal.ShaderGUI.Custom
{
    internal class LitCustomShader : LitShader
    {
        MaterialProperty uvSetSecondary = null;
        MaterialProperty onlyShadows = null;
        MaterialProperty onlyShadowColor = null;

        private static class StylesCustom
        {
            public static GUIContent uvSetLabel = EditorGUIUtility.TrTextContent("UV Set", "OcclusionMap, DetailInput Map : UV Channel choice..");
            public static GUIContent onlyShadows = EditorGUIUtility.TrTextContent("Only Shadows", "Only draw Shadows : 0 don't draw mesh");
            public static GUIContent onlyShadowColor = EditorGUIUtility.TrTextContent("Only ShadowColor", "Only draw Shadows Color");
        }

        public override void FindProperties(MaterialProperty[] props)
        {
            base.FindProperties(props);

            uvSetSecondary = FindProperty("_UVSec", props);
            onlyShadows = FindProperty("_OnlyShadows", props);
            onlyShadowColor = FindProperty("_OnlyShadowColor", props);
        }

        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditorIn, properties);

            GUILayout.Label("-- Custom Options --", EditorStyles.boldLabel);
            materialEditorIn.ShaderProperty(uvSetSecondary, StylesCustom.uvSetLabel.text);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Only Shadows");
            onlyShadows.floatValue = EditorGUILayout.Slider(onlyShadows.floatValue, 0f, 10f);
            onlyShadowColor.colorValue = EditorGUILayout.ColorField(onlyShadowColor.colorValue);
            GUILayout.EndHorizontal();
        }
    }
}
