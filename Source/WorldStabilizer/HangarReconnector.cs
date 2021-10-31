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

