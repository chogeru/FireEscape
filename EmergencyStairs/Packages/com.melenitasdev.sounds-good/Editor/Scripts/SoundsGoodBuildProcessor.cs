using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using MelenitasDev.SoundsGood.Domain;

namespace MelenitasDev.SoundsGood.Editor
{
    public class SoundsGoodBuildProcessor : IPreprocessBuildWithReport
    {
        // ----- Properties
        public int callbackOrder { get; } = 0;
    
        // ----- Public Methods
        public void OnPreprocessBuild (BuildReport report)
        {
            string result = $"[Sounds Good] Starting build... Searching references:\n" +
                            $"{AssetLocator.Instance.SoundDataCollection}, {AssetLocator.Instance.MusicDataCollection}, " +
                            $"{AssetLocator.Instance.OutputDataCollection}, {AssetLocator.Instance.SfxEnum}, " +
                            $"{AssetLocator.Instance.TracksEnum}, {AssetLocator.Instance.OutputsEnum}, " +
                            $"{AssetLocator.Instance.MasterAudioMixer}";
            Debug.Log(result);
        }
    }
}
