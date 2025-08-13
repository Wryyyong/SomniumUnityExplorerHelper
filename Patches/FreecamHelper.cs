using System.Collections.Generic;

using HarmonyLib;

using Il2CppCinemachine;

using UnityExplorer.UI.Panels;

namespace UnityExplorer.AITSF_UnityExplorerHelper;

[HarmonyPatch(typeof(FreeCamPanel))]
internal class FreecamHelper : InputBlocker {
	internal static bool isFreeCamEnabled = false;

	[HarmonyPatch(nameof(FreeCamPanel.BeginFreecam))]
	[HarmonyPostfix]
	private static void BeginFreecam() {
		isFreeCamEnabled = true;

		MultiToggle();
	}

	[HarmonyPatch(nameof(FreeCamPanel.EndFreecam))]
	[HarmonyPostfix]
	private static void EndFreecam() {
		isFreeCamEnabled = false;

		MultiToggle();
	}

	private static void MultiToggle() {
		EvaluateAndToggle();

		bool enabled = !isFreeCamEnabled;

		foreach (List<CinemachineBrain> brains in SceneMonitor.BrainCache.Values) {
			foreach (CinemachineBrain brain in brains) {
				brain.enabled = enabled;
				Melon.EasyLog($"{brain.tag}.CinemachineBrain.enabled set to {enabled}");
			}
		}
	}
}
