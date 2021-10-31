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
	public abstract class GenericReconnector : PartModule
	{
		protected int selfDestructTimer = -1;

		public GenericReconnector ()
		{
		}

		public void FixedUpdate() {
			if (selfDestructTimer > 0)
				selfDestructTimer--;
			if (selfDestructTimer == 0) {
				Log.detail("Removing reconnector module");
				CancelInvoke ();
				part.RemoveModule (this);
			}
		}

		public void OnCollisionEnter(Collision c) {

			// It's very unlikely that we hit something else and not the ground
			// Just check for the colliding GameObject layer, should be 15

			if (c.gameObject.layer == 15) {
				reattach ();
			}
			selfDestructTimer = 3;
		}

		protected void finalCheck() {

			Log.detail("KASReconnector: GroundContact = {0}", part.GroundContact);
			if (part.GroundContact) {
				reattach ();
			}
		}

		protected abstract void reattach();
	}
}

