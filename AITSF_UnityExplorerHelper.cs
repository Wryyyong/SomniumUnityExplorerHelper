using MelonLoader;

[assembly: MelonInfo(typeof(UnityExplorer.AITSF_UnityExplorerHelper.Melon),"AITSF_UnityExplorerHelper","1.0.0","Wryyyong")]
[assembly: MelonGame("SpikeChunsoft","NoSleepForKanameDate")]

namespace UnityExplorer.AITSF_UnityExplorerHelper;

internal class Melon : MelonMod {
	private static MelonLogger.Instance Logger;

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

		PatchBase.InitAll();
	}
}
