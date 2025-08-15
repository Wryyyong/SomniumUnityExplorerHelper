using HarmonyLib;

using UnityExplorer.UI;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch(typeof(UIManager),nameof(UIManager.ShowMenu),MethodType.Setter)]
internal class InputBlockerShowMenu : InputBlocker {
	[HarmonyPostfix]
	internal static void Toggle() => EvaluateAndToggle();
}
