using MelonLoader;

using UnityEngine;

using UnityExplorer.SomniumUnityExplorerHelper;

[assembly: MelonInfo(typeof(SomniumMelon),SomniumMelonBase.ModTitle,"1.0.0","Wryyyong")]
[assembly: MelonGame("SpikeChunsoft",SomniumMelon.ModTarget)]
[assembly: VerifyLoaderVersion(0,6,0,true)]

namespace UnityExplorer.SomniumUnityExplorerHelper;

internal class SomniumMelonBase : MelonMod {
	public const string ModTitle = "SomniumUnityExplorerHelper";
	public const string ModTarget = null;

	private static MelonLogger.Instance Logger;

	internal static MelonPreferences_Category Settings;
	internal static MelonPreferences_Entry<bool> bLogVerbose;
	internal static MelonPreferences_Entry<KeyCode> KeyInputBlockerForceToggle;
	internal static MelonPreferences_Entry<KeyCode> KeyToggleUI;

	internal static void EasyLog(string logMsg) {
		if (!bLogVerbose.Value) return;

		Logger.Msg(logMsg);
	}

	public override void OnInitializeMelon() {
		Logger = LoggerInstance;

		Settings = MelonPreferences.CreateCategory(ModTitle);
		bLogVerbose = Settings.CreateEntry("LogVerbose",false,"Enable debug mode","Set to true to enable verbose logging");
		KeyInputBlockerForceToggle = Settings.CreateEntry("KeyInputBlockerForceToggle",KeyCode.F1,"InputBlocker force toggle","The keyboard button to force toggle InputBlocker functionality while either ShowMenu or Freecam are active");
		KeyToggleUI = Settings.CreateEntry("KeyToggleGameUI",KeyCode.F2,"GameUI toggle","The keyboard button to toggle the GameUI");

		PatchBase.InitAll();
	}

	public override void OnUpdate() {
		InputBlocker.Update();
		ToggleUI.Update();
	}
}
