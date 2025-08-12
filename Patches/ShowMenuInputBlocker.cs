using HarmonyLib;

using Il2CppGame;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UnityExplorer.AITSF_UnityExplorerHelper;

[HarmonyPatch]
internal class ShowMenuInputBlocker : PatchBase {
	private static UIManager uiManager;
	private static bool inputToggle = false;
	private static bool isFreeCamEnabled = false;

	protected override void Init() =>
		SceneManager.add_sceneLoaded((UnityEngine.Events.UnityAction<Scene,LoadSceneMode>)((scene,mode) => {
			uiManager = GameObject.Find("GameController")?.GetComponent<UIManager>();

			if (uiManager == null) return;
			Melon.EasyLog($"UIManager GameObject cached successfully");

			EvaluateAndToggle();
		}));

	[HarmonyPatch(typeof(UI.UIManager),nameof(UI.UIManager.ShowMenu),MethodType.Setter)]
	[HarmonyPostfix]
	private static void ShowMenuToggle() => EvaluateAndToggle();

	[HarmonyPatch(typeof(UI.Panels.FreeCamPanel),nameof(UI.Panels.FreeCamPanel.BeginFreecam))]
	[HarmonyPostfix]
	private static void BeginFreecam() {
		isFreeCamEnabled = true;

		EvaluateAndToggle();
	}

	[HarmonyPatch(typeof(UI.Panels.FreeCamPanel),nameof(UI.Panels.FreeCamPanel.EndFreecam))]
	[HarmonyPostfix]
	private static void EndFreecam() {
		isFreeCamEnabled = false;

		EvaluateAndToggle();
	}

	private static void EvaluateAndToggle() {
		bool newStatus = UI.UIManager.ShowMenu || isFreeCamEnabled;

		if (newStatus == inputToggle) return;
		inputToggle = newStatus;

		ToggleInputs(newStatus);
	}

	private static void ToggleInputs(bool doDisable) {
		foreach (InputDevice device in InputSystem.devices) {
			device.disabledInRuntime = doDisable;
			Melon.EasyLog($"Device {device.name}.disabledInRuntime set to {doDisable}");
		}

		if (uiManager == null) return;

		bool doDisableInv = !doDisable;

		uiManager.SetInteractable(doDisableInv);
		Melon.EasyLog($"UIManager.SetInteractable set to {doDisableInv}");
	}
}
