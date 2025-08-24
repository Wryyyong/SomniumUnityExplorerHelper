using UnityEngine.InputSystem;

using InputManager = UniverseLib.Input.InputManager;
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
static class InputBlocker {
	static GameSpecificUIManager Manager;
	static bool InputToggle = false;
	static bool Override = true;

	const string ManagerParentObject =
	#if AINI
		"Somnium"
	#elif AINS
		"Game"
	#endif
	+ "Controller";

	[HarmonyPatch(typeof(SceneManager),nameof(SceneManager.Internal_SceneLoaded))]
	[HarmonyPostfix]
	static void Internal_SceneLoaded() {
		GameObject.Find(ManagerParentObject)?.TryGetComponent(out Manager);

		if (Manager != null)
			SomniumMelon.EasyLog($"GameSpecificUIManager Component cached successfully");

		EvaluateAndToggle();
	}

	internal static void Update() {
		if (!(InputToggle && InputManager.GetKeyDown(SomniumMelon.KeyInputBlockerForceToggle.Value))) return;

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

		if (newStatus == false)
			Override = true;

		ToggleInputs(newStatus);
	}

	static void ToggleInputs(bool doDisable) {
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
