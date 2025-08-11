using HarmonyLib;

using Il2CppGame;

using MelonLoader;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(UnityExplorer.AITSF_UnityExplorerHelper.Melon),"AITSF_UnityExplorerHelper","1.0.0","Wryyyong")]
[assembly: MelonGame("SpikeChunsoft","NoSleepForKanameDate")]

namespace UnityExplorer.AITSF_UnityExplorerHelper;

public class Melon : MelonMod {
	internal static MelonLogger.Instance Logger;

	internal static MelonPreferences_Category Settings;
	internal static MelonPreferences_Entry<bool> bLogVerbose;

	internal static void EasyLog(string logMsg) {
		if (!bLogVerbose.Value) return;

		Logger.Msg(logMsg);
	}

	public override void OnInitializeMelon() {
		Logger = LoggerInstance;

		Settings = MelonPreferences.CreateCategory("AITSF_UnityExplorerHelper");
		bLogVerbose = Settings.CreateEntry("LogVerbose",false,"Enable debug mode","Set to true to enable verbose logging");

		Patches.Init();
	}
}

[HarmonyPatch]
class Patches {
	private static UIManager uiManager;

	private static void SceneChange(Scene scene,LoadSceneMode mode) {
		uiManager = GameObject.Find("GameController").GetComponent<UIManager>();

		ToggleInputs(UI.UIManager.ShowMenu);
	}

	internal static void Init() =>
		SceneManager.add_sceneLoaded((UnityEngine.Events.UnityAction<Scene,LoadSceneMode>)SceneChange);

	[HarmonyPatch(typeof(UI.UIManager),nameof(UI.UIManager.ShowMenu),MethodType.Setter)]
	[HarmonyPostfix]
	internal static void ToggleInputs(bool value) {
		bool valueNOT = !value;

		foreach (InputDevice device in InputSystem.devices) {
			device.disabledInRuntime = value;

			Melon.EasyLog($"Device {device.name}.disabledInRuntime set to {value}");
		}

		uiManager.SetInteractable(valueNOT);

		Melon.EasyLog($"UIManager.SetInteractable set to {valueNOT}");
	}
}
