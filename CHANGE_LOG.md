# WorldStabilizer :: Change Log

* 2018-0108: 0.7.1 (whale_2) for KSP 1.3.1
	+ Bugfix Release
	+ There should be a bit less explosions.
	+ Changes:
		- Fixed some part meshes being excluded when determining vessel bounds
* 2018-0103: 0.7 (whale_2) for KSP 1.3.1 
	+ First Public Release
	+ Expect some bugs and/or explosions. Backup your save prior to trying this out!
	+ Please report problems here or on KSP forums.
	+ Changes:
		- Debug is disabled by default
		- Added MiniAVC and .version file
		- Changed versioning
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
