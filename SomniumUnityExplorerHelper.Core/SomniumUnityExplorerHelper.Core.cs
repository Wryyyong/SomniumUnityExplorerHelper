global using HarmonyLib;

global using UnityEngine;
global using UnityEngine.SceneManagement;

using MelonLoader;

using UnityExplorer.SomniumUnityExplorerHelper;

[assembly: MelonInfo(typeof(SomniumMelon),SomniumMelon.ModTitle,"1.0.0","Wryyyong")]
[assembly: MelonGame("SpikeChunsoft",SomniumMelon.ModTarget)]
[assembly: VerifyLoaderVersion(0,6,0,true)]

namespace UnityExplorer.SomniumUnityExplorerHelper;

class SomniumMelon : MelonMod {
	public const string ModTitle = "SomniumUnityExplorerHelper";
	public const string ModTarget =
	#if AITSF
		"AI_TheSomniumFiles"
	#elif AINI
		"AI_TheSomniumFiles2"
	#elif AINS
		"NoSleepForKanameDate"
	#endif
	;

	static MelonLogger.Instance Logger;

	internal static MelonPreferences_Category Settings;
	internal static MelonPreferences_Entry<bool> LogVerbose;
	internal static MelonPreferences_Entry<KeyCode> KeyInputBlockerForceToggle;
	internal static MelonPreferences_Entry<KeyCode> KeyToggleUI;

	internal static void EasyLog(string logMsg) {
		if (!LogVerbose.Value) return;

		Logger.Msg(logMsg);
	}

	public override void OnInitializeMelon() {
		Logger = LoggerInstance;

		Settings = MelonPreferences.CreateCategory(ModTitle);

		LogVerbose = Settings.CreateEntry(
			"LogVerbose",
			false,
			"Enable debug mode",
			"Set to true to enable verbose logging"
		);

		KeyInputBlockerForceToggle = Settings.CreateEntry(
			"KeyInputBlockerForceToggle",
			KeyCode.F3,
			"InputBlocker force toggle",
			"The keyboard button to force toggle InputBlocker functionality while either ShowMenu or Freecam are active"
		);

		KeyToggleUI = Settings.CreateEntry(
			"KeyToggleUI",
			KeyCode.F4,
			"UI toggle",
			"The keyboard button to toggle the in-game UI"
		);
	}

	public override void OnUpdate() {
		InputBlocker.Update();
		ToggleUI.Update();
	}
}
