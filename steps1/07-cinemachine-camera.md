# Step 07 — Cinemachine 3 Third Person Camera [Cursor Agent]

## Context
Game: "Paws Up!" — Unity 6 (6000.3.11f1), Cinemachine 3.1.2 already installed.
Step 7 of 21. Depends on: Step 03 (character controller).

IMPORTANT: This is Cinemachine 3 (namespace `Unity.Cinemachine`), NOT the legacy Cinemachine 2.x.

## Task
Create a CameraManager and configure third-person camera with three presets.

## 1. CameraManager.cs

Create `Assets/_PawsUp/Scripts/Camera/CameraManager.cs`:

```csharp
using UnityEngine;
using Unity.Cinemachine;

namespace PawsUp.CameraSystem
{
    public enum CameraPreset
    {
        Exploration, // outdoor, default
        Indoor,      // closer for small rooms
        Stealth      // higher, wider view
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

        public void SetPreset(CameraPreset preset)
        {
            _currentPreset = preset;

            // Higher priority = active camera. Base priority is 0.
            explorationCam.Priority = preset == CameraPreset.Exploration ? 10 : 0;
            indoorCam.Priority = preset == CameraPreset.Indoor ? 10 : 0;
            stealthCam.Priority = preset == CameraPreset.Stealth ? 10 : 0;
        }

        /// <summary>
        /// Call this after scene loads to re-bind cameras to the player.
        /// </summary>
        public void BindToTarget(Transform target)
        {
            if (explorationCam != null) explorationCam.Follow = target;
            if (indoorCam != null) indoorCam.Follow = target;
            if (stealthCam != null) stealthCam.Follow = target;
        }
    }
}
```

## 2. CameraPresetTrigger.cs

Create `Assets/_PawsUp/Scripts/Camera/CameraPresetTrigger.cs`:

```csharp
using UnityEngine;

namespace PawsUp.CameraSystem
{
    /// <summary>
    /// Place on trigger colliders to switch camera preset when Pierre enters/exits.
    /// </summary>
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
```

## 3. Unity Editor Setup (instructions for human in Step 21)

Create a camera rig in the Bootstrap scene or as a prefab:

### Main Camera
- The default `Main Camera` should have a `CinemachineBrain` component (Cinemachine 3 auto-adds this)

### Exploration Camera (outdoor, default)
1. GameObject → Cinemachine → Cinemachine Camera → name it `CM_Exploration`
2. In Inspector:
   - **Follow:** (will be set at runtime via CameraManager.BindToTarget)
   - **Body:** add `CinemachineThirdPersonFollow`
     - Shoulder Offset: (0.5, 0.5, 0)
     - Camera Distance: 4.0
     - Vertical Arm Length: 0.5
     - Camera Side: 0.6
     - Damping: (0.5, 0.5, 0.5)
   - **Aim:** add `CinemachineRotationComposer`
     - Tracked Object Offset: (0, 0.6, 0)
   - **Add Extension:** `CinemachineDeoccluder`
     - Strategy: Pull Camera Forward
     - Damping: 0.5
   - **Add Extension:** `CinemachineInputAxisController`
     - This reads Input System for look rotation

### Indoor Camera (closer for attic, shops)
1. Duplicate CM_Exploration → rename `CM_Indoor`
2. Change `CinemachineThirdPersonFollow`:
   - Camera Distance: 2.5
   - Vertical Arm Length: 0.3
   - Shoulder Offset: (0.3, 0.4, 0)
3. Priority: 0 (not active by default)

### Stealth Camera (higher, wider view)
1. Duplicate → rename `CM_Stealth`
2. Change:
   - Camera Distance: 6.0
   - Vertical Arm Length: 2.0
   - Shoulder Offset: (0, 1.0, 0)
3. Priority: 0

### Camera Manager GameObject
1. Create empty GO `--- CAMERA ---`
2. Add `CameraManager` component
3. Drag the 3 CinemachineCamera objects into the slots
4. Make this object DontDestroyOnLoad (handled in code)

### Player Tag
Make sure Pierre prefab has tag `Player` (for CameraPresetTrigger).

## Verification
- Camera follows Pierre smoothly
- Mouse rotates camera around Pierre
- Camera doesn't clip through walls (CinemachineDeoccluder)
- Calling `CameraManager.Instance.SetPreset(CameraPreset.Indoor)` switches to closer camera
- CameraPresetTrigger on a box collider switches camera when Pierre enters
