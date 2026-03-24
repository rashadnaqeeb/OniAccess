# Guide/Refinery Cooling

This guide demonstrates options to deal with the massive heat produced by
refining metals
.
Self-Cooled Steam Turbine
Unlike regular cooling designs with Aquatuners, this self-cooled design barely requires any extra power. It also doesn't require any further attention or tweaking once it's set up. It allows for a slower, but safe Metal Refinery use, without limit.
Metal Refinery + SCST
An alternative design can be found [
here
].
Construction
Steam Turbine and Steam room
Put
Hydrogen Gas
(
2,000 g
+) inside the Steam Turbine room, or any other way to conduct heat with the Steam Turbine (e.g. liquids).
The Steam room is to be vacuumed, then put at least
400 kg
Water on the 5 tiles at the bottom (in order to reach
100 kg
Steam pressure in the whole room).
Metal Refinery room
Liquid Pipe Thermo Sensor
set to "Above
135 °C
/
275 °F
", and is linked with automation to the
Liquid Shutoff
.
The coolant used should have the highest temperature range & SHC, as well as the heat added being below
125 °C
/
257 °F
+ their boiling point. For move information, check
this page
.
^ To that end, using
Crude Oil
is recommended early game. It can be replaced by
Petroleum
later on, or better:
Naphtha
, even though any of those 3 are equivalent for this design.
Only fill the Metal Refinery with the coolant (
800 kg
), not the
Liquid Reservoir
. The latter is used as a buffer inside the cooling loop.
Insulated Tiles are put below the Liquid Reservoir in order to limit heat exchange with the surroundings.
[OPTIONAL] Put a light source linked to a
Duplicant Motion Sensor
above the Metal Refinery in order to have the "Lit Workspace" bonus (+15% speed).
Operation
Once the Metal Refinery has finished, its coolant will stay within the Steam Room until it reaches
135 °C
/
275 °F
thanks to the automation.
Once cooled enough, it goes back into the Metal Refinery, ready to be used again.
No power is consumed if the Metal Refinery is not used, since the Liquid Shutoff is only active when a coolant is actively cooling.
The Steam Turbine will generate around 330 W extra power while active.
Note: the Metal Refinery itself, like most powered buildings, will also slowly heat over time. However, this is out of this guide's scope.
Blueprint
Using the
Blueprints Expanded
mod, you can directly build the above example by putting the following file into the
C:\Users\%USERNAME%\Documents\Klei\OxygenNotIncluded\blueprints
directory:
metal refinery scst.blueprint
