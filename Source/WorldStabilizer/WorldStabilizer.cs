using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

using UnityEngine;

using Asset = KSPe.IO.Asset<WorldStabilizer.Startup>;
using Data = KSPe.IO.Data<WorldStabilizer.Startup>;

namespace WorldStabilizer
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class WorldStabilizer: MonoBehaviour
	{
		// configuration parameters

		// How many ticks do we hold the vessel
		public static int stabilizationTicks = 100;
		// How many ticks do we try to put the vessel down
		public static int groundingTicks = 5;
		// Should we stabilize vessels in PRELAUNCH state
		public static bool stabilizeInPrelaunch = true;
		// Should we stabilize Kerbals
		public static bool stabilizeKerbals = false;
		// Should we recalculate vessel bounds before every attempt to move it
		public static bool recalculateBounds = true;
		// Should we draw markers around topmost and bottommost vessel points
		public static bool drawPoints = true;
		// Should we display 'World has been stabilized' message
		public static bool displayMessage = true;
		// How long to wait for harpoon reattaching after landing gear ground contact
		public static float harpoonReattachTimeout = 1;
		// How often to check for landed state
		// (See GearHarpoonReconnector)
		public static float checkLandedPeriod = 0.5f;
		// If there was no ground contact on landing gear after waiting for this long
		// give up on harpoon reattachment
		public static float checkLandedTimeout = 10f;

		// If downmovement is below this value, leave the vessel as is
		public static float minDownMovement = 0.05f;
		// Minimum upmovement in case we're beneath the ground
		public static float upMovementStep = 0.2f;
		// Max upmovement in case upward movement is required; should cancel
		// moving the craft to space in case we messed the things up
		public static float maxUpMovement = 2.0f;
		// Last resort drop altitude
		// If the mod can't reliably determine the height above obstacles, like when
		// vessel lies on different colliders, it still will be lowered, but to this
		// altitude
		public static float lastResortAltitude = 2.0f;

		private const int rayCastMask = (1 << 28) | (1 << 15) ;
		private const int rayCastExtendedMask = rayCastMask | 1;

		private int stabilizationTimer;
		private int count = 0;
		private Dictionary<Guid, int> vesselTimer;
		private Dictionary<Guid, LineRenderer> renderer0;
		private Dictionary<Guid, LineRenderer> renderer1;

		private Dictionary<Guid, VesselBounds> bounds;
		private Dictionary<Guid, List<PartModule>> anchors;
		private Dictionary<Guid, double> initialAltitude;
		private Dictionary<uint, CollisionEnhancerBehaviour> ceBehaviors;

		private List<Vessel> vesselsToMoveUp;

		private List<string> excludeVessels;
		private List<string> excludePartModules;
		private List<string> excludeParts;


		public static EventVoid onWorldStabilizationStartEvent;
		public static EventVoid onWorldStabilizedEvent;

		public static WorldStabilizer instance;

		private static Data.ConfigNode SETTINGS = Data.ConfigNode.For("WorldStabilizer", "settings.cfg");

		public WorldStabilizer ()
		{
			onWorldStabilizationStartEvent = new EventVoid ("onWorldStabilizationStart");
			onWorldStabilizedEvent = new EventVoid ("onWorldStabilized");
		}

		public void Awake() {

			Log.dbg("Awake");

			instance = this;

			excludeVessels = new List<string>();
			excludeParts = new List<string>();
			excludePartModules = new List<string>();
			configure ();

			KASAPI.initialize ();
		}

		public void Start() {

			Log.dbg("Start");
			vesselTimer = new Dictionary<Guid, int> ();
			renderer0 = new Dictionary<Guid, LineRenderer> ();
			renderer1 = new Dictionary<Guid, LineRenderer> ();
			bounds = new Dictionary<Guid, VesselBounds> ();
			anchors = new Dictionary<Guid, List<PartModule>> ();
			vesselsToMoveUp = new List<Vessel> ();
			initialAltitude = new Dictionary<Guid, double> ();
			ceBehaviors = new Dictionary<uint, CollisionEnhancerBehaviour> ();

			GameEvents.onVesselGoOffRails.Add (onVesselGoOffRails);
			GameEvents.onVesselGoOnRails.Add (onVesselGoOnRails);
			GameEvents.onVesselSwitching.Add (onVesselSwitching);

			stabilizationTimer = stabilizationTicks;
		}

		public void OnDestroy() {
			Log.dbg("OnDestroy");
			GameEvents.onVesselGoOffRails.Remove (onVesselGoOffRails);
			GameEvents.onVesselGoOnRails.Remove (onVesselGoOnRails);
			GameEvents.onVesselSwitching.Remove (onVesselSwitching);
		}

		public void onVesselGoOnRails(Vessel v) {
			if (vesselTimer.ContainsKey (v.id) && vesselTimer [v.id] > 0) {
				vesselTimer [v.id] = 0;
				count--;
				if (drawPoints) {
					renderer0 [v.id].gameObject.DestroyGameObject ();
					renderer1 [v.id].gameObject.DestroyGameObject ();
				}
			}
			//this.tryDetachAnchor (v); // If this vessel has anchors (from Hangar), detach them
			//KASAPI.tryDetachPylon (v); // Same with KAS pylons
			//KASAPI.tryDetachHarpoon (v);
		}

		public void onVesselGoOffRails(Vessel v) {

			if (v.situation == Vessel.Situations.LANDED ||
			    (stabilizeInPrelaunch && v.situation == Vessel.Situations.PRELAUNCH)) {

				Log.info("off rails: {0}: alt: {1}; radar alt: {2}; alt: {3}", v.name, v.altitude, v.radarAltitude, v.protoVessel.altitude);
				if (v.isEVA && !stabilizeKerbals) { // Kerbals are usually ok
					return;
				}
				if (v.packed) { // no physics, leave it alone
					return;
				}
				if (checkExcludes (v)) { // don't touch particular vessels
					return;
				}

				// missionTime seems to be not very reliable - doesn't work if the vessel wasn't touched after launch
				Log.detail("mission time: {0}", v.missionTime);
				if (v.missionTime < 3)
				{
					Log.detail("checking if we're inside the hangar");
					if (checkIfInsideHangar(v))
					{
						return;
					}
				}

				if (count == 0) {
					onWorldStabilizationStartEvent.Fire ();
				}

				vesselTimer [v.id] = stabilizationTimer;
				bounds [v.id] = new VesselBounds (v);
				if (drawPoints) {
					initLR (v, bounds [v.id]);
				}
				count++;

				// Scheduling moving up at fixed update to allow other modules to fully initialize
				initialAltitude[v.id] = v.altitude;
				vesselsToMoveUp.Add (v);
			}
		}

		public void onVesselSwitching(Vessel from, Vessel to) {

			if (to == null || to.situation != Vessel.Situations.LANDED) { // FIXME: Do we need PRELAUNCH here?
				return;
			}

			string fromString = from != null ? (from.name + "(packed=" + from.packed + ")") : "non-vessel";
			Log.info("{0} -> {1}(packed={2})", fromString, to.name, to.packed);

			this.tryDetachAnchor (to); // If this vessel has anchors (from Hangar), detach them
			KASAPI.tryDetachPylon (to); // Same with KAS pylons
			KASAPI.tryDetachHarpoon (to);
		}

		public void FixedUpdate() {
			if (count == 0) {
				return;
			}

			foreach (Vessel v in FlightGlobals.VesselsLoaded) {
				if (vesselTimer.ContainsKey (v.id) && vesselTimer[v.id] > 0) {

					stabilize (v);

					vesselTimer [v.id] --;
					if (vesselTimer [v.id] == 0) {
						count--;
						Log.detail("Stopping stabilizing {0}", v.name);

						if (count == 0) {
							if (displayMessage) {
								ScreenMessages.PostScreenMessage ("World has been stabilized");
								Log.info("World has been stabilized");
							}
							onWorldStabilizedEvent.Fire ();
						}
					}
				}
			}

//			KAS Harpoons are out of order in KAS 1.0
			if (!KASAPI.isNewKsp)
				foreach (Rigidbody rb in KASAPI.harpoonsToHold) {
					KASAPI.holdHarpoon (rb);
				}
		}

		private void moveUp(Vessel v) {

			v.ResetCollisionIgnores();
			v.ResetGroundContact();

			// TODO: Try DisableSuspension() on wheels

			float upMovement = 0.0f;

			float vesselHeight = bounds [v.id].topLength + bounds [v.id].bottomLength;
			Vector3 up = (v.transform.position - FlightGlobals.currentMainBody.transform.position).normalized;

			while (upMovement < maxUpMovement) {

				RayCastResult alt = GetRaycastAltitude (v, bounds [v.id].bottomPoint + up * vesselHeight, rayCastMask); // mask: ground only
				RayCastResult alt2 = GetRaycastAltitude (v, bounds [v.id].topPoint, rayCastMask); // mask: ground only

				Log.detail("{0}: alt from top - height = {1}; alt from top: {2}; vessel height = {3}; minDownMovement = {4}", v.name, (alt.altitude - vesselHeight), alt, vesselHeight, minDownMovement);
				if (alt.altitude - vesselHeight < minDownMovement) {

					Log.detail("{0}: hit colliders: {1} and {2}", v.name, alt, alt2);

					v.Translate (up * upMovementStep);
					Log.detail("Moving up: {0} by {1}", v.name, upMovementStep);
					upMovement += upMovementStep;

				} else {
					Log.detail("{0}: minumum downmovement reached; alt from bottom: {1}", v.name, alt);
					break;
				}
			}
			Log.detail("{0}; new alt = {1}; alt from top = {2}"
				, GetRaycastAltitude(v, bounds[v.id].bottomPoint, rayCastMask)
				, v.name
				, GetRaycastAltitude(v, bounds[v.id].topPoint, rayCastMask)
			);
		}

		private void moveDown(Vessel v) {

			if (recalculateBounds) {
				Log.detail("Recalculating bounds for vessel {0}; id={1}", v.name, v.id);
				bounds [v.id].findBoundPoints ();
			}
			RayCastResult alt = GetRaycastAltitude (v, bounds[v.id].bottomPoint,  rayCastMask);
			RayCastResult alt3 = GetRaycastAltitude(v, bounds[v.id].topPoint, rayCastMask);

			Vector3 referencePoint = bounds [v.id].bottomPoint;
			// So what's wrong if we hit different colliders?
			// We can even hit the same collider, but it will have a non-flat mesh in that point
			//
			// Seems like the right way would be raycasting from every downward facing point
			// or from middle of every downward facing triangle

			if (alt.collider != alt3.collider) {
				Log.detail("{0}: hit different colliders: {1} and {2}", v.name, alt, alt3 ); //; using lastResortAltitude as a guard point");
				//minDownMovement = lastResortAltitude;
				if (alt3.altitude < alt.altitude) {
					referencePoint = bounds [v.id].topPoint;
					Log.detail("{0}: reference point set to top", v.name);
				}
			}

			// Re-cast raycast including parts into the mask
			alt = GetRaycastAltitude (v, referencePoint,  rayCastExtendedMask);
			Log.detail("{0}: raycast including parts; hit collider: {1}", v.name, alt);
			float downMovement = alt.altitude ;

			Vector3 up = (v.transform.position - FlightGlobals.currentMainBody.transform.position).normalized;

			if (downMovement < minDownMovement ) {
				Log.detail("downmovement for {0} is below threshold ({1}<{2}); leaving as is: {3}", v.name, downMovement, minDownMovement, downMovement);
				return;
			}

			downMovement -= minDownMovement;

			Log.detail("Moving down: {0} by {1}; alt = {2}; timer = {3}; radar alt = {4}; alt from top = {5}", v.name, downMovement, alt.altitude, vesselTimer[v.id], v.radarAltitude, alt3.altitude);
			v.Translate (-downMovement * (Vector3d)up);
		}

		private void vesselSleep(Vessel v) {

			foreach (Part p in v.parts) {
				if (p.Rigidbody != null)
					p.Rigidbody.Sleep ();
			}
		}

		private void ignoreColliders(Vessel v, Collider c) {

			foreach (Part p in v.parts) {
				if (p.collisionEnhancer != null) {
					ceBehaviors [p.flightID] = p.collisionEnhancer.OnTerrainPunchThrough;
					p.collisionEnhancer.OnTerrainPunchThrough = CollisionEnhancerBehaviour.DO_NOTHING;
				}
				foreach (Collider c2 in p.GetComponents<Collider>()) {
					Physics.IgnoreCollision (c, c2, true);
				}
				foreach (Collider c2 in p.GetComponentsInChildren<Collider>()) {
					Physics.IgnoreCollision (c, c2, true);
				}
			}
		}

		private void restoreColliders(Vessel v, Collider c) {
			foreach (Part p in v.parts) {
				foreach (Collider c2 in p.GetComponents<Collider>()) {
					Physics.IgnoreCollision (c, c2, false);
				}
				foreach (Collider c2 in p.GetComponentsInChildren<Collider>()) {
					Physics.IgnoreCollision (c, c2, false);
				}
			}
		}

		private void restoreCEBehavior(Vessel v) {
			foreach (Part p in v.parts) {
				if (p.collisionEnhancer != null && ceBehaviors.ContainsKey (p.flightID)) {
					p.collisionEnhancer.OnTerrainPunchThrough = ceBehaviors [p.flightID];
				}
			}
		}

		private void restoreInitialAltitude(Vessel v) {

			// Restoring initial altitude
			// This is to address such situations as:
			// 1. Placing a vessel under the (static) roof. KSP restores it to be on top of the roof
			// 2. Using of AirPark mod. AirPark sets vessel state to LANDED if it is left in atmosphere
			// and KSP pins it to the ground on unpackling.

			Vector3 up = (v.transform.position - FlightGlobals.currentMainBody.transform.position).normalized;
			double altDiff = initialAltitude[v.id] - v.altitude;
			Log.detail("{0}: initial alt: {1}; current alt = {2}; moving up by: {3}", v.name, initialAltitude[v.id], v.altitude, altDiff);

			RaycastHit upHit;
			RaycastHit downHit;

			if (Physics.Raycast (bounds[v.id].bottomPoint, -up, out downHit, v.vesselRanges.landed.unload, rayCastMask)) {
				Log.detail("{0}: downward hit: {1}; collider = {2}", v.name, downHit, downHit.collider);
				ignoreColliders(v, downHit.collider);
			}
			else {
				Log.detail("{0}: no downward hit", v.name);
			}

			if (Physics.Raycast (bounds[v.id].topPoint, up, out upHit, v.vesselRanges.landed.unload, rayCastExtendedMask)) {
				Log.detail("{0}: upward hit: {1}; collider = {2}", v.name, upHit, upHit.collider);
				ignoreColliders (v, upHit.collider);
				Log.detail("uphit distance: {0}", upHit.distance);
				if (upHit.distance > 0.2)
				{
					// FIXME: Account for suspension travel at last?
					altDiff = 0.2;
				}
			}
			else {
				Log.detail("{0}: no upward hit; moving up by 0.2 m just in case", v.name);
				altDiff = 0.2;
			}

			v.Translate (up * (float)altDiff);

			if (upHit.collider != null) {
				restoreColliders (v, upHit.collider);
			}

			if (downHit.collider != null) {
				restoreColliders (v, downHit.collider);
			}
		}

		private void stabilize(Vessel v) {

			// At the very first tick we detach what could be possibly detached and restore initial altitude.
			// At the second and third tick we move vessel up if needed
			// At the 4-8 ticks we move vessel down to the safe altitude

			if (vesselsToMoveUp.Contains(v)) {

				Log.detail("{0}: timer = {1}", v.name, vesselTimer[v.id]);
				if (vesselTimer [v.id] == stabilizationTimer) {
					// Detaching what should be detached at the very start of stabilization
					tryDetachAnchor (v); // If this vessel has anchors (from Hangar), detach them
					KASAPI.tryDetachPylon (v); // Same with KAS pylons
					KASAPI.tryDetachHarpoon (v);

					restoreInitialAltitude (v);

				} else {

					Log.detail("{0}: timer = {1}; moving up", v.name, vesselTimer[v.id]);
					moveUp (v);
					// Setting up attachment procedure early
					KASAPI.tryAttachPylon (v);
					tryAttachAnchor (v);
					scheduleHarpoonReattachment (v);
					restoreCEBehavior (v);

					vesselsToMoveUp.Remove (v);
				}
			}
			else {

				if (vesselTimer [v.id] > stabilizationTimer - groundingTicks) // next 3(?) ticks after detaching and moving up
				{
					Log.detail("{0}: timer = {1}; moving down", v.name, vesselTimer[v.id]);
					moveDown (v);
				}
			}

			if (drawPoints) {
				updateLR (v, bounds [v.id]);
			}

			v.IgnoreGForces(2);
			v.SetWorldVelocity (Vector3.zero);
			v.angularMomentum = Vector3.zero;
			v.angularVelocity = Vector3.zero;
			vesselSleep (v);

			if (vesselTimer [v.id] % 10 == 0) {
				Log.detail("Stabilizing; v = {0}; radar alt = {1}; timer = {2}", v.name, v.radarAltitude, vesselTimer[v.id]);
			}
		}

		private List<PartModule> findAnchoredParts(Vessel v) {

			Log.detail("Looking for anchors in {0}", v.name);
			List<PartModule> anchorList = new List<PartModule> ();

			foreach (Part p in v.parts) {
				foreach (PartModule pm in p.Modules) {
					if (pm.moduleName == "GroundAnchor") {
						Log.detail("{0}: Found anchor on part {1}; attached = {2}", v.name, p.name, pm.Fields.GetValue("isAttached"));
						if ((bool)pm.Fields.GetValue ("isAttached"))
							anchorList.Add (pm);
					}
				}
			}
			Log.detail("Found {0} anchors", anchorList.Count);

			return anchorList;
		}

		private void tryDetachAnchor(Vessel v) {

			List<PartModule> anchoredParts = findAnchoredParts (v);
			if (anchoredParts.Count > 0) {
				anchors [v.id] = anchoredParts;
				foreach (PartModule pm in anchors[v.id]) {
					invokeAction (pm, "Detach anchor");
				}
			}
		}

		private void tryAttachAnchor(Vessel v) {
			if (!anchors.ContainsKey (v.id))
				return;
			foreach (PartModule pm in anchors[v.id]) {
				// Adding parasite module to the part
				// It will re-activate ground conneciton upon ground contact
				// and destroy itself afterwards
				Log.detail("Adding HangarReconnector to {0}", pm.part.name);
				pm.part.AddModule("HangarReconnector", true);
			}
			anchors.Remove (v.id);
		}

		private void scheduleHarpoonReattachment(Vessel v) {
			if (!KASAPI.hasKASAddOn)
				return;
			if (!KASAPI.winches.ContainsKey (v.id) || KASAPI.winches[v.id].Count() == 0)
				return;

			Part bottom = bounds [v.id].bottomPart;
			// FIXME: What about KSPWheel?
			if (bottom.Modules.Contains ("ModuleWheelBase")) {
				bottom.AddModule ("GearHarpoonReconnector", true);
				Log.detail("Added GearHarpoonReconnector for part {0}; vessel: {1}", bottom.name, v.name);
			} else {
				bottom.AddModule ("HarpoonReconnector", true);
			}
		}


		// Keeping here as we ingerit from MonoBehavior
		internal void tryAttachHarpoon(Vessel v) {
			if (!KASAPI.winches.ContainsKey (v.id))
				return;
			StartCoroutine (this.tryAttachHarpoonCoro (v));
		}

		private IEnumerator tryAttachHarpoonCoro(Vessel v) {

			Log.dbg("re-attaching harpoons in {0} sec", harpoonReattachTimeout);
			yield return new WaitForSeconds (harpoonReattachTimeout);

			KASAPI.tryAttachHarpoonImmediately (v);
		}

		internal static void invokeAction(PartModule pm, string actionName) {
			Log.detail("Invoking action {0} on part {1}", actionName, pm.part.name);
			// https://forum.kerbalspaceprogram.com/index.php?/topic/65106-trigger-a-parts-action-from-code/
			BaseActionList bal = new BaseActionList(pm.part, pm); //create a BaseActionList bal with the available actions on the part.
																  //p being our current part, pm being our current partmodule
			if (bal.Count == 0)
				return;
			foreach (BaseAction ba in bal) //start cycling through baseActions in the BaseActionList
			{
				if (ba.guiName == actionName) //Trigger is a bool set to true via a GUI button on-screen (code not shown)
				{
					KSPActionParam ap = new KSPActionParam(KSPActionGroup.None, KSPActionType.Deactivate); //an important line, see post
					ba.Invoke(ap); //Invoke the "Extend Panel" command (our current ba. variable) with the ActionParameter from the previous line.
				}
			}
		}

		private void initLR(Vessel v, VesselBounds vbounds) {

			LineRenderer Lr0;
			Lr0 = new GameObject ().AddComponent<LineRenderer> ();
			Lr0.material = new Material (Shader.Find ("KSP/Emissive/Diffuse"));
			Lr0.useWorldSpace = true;
			Lr0.material.SetColor ("_EmissiveColor", Color.green);
			/*Lr0.startWidth = 0.15f;
			Lr0.endWidth = 0.15f;
			Lr0.positionCount = 4;*/
			Lr0.enabled = true;

			LineRenderer Lr1;
			Lr1 = new GameObject ().AddComponent<LineRenderer> ();
			Lr1.material = new Material (Shader.Find ("KSP/Emissive/Diffuse"));
			Lr1.useWorldSpace = true;
			Lr1.material.SetColor ("_EmissiveColor", Color.green);
			/*Lr1.startWidth = 0.15f;
			Lr1.endWidth = 0.15f;
			Lr1.positionCount = 4;*/
			Lr1.enabled = true;

			renderer0 [v.id] = Lr0;
			renderer1 [v.id] = Lr1;
			updateLR (v, vbounds);
		}

		private void updateLR(Vessel v, VesselBounds vbounds) {

			LineRenderer Lr0 = renderer0 [v.id];
			LineRenderer Lr1 = renderer1 [v.id];
			Lr0.SetPosition (0, vbounds.bottomPoint);
			Lr0.SetPosition (1, vbounds.bottomPoint + v.transform.TransformPoint(v.transform.forward));

			Lr0.SetPosition (2, vbounds.bottomPoint);
			Lr0.SetPosition (3, vbounds.bottomPoint + v.transform.TransformPoint(v.ReferenceTransform.right));
				//Vector3.ProjectOnPlane(v.CoM-FlightCamera.fetch.mainCamera.transform.position, vbounds.up).normalized);
			Lr1.SetPosition (0, vbounds.topPoint);
			Lr1.SetPosition (1, vbounds.topPoint + v.transform.TransformPoint(v.ReferenceTransform.forward));
			Lr1.SetPosition (2, vbounds.topPoint);
			Lr1.SetPosition (3, vbounds.topPoint + v.transform.TransformPoint(v.ReferenceTransform.right));
				//Vector3.ProjectOnPlane(v.CoM-FlightCamera.fetch.mainCamera.transform.position, vbounds.up).normalized);

			//printDebug ("line: " + vbounds.bottomPoint + " -> " + (vbounds.bottomPoint + v.transform.TransformPoint(v.transform.forward)));
			//printDebug ("line: " + vbounds.bottomPoint + " -> " + v.ReferenceTransform.TransformPoint(v.transform.forward));
		}

		public class Pair<T, U> {
			public Pair() {
			}

			public Pair(T first, U second) {
				this.First = first;
				this.Second = second;
			}

			public T First { get; set; }
			public U Second { get; set; }
		}

		public struct VesselBounds
		{

			public Vessel vessel;
			public float bottomLength;
			public float topLength;

			public Vector3 localBottomPoint;
			public Vector3 bottomPoint {
				get {
					return vessel.transform.TransformPoint(localBottomPoint);
				}
			}

			public Vector3 localTopPoint;
			public Vector3 topPoint {
				get {
					return vessel.transform.TransformPoint (localTopPoint);
				}
			}

			public Vector3 up;
			public float maxSuspensionTravel;
			public Part bottomPart;

			public VesselBounds(Vessel v)
			{
				vessel = v;
				bottomLength = 0;
				topLength = 0;
				localBottomPoint = Vector3.zero;
				localTopPoint = Vector3.zero;
				up = Vector3.zero;
				maxSuspensionTravel = 0f;
				bottomPart = v.rootPart;
				findBoundPoints();
			}

			public void findBoundPoints() {

				Vector3 lowestPoint = Vector3.zero;
				Vector3 highestPoint = Vector3.zero;
				//float maxSqrDist = 0.0f;
				Part downwardFurthestPart = vessel.rootPart;
				Part upwardFurthestPart = vessel.rootPart;
				up = (vessel.CoM-vessel.mainBody.transform.position).normalized;
				Vector3 downPoint = vessel.CoM - (2000 * up);
				Vector3 upPoint = vessel.CoM + (2000 * up);
				Vector3 closestVert = vessel.CoM;
				Vector3 farthestVert = vessel.CoM;
				float closestSqrDist = Mathf.Infinity;
				float farthestSqrDist = Mathf.Infinity;

				foreach (Part p in vessel.parts) {

					if (p.Modules.Contains ("KASModuleHarpoon"))
						continue;

					HashSet<Pair<Transform, Mesh>> meshes = new HashSet<Pair<Transform, Mesh>>();
					foreach (MeshFilter filter in p.GetComponentsInChildren<MeshFilter>()) {

						Collider[] cdr = filter.GetComponentsInChildren<Collider> ();
						if (cdr.Length > 0 || p.Modules.Contains("ModuleWheelSuspension")) {
							// for whatever reason suspension needs an additional treatment
							// TODO: Maybe address it by searching for wheel collider
							meshes.Add (new Pair<Transform, Mesh>(filter.transform,  filter.mesh));
						}
					}

					foreach (MeshCollider mcdr in p.GetComponentsInChildren<MeshCollider> ()) {
						meshes.Add(new Pair<Transform, Mesh>(mcdr.transform, mcdr.sharedMesh));
					}

					foreach (Pair<Transform, Mesh> meshpair in meshes) {
						Mesh mesh = meshpair.Second;
						Transform tr = meshpair.First;
						foreach (Vector3 vert in mesh.vertices) {
							//bottom check
							Vector3 worldVertPoint = tr.TransformPoint (vert);
							float bSqrDist = (downPoint - worldVertPoint).sqrMagnitude;
							if (bSqrDist < closestSqrDist) {
								closestSqrDist = bSqrDist;
								closestVert = worldVertPoint;
								downwardFurthestPart = p;

								// TODO: Not used at the moment, but we might infer amount of
								// TODO: upward movement from this
								// If this is a landing gear, account for suspension compression
								/*if (p.Modules.Contains ("ModuleWheelSuspension")) {
									ModuleWheelSuspension suspension = p.GetComponent<ModuleWheelSuspension> ();
									if (maxSuspensionTravel < suspension.suspensionDistance)
										maxSuspensionTravel = suspension.suspensionDistance;
									printDebug ("Suspension: dist=" + suspension.suspensionDistance + "; offset="
										+ suspension.suspensionOffset + "; pos=(" + suspension.suspensionPos.x + "; "
										+ suspension.suspensionPos.y + "; " + suspension.suspensionPos.z + ")");
								}*/
							}
							bSqrDist = (upPoint - worldVertPoint).sqrMagnitude;
							if (bSqrDist < farthestSqrDist) {
								farthestSqrDist = bSqrDist;
								farthestVert = worldVertPoint;
								upwardFurthestPart = p;
							}
						}
					}
				}

				bottomLength = Vector3.Project(closestVert - vessel.CoM, up).magnitude;
				localBottomPoint = vessel.transform.InverseTransformPoint(closestVert);
				topLength = Vector3.Project (farthestVert - vessel.CoM, up).magnitude;
				localTopPoint = vessel.transform.InverseTransformPoint (farthestVert);
				bottomPart = downwardFurthestPart;
				try {
					Log.detail("vessel = {0}; furthest downward part = {1}; upward part = {2}", vessel.name, downwardFurthestPart.name, upwardFurthestPart.name);
					Log.detail("vessel = {0}; bottomLength = {1}; bottomPoint = {2}; topLength = {3}; topPoint = {4}", vessel.name, bottomLength, bottomPoint, topLength, topPoint);
				}
				catch(Exception e) {
					Log.error(e, "Can't print vessel stats: {0}", e);
				}
			}
		}

		public class RayCastResult
		{
			public Collider collider;
			public float altitude;

			public RayCastResult() {
				collider = null;
				altitude = 0.0f;
			}

			public override string ToString() {

				return "(alt = " + altitude + "; collider = " + (collider != null ? collider.name : "no hit") + ")";
			}
		}

		private RayCastResult GetRaycastAltitude(Vessel v, Vector3 originPoint, int layerMask)
		{
			RaycastHit hit;
			Vector3 up = (v.transform.position - FlightGlobals.currentMainBody.transform.position).normalized;
			RayCastResult result = new RayCastResult ();
			if(Physics.Raycast(originPoint, -up, out hit, v.vesselRanges.landed.unload, layerMask))
			{
				Log.dbg( "{0}: raycast mask: {1}; hit collider: {2}", v.name, layerMask, hit.collider.name);
				result.altitude = Vector3.Project(hit.point - originPoint, up).magnitude;
				result.collider = hit.collider;
			}
			return result;
		}

		private bool checkExcludes(Vessel v) {
			foreach (Part p in v.parts) {
				if (excludeVessels.Contains(v.GetName())) {
					Log.detail($"Vessel {0} is in exclusion list", v.name);
					return true;
				}
				if (excludeParts.Contains(p.name))
				{
					Log.detail($"Part {0} is in exclusion list", p.name);
					return true;
				}
				if (
					p.Modules.GetModules<PartModule>().Any(m => excludePartModules.Contains(m.moduleName)))
				{
					Log.detail($"Part {0} contains PartModule from exclusion list", p.name);
					foreach (PartModule mod in p.Modules.GetModules<PartModule>().Where(m => excludePartModules.Contains(m.moduleName)))
					{
						Log.detail($"Excluded Module: {0}", mod.moduleName);
					}
					return true;
				}
				if (p.Modules.Contains("AirPark") && checkParked (p)) {
					return true;
				}
				// TODO: Check if there's KAS port in attached, but undocked state
				// TODO: Check if there's KAS winch in attached, but undocked state
			}
			return false;
		}

		private bool checkParked(Part p) {
			PartModule airpark = p.Modules ["AirPark"];
			var parked = airpark.Fields.GetValue ("Parked");
			return parked != null && (bool)parked;
		}

		// Find out if we're inside hangar
		private bool checkIfInsideHangar(Vessel v)
		{
			// RaycastAll to 6 directions, if two or more hit the same part and this part has
			// "Hangar" module, we're inside the hangar
			Vector3[] directions =
				{Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.back, Vector3.forward};

			int surrounding = 0;
			foreach (Vector3 vec in directions)
			{
				Part p = getClosestForeignPart(v, vec);
				if (p == null || !p.Modules.Contains("Hangar"))
				{
					continue;
				}
				surrounding ++;
			}

			if (surrounding < 2)
			{
				Log.detail("doesn't look like we're inside the hangar");
				return false;
			}

			Log.detail($"surrounded by hangar from {0} directions", surrounding);
			return true;
		}

		private Part getClosestForeignPart(Vessel v, Vector3 direction)
		{
			float distance = 1000f;
			Part closest = null;
			foreach (RaycastHit hit in Physics.RaycastAll(v.CoM, direction, 1000, 1))
			{
				Part p = hit.collider.gameObject.GetComponentInParent<Part>();
				if (p.vessel == v)
				{
					continue;
				}

				if (hit.distance < distance)
				{
					distance = hit.distance;
					closest = p;
				}
			}
			Log.detail($"closest part for direction {0}: {1}, distance: {2}", direction, closest, distance);
			return closest;
		}

		private void configure() {

			// FIXME: How do I use KSPField here for configuration?

			if (!SETTINGS.IsLoadable)
			{
				Asset.ConfigNode defaults = Asset.ConfigNode.For("WorldStabilizer", "settings.cfg");
				if (!defaults.IsLoadable)
				{
					Log.error("Where is the default settings.cfg? World Stabilizer will not work properly without it!");
					return;
				}
				SETTINGS.Clear();
				SETTINGS.Save(defaults.Load().Node);
			}

			ConfigNode config = SETTINGS.Load().Node;

			if (null == config)
				Log.error("config is null!");
			else
				Log.detail("config is ok");

			string nodeValue = config.GetValue ("stabilizationTicks");
			if (nodeValue != null)
				stabilizationTicks = Int32.Parse (nodeValue);

			nodeValue = config.GetValue ("groundingTicks");
			if (nodeValue != null)
				groundingTicks = Int32.Parse (nodeValue);

			nodeValue = config.GetValue ("minDownMovement");
			if (nodeValue != null)
				minDownMovement = float.Parse (nodeValue);

			nodeValue = config.GetValue ("maxUpMovement");
			if (nodeValue != null)
				maxUpMovement = float.Parse (nodeValue);

			nodeValue = config.GetValue ("upMovementStep");
			if (nodeValue != null)
				upMovementStep = float.Parse (nodeValue);

			nodeValue = config.GetValue ("stabilizeInPrelaunch");
			if (nodeValue != null)
				stabilizeInPrelaunch = Boolean.Parse (nodeValue);

			nodeValue = config.GetValue ("stabilizeKerbals");
			if (nodeValue != null)
				stabilizeKerbals = Boolean.Parse (nodeValue);

			nodeValue = config.GetValue ("recalculateBounds");
			if (nodeValue != null)
				recalculateBounds = Boolean.Parse (nodeValue);

			nodeValue = config.GetValue ("displayMessage");
			if (nodeValue != null)
				displayMessage = Boolean.Parse (nodeValue);

			nodeValue = config.GetValue ("drawPoints");
			if (nodeValue != null)
				drawPoints = Boolean.Parse (nodeValue);

			nodeValue = config.GetValue ("harpoonReattachTimeout");
			if (nodeValue != null)
				harpoonReattachTimeout = float.Parse (nodeValue);

			nodeValue = config.GetValue ("checkLandedPeriod");
			if (nodeValue != null)
				checkLandedPeriod = float.Parse (nodeValue);

			nodeValue = config.GetValue ("checkLandedTimeout");
			if (nodeValue != null)
				checkLandedTimeout = float.Parse (nodeValue);

			nodeValue = config.GetValue ("excludeVessels");
			if (nodeValue != null) {
				foreach(string exc in nodeValue.Split (','))
					excludeVessels.Add(exc.Trim());
			}
			nodeValue = config.GetValue ("excludeParts");
			if (nodeValue != null) {
				foreach(string exc in nodeValue.Split (','))
					excludeParts.Add(exc.Trim());
			}
			nodeValue = config.GetValue ("excludePartModules");
			if (nodeValue != null) {
				foreach(string exc in nodeValue.Split (','))
					excludePartModules.Add(exc.Trim());
			}
		}
	}
}
