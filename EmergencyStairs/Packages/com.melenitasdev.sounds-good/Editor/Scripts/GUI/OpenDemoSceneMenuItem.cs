using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using MelenitasDev.SoundsGood.Domain;

namespace MelenitasDev.SoundsGood.Editor
{
    public static class OpenDemoSceneMenuItem
    {
        [MenuItem("Tools/Melenitas Dev/Sounds Good/Open Demo Scene", priority = 100)]
        public static void OpenDemoShowcase ()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var demoAsset = AssetLocator.Instance.ShowcaseScene;
            if (demoAsset == null)
            {
                EditorUtility.DisplayDialog("Sounds Good", "Demo scene is not imported.", "OK");
                return;
            }

            string scenePath = AssetDatabase.GetAssetPath(demoAsset);
            if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("Sounds Good",
                    $"Demo scene asset exists but could not find it at path:\n{scenePath}\n", "OK");
                return;
            }

            EditorSceneManager.OpenScene(scenePath);
        }

        [MenuItem("Tools/Melenitas Dev/Sounds Good/Open Demo Occlusion Scene", priority = 101)]
        public static void OpenDemoOcclusion ()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var demoAsset = AssetLocator.Instance.OcclusionScene;
            if (demoAsset == null)
            {
                EditorUtility.DisplayDialog("Sounds Good", "Demo scene is not imported.", "OK");
                return;
            }

            string scenePath = AssetDatabase.GetAssetPath(demoAsset);
            if (string.IsNullOrEmpty(scenePath) || !File.Exists(scenePath))
            {
                EditorUtility.DisplayDialog("Sounds Good",
                    $"Demo scene asset exists but could not find it at path:\n{scenePath}\n", "OK");
                return;
            }

            EditorSceneManager.OpenScene(scenePath);
        }
    }
}