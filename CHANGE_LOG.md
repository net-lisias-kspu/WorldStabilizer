# WorldStabilizer :: Change Log

* 2017-1217: 0.6 (whale_2) for KSP 1.3.1 PRE-RELEASE
	+ Rewrote whole altitude detection system
	+ Added debug markers for vessel top and bottom points
	+ Added config file
	+ Added vessel exclusions
* 2017-1214: 0.5 (whale_2) for KSP 1.3.1 PRE-RELEASE
	+ More safeguarding, as for some reason we can't reliably determine altitude 100% of the time
	+ Craft still can explode, use with caution.
* 2017-1211: 0.4 (whale_2) for KSP 1.3.1 PRE-RELEASE
	+ Excluded meshes without colliders from size calculations, but forced inclusion of wheels. For some reasons wheel meshes have no colliders attached to them. Some more safeguarding against going underground.
* 2017-1210: 0.3 (whale_2) for KSP 1.3.1 PRE-RELEASE
	+ More safeguarding 
* 2017-1210: 0.2 (whale_2) for KSP 1.3.1 PRE-RELEASE
	+ Tried to work around mysteriois situation where raycast from bottom point does not hit the ground, but hits something beneath.
	+ Excluded KerbalEVA from stabilizing.
	+ Tweaked some safety params - 110% up, 90% down.
* 2017-1210: 0.1 (whale_2) for KSP 1.3.1 PRE-RELEASE
	+ Support for KAS harpoons, Hangar anchors. Raising up by minimum safe height.
	+ Still highly experimental.
	+ NOTE:
		- In previous pre-release, 0.0, I've made a type - there's directory GameData/WorlStabilizer - without letter "d". Make sure you delete this old dir when installing 0.1.
* 2017-1210: 0.0 (whale_2) for KSP 1.3.1 PRE-RELEASE
	+ Alpha Version
