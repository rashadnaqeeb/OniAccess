# Mechanized Airlock

Mechanized Airlock
Blocks
Liquid
and
Gas
flow, maintaining pressure between areas.
Sets Duplicant Access Permissions for area restriction.
Functions as a
Manual Airlock
when no Power is available. Mechanized Airlocks open and close more quickly than other types of door.
Research
Decontamination
Dimensions
1×2 tiles
Rotation
Turnable
Category
Base
Power
-120 W
Decor
-10 (Radius: 2 tiles)
Auto In
Green
: Open
Red
: Close
Materials
Metal Ore
400 kg
Mechanized Airlock
creates an airtight seal that maintains atmosphere and pressure between areas when closed. It can also control
Duplicant
movement by acting as a one-way door, or by blocking specific dupes from passing.
The Mechanized Airlock requires
Power
to open and close at the faster rate. Without Power, the opening and closing of the airlock takes 3 seconds, which is ~1 second longer than a
Manual Airlock
. It only consumes Power during the open/close animation, and it will not draw any power while static.
Automation is still possible without power, but the door will open/close slower.
Usage
Mechanized Airlocks are an upgrade to Manual Airlocks. Upgrading manual airlocks to these in high traffic areas can help save some time opening the door. If the airlock is leading to an unbreathable area, its fast opening speed can be a safety measure in order to not have Duplicants suffocating while the door opens.
Automation
Mechanized Airlocks have a wide variety of uses outside of traffic control. They have a single input port; a green signal will open the door, while a red signal will close (and
lock
) the door.
An opened mechanized airlock will not transfer
heat
if there is no liquid or gas to occupy the space. This property can be combined with automation to control thermal conductivity between areas.
Mechanized Airlocks can also be used to push around gasses, liquids and items through a sequencer automation grid. This can be used to compress gasses and liquids beyond the densities allowed by
Vents
.
Matter will be destroyed if there is nowhere for it to go, however.
Automation Signal Details
It appears the simulation speed isn't perfectly deterministic and will change the required signal durations for which the door reliably triggers. All tests  & values found were done for the "Fast" simulation speed as any speed slower
should
be more reliable.
Minimum Green Signal time to reliably trigger an open: 0.4 Seconds
Minimum Red Signal time to reliably trigger a close: 0.6 Seconds
Fastest stable oscillation wavelength: 1.4 Seconds. As long as the above minimums are maintained, you can vary the duty cycle so as to have the door spend more time in the open position or the closed position. Decreasing the wavelength below 1.4 seconds will cause hiccups at faster game speed.
It is recommended to add an extra bit of time to each of these values for reliability in critical designs (such as a high pressure door pump.)
Note that this door has the same pathfinding breaking behavior as the
Pneumatic Door
.
Tips
This airlock, and all other doors can be built horizontally by rotating the blueprint. The default button is the letter: O
Manual and Mechanized Airlocks count as floor tiles when closed, but not when open.
Since Mechanized Airlocks can be opened and closed with Automation, buildings placed atop them can be effectively disabled/enabled without Duplicant intervention. This allows the automation of some buildings that do not have Automation inputs.
Manual Airlocks and Mechanized Airlocks do not get damaged by high
liquid
pressure, which makes them, alongside with
Airflow Tiles
, perfect for building large liquid tanks or liquid compressors.
Despite the name, the Airlock is not a fully-functional airlock, but rather an airtight door. Gases and liquids pass through it when opened. A proper airlock can be constructed by combing two Airlocks and some pumps.
Keeping airlocks unpowered and slow in this case can help minimize gas-leakage, as pump will have more time to catch and vent-out incoming gasses.
The sequencer automation for airlock pumps can be simplified
with the pipe-loop
.
When open, a Mechanized Airlock does not transfer heat. With automation, a Mechanized Airlock in a vacuum can be used as a "heat switch" to regulate heat transfer between two solid objects.
When used for heat transfer,
Steel
,
Niobium
and
Thermium
are the best building materials.
Aluminum Ore
and
Wolframite
are the only decent cheap options before Steel, since other
Metal Ore
s have poor thermal conductivity.
If placed in space, it will have the tooltip space exposure although it will not leak
liquid
or
gas
when opened.
Deconstructing a vertical mechanized airlock surrounded by
tiles
on the left, right and bottom will result in a natural tile, of the same material as the door, being created on the bottom block.
The mass of tile will be half the mass of the door.
This does not work for
Uranium Ore
,
Niobium
, or
Aluminum Ore
See Also
Pneumatic Door
Manual Airlock
Movement & Reach#Doors
v
·
d
Buildings
Base
Oxygen
Power
Food
Plumbing
Ventilation
Refinement
Medicine
Furniture
Stations
Utilities
Automation
Shipping
Rocketry
Radiation
Airflow Tile
Automatic Dispenser
Bunker Door
Bunker Tile
Carpeted Tile
Drywall
Fire Pole
Gas Reservoir
Insulated Door
Insulated Tile
Ladder
Liquid Reservoir
Manual Airlock
Mechanized Airlock
Mesh Tile
Metal Tile
Mini-Pod
Plastic Ladder
Plastic Tile
Pneumatic Door
Smart Storage Bin
Snow Tile
Storage Bin
Storage Tile
Tile
Transit Tube
Transit Tube Access
Transit Tube Crossing
Wicker Door
Window Tile
Wood Tile
see all buildings
