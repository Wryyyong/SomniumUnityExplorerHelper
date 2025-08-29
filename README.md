# SomniumUnityExplorerHelper
A MelonLoader plugin to assist with using [UnityExplorer](https://github.com/yukieiji/UnityExplorer) on games in the _AI: The Somnium Files_ series.

## Supported titles
- [_AI: THE SOMNIUM FILES - nirvanA Initiative_](https://store.steampowered.com/app/1449200)
- [_No Sleep For Kaname Date - From AI: THE SOMNIUM FILES_](https://store.steampowered.com/app/2752180)

## Features
- Automatically block the game from receiving player input while UnityExplorer's navbar is visible, or while Freecam is enabled
  - This functionality can be toggled at will by pressing a key (F3 by default\*)
- Automatically disable any active `CinemachineBrain` components when enabling Freecam, and re-enabling them when disabling Freecam
- Toggle the in-game UI at any time by pressing a key (F4 by default\*)
- Automatically updates the `maxVisibleCharacters` property of TMP_Text objects whenever their `text` properties are modified, to speed up live text-editing

<sub>\* All keybinds can be customized via `MelonPreferences.cfg`</sub>