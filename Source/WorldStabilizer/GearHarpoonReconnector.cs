/*
	This file is part of World Stabilizer /L Unleashed
		© 2019-2021 Lisias T : http://lisias.net <support@lisias.net>
		© 2017-2019 whale_2

	World Stabilizer is double licensed, as follows:

		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* GPL 2.0 : https://www.gnu.org/licenses/gpl-2.0.txt

	And you are allowed to choose the License that better suit your needs.

	World Stabilizer /L Unleashed is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with World Stabilizer /L Unleashed.
	If not, see <https://ksp.lisias.net/SKL-1_0.txt>.

	You should have received a copy of the GNU General Public License 2.0
	along with World Stabilizer /L Unleashed.
	If not, see <https://www.gnu.org/licenses/>.

*/
using System;
using UnityEngine;
using System.Reflection;
using ModuleWheels;

namespace WorldStabilizer
{
	public class GearHarpoonReconnector: GenericReconnector
	{
		bool reattached = false;
		private ModuleWheelBase wheelBase = null;
		
		public GearHarpoonReconnector ()
		{
		}

		public override void OnAwake ()
		{
			base.OnAwake ();
			Log.dbg("GearHarpoonReconnector: awaking");

			// Still no idea how to reliably receive OnCollisionEnter event on wheels
			// so polling state each 0.5s and giving up in 10s
			// TODO: Make configurable
			wheelBase = (ModuleWheelBase)part.Modules ["ModuleWheelBase"];
			InvokeRepeating ("checkLanded", WorldStabilizer.checkLandedPeriod, WorldStabilizer.checkLandedPeriod);
			Invoke ("selfDestruct", WorldStabilizer.checkLandedTimeout);
		}

		private void selfDestruct() {
			selfDestructTimer = 1;
		}

		protected void checkLanded() {
			if (wheelBase.isGrounded) {
				Log.detail("GearHarpoonReconnector: detected landed state on part {0}", part.name);
				reattach ();
				selfDestructTimer = 3;
			}
		}

		protected override void reattach() {

			if (reattached)
				return;
			Log.detail("GearHarpoonReconnector: re-attaching to the ground; part = {0}", part.name);
			WorldStabilizer.instance.tryAttachHarpoon (vessel);
			reattached = true;
		}
	}

}

