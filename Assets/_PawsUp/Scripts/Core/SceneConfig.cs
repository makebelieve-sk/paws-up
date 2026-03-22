using UnityEngine;

namespace PawsUp.Core
{
    [CreateAssetMenu(fileName = "NewSceneConfig", menuName = "PawsUp/Scene Config")]
    public class SceneConfig : ScriptableObject
    {
        public string sceneName;
        public string displayName;

        [Header("Camera")]
        public string cameraPreset = "Exploration"; // Exploration, Indoor, Stealth

        [Header("Audio")]
        public AudioClip bgmClip;
        public AudioClip ambientClip;
        [Range(0f, 1f)] public float bgmVolume = 0.5f;
        [Range(0f, 1f)] public float ambientVolume = 0.3f;
    }
}
