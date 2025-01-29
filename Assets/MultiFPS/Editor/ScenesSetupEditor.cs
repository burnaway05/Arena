using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace MultiFPS
{
    public class ScenesSetupEditor : Editor
    {



        public static void SetEditorBuildSettingsScenes(List<SceneAsset> m_SceneAssets)
        {
            // Find valid Scene paths and make a list of EditorBuildSettingsScene
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            foreach (var sceneAsset in m_SceneAssets)
            {
                string scenePath = AssetDatabase.GetAssetPath(sceneAsset);
                if (!string.IsNullOrEmpty(scenePath))
                    editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            }

            // Set the Build Settings window Scene list
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }
    }
}