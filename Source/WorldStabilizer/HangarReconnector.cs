using System;
using UnityEngine;
using System.Reflection;

namespace WorldStabilizer
{
	public class HangarReconnector : GenericReconnector
	{
		private PartModule moduleHangar = null;
		private bool collisionDetected = false;

		public HangarReconnector ()
		{
		}

		public override void OnAwake ()
		{
			base.OnAwake ();
			if (moduleHangar != null)
				return;
			if (!part.Modules.Contains ("GroundAnchor"))
				return;
			moduleHangar = part.Modules ["GroundAnchor"];
			Log.detail("Hangar Module found for part {0} ({1})", part.name, moduleHangar);
		}

		protected override void reattach() {

			if (moduleHangar != null) {
				if (!collisionDetected) {
					WorldStabilizer.invokeAction (moduleHangar, "Attach anchor");
					collisionDetected = true;
				}
			} else {
				Log.detail("Hangar module is null");
			}
		}
	}
}

