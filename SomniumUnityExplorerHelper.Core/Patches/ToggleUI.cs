using System.Collections.Generic;
using System.Linq;

using UnityEngine.Rendering;

using UniverseLib.Input;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch]
internal static class ToggleUI {
	private static bool HideUI = false;
	private static bool AllowCaching = true;
	private static readonly Vector3 EmptyVector3 = new(0f,0f,0f);

	private static readonly Dictionary<Canvas,bool> CacheCanvas = [];
	private static readonly Dictionary<Transform,Vector3> CacheTransform = [];
	private static readonly List<VolumeComponent> CacheVolumeComponent = [];

	[HarmonyPatch(typeof(SceneManager),nameof(SceneManager.Internal_SceneLoaded))]
	[HarmonyPatch(typeof(SceneManager),nameof(SceneManager.Internal_SceneUnloaded))]
	[HarmonyPostfix]
	private static void Recache() {
		SomniumMelon.EasyLog($"ToggleUI: Recaching components");

		Switch(false);

		CacheCanvas.Clear();
		CacheTransform.Clear();
		CacheVolumeComponent.Clear();

		static void AddCanvas(Canvas canvas) => CacheCanvas.Add(canvas,canvas.enabled);
		static void AddTransform(Transform transform) => CacheTransform.Add(transform,transform.localScale);
		static void AddVolumeComponent(VolumeComponent volumeComponent) => CacheVolumeComponent.Add(volumeComponent);

		GameObject.Find("cursorpointer Variant")?
			.GetComponents<Canvas>().ToList()
			.ForEach(AddCanvas);

		GameObject.Find("Canvas2")?
			.GetComponentsInChildren<RectTransform>(true).ToList()
			.ForEach(AddTransform);

		#if AINS
		GameObject.Find("TargetRoot")?
			.GetComponentsInChildren<Transform>(true).ToList()
			.ForEach(AddTransform);
		#endif

		GameObject.Find("Volume")?
			.GetComponentsInChildren<Volume>(true).ToList()
			.ForEach(static volume => volume.profile?.components?
				.ForEach((Il2CppSystem.Action<VolumeComponent>)AddVolumeComponent)
			);

		if (!HideUI) return;
		Switch(true);
	}

	private static void Switch(bool newState) {
		bool newStateInv = !newState;
		AllowCaching = false;

		foreach (KeyValuePair<Canvas,bool> canvas in CacheCanvas)
			canvas.Key.enabled = newStateInv && canvas.Value;

		foreach (KeyValuePair<Transform,Vector3> transform in CacheTransform)
			transform.Key.localScale = newState ? EmptyVector3 : transform.Value;

		foreach (VolumeComponent vComp in CacheVolumeComponent)
			vComp.active = newStateInv;

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

	[HarmonyPatch(typeof(Transform),nameof(Transform.localScale),MethodType.Setter)]
	[HarmonyPrefix]
	private static bool UpdateTransform(Transform __instance,ref Vector3 __0) {
		if (!(AllowCaching && CacheTransform.ContainsKey(__instance)))
			return true;

		CacheTransform[__instance] = __0;
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
