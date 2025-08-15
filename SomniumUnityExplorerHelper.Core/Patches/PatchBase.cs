using System;
using System.Linq;

namespace UnityExplorer.SomniumUnityExplorerHelper;

internal class PatchBase {
	protected virtual void Init() {}

	internal static void InitAll() {
		Type tPatchBase = typeof(PatchBase);

		tPatchBase.Assembly.GetTypes()
			.Where(type => type.IsSubclassOf(tPatchBase))
			.Select(type => Activator.CreateInstance(type) as PatchBase)
			.ToList().ForEach(patch => patch.Init());

		SomniumMelon.EasyLog("Patches initialised");
	}
}
