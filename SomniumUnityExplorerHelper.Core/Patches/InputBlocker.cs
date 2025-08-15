using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using Il2CppCinemachine;

using Il2CppGame;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch]
internal class InputBlocker : PatchBase {
	internal static UIManager UiManager;
	private static bool InputToggle = false;
	private static bool Override = true;

	[HarmonyPatch(typeof(SceneManager),nameof(SceneManager.Internal_SceneLoaded))]
	[HarmonyPostfix]
	private static void Internal_SceneLoaded() {
		if (UiManager == null) UiManager = null;

		UIManager tmpUiManager = null;
		GameObject.Find("GameController")?.TryGetComponent(out tmpUiManager);

		if (tmpUiManager != null) {
			UiManager = tmpUiManager;
			SomniumMelonBase.EasyLog($"UIManager Component cached successfully");
		}

		EvaluateAndToggle();
	}

	internal static void Update() {
		if (!(InputToggle && UniverseLib.Input.InputManager.GetKeyDown(SomniumMelonBase.KeyInputBlockerForceToggle.Value))) return;

		Override = !Override;
		SomniumMelonBase.EasyLog($"ForceToggle triggered, set to {Override}");
		ToggleInputs(Override);
	}

	protected static void EvaluateAndToggle() {
		bool newStatus = UI.UIManager.ShowMenu || FreecamHelper.isFreeCamEnabled;

		if (newStatus == InputToggle) return;

		InputToggle = newStatus;
		if (newStatus == false) Override = true;

		ToggleInputs(newStatus);
	}

	private static void ToggleInputs(bool doDisable) {
		foreach (InputDevice device in InputSystem.devices) {
			device.disabledInRuntime = doDisable;
			SomniumMelonBase.EasyLog($"Device {device.name}.disabledInRuntime set to {doDisable}");
		}

		if (UiManager == null) return;

		bool doDisableInv = !doDisable;

		UiManager.SetInteractable(doDisableInv);
		SomniumMelonBase.EasyLog($"UIManager.SetInteractable set to {doDisableInv}");
	}
}

[HarmonyPatch(typeof(SceneManager))]
internal static class SceneMonitor {
	internal static Dictionary<Scene,List<CinemachineBrain>> BrainCache = [];

	[HarmonyPatch(nameof(SceneManager.Internal_SceneLoaded))]
	[HarmonyPostfix]
	private static void Internal_SceneLoaded(Scene scene) {
		List<CinemachineBrain> newList = [];

		foreach (GameObject obj in scene.GetRootGameObjects())
			foreach (CinemachineBrain brain in obj.GetComponentsInChildren<CinemachineBrain>(true))
				newList.Add(brain);

		if (!newList.Any()) return;

		BrainCache.Add(scene,newList);
		SomniumMelonBase.EasyLog($"{newList.Count} CinemachineBrain components cached for scene {scene.name}");
	}

	[HarmonyPatch(nameof(SceneManager.Internal_SceneUnloaded))]
	[HarmonyPostfix]
	private static void Internal_SceneUnloaded(Scene scene) {
		if (!BrainCache.Remove(scene)) return;

		SomniumMelonBase.EasyLog($"CinemachineBrain compoments uncached for scene {scene.name}");
	}
}
