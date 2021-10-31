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

