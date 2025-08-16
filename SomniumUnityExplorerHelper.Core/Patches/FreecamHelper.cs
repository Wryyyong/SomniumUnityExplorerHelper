using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Il2CppCinemachine;

using UnityExplorer.UI.Panels;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch]
internal static class FreecamHelper {
	internal static bool IsFreeCamEnabled = false;

	[HarmonyPatch(typeof(FreeCamPanel))]
	internal static class FreecamPatches {
		private const string nameBegin = nameof(FreeCamPanel.BeginFreecam);
		private const string nameEnd = nameof(FreeCamPanel.EndFreecam);

		private static IEnumerable<MethodBase> TargetMethods() {
			Type type = typeof(FreeCamPanel);

			yield return AccessTools.Method(type,nameBegin);
			yield return AccessTools.Method(type,nameEnd);
		}

		private static void Postfix(MethodBase __originalMethod) {
			bool newState;

			switch (__originalMethod.Name) {
				case nameBegin:
					newState = true;
					break;

				case nameEnd:
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
}
