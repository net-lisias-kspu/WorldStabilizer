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

