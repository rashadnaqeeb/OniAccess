# Oil Refinery

Oil Refinery
Converts
Crude Oil
into
Petroleum
and
Natural Gas
.
Petroleum can only be produced from the refinement of crude oil.
Research
Fossil Fuels
Dimensions
4×4 tiles
Rotation
Mirrored
Category
Refinement
Power
-480 W
Heat
+
10 kDTU/s
Overheat
at
75 °C
/
167 °F
Decor
-10 (Radius: 2 tiles)
Piping
Input: Crude Oil
Output: Petroleum
Requires
Crude Oil
: -10 kg/s
Duplicant operation
Effects
Petroleum
: +5 kg/s @
75 °C
/
167 °F
+
Natural Gas
: +90 g/s @
75 °C
/
167 °F
+
Auto In
Green
: Enable
Red
: Disable
Other Properties
Susceptible to
flooding
Materials
Metal Ore
200 kg
Oil Refinery
converts
Crude Oil
into
Petroleum
in 2:1 ratio. Both input and main output use
Liquid Pipes
, and it will also emit
Natural Gas
to the environment. It has to be
operated
by
Duplicant
.
The building will be disabled when the surrounding pressure exceeds 5 kg/tile.
Both output resources have a minimal temperature of
75 °C
/
167 °F
, and will be hotter if input Crude Oil was above
75 °C
/
167 °F
.
It does not benefit from Machinery attribute or
Lit Workspace
as it outputs continuously at a fixed rate while being operated.
Ratios
The resulting Petroleum can be used either to generate power in
Petroleum Generator
, produce plastic in
Polymer Press
or as an essential ingredient to produce
Super Coolant
in a
Molecular Forge
.
One Oil refinery can supply up to:
6
Polymer Presses
, or
2.5
Petroleum Generators
.
By-product Natural Gas can be burned in
Natural Gas Generator
, Oil Refinery produces exactly enough to power one generator continuously. Counting a
Liquid Pump
at 100% uptime for the
Crude Oil
input, a
Gas Pump
at 18% uptime to pump
Natural Gas
(assuming no filtering is needed), the system consumes 763.2 W, which makes it slightly power-positive without any
Petroleum Generator
, as a Natural Gas Generator can produce 800 W.
Automation
It may be advisable to automate the Oil Refinery using a
Liquid Reservoir
so the operator will process large batches at a time. The Liquid Reservoir uses the same logic as a
Smart Battery
, sending a green signal when it needs filling, as such it can be wired directly to the Oil Refinery.
Heat
economy
Its conversion inefficiency becomes an enormous boon for the purposes of cooling. The refinery becomes net heat negative with ~
40 °C
/
104 °F
oil. Supplying it with
50 °C
/
122 °F
oil will erase 20% of input's heat (or ~160 kDTU/s in absolute numbers, as much as 2
AETN
). At
75 °C
/
167 °F
it raises to 46%, but there is no meaningful gain in efficiency (only absolute numbers) at temperatures above that.
See also
Guide/Petroleum Boiler
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
Algae Distiller
Bleach Stone Hopper
Brackwax Gleaner
Compost
Desalinator
Diamond Press
Emulsifier
Ethanol Distiller
Fertilizer Synthesizer
Glass Forge
Kiln
Metal Refinery
Molecular Forge
Oil Refinery
Oxylite Refinery
Plant Pulverizer
Plywood Press
Polymer Press
Rock Crusher
Sludge Press
Water Sieve
see all buildings
