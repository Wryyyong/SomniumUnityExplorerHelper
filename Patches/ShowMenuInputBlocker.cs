using HarmonyLib;

using Il2CppGame;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UnityExplorer.AITSF_UnityExplorerHelper;

[HarmonyPatch]
internal class ShowMenuInputBlocker : PatchBase {
	private static UIManager uiManager;

	private static void SceneChange(Scene scene,LoadSceneMode mode) {
		uiManager = GameObject.Find("GameController").GetComponent<UIManager>();

		ToggleInputs(UI.UIManager.ShowMenu);

		Melon.EasyLog($"UIManager GameObject found ({uiManager})");
	}

	protected override void Init() =>
		SceneManager.add_sceneLoaded((UnityEngine.Events.UnityAction<Scene,LoadSceneMode>)SceneChange);

	[HarmonyPatch(typeof(UI.UIManager),nameof(UI.UIManager.ShowMenu),MethodType.Setter)]
	[HarmonyPostfix]
	private static void ToggleInputs(bool value) {
		bool valueNOT = !value;

		foreach (InputDevice device in InputSystem.devices) {
			device.disabledInRuntime = value;

			Melon.EasyLog($"Device {device.name}.disabledInRuntime set to {value}");
		}

		uiManager.SetInteractable(valueNOT);

		Melon.EasyLog($"UIManager.SetInteractable set to {valueNOT}");
	}
}
