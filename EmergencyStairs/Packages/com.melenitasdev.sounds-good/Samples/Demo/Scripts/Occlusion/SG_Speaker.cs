using TMPro;
using UnityEngine;

namespace MelenitasDev.SoundsGood.Demo
{
    public class SG_Speaker : MonoBehaviour
    {
        // ----- Serialized Fields
        [SerializeField] private Track musicTrack;
        [SerializeField] private float speakerVolume = 0.5f;
        [SerializeField] private SG_Button switchOcclusionButton;
        [SerializeField] private TextMeshProUGUI[] occlusionStateLabel;
    
        // ----- Fields
        private Music music = new Music();
        private bool usingOcclusion = false;
    
        // ----- Unity Events
        void Start ()
        {
            music.SetClip(musicTrack).SetLoop().SetVolume(speakerVolume).SetPosition(transform.position)
                .SetSpatialSound().SetDopplerLevel(0).SetHearDistance(1f, 5f).Play();
            
            switchOcclusionButton.AddOnPressedListener(SwitchOcclusion);
        }
        
        // ----- Public Methods
        
        // ----- Private Methods
        private void SwitchOcclusion ()
        {
            usingOcclusion = !usingOcclusion;
            music.SetOcclusion(usingOcclusion).Play();
            
            foreach (var label in occlusionStateLabel)
            {
                label.text = $"Occlusion: {(usingOcclusion ? "On" : "Off")}";
            }
        }
    }
}
