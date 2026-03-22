# Step 01 — Project Setup & Structure [Cursor Agent]

## Context
Game: "Paws Up!" (Лапки вверх!) — 3D detective adventure, Unity 6 (6000.3.11f1), URP.
This is step 1 of 21. We're setting up the project foundation.

## Task
Create the folder structure, GameManager singleton, and Bootstrap scene for the Paws Up project.
Do NOT delete or modify existing StarterAssets files — they serve as reference.

## 1. Create Folder Structure

Create the following folders under `Assets/_PawsUp/`:

```
Assets/_PawsUp/
  Scripts/
    Core/
    Player/
    Camera/
    Interaction/
    Inventory/
    Dialogue/
    Quest/
    SmellSense/
    UI/
    Audio/
    NPC/
    SceneManagement/
  Prefabs/
    Player/
    NPC/
    UI/
    Environment/
  ScriptableObjects/
    Items/
    Quests/
  Scenes/
  Materials/
  UI/
    Sprites/
    Fonts/
  Audio/
    BGM/
    SFX/
    Ambient/
  VFX/
  Data/
    Dialogues/
  Models/
  Animations/
```

## 2. Assembly Definition

Create `Assets/_PawsUp/Scripts/PawsUp.asmdef`:

```json
{
    "name": "PawsUp",
    "rootNamespace": "PawsUp",
    "references": [
        "Unity.InputSystem",
        "Unity.TextMeshPro",
        "Unity.Cinemachine"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": false,
    "precompiledReferences": [],
    "autoReferenced": true,
    "defineConstraints": [],
    "versionDefines": [],
    "noEngineReferences": false
}
```

## 3. GameManager.cs

Create `Assets/_PawsUp/Scripts/Core/GameManager.cs`:

```csharp
using UnityEngine;

namespace PawsUp.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Dialogue,
        Cutscene,
        Investigation, // smell sense active
        Inventory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameState _currentState = GameState.Playing;
        public GameState CurrentState => _currentState;

        public event System.Action<GameState, GameState> OnGameStateChanged;

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

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;
            var oldState = _currentState;
            _currentState = newState;
            OnGameStateChanged?.Invoke(oldState, newState);
        }

        public bool IsPlaying => _currentState == GameState.Playing;
        public bool IsInDialogue => _currentState == GameState.Dialogue;
        public bool IsPaused => _currentState == GameState.Paused;
    }
}
```

## 4. Bootstrap Scene

Create a new scene `Assets/_PawsUp/Scenes/Bootstrap.unity` (empty scene).

In this scene, create a single GameObject called `--- MANAGERS ---` with:
- `GameManager` component attached
- This object should persist across scenes (DontDestroyOnLoad is handled in code)

Later steps will add more manager components (InventoryManager, QuestManager, etc.) to this same object.

## 5. SceneConfig ScriptableObject

Create `Assets/_PawsUp/Scripts/Core/SceneConfig.cs`:

```csharp
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
```

## Verification
- Project compiles with no errors
- `Assets/_PawsUp/` folder structure exists
- `GameManager.cs` and `SceneConfig.cs` compile
- Bootstrap scene can be opened and contains the Managers object
