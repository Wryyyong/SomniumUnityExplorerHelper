using HarmonyLib;

using Il2CppGame;

using UniverseLib.Input;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch(typeof(UIManager),nameof(UIManager.ShowUI))]
internal class ToggleUI : PatchBase {
	private static bool UIEnabled = true;

	[HarmonyPostfix]
	private static void ShowUI(bool visible) {
		UIEnabled = visible;
	}

	internal static void Update() {
		if (InputBlocker.UiManager == null || !InputManager.GetKeyDown(SomniumMelonBase.KeyToggleUI.Value)) return;

		bool newState = !UIEnabled;
		InputBlocker.UiManager.ShowUI(newState);

		SomniumMelonBase.EasyLog($"UI toggled, new value {newState}");
	}
}
