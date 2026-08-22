using UnityEditor;
using UnityEngine;
using System.IO;
using MelenitasDev.SoundsGood.Domain;

namespace MelenitasDev.SoundsGood.Editor
{
    static class DefaultDataInitializer
    {
        // ----- Constants
        private const string LEGACY_USER_DATA_PATH = "Assets/SoundsGood/Data/";
        private const string DEFAULT_DATA_PATH = "Packages/com.melenitasdev.sounds-good/Runtime/Data/Default/";
        private const string SETTINGS_ASSET_PATH = "Packages/com.melenitasdev.sounds-good/Runtime/Resources/SoundsGoodSettings.asset";

        private static readonly string[] dataSubfolders = { "Collections", "Mixers", "Generated" };

        // ----- Fields
        private static bool subscribedToUpdate;
        private static bool enumsCreated;
        private static bool enumsInitialized;

        // ----- Properties
        private static SoundsGoodSettings Settings => AssetLocator.SoundsGoodSettings;

        // ----- Unity Events
        [InitializeOnLoadMethod]
        private static void OnInitialize ()
        {
            if (subscribedToUpdate) return;
            EditorApplication.update += OnEditorUpdate;
            subscribedToUpdate = true;
        }

        private static void OnEditorUpdate ()
        {
            EnsureSettingsAsset();
            EnsureUserDataRoot();
            CopyAllDefaultsToUserData();

            if (enumsCreated && !enumsInitialized)
            {
                WriteEnumsContent();
            }
        }

        // ----- Private Methods
        private static void EnsureSettingsAsset ()
        {
            if (Settings != null) return;
            
            var instance = ScriptableObject.CreateInstance<SoundsGoodSettings>();
            instance.ResetToDefaults();

            AssetDatabase.CreateAsset(instance, SETTINGS_ASSET_PATH);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private static string GetUserDataRootPath ()
        {
            string root = Settings.GetNormalizedDataRootPath();
            root = root.Trim().Replace("\\", "/");

            if (PathUtility.TrySanitizeDataRootPath(root, out var safe, out _)) return safe;
            
            safe = "Assets/SoundsGood/Data/";
            
            if (Settings == null) return safe;
            
            Settings.DataRootPath = safe;
            Settings.LastAppliedDataRootPath = safe;
            EditorUtility.SetDirty(Settings);
            AssetDatabase.SaveAssets();

            return safe;
        }

        private static string NormalizeAssetsPath (string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            string result = path.Trim().Replace("\\", "/");

            if (!result.StartsWith("Assets/"))
                result = "Assets/" + result.TrimStart('/');

            if (!result.EndsWith("/"))
                result += "/";

            return result;
        }

        private static void EnsureUserDataRoot ()
        {
            string currentRoot = GetUserDataRootPath();
            string legacyRoot = LEGACY_USER_DATA_PATH;

            bool currentHasData = HasAnyUserData(currentRoot);
            bool legacyHasData = HasAnyUserData(legacyRoot);
            bool settingsDirty = false;

            if (Settings == null)
            {
                if (currentHasData || !legacyHasData) return;
                
                MoveDataSubfolders(legacyRoot, currentRoot);
                TryDeleteFolderIfEmpty(legacyRoot);
                return;
            }

            string lastApplied = Settings.LastAppliedDataRootPath;
            bool hasLastApplied = !string.IsNullOrWhiteSpace(lastApplied);

            if (!hasLastApplied)
            {
                if (!currentHasData && legacyHasData)
                {
                    MoveDataSubfolders(legacyRoot, currentRoot);
                    TryDeleteFolderIfEmpty(legacyRoot);
                }

                Settings.LastAppliedDataRootPath = currentRoot;
                settingsDirty = true;
            }
            else
            {
                string normalizedLast = NormalizeAssetsPath(lastApplied);

                if (normalizedLast != currentRoot)
                {
                    bool lastHasData = HasAnyUserData(normalizedLast);

                    if (lastHasData)
                    {
                        MoveDataSubfolders(normalizedLast, currentRoot);
                        TryDeleteFolderIfEmpty(normalizedLast);
                    }
                    else if (!currentHasData && legacyHasData)
                    {
                        MoveDataSubfolders(legacyRoot, currentRoot);
                        TryDeleteFolderIfEmpty(legacyRoot);
                    }
                }
                else
                {
                    if (!currentHasData && legacyHasData)
                    {
                        MoveDataSubfolders(legacyRoot, currentRoot);
                        TryDeleteFolderIfEmpty(legacyRoot);
                    }
                }

                if (Settings.LastAppliedDataRootPath != currentRoot)
                {
                    Settings.LastAppliedDataRootPath = currentRoot;
                    settingsDirty = true;
                }
            }

            if (settingsDirty) EditorUtility.SetDirty(Settings);
        }

        private static bool HasAnyUserData (string rootPath)
        {
            string normalizedRoot = NormalizeAssetsPath(rootPath);
            if (!AssetDatabase.IsValidFolder(normalizedRoot)) return false;

            foreach (var folder in dataSubfolders)
            {
                string subfolder = normalizedRoot + folder;
                if (!AssetDatabase.IsValidFolder(subfolder)) continue;

                string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { subfolder });
                if (guids != null && guids.Length > 0) return true;
            }

            return false;
        }

        private static void CopyAllDefaultsToUserData ()
        {
            bool refreshAssets = false;

            string userDataRoot = GetUserDataRootPath();

            EnsureFolderHierarchy(userDataRoot.TrimEnd('/'));

            string collectionsPath = userDataRoot + "Collections";
            if (!AssetDatabase.IsValidFolder(collectionsPath))
            {
                EnsureFolderHierarchy(collectionsPath);
                CopyAssetIfNotExists(
                    DEFAULT_DATA_PATH + "Collections/DefaultSoundCollection.asset",
                    userDataRoot + "Collections/SoundCollection.asset");
                CopyAssetIfNotExists(
                    DEFAULT_DATA_PATH + "Collections/DefaultMusicCollection.asset",
                    userDataRoot + "Collections/MusicCollection.asset");
                CopyAssetIfNotExists(
                    DEFAULT_DATA_PATH + "Collections/DefaultOutputCollection.asset",
                    userDataRoot + "Collections/OutputCollection.asset");

                refreshAssets = true;
            }

            string mixersPath = userDataRoot + "Mixers";
            if (!AssetDatabase.IsValidFolder(mixersPath))
            {
                EnsureFolderHierarchy(mixersPath);
                CopyAssetIfNotExists(
                    DEFAULT_DATA_PATH + "Mixers/DefaultMaster.mixer",
                    userDataRoot + "Mixers/Master.mixer");

                refreshAssets = true;
            }

            string generatedPath = userDataRoot + "Generated";
            if (!AssetDatabase.IsValidFolder(generatedPath))
            {
                EnsureFolderHierarchy(generatedPath);

                File.WriteAllText(userDataRoot + "Generated/SFX_Generated.cs", string.Empty);
                File.WriteAllText(userDataRoot + "Generated/Track_Generated.cs", string.Empty);
                File.WriteAllText(userDataRoot + "Generated/Output_Generated.cs", string.Empty);

                CopyAssetIfNotExists(DEFAULT_DATA_PATH + "Generated/SoundsGood.Application.asmref",
                    userDataRoot + "Generated/SoundsGood.Application.asmref");

                enumsInitialized = false;
                enumsCreated = true;

                refreshAssets = true;
            }

            if (!refreshAssets) return;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            PrintResult();
        }

        private static void CopyAssetIfNotExists (string sourcePath, string destPath)
        {
            string fullDestPath = Path.Combine(Application.dataPath, "../", destPath);
            if (File.Exists(fullDestPath)) return;

            if (AssetDatabase.CopyAsset(sourcePath, destPath))
            {
                Debug.Log($"[SoundsGood] Copied: {destPath}");
                return;
            }

            Debug.LogError($"[SoundsGood] Failed to copy {sourcePath} to {destPath}");
        }

        private static void EnsureFolderHierarchy (string assetFolderPath)
        {
            if (string.IsNullOrWhiteSpace(assetFolderPath))
                return;

            string formatted = assetFolderPath.Trim().Replace("\\", "/");
            string[] segments = formatted.Split('/');

            if (segments.Length == 0 || segments[0] != "Assets")
            {
                Debug.LogError(
                    $"[SoundsGood] EnsureFolderHierarchy only supports paths under 'Assets/': {assetFolderPath}");
                return;
            }

            string currentPath = "Assets";
            for (int i = 1; i < segments.Length; i++)
            {
                if (string.IsNullOrEmpty(segments[i]))
                    continue;

                string nextPath = currentPath + "/" + segments[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, segments[i]);

                currentPath = nextPath;
            }
        }

        private static void MoveDataSubfolders (string sourceRoot, string destinationRoot)
        {
            string srcRoot = NormalizeAssetsPath(sourceRoot);
            string dstRoot = NormalizeAssetsPath(destinationRoot);

            foreach (var sub in dataSubfolders)
            {
                string sourceFolder = srcRoot + sub;
                if (!AssetDatabase.IsValidFolder(sourceFolder)) continue;

                string destFolder = dstRoot + sub;
                EnsureFolderHierarchy(destFolder);

                string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { sourceFolder });
                foreach (var guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (AssetDatabase.IsValidFolder(assetPath)) continue;

                    string fileName = Path.GetFileName(assetPath);
                    string newPath = destFolder.TrimEnd('/') + "/" + fileName;

                    if (assetPath == newPath) continue;

                    string fullDestPath = Path.Combine(Application.dataPath, "../", newPath);
                    if (File.Exists(fullDestPath)) continue;

                    string error = AssetDatabase.MoveAsset(assetPath, newPath);
                    if (!string.IsNullOrEmpty(error))
                        Debug.LogError($"[SoundsGood] Error moving asset from {assetPath} to {newPath}: {error}");
                }

                TryDeleteFolderIfEmpty(sourceFolder);
            }
            
            enumsInitialized = false;
            enumsCreated = true;
            PrintResult();
        }

        private static void TryDeleteFolderIfEmpty (string folderPath)
        {
            string normalized = NormalizeAssetsPath(folderPath);
            if (!AssetDatabase.IsValidFolder(normalized))
                return;

            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { normalized });
            if (guids != null && guids.Length > 0)
                return;

            AssetDatabase.DeleteAsset(normalized);
        }

        private static void WriteEnumsContent ()
        {
            if (AssetLocator.Instance == null ||
                AssetLocator.Instance.SoundDataCollection == null ||
                AssetLocator.Instance.MusicDataCollection == null ||
                AssetLocator.Instance.OutputDataCollection == null ||
                AssetLocator.Instance.SfxEnum == null ||
                AssetLocator.Instance.TracksEnum == null ||
                AssetLocator.Instance.OutputsEnum == null)
            {
                return;
            }

            EditorHelper.SaveCollectionChanges(Sections.Sounds, false);
            EditorHelper.SaveCollectionChanges(Sections.Music, false);
            EditorHelper.ReloadOutputsDatabase(false);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            enumsInitialized = true;
        }

        private static void PrintResult ()
        {
            string result = $"[Sounds Good] Data moved or created in {AssetLocator.SoundsGoodSettings.DataRootPath}\n" +
                            $"Content: {AssetLocator.Instance.SoundDataCollection}, {AssetLocator.Instance.MusicDataCollection}, " +
                            $"{AssetLocator.Instance.OutputDataCollection}, {AssetLocator.Instance.SfxEnum}, " +
                            $"{AssetLocator.Instance.TracksEnum}, {AssetLocator.Instance.OutputsEnum}, " +
                            $"{AssetLocator.Instance.MasterAudioMixer}";
            Debug.Log(result);
        }
    }
}