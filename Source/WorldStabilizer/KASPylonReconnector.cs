using System;
using UnityEngine;

namespace WorldStabilizer
{
	public class KASPylonReconnector : GenericReconnector
	{
		private PartModule moduleKISItem = null;

		public KASPylonReconnector ()
		{
		}

		public override void OnAwake ()
		{
			base.OnAwake ();
			if (moduleKISItem != null)
				return;
			if (!part.Modules.Contains ("ModuleKISItem"))
				return;
			moduleKISItem = part.Modules ["ModuleKISItem"];
			Invoke ("finalCheck", WorldStabilizer.checkLandedTimeout);
			Log.detail("KASReconnector: KIS Module found for part {0} ({1})", part.name, moduleKISItem);
		}

		protected override void reattach() {
			
			if (moduleKISItem != null) {
				Log.detail("KASReconnector: re-attaching pylon to the ground");
				KASAPI.groundAttach (moduleKISItem);
			} else {
				Log.detail("KASReconnector: module is null");
			}
		}
	}
}

