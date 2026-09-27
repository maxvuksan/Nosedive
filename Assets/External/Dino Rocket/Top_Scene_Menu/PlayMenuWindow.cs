#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DinoRocket.Tools
{
    [InitializeOnLoad]
    public static class PlayMenuWindow
    {
        static PlayMenuWindow()
        {
            UnityToolbarExtender.ToolbarExtender.LeftToolbarGUI.Add(OnLeftToolbarGUI);
            UnityToolbarExtender.ToolbarExtender.RightToolbarGUI.Add(OnRightToolbarGUI);
        }

        static void OnLeftToolbarGUI()
        {
            using (new EditorGUI.DisabledScope(EditorApplication.isCompiling || EditorApplication.isPlaying))
            {
                DrawPlayFromFirstSceneButton();
                GUILayout.Space(10f);
            }
        }

        static void OnRightToolbarGUI()
        {
            GUILayout.Space(10f);

            using (new EditorGUI.DisabledScope(EditorApplication.isCompiling || EditorApplication.isPlaying))
            {
                DrawBuildScenesButtons();
            }
        }

        static void DrawBuildScenesButtons()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                GUILayout.Label("No scenes in Build Settings");
                return;
            }

            for (int i = 0; i < scenes.Length; i++)
            {
                var s = scenes[i];
                if (s == null || string.IsNullOrWhiteSpace(s.path) || s.enabled == false)
                    continue;

                string label = Path.GetFileNameWithoutExtension(s.path);
                if (!s.enabled) label = $"({label})";

                if (GUILayout.Button(label, GUILayout.MinWidth(70f)))
                    OpenSceneWithSavePrompt(s.path);
            }
        }

        static void DrawPlayFromFirstSceneButton()
        {
            var scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0) return;

            string first = scenes[0].path;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                {
                    first = scenes[i].path;
                    break;
                }
            }

            if (GUILayout.Button("PLAY", GUILayout.Width(90f)))
            {
                if (!OpenSceneWithSavePrompt(first))
                    return;
                EditorApplication.EnterPlaymode();
            }
        }

        static bool OpenSceneWithSavePrompt(string scenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            if (string.IsNullOrWhiteSpace(scenePath))
                return false;

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            return true;
        }
    }
}
#endif