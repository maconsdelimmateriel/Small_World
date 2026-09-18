# Small World

A VRChat world (Unity/UdonSharp) set on a small orbiting asteroid. Players keep a central heater fueled to trigger seasonal changes on the asteroid, catch smaller asteroids with a fishing rod to use as fuel, and talk to alien NPCs whose dialog changes with the season and can be read in French or English.

## Key Features

- **Seasonal environment change** — [Heater.cs](Assets/Scripts/Heater.cs) accumulates fuel over time and, once a fuel threshold is reached, triggers [AsteroidSeasonChange.cs](Assets/Scripts/AsteroidSeasonChange.cs), which sweeps a shader-driven transition across the asteroid's material (`_Center` / `_Radius` shader properties) from an origin point and grows a sequence of plant animations.
- **Asteroid fishing** — [FishingRod.cs](Assets/Scripts/Fishing/FishingRod.cs) lets a player cast a line (with an arc trajectory), reel it in, and catch a small asteroid via [HookAttractor.cs](Assets/Scripts/Fishing/HookAttractor.cs), which magnetically pulls a nearby [SmallAsteroid.cs](Assets/Scripts/Asteroids/SmallAsteroid.cs) toward the hook.
- **Orbiting asteroid field** — [SmallAsteroidsManager.cs](Assets/Scripts/Asteroids/SmallAsteroidsManager.cs) activates asteroids from a pool at a fixed interval; each [SmallAsteroid.cs](Assets/Scripts/Asteroids/SmallAsteroid.cs) follows a randomized elliptical orbit computed deterministically from VRChat server time (so orbits stay in sync for all clients without per-frame position syncing).
- **Fuel loop** — once caught and reeled in, an asteroid is swapped for a [SmallAsteroidFuelPrefab.cs](Assets/Scripts/Asteroids/SmallAsteroidFuelPrefab.cs) instance, which can be carried to the Heater to add fuel. Consuming it is deliberately *not* networked via `SendCustomNetworkEvent` (see Known Issues) — each client destroys its own local copy independently when its own `OnTriggerEnter` fires.
- **Alien dialog system** — [Alien.cs](Assets/Scripts/Alien/Alien.cs) drives an interactable NPC with a dialog canvas, season-filtered lines, a French/English toggle, and Next/Close/Translate buttons ([AlienButtonNext.cs](Assets/Scripts/Alien/AlienButtonNext.cs), [AlienButtonClose.cs](Assets/Scripts/Alien/AlienButtonClose.cs), [AlienButtonTranslate.cs](Assets/Scripts/Alien/AlienButtonTranslate.cs)). A [AlienButtonTestNextSeason.cs](Assets/Scripts/Alien/AlienButtonTestNextSeason.cs) button exists to manually cycle seasons for testing. Dialog lines are authored as [DialogLine.cs](Assets/Scripts/Alien/DialogLine.cs) `ScriptableObject` assets (see [Assets/DialogLines/Musica/](Assets/DialogLines/Musica)), keyed by speaker, season, and language.
- **Fall-off respawn** — [RespawnNet.cs](Assets/Scripts/Other/RespawnNet.cs) teleports a player back to a spawn point when they enter a trigger volume (e.g. a net/void catcher below the world).
- **Networked state** — gameplay state (fuel counters, dialog canvas state, asteroid orbit parameters, catch state) uses `UdonSynced` fields with manual `RequestSerialization()` calls and `SendCustomNetworkEvent` for cross-client event calls, per VRChat's networking model. Exception: [SmallAsteroidFuelPrefab.cs](Assets/Scripts/Asteroids/SmallAsteroidFuelPrefab.cs)'s destroy-on-heater-contact logic avoids `SendCustomNetworkEvent` entirely — see Known Issues.

## Requirements

- **Unity Editor 2022.3.22f1** (see [ProjectSettings/ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)) — use this exact version to avoid asset/serialization drift.
- **VRChat Creator Companion (VCC)** to manage the VPM packages below (see [Packages/vpm-manifest.json](Packages/vpm-manifest.json)):

  | Package | Version |
  |---|---|
  | `com.vrchat.worlds` (VRChat Worlds SDK3, bundles UdonSharp) | 3.10.5 |
  | `com.vrchat.base` | 3.10.5 |
  | `com.vrchat.clientsim` | 1.2.7 |
  | `com.vrchat.udonsharp` | 1.1.9 |
  | `com.vrchat.core.vpm-resolver` | 0.1.29 |
  | `dev.onevr.vrworldtoolkit` (VRWorld Toolkit) | 3.4.1 |
  | `vrchat.jordo.easyquestswitch` (Easy Quest Switch) | 1.4.0 |

- Relevant built-in Unity packages (see [Packages/manifest.json](Packages/manifest.json)): TextMeshPro 2.1.6, Timeline 1.7.5, uGUI 1.0.0, Test Framework 1.1.29, plus the standard XR/physics/UI modules and the Linux IL2CPP toolchain (`com.unity.toolchain.linux-x86_64`).

## Installation / Setup

1. Install [VRChat Creator Companion](https://vcc.docs.vrchat.com/) and Unity **2022.3.22f1** (matching the version above) via Unity Hub.
2. Clone this repository and add it to VCC as an existing project (or open it directly in Unity if VCC packages are already resolved under `Packages/`).
3. Let VCC/Unity Package Manager resolve the VPM packages listed above.
4. Open the working scene: [Assets/Scenes/NoAssets_VRCDefaultWorldScene 1.unity](<Assets/Scenes/NoAssets_VRCDefaultWorldScene 1.unity>) — **not** `VRCDefaultWorldScene.unity`, which is an empty default template left over from world creation (5 GameObjects, no gameplay content). [NEEDS REVIEW: confirm which scene is intended to ship — the emptier one may be stale and worth removing or renaming to avoid confusion.]
5. On first open, VRChat's UdonSharp compiler should run automatically and (re)generate the Udon program assets under `Assets/Scenes/..._UdonProgramSources/`.

### Prefabs / references that must be wired up

These scripts expect specific references to be assigned in the Inspector — missing links will cause `NullReferenceException`s at runtime:

- **Heater** ([Assets/Prefabs](Assets/Prefabs) has no Heater prefab; it exists directly in-scene) needs its `AsteroidSeasonChange` reference and UI `Slider` assigned.
- **AsteroidSeasonChange** needs a `targetRenderer` (asteroid map material) and `changeOrigin` transform, plus its `_plantAnimators` array populated.
- **FishingRod** (prefab: [Assets/Prefabs/FishingRod.prefab](Assets/Prefabs/FishingRod.prefab)) needs `_rodTip`, `_hook`, `lineRenderer`, and `_asteroidFuelPrefab` (→ [Assets/Prefabs/SmallAsteroidFuelPrefab.prefab](Assets/Prefabs/SmallAsteroidFuelPrefab.prefab)) assigned.
- **HookAttractor** needs a `_rod` reference back to its `FishingRod`.
- **SmallAsteroidsManager** needs its `_asteroidPool` array populated with [Assets/Prefabs/SmallAsteroidPrefab.prefab](Assets/Prefabs/SmallAsteroidPrefab.prefab) instances.
- **Alien** needs `_canvas` and `_text` (TextMeshProUGUI) assigned, plus `lineFrench` / `lineEnglish` / `season` string arrays populated in parallel (see Known Issues — this is currently a manual/parallel-array setup even though `DialogLine` scriptable objects also exist).
- **AlienButtonNext / AlienButtonClose / AlienButtonTranslate / AlienButtonTestNextSeason** each need `_alien` assigned to the corresponding `Alien` instance.
- **RespawnNet** needs `_spawnPoint` assigned to a valid Transform.

## Project Structure

```
Assets/
├── Scripts/
│   ├── Heater.cs                    # Fuel accumulation, triggers season change
│   ├── AsteroidSeasonChange.cs      # Shader-driven season transition + plant growth
│   ├── Fishing/
│   │   ├── FishingRod.cs            # Cast / reel / catch state machine
│   │   └── HookAttractor.cs         # Magnetic pull + catch detection
│   ├── Asteroids/
│   │   ├── SmallAsteroidsManager.cs # Pool activation, owner-driven spawning
│   │   ├── SmallAsteroid.cs         # Deterministic server-time-based orbit
│   │   └── SmallAsteroidFuelPrefab.cs # Caught-asteroid → fuel object
│   ├── Alien/
│   │   ├── Alien.cs                 # Dialog state, season filtering, translation
│   │   ├── DialogLine.cs            # ScriptableObject dialog line definition
│   │   └── AlienButton*.cs          # Next / Close / Translate / TestNextSeason buttons
│   └── Other/
│       └── RespawnNet.cs            # Fall-off-world respawn trigger
├── Scenes/
│   ├── NoAssets_VRCDefaultWorldScene 1.unity   # Main/working world scene (202 GameObjects, 181 prefab instances)
│   └── VRCDefaultWorldScene.unity              # Empty default template (not the working scene)
├── Prefabs/                # FishingRod, SmallAsteroidPrefab, SmallAsteroidFuelPrefab, VRCButton2, VRCToggle
├── DialogLines/Musica/     # DialogLine ScriptableObject assets (ID-numbered dialog lines)
├── 3DModels/               # Alien Pod, Big/Small Asteroid, FishingRod, Generator, plants_withAnim
├── UI_SmallWorld/          # Dialogue box, progress bar, and credits UI (animators, prefabs, fonts)
└── Audio/                  # Includes a "Winter Track" audio object referenced in the scene
```

Top-level GameObjects in the working scene: `Directional Light`, `EventSystem`, `Main Camera`, `VRCWorld`, `Heater`, `SmallAsteroidsManager`, `PodPlaceholder`, `RespawnNet`, `Winter Track`, and five `FishingRod2*` instances.

## Build & Publish for VRChat

1. Open **VRChat SDK → Show Control Panel** in Unity, sign in, and use **Builder** to build & test locally, or **Build & Publish** to upload.
2. The scene's `PipelineManager` already has a **World ID** assigned (`wrld_07dde3c5-6703-4a3d-a3b8-6d1330d0e922`), meaning this world has been uploaded before — publishing again will update that existing world listing rather than create a new one. [NEEDS REVIEW: confirm this is the intended target world before publishing.]
3. **[NEEDS REVIEW]** [ProjectSettings/EditorBuildSettings.asset](ProjectSettings/EditorBuildSettings.asset) currently lists **no scenes** (`m_Scenes: []`). Add the working scene (`NoAssets_VRCDefaultWorldScene 1.unity`) to Build Settings before building/publishing.
4. `dev.onevr.vrworldtoolkit` (VRWorld Toolkit) is installed — run its world validation checks (broken references, missing colliders, performance rank, etc.) before publishing.
5. `vrchat.jordo.easyquestswitch` is installed, indicating this world is intended to build for both **PC (Windows)** and **Android (Quest)** platforms. Test both platform toggles in the SDK control panel; verify shaders/scripts used (e.g. the custom season-change shader) are Quest-compatible. [NEEDS REVIEW: no explicit Quest-specific asset variants were found — confirm Quest support is fully implemented, not just scaffolded.]
6. Project settings currently show `companyName: DefaultCompany` in [ProjectSettings/ProjectSettings.asset](ProjectSettings/ProjectSettings.asset) — the default, unedited value. `productName` is set to `Small_world`. [NEEDS REVIEW: update companyName if desired before publishing.]

## Known Issues / TODOs

- **[SmallAsteroidFuelPrefab.cs](Assets/Scripts/Asteroids/SmallAsteroidFuelPrefab.cs)** — this prefab is dynamically spawned via `VRCInstantiate` and has no `UdonSynced` fields, which causes VRChat to force its UdonBehaviour's sync type to `None`; `SendCustomNetworkEvent` silently fails on any UdonBehaviour with SyncType `None` (logged as `Unable to send network event ... with SyncType 'None'`). Adding a dummy synced field and/or switching Synchronization Method to Continuous did not resolve it in testing. The current fix instead avoids networking the destroy step altogether: `OnTriggerEnter` already fires locally on every client (since VRC Object Sync keeps the prefab's transform synced), so each client destroys its own local copy directly instead of the owner broadcasting a destroy event. If this pattern needs to be reused elsewhere for a dynamically-instantiated object, prefer this local-trigger approach over `SendCustomNetworkEvent`.
- **[Heater.cs:44-69](Assets/Scripts/Heater.cs#L44-L69)** — `Update()` decrements the synced fuel `_counter` without checking object ownership (unlike [SmallAsteroidsManager.cs](Assets/Scripts/Asteroids/SmallAsteroidsManager.cs), which explicitly gates its `Update()` behind `Networking.IsOwner(gameObject)`). Every client running this loop independently could cause fuel-counter desync or conflicting `RequestSerialization()` calls. [NEEDS REVIEW]
- **[Heater.cs:72-76](Assets/Scripts/Heater.cs#L72-L76)** and **[SmallAsteroid.cs:80-88](Assets/Scripts/Asteroids/SmallAsteroid.cs#L80-L88)** — `OnDeserialization()` overrides are empty except for comments listing what late joiners "now get" (e.g. "- counter", "- orbitSpeed"). No corrective/UI-refresh logic actually runs here; it's unclear if this is intentional (relying purely on synced field auto-updates) or an incomplete stub.
- **[AsteroidSeasonChange.cs:42](Assets/Scripts/AsteroidSeasonChange.cs#L42)** — `GrowPlant()` uses `if (_plantIndex <= _plantAnimators.Length)`, an off-by-one bounds check; when `_plantIndex == _plantAnimators.Length`, indexing `_plantAnimators[_plantIndex]` will throw an `IndexOutOfRangeException`.
- **[Other/RespawnNet.cs:9-12](Assets/Scripts/Other/RespawnNet.cs#L9-L12)** — a dangling `[SerializeField]` attribute (line 11) has no field after it before the next method; likely a leftover from a deleted field declaration. Should be removed or completed.
- **[Alien.cs:11](Assets/Scripts/Alien/Alien.cs#L11)** — the `dialogLines` array (`UnityEngine.Object[]`, presumably meant to hold `DialogLine` ScriptableObjects) is declared but never read anywhere in the script; dialog is actually driven by the separate parallel `lineFrench` / `lineEnglish` / `season` string arrays. This suggests a planned migration to data-driven `DialogLine` assets (which do exist under `Assets/DialogLines/Musica/`) that was never finished.
- **[Fishing/FishingRod.cs](Assets/Scripts/Fishing/FishingRod.cs)** — contains several large commented-out blocks (alternate `SendCustomNetworkEvent` call paths, an `OnTriggerEnter` catch method, an early-cancel-cast branch) suggesting the cast/reel state machine went through in-place experimentation and isn't fully cleaned up. Worth reviewing whether the commented code should be restored or deleted.
- **Build Settings** has no scenes configured (see Build & Publish, item 3).
- **Scene ambiguity** — two scene files exist ([VRCDefaultWorldScene.unity](<Assets/Scenes/VRCDefaultWorldScene.unity>) and [NoAssets_VRCDefaultWorldScene 1.unity](<Assets/Scenes/NoAssets_VRCDefaultWorldScene 1.unity>)); the former appears to be an unused leftover template.
- No dedicated Quest-specific content or shader variants were found despite the Easy Quest Switch package being installed — Quest build readiness is unverified. [NEEDS REVIEW]

## Credits & License

- **Author(s):** [NEEDS REVIEW — not found in code or project files]
- **3D Models / Art / Audio:** See [Assets/3DModels/](Assets/3DModels), [Assets/UI_SmallWorld/CREDIT_CONTENT/](Assets/UI_SmallWorld/CREDIT_CONTENT) (an in-world credits panel exists but its content wasn't inspected here), and [Assets/Audio/](Assets/Audio). [NEEDS REVIEW — attribute individual asset sources/authors here.]
- **License:** [NEEDS REVIEW — no `LICENSE` file found in the repository root.]
