# Gas Filter

Gas Filter
Sieves one Gas from the air, sending it into a dedicated Pipe.
All gases are sent into the building's output pipe, except the gas chosen for filtering.
Research
Filtration
Dimensions
3×1 tiles
Rotation
Turnable
Category
Ventilation
Power
-120 W
Overheat
at
75 °C
/
167 °F
Decor
-5 (Radius: 1 tile)
Ventilation
Input: Gas Intake
Output: Filtered Gas Output
Other Gas Output
Auto In
Green
: Enable
Red
: Disable
Materials
Metal Ore
50 kg
Gas Filter
sieves the selected
Gas
packets out of mixed input, sending them into a separate
pipe
.
Usage
Gas Filter has 2 outputs, the standard green output and another orange output in the center. The orange output is for the gas that was selected to be filtered out, and the green is for all the other gases that come through the filter. For example, using a
Gas Pump
, you may send the mixture of gases produced by an
Electrolyzer
into the gas filter and send
Hydrogen
to a separate pipe for use in a
Hydrogen Generator
.
It only draws Power while it is in use (that is, has gas flowing through it).  This means it is optimal to sort out the expected gasses first. For example: an air filtration system that primally finds oxygen with
CO2
and
polluted oxygen
sometimes should have an oxygen Gas Filter first and then the second most common gas then the third.  Note: you can work out how common a given gas is by
pumping
gasses from the area into
Gas Reservoir
until full then looking at the contents.
Mid/Late game alternatives
(#1) Cheap filter
A better alternative become available once you have access to
Refined Metal
and have unlocked the following
research
:
Automatic Control
,
HVAC
and
Improved Ventilation
.
As you get further into the game
power
becomes increasingly important to conserve, often a base at this stage will have quite a few gas filters that are each draining 120W while in use this creates a noticeable drain on the system.  A solution to this is to replace gas filters with the setup #1,  this will check if a
gas
incoming from the left is a particular
gas
and if so turn off the
shutoff
filtering it.  This setup only requires 10W per filter, a massive improvement.
Limitations
Whilst the above system is cheaper it has an important limitation to be aware of:
(#2) A fully substituted gas filter
If power is lost the filter will start passing gas without filtering it, rather than stop passing gas completely as the Gas filter would.
There is a chance that the filter will not work correctly if the pipe is backed up (full). Because the detection happens before the gas gets filtered, the automation may activate with the correct gas sitting on the element sensor, allowing the wrong gas behind it to pass through.
To solve the power loss issue you can add a
Smart Battery
(Requiring
Generic Sensors
) set to: Standby 40%, Active 20% connected to a
not gate
connected to a
Gas Shutoff
at the start of the filter mechanism,  this will then prevent the filter from trying to filter if there is insufficient power to do so.  As shown to the right (#2).  If this new design is used then 2 things also become considerations:  The filter now takes up significant space and, the whole design now costs 20W per sort.  In response to the first concern unless space is very low the
power
saved is normally worth more than the lost space as and as for the second, this problem can be mitigated by putting multiple filters next to each other (as only one power cut off is required for the whole system) and regardless it is better than using actual
gas filters
.
To solve the backed up pipe issue you can let the backed up pipe run through a gas reservoir and connect it to the main shutoff valve with the Smart Battery through an AND gate.
Overall this is normally a good improvement on the gas filter for most players.
There is also an earlier, more rudimentary
gas filter
using only bridges and a gas valve, with the advantage of never under any circumstance outputting the wrong gas. The downside is that it is more voluminous and must be primed with an initial packet of the gas being filtered.
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
Canister Drainer
Canister Emptier
Canister Filler
Gas Bridge
Gas Filter
Gas Meter Valve
Gas Pipe
Gas Pipe Element Sensor
Gas Pipe Germ Sensor
Gas Pipe Thermo Sensor
Gas Pump
Gas Rocket Port Loader
Gas Rocket Port Unloader
Gas Shutoff
Gas Valve
Gas Vent
High Pressure Gas Vent
Insulated Gas Pipe
Mini Gas Pump
Radiant Gas Pipe
see all buildings
