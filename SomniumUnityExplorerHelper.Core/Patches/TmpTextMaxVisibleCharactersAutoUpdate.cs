using System.Text.RegularExpressions;

using Il2CppTMPro;

namespace UnityExplorer.SomniumUnityExplorerHelper;

[HarmonyPatch(typeof(TMP_Text),nameof(TMP_Text.text),MethodType.Setter)]
static class TmpTextMaxVisibleCharactersAutoUpdate {
	static readonly Regex regex = new(@"<+.*?>+",RegexOptions.Compiled);

	static void Postfix(TMP_Text __instance,string __0) {
		if (__0 == null) return;

		string useText = __0;

		if (__instance.richText)
			useText = regex.Replace(useText,"");

		SomniumMelon.EasyLog($"{__instance.gameObject.name}: maxVisibleCharacters updated from {__instance.maxVisibleCharacters} to {useText.Length}");
		__instance.maxVisibleCharacters = useText.Length;
	}
}
