using UnityEngine;

namespace PawsUp.CameraSystem
{
    public class CameraPresetTrigger : MonoBehaviour
    {
        [SerializeField] private CameraPreset enterPreset = CameraPreset.Indoor;
        [SerializeField] private CameraPreset exitPreset = CameraPreset.Exploration;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && CameraManager.Instance != null)
                CameraManager.Instance.SetPreset(enterPreset);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && CameraManager.Instance != null)
                CameraManager.Instance.SetPreset(exitPreset);
        }
    }
}
