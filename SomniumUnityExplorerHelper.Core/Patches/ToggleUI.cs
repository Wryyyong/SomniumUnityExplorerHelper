using System.Collections.Generic;
using System.Linq;

using UnityEngine.Rendering;

using UniverseLib.Input;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch]
static class ToggleUI {
	static readonly MelonPreferences_Entry<KeyCode> KeyToggleUI = SomniumMelon.Settings.CreateEntry(
		"KeyToggleUI",
		KeyCode.F4,
		"UI toggle",
		"The keyboard button to toggle the in-game UI"
	);

	static bool HideUI = false;
	static bool AllowCaching = true;
	static readonly Vector3 EmptyVector3 = new(0f,0f,0f);

	static readonly Dictionary<Canvas,bool> CacheCanvas = [];
	static readonly Dictionary<Transform,Vector3> CacheTransform = [];
	static readonly List<VolumeComponent> CacheVolumeComponent = [];

	static void AddCanvas(Canvas canvas) =>
		CacheCanvas.Add(canvas,canvas.enabled);

	static void AddTransform(Transform transform) =>
		CacheTransform.Add(transform,transform.localScale);

	static void AddVolumeComponent(VolumeComponent volumeComponent) =>
		CacheVolumeComponent.Add(volumeComponent);

	[HarmonyPatch(typeof(SceneManager),nameof(SceneManager.Internal_SceneLoaded))]
	[HarmonyPatch(typeof(SceneManager),nameof(SceneManager.Internal_SceneUnloaded))]
	[HarmonyPostfix]
	static void Recache() {
		SomniumMelon.EasyLog($"ToggleUI: Recaching components");

		Switch(false);

		CacheCanvas.Clear();
		CacheTransform.Clear();
		CacheVolumeComponent.Clear();

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

	static void Switch(bool newState) {
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
	[HarmonyPatch(typeof(Transform),nameof(Transform.localScale),MethodType.Setter)]
	[HarmonyPrefix]
	static bool UpdateComponent(Component __instance,ref dynamic __0) {
		if (!AllowCaching)
			return true;

		dynamic instCast,targetCache;

		switch (__instance) {
			case Canvas:
				instCast = (Canvas)__instance;
				targetCache = CacheCanvas;
				break;

			case Transform:
				instCast = (Transform)__instance;
				targetCache = CacheTransform;
				break;

			default:
				return true;
		}

		if (!targetCache.ContainsKey(instCast))
			return true;

		targetCache[instCast] = __0;

		if (HideUI)
			SomniumMelon.EasyLog($"{__instance.name} tried to {__0}");

		return !HideUI;
	}

	internal static void Update() {
		if (!InputManager.GetKeyDown(KeyToggleUI.Value)) return;

		HideUI = !HideUI;
		SomniumMelon.EasyLog($"UI toggled, new value {HideUI}");

		Switch(HideUI);
	}
}
