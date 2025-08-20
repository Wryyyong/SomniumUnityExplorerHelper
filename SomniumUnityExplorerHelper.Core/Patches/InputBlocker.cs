using UnityEngine.InputSystem;

using UnityExplorer.UI;

using GameSpecificUIManager = Il2CppGame.
#if AINI
	GameManager
#elif AINS
	UIManager
#endif
;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch]
internal static class InputBlocker {
	internal static GameSpecificUIManager Manager;
	private static bool InputToggle = false;
	private static bool Override = true;

	private const string ManagerParentObject =
	#if AINI
		"SomniumController"
	#elif AINS
		"GameController"
	#endif
	;

	[HarmonyPatch(typeof(SceneManager),nameof(SceneManager.Internal_SceneLoaded))]
	[HarmonyPostfix]
	private static void Internal_SceneLoaded() {
		if (Manager == null) Manager = null;

		GameSpecificUIManager tmpManager = null;
		GameObject.Find(ManagerParentObject)?.TryGetComponent(out tmpManager);

		if (tmpManager != null) {
			Manager = tmpManager;
			SomniumMelon.EasyLog($"GameSpecificUIManager Component cached successfully");
		}

		EvaluateAndToggle();
	}

	internal static void Update() {
		if (!(InputToggle && UniverseLib.Input.InputManager.GetKeyDown(SomniumMelon.KeyInputBlockerForceToggle.Value))) return;

		Override = !Override;
		SomniumMelon.EasyLog($"ForceToggle triggered, set to {Override}");
		ToggleInputs(Override);
	}

	[HarmonyPatch(typeof(UIManager),nameof(UIManager.ShowMenu),MethodType.Setter)]
	[HarmonyPostfix]
	internal static void EvaluateAndToggle() {
		bool newStatus = UIManager.ShowMenu || FreecamHelper.IsFreeCamEnabled;

		if (newStatus == InputToggle) return;

		InputToggle = newStatus;
		if (newStatus == false) Override = true;

		ToggleInputs(newStatus);
	}

	private static void ToggleInputs(bool doDisable) {
		foreach (InputDevice device in InputSystem.devices) {
			device.disabledInRuntime = doDisable;
			SomniumMelon.EasyLog($"Device {device.name}.disabledInRuntime set to {doDisable}");
		}

		if (Manager == null) return;

		bool doDisableInv = !doDisable;

		Manager.SetInteractable(doDisableInv);
		SomniumMelon.EasyLog($"Manager.SetInteractable set to {doDisableInv}");
	}
}
