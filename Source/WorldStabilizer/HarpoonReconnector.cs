using System;
using UnityEngine;
using System.Reflection;

namespace WorldStabilizer
{
	public class HarpoonReconnector: GenericReconnector
	{
		bool reattached = false;
		
		public HarpoonReconnector ()
		{
		}

		public override void OnAwake ()
		{
			base.OnAwake ();
			Log.dbg("HarpoonReconnector: awaking");
			Invoke ("finalCheck", WorldStabilizer.checkLandedTimeout);
		}

		protected override void reattach() {

			if (reattached)
				return;
			Log.detail("HarpoonReconnector: re-attaching to the ground; part = {0}", part.name);
			KASAPI.tryAttachHarpoonImmediately (vessel);
			reattached = true;
		}
	}

}

