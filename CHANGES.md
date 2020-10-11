# WorldStabilizer :: Changes

* 2019-1202: 0.9.7.1 (lisias) for KSP >= 1.3
	+ Using KSPe logging, installment checks, abstract file system facilities
	+ Making the thing working on from KSP 1.3 to the newest
		- Including handling correctly older and newer versions of KAS and KIS  
	+ Moving the whole shebang into the `net.lisias.ksp` "vendor" hierarchy.
	+ Moving user customisable settings into `<KSP_ROOT>Plugins` folder.
		- Automatically created on first run. 
	+ Merging all the fixes from the upstream:
		- 0.9.7  
			- Added configurable parts and modules excludes list
			- Fixes for Breaking Ground expansion - ground experiments are excluded from stabilizing
			- Also fixed Mk1 capsule not recognized as landed when launched from Level 1-2 pads
		- 0.9.6
			+ Some minor adjustments and 1.8.0 build
		- 0.9.5
			+ This release fixes rare case when you launch a craft from Hangar and it falls through the floor.
