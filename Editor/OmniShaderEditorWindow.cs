//  Copyright (c) 2025-present amlovey
//  
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using OmniShader.Common.UI;

namespace OmniShader.Editor
{
    public class OmniShaderEditorWindow : EditorWindow
    {
        [MenuItem("Tools/Omni Shader")]
        private static void ShowWindow()
        {
            var window = GetWindow<OmniShaderEditorWindow>();
            window.titleContent = new GUIContent("OmniShader");
            window.Show();
            window.Focus();
        }

        [SerializeField]
        private OmniShaderEditorState appState;

        private OmniShaderEditor appEditor;

        private void OnEnable()
        {
            var root = rootVisualElement;
            var appStyle = UIExtensions.GetAppStylePath();
            root.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(appStyle));
            root.AddToClassList("os");

            if (appState == null)
            {
                appState = CreateInstance<OmniShaderEditorState>();
                appState.rulesStoreInPorject = OSBuildSetting.Load();
            }

            if (appEditor == null)
            {
                appEditor = new OmniShaderEditor(appState);
                root.Add(appEditor);
                root.schedule.Execute(() =>
                {
                    Theme.DetectTheme();
                    rootVisualElement.ToggleInClassList(EditorGUIUtility.isProSkin ? "pro" : "");
                });
            }
        }
    }
}