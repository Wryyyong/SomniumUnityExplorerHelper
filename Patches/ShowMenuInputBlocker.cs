using HarmonyLib;

using Il2CppGame;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UnityExplorer.AITSF_UnityExplorerHelper;

[HarmonyPatch]
internal class ShowMenuInputBlocker : PatchBase {
	private static UIManager uiManager;

	protected override void Init() =>
		SceneManager.add_sceneLoaded((UnityEngine.Events.UnityAction<Scene,LoadSceneMode>)((scene,mode) => {
			uiManager = GameObject.Find("GameController")?.GetComponent<UIManager>();

			if (uiManager == null) return;
			Melon.EasyLog($"UIManager GameObject found ({uiManager})");

			ToggleInputs(UI.UIManager.ShowMenu);
		}));

	[HarmonyPatch(typeof(UI.UIManager),nameof(UI.UIManager.ShowMenu),MethodType.Setter)]
	[HarmonyPostfix]
	private static void ToggleInputs(bool value) {
		bool valueNOT = !value;

		foreach (InputDevice device in InputSystem.devices) {
			device.disabledInRuntime = value;
			Melon.EasyLog($"Device {device.name}.disabledInRuntime set to {value}");
		}

		if (uiManager == null) return;

		uiManager.SetInteractable(valueNOT);
		Melon.EasyLog($"UIManager.SetInteractable set to {valueNOT}");
	}
}
