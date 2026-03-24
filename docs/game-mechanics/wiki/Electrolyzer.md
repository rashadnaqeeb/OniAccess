# Electrolyzer

Electrolyzer
Converts
Water
into
Oxygen
and
Hydrogen
.
Becomes idle when the area reaches maximum pressure capacity.
Water goes in one end, life sustaining oxygen comes out the other.
Research
Air Systems
Dimensions
2×2 tiles
Category
Oxygen
Power
-120 W
Heat
+
1.25 kDTU/s
Overheat
at
75 °C
/
167 °F
Decor
-10 (Radius: 2 tiles)
Piping
Input: Water Intake Pipe
Requires
Water
: -1000 g/s
Storage Capacity
Water
: 2
kg
Effects
Oxygen
: +888 g/s @
70 °C
/
158 °F
+
Hydrogen
: +112 g/s  @
70 °C
/
158 °F
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
The
Electrolyzer
is a machine that uses electricity to split
Water
into its constituent
Oxygen
and
Hydrogen
.
Both outputs have a minimum temperature of
70 °C
/
158 °F
and will be hotter if the inputs were hotter.
Usage
Electrolyzers require more planning than other oxygen sources as not only do they require a water supply via
pipes
and
pumps
, but the hydrogen must also be handled. Another aspect is the temperature of the oxygen being created. The gas outputs are always
70 °C
/
158 °F
or hotter, so it is advised to cool the oxygen before using it in the core of the colony, or to cool the colony itself.
Electrolyzers may be used to meet a colony's Oxygen needs for as long as there is a source of water. The produced hydrogen will rise up and can be collected at ceilings using a
Gas Pump
. Alternately the electrolysis can be performed in a small air-tight room and the gases pumped out before the pressure rises above the electrolyzer's operational limit.  The hydrogen can then be separated from the oxygen using a
Gas Filter
and directed to a
Hydrogen Generator
, producing a small amount of power. The Oxygen can be distributed within the colony via
Gas Vents
or sent to other uses such as an
Atmo Suit Dock
,
Oxylite Refinery
, or a
Telescope
. Multiple vents can be used, instead of a single vent, to ensure that there is always a place for the Oxygen to go as an individual vent will stop releasing oxygen as soon the vent's environment has reached its operational the pressure limit. A
Geyser
may be used to provide water for an Electrolyzer-based oxygen supply.
To supply oxygen for 1 duplicant requires
112.5 g/s
, or
67.5 kg/cycle
of water used (assuming the electrolyzer works at 100% capacity).
Heat
economy
Consider cooling water as little as possible or even pre-heat it before delivering into electrolyzers. The input water has a higher specific heat capacity than the gases produced, which makes gases easier to cool down afterwards.
It gets better – assuming that the base is at
0 °C
/
32 °F
, 1 kg of mixed oxygen and hydrogen produced at
70 °C
/
158 °F
carries as much heat as 1 kg of water at
19.45 °C
/
67.01 °F
, which means that, when using water above
19.45 °C
/
67.01 °F
, the electrolysis becomes net heat negative, removing
4
.
1
7
9
kDTU/°C/s
(1 kg water per second
×
the specific heat of
water
) per degree centigrade above that), up to
70 °C
/
158 °F
when the outputs also begin to rise in temperature and the efficiency drops to around
3 kDTU/°C
. With near-boiling
96 °C
/
204.8 °F
water, the heat removed is ~
288
kDTU/s, which is comparable to 3.6
AETNs
.
Note, that electrolyzer will not process water but will instead release
steam
into the environment if the input is too hot.
As a corollary, feeding water colder than
19.45 °C
/
67.01 °F
will
create
heat.
That said, the
70 °C
/
158 °F
output is sure to cook any farm relying on a steady temperature of
30 °C
/
86 °F
or lower without any additional heat transfer/deletion. For each kilogram of water input, the electrolyzer outputs mass with a heat capacity of ~
1.161
DTU
g
∘
C
/
0.65
DTU
g
∘
F
. The hot hydrogen typically (1) remains in the system, (2) is deleted along with its heat by hydrogen generator(s), or (3) is later cooled by other means. Given that a duplicant consumes 100 g/s oxygen, cooling the total output of an electrolyzer requires
1
1
6
.
1
DTU/°C/s
per duplicant, or, for the
+40 °C
/
+72 °F
drop from
70 °C
/
158 °F
to
30 °C
/
86 °F
,
4.64 kDTU/s
. This is slightly less heat than is deleted by a single fertilized
Wheezewort
in oxygen  (~
5 kDTU/s
).
If we consider cooling only the oxygen, the heat change is
1
0
0
.
5
DTU/°C/s
per duplicant, or for the same
+40 °C
/
+72 °F
temperature change, ~
4 kDTU/s
. In contrast, cooling the corresponding input water (
112.6 g
) for the same temperature difference would require ~
18.8 kDTU/s
, ~4.7
×
more. In other words, cooling only the oxygen reduces the cooling needed by ~78.6%.
Mechanics
The Electrolyzer has a maximum pressure limit of
1.8 kg
. It checks the pressure of any
Gases
on the four tiles it occupies;
Liquids
are not counted toward this limit.
When below the pressure limit, the Electrolyzer emits both
Hydrogen
and
Oxygen
once per second on its top-left tile. Because a cell can only contain one element at a time, one of the gases is displaced to a neighboring cell. Emission prefers merging into existing gas in nearby cells, which can be used to influence which tiles the gases end up in.
Tips
The Electrolyzer is an
Algae
-free way of producing oxygen and should be researched before you run out of algae.
Filling the Electrolyzer with anything other than
Water
will cause damage to it. This limitation includes
Polluted Water
.
The Electrolyzer has a 10 kg internal reserve, which lasts for 10 seconds under ideal conditions.
The Electrolyzer will not remove
Food Poisoning
or
Slimelung
germs within the water, and will output the germs with the Oxygen. Both germs will gradually die out in Oxygen.
Using
High Pressure Gas Vents
will more efficiently spread oxygen in the base (however too high pressure will cause popped eardrums which give stress, to avoid this use automation to shut off the vent at a sufficiently high pressure
below 4kg
).
The Electrolyzer can be combined with the
Anti Entropy Thermo-Nullifier
to cool down its output for very little cost in
Hydrogen
. In addition, the AETN it can prevent the electrolyzer from overheating if the two are close enough.
Bugs
Placing the Electrolyzer with two
Gas Pumps
on the left and right sides of a
6
×
2
sealed room will result in significant amount of hydrogen disappearing, effectively producing only 75 g/s of hydrogen (about 66% of what it should be). The workaround is to make the room 3 tiles high (with pumps and electrolyzer placed on the ground).
Trivia
The tooltip is a reference to the movie "Cloudy with a Chance of Meatballs", in which a machine is invented that can turn water into food.
Blueprints
Available blueprints
Regal Neutronium Electrolyzer
History
U52-622222
: Insulated the Super Computer, Water Cooler, and Electrolyzer storages to prevent contents from freezing in cold environments.
See Also
Self-Powering Oxygen Machine
Hydra
Electrolyzer and pump layouts
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
Algae Terrarium
Carbon Skimmer
Deodorizer
Electrolyzer
Oxygen Diffuser
Oxylite Sconce
Rust Deoxidizer
Sublimation Station
see all buildings
