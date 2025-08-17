using System.Collections.Generic;
using System.Linq;

using UnityEngine.Rendering;

using UniverseLib.Input;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch]
internal static class ToggleUI {
	private static bool HideUI = false;
	private static bool AllowCaching = true;
	private static readonly Dictionary<Canvas,bool> CacheCanvas = [];
	private static readonly Dictionary<Volume,float> CacheVolume = [];
	private static readonly Dictionary<RectTransform,Dictionary<bool,Vector3>> CacheRectTransform = [];

	[HarmonyPatch(typeof(SceneManager),nameof(SceneManager.Internal_SceneLoaded))]
	[HarmonyPatch(typeof(SceneManager),nameof(SceneManager.Internal_SceneUnloaded))]
	[HarmonyPostfix]
	private static void Recache() {
		SomniumMelon.EasyLog($"ToggleUI: Recaching components");

		Switch(false);

		CacheCanvas.Clear();
		CacheVolume.Clear();
		CacheRectTransform.Clear();

		GameObject.Find("cursorpointer Variant")?
			.GetComponents<Canvas>().ToList()
			.ForEach(static cursor => CacheCanvas.Add(cursor,cursor.enabled));

		GameObject.Find("Volume")?
			.GetComponentsInChildren<Volume>(true).ToList()
			.ForEach(static volume => CacheVolume.Add(volume,volume.weight));

		GameObject.Find("Canvas2")?
			.GetComponentsInChildren<RectTransform>(true).ToList()
			.ForEach(static transform => CacheRectTransform.Add(transform,new() {
				{true,new Vector3(0,0,0)},
				{false,transform.localScale},
			}));

		if (!HideUI) return;
		Switch(true);
	}

	private static void Switch(bool newState) {
		AllowCaching = false;

		foreach (Canvas canvas in CacheCanvas.Keys)
			canvas.enabled = !newState && CacheCanvas[canvas];

		foreach (Volume volume in CacheVolume.Keys)
			volume.weight = newState ? 0f : CacheVolume[volume];

		foreach (RectTransform transform in CacheRectTransform.Keys)
			transform.localScale = CacheRectTransform[transform][newState];

		AllowCaching = true;
	}

	[HarmonyPatch(typeof(Behaviour),nameof(Behaviour.enabled),MethodType.Setter)]
	[HarmonyPrefix]
	private static bool UpdateCanvas(Canvas __instance,ref bool __0) {
		if (!(AllowCaching && CacheCanvas.ContainsKey(__instance)))
			return true;

		CacheCanvas[__instance] = __0;
		if (HideUI) SomniumMelon.EasyLog($"{__instance.name} tried to {__0}");

		return !HideUI;
	}

	[HarmonyPatch(typeof(Volume),nameof(Volume.weight),MethodType.Setter)]
	[HarmonyPrefix]
	private static bool UpdateVolume(Volume __instance,ref float __0) {
		if (!(AllowCaching && CacheVolume.ContainsKey(__instance)))
			return true;

		CacheVolume[__instance] = __0;
		if (HideUI) SomniumMelon.EasyLog($"{__instance.name} tried to {__0}");

		return !HideUI;
	}

	[HarmonyPatch(typeof(Transform),nameof(Transform.localScale),MethodType.Setter)]
	[HarmonyPrefix]
	private static bool UpdateRectTransform(RectTransform __instance,ref Vector3 __0) {
		if (!(AllowCaching && CacheRectTransform.ContainsKey(__instance)))
			return true;

		CacheRectTransform[__instance][false] = __0;
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
