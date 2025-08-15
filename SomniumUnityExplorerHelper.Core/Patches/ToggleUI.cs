using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

using UniverseLib.Input;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch]
internal class ToggleUI : PatchBase {
	private static bool HideUI = false;
	private static bool Test = true;
	private static Dictionary<Behaviour,bool> ComponentCache = [];
	private static readonly Dictionary<string,bool> ComponentGameObjectBlacklist = new() {
		{"VolumeWinkPsync",true},
	};

	private static void Switch(bool newState) {
		Test = false;

		if (newState)
			foreach (Behaviour behaviour in ComponentCache.Keys) behaviour.enabled = false;
		else
			foreach (Behaviour behaviour in ComponentCache.Keys) behaviour.enabled = ComponentCache[behaviour];

		Test = true;
	}

	private static void ComponentAdd(Behaviour behaviour) {
		if (ComponentGameObjectBlacklist.ContainsKey(behaviour.gameObject.name)) return;

		ComponentCache.Add(behaviour,behaviour.enabled);
	}

	private static void RecacheComponents() {
		SomniumMelon.EasyLog($"ToggleUI: Recaching components");

		Switch(false);
		ComponentCache.Clear();

		GameObject.Find("Volume")?.GetComponentsInChildren<Volume>(true).ToList().ForEach(static behaviour => ComponentAdd(behaviour));
		GameObject.Find("Canvas2")?.GetComponentsInChildren<Canvas>(true).ToList().ForEach(static behaviour => ComponentAdd(behaviour));

		Canvas cursor = GameObject.Find("cursorpointer Variant")?.GetComponent<Canvas>();
		if (cursor != null) ComponentAdd(cursor);

		if (!HideUI) return;
		Switch(true);
	}

	[HarmonyPatch(typeof(SceneManager))]
	class SceneManagerPatches {
		[HarmonyPatch(nameof(SceneManager.Internal_SceneLoaded))]
		[HarmonyPostfix]
		private static void Internal_SceneLoaded() => RecacheComponents();

		[HarmonyPatch(nameof(SceneManager.Internal_SceneUnloaded))]
		[HarmonyPostfix]
		private static void Internal_SceneUnloaded() => RecacheComponents();
	}

	[HarmonyPatch(typeof(Behaviour),nameof(Behaviour.enabled),MethodType.Setter)]
	[HarmonyPrefix]
	private static void BehaviourEnabled(Behaviour __instance,ref bool __0) {
		if (!(HideUI && Test && ComponentCache.ContainsKey(__instance))) return;

		ComponentCache[__instance] = __0;
		SomniumMelon.EasyLog($"{__instance.name} tried to {__0}");
		__0 = false;
	}

	internal static void Update() {
		if (!InputManager.GetKeyDown(SomniumMelon.KeyToggleUI.Value)) return;

		HideUI = !HideUI;
		SomniumMelon.EasyLog($"UI toggled, new value {HideUI}");

		Switch(HideUI);
	}
}
