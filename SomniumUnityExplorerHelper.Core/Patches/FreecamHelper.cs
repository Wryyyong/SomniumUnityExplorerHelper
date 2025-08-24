using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Il2CppCinemachine;

using UnityExplorer.UI.Panels;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch]
static class FreecamHelper {
	const string NameBegin = nameof(FreeCamPanel.BeginFreecam);
	const string NameEnd = nameof(FreeCamPanel.EndFreecam);
	internal static bool IsFreeCamEnabled = false;

	[HarmonyPatch(typeof(FreeCamPanel),NameBegin)]
	[HarmonyPatch(typeof(FreeCamPanel),NameEnd)]
	[HarmonyPostfix]
	static void FreeCamToggle(MethodBase __originalMethod) {
		bool newState;

		switch (__originalMethod.Name) {
			case NameBegin:
				newState = true;
				break;

			case NameEnd:
				newState = false;
				break;

			default:
				return;
		}

		IsFreeCamEnabled = newState;
		InputBlocker.EvaluateAndToggle();

		bool enabled = !IsFreeCamEnabled;

		foreach (List<CinemachineBrain> brains in SceneMonitor.BrainCache.Values) {
			foreach (CinemachineBrain brain in brains) {
				brain.enabled = enabled;
				SomniumMelon.EasyLog($"{brain.tag}.CinemachineBrain.enabled set to {enabled}");
			}
		}
	}

	[HarmonyPatch(typeof(SceneManager))]
	static class SceneMonitor {
		internal static Dictionary<Scene,List<CinemachineBrain>> BrainCache = [];

		[HarmonyPatch(nameof(SceneManager.Internal_SceneLoaded))]
		[HarmonyPostfix]
		static void Internal_SceneLoaded(Scene scene) {
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
		static void Internal_SceneUnloaded(Scene scene) {
			if (!BrainCache.Remove(scene)) return;

			SomniumMelon.EasyLog($"CinemachineBrain compoments uncached for scene {scene.name}");
		}
	}
}
