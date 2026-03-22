using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace PawsUp.CameraSystem
{
    public enum CameraPreset
    {
        Exploration,
        Indoor,
        Stealth
    }

    public class CameraManager : MonoBehaviour
    {
        public static CameraManager Instance { get; private set; }

        [Header("Camera References")]
        [SerializeField] private CinemachineCamera explorationCam;
        [SerializeField] private CinemachineCamera indoorCam;
        [SerializeField] private CinemachineCamera stealthCam;

        private CameraPreset _currentPreset = CameraPreset.Exploration;
        public CameraPreset CurrentPreset => _currentPreset;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetPreset(CameraPreset.Exploration);
        }

        #if UNITY_EDITOR
        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb.digit1Key.wasPressedThisFrame) SetPreset(CameraPreset.Exploration);
            if (kb.digit2Key.wasPressedThisFrame) SetPreset(CameraPreset.Indoor);
            if (kb.digit3Key.wasPressedThisFrame) SetPreset(CameraPreset.Stealth);
        }
        #endif

        public void SetPreset(CameraPreset preset)
        {
            _currentPreset = preset;

            explorationCam.Priority = preset == CameraPreset.Exploration ? 10 : 0;
            indoorCam.Priority = preset == CameraPreset.Indoor ? 10 : 0;
            stealthCam.Priority = preset == CameraPreset.Stealth ? 10 : 0;
        }

        public void BindToTarget(Transform target)
        {
            if (explorationCam != null) explorationCam.Follow = target;
            if (indoorCam != null) indoorCam.Follow = target;
            if (stealthCam != null) stealthCam.Follow = target;
        }
    }
}
