# WorldStabilizer :: Change Log

* 2019-1202: 0.9.7 (whale_2) for KSP 1.7.3
	+ Added configurable parts and modules excludes list
	+ Fixes for Breaking Ground expansion - ground experiments are excluded from stabilizing
	+ Also fixed Mk1 capsule not recognized as landed when launched from Level 1-2 pads
* 2019-1022: 0.9.6 (whale_2) for KSP 1.7.3
	+ Some minor adjustments and 1.8.0 build
* 2019-0807: 0.9.5 (whale_2) for KSP 1.7.3
	+ This release fixes rare case when you launch a craft from Hangar and it falls through the floor.
	+ The fix work in following way - vessel just launched from Hangar is not being stabilized (so far it appears stable enough so doesn't need any additional babysitting).
* 2019-0321: 0.9.4 (whale_2) for KSP 1.5
	+ Now supporting multi-build for several game versions. Chances are I broke CKAN and/or AVC with this release.
* 2018-1020: 0.9.3 (whale_2) for KSP 1.5
	+ Recompile against 1.5.1, AVC version update
* 2019-0519: 0.9.4.1 (lisias) for KSP >= 1.4
	+  Updating some values to cope to KSP 1.5.1
	+  Bumping version to match upstream's.
* 2018-0802: 0.9.2.1 (lisias) for KSP 1.4.x
	+  Moving settings into <KSP_ROOT>/PluginData .
* 2018-0510: 0.9.2 (whale_2) for KSP 1.4.3
	+ Recompile against 1.4.3
	+ AVC version update
	+ very minor logging tweaks. 
* 2018-0408: 0.9.1 (whale_2) for KSP 1.4.2
	+ Compatibility Release
	+ Although the bouncing bug was almost fixed in KSP 1.4.x, there are still some odd situations where it happens, according to several reports. This release does not add features or bugfixes, it's just a recompile against 1.4.2 DLLs and correct version for MiniAVC. 
* 2018-0319: 0.9.0 beta (whale_2) for KSP 1.4.x
	+ Seems like some odd situations are still not very well addressed by Squad.
	+ Particularly, a mix of low detail terrain setting and edge of biomes or colliders. 
* 2018-0316: 0.8.4 (whale_2) for KSP 1.3.1
	+ Probably final release for 1.3.1
	+ The 1.4.1 is here and from the first glance it works much smoother - I was not able to find something to complain about regarding bouncing on vessel unpacking. Some unorthodox stuff like mooring vessels with KAS still might be tricky, but we'll see it later.
	+ Changes:
		- Addressed a bug when vessel is placed under the roof. See [Forum Thread](https://forum.kerbalspaceprogram.com/index.php?/topic/169206-131-worldstabilizer-bugfix-for-vessels-bouncing-on-scene-load/&do=findComment&comment=3310632)
		- Some more refactoring done
* 2018-0114: 0.8.3 (whale_2) for KSP 1.3.1
	+ Bugfix Release
	+ Changes
		- Previous version introduced hard dependency on specific KAS/KIS version. This versions drops the dependency
		- Some black magic around moored vessels has a slight chance to work. I.e. vessel, moored by KAS harpoon and winch more often stays moored and intact than not.
* 2018-0113: 0.8.2 (whale_2) for KSP 1.3.1
	+ Bugfix Release
		- Please delete old version before updating to this
	+ Changes
		- KIS/KAS blocks and Hangars should not float above the surface any more
		- Reworked the way vessels, moored by harpoons, are treated - should be more stable
		- MiniAVC should work properly now.
* 2018-0110: 0.8.1 (whale_2) for KSP 1.3.1
	+ Bugfix Release
	+ Changes:
		- Fixed ignoring of actual attachment state of KIS/KAS pylon.
* 2018-0110: 0.8 (whale_2) for KSP 1.3.1
	+ Support For KIS/KAS pylons
	+ Changes:
		- If the vessel contains KIS/KAS pylons with staticAttach = true, i.e they have a physical connection to the ground, they will be detached before raising the vessel and re-attached when stabilization ends.
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
