using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using UnityEngine.Rendering;

using UniverseLib.Input;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch]
internal static class ToggleUI {
	private static bool HideUI = false;
	private static bool AllowCaching = true;
	private static readonly Dictionary<Behaviour,bool> ComponentCache = [];
	private static readonly Dictionary<string,bool> ComponentGameObjectBlacklist = new() {
		{"VolumeWinkPsync",true},
	};

	private static void Switch(bool newState) {
		AllowCaching = false;

		foreach (Behaviour behaviour in ComponentCache.Keys)
			behaviour.enabled = !newState && ComponentCache[behaviour];

		AllowCaching = true;
	}

	private static void ComponentAdd(Behaviour behaviour) {
		if (ComponentGameObjectBlacklist.ContainsKey(behaviour.gameObject.name)) return;

		ComponentCache.Add(behaviour,behaviour.enabled);
	}

	[HarmonyPatch(typeof(SceneManager))]
	internal static class SceneManagerPatches {
		private const string nameLoaded = nameof(SceneManager.Internal_SceneLoaded);
		private const string nameUnloaded = nameof(SceneManager.Internal_SceneUnloaded);

		private static IEnumerable<MethodBase> TargetMethods() {
			Type type = typeof(SceneManager);

			yield return AccessTools.Method(type,nameLoaded);
			yield return AccessTools.Method(type,nameUnloaded);
		}

		private static void Postfix() {
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
	}

	[HarmonyPatch(typeof(Behaviour),nameof(Behaviour.enabled),MethodType.Setter)]
	[HarmonyPrefix]
	private static bool BehaviourEnabled(Behaviour __instance,bool __0) {
		if (!(AllowCaching && ComponentCache.ContainsKey(__instance)))
			return true;

		ComponentCache[__instance] = __0;
		if (HideUI) SomniumMelon.EasyLog($"{__instance.name} tried to {__0}");

		return !HideUI;
	}

	internal static void Update() {
		if (!InputManager.GetKeyDown(SomniumMelon.KeyToggleUI.Value)) return;

		HideUI = !HideUI;
		SomniumMelon.EasyLog($"UI toggled, new value {HideUI}");

		Switch(HideUI);
	}
}
