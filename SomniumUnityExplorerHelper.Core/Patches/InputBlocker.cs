using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using Il2CppCinemachine;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using GameSpecificUIManager = Il2CppGame.
#if AINI
	GameManager
#elif AINS
	UIManager
#endif
;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch]
internal class InputBlocker : PatchBase {
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
			SomniumMelon.EasyLog($"Device {device.name}.disabledInRuntime set to {doDisable}");
		}

		if (Manager == null) return;

		bool doDisableInv = !doDisable;

		Manager.SetInteractable(doDisableInv);
		SomniumMelon.EasyLog($"Manager.SetInteractable set to {doDisableInv}");
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
		SomniumMelon.EasyLog($"{newList.Count} CinemachineBrain components cached for scene {scene.name}");
	}

	[HarmonyPatch(nameof(SceneManager.Internal_SceneUnloaded))]
	[HarmonyPostfix]
	private static void Internal_SceneUnloaded(Scene scene) {
		if (!BrainCache.Remove(scene)) return;

		SomniumMelon.EasyLog($"CinemachineBrain compoments uncached for scene {scene.name}");
	}
}
