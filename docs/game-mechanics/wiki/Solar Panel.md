# Solar Panel

Solar Panel
Converts Sunlight into electrical
Power
.
Must be exposed to space.
Solar panels convert high intensity Sunlight into power and produce zero waste.
Research
Renewable Energy
Dimensions
7×3 tiles
Category
Power
Power
+
380 W
Overheat
at
75 °C
/
167 °F
Decor
-15 (Radius: 3 tiles)
Requires
Light access from above
Other Properties
Susceptible to
flooding
Materials
Glass
200 kg
Solar Panel
is a building that converts
light
into
power
. It generates
0.00053 W
for every Lux absorbed, up to a maximum output of
380 W
. This cap is reached when the panel receives a total illumination of
716,981
Lux across its tiles. Because it can absorb light from 14 tiles (a 7×2 area), maximum output occurs at an average illumination of
51,213 Lux (Brilliant)
per tile.
It has two separate panels and one panel is three tiles wide, however they have no effect on the functionality.
The solar panel’s foundation tiles are made of
Vacuum
. They are impermeable to gases and liquids, and they allow Duplicants to walk across them. They do not block radiation, but they block all light.
Usage
While their mechanics are straightforward, in the base game, using Solar Panels requires heavy investment in infrastructure.
Solar Panels cannot withstand
Meteor
strikes,
overheat
as easily as generic buildings, and there is no building materials that could mitigate it. This means they have to be protected both from direct damage and hot materials falling on them.
Transparent tiles, such as
Window
,
Airflow
or
Mesh
can be used to separate hot
Regolith
from Solar Panels. However by themselves they cannot withstand Meteor hits either and will require a lot of manual labor and constant material investment to regularly break up solid
Regolith
tiles and fix accumulated damage.
Window Tiles also partially block light, reducing Solar Panel's efficiency by about 10%. They also soak
heat
from
regolith
and might transfer it to machinery.
Robo-Miners
can help with
regolith
dismantling, but will require cooling and protection themselves.
Bunker Doors
can serve as reliable upper line of defense, but they require
automation
setup involving
Space Scanners
, which are complicated by themselves.
Meteor storms also decrease the amount of solar power generated each
cycle
, because either accumulated
regolith
tiles or closed Bunker Doors will block panels from receiving light. Badly timed meteor shower can block almost an entire cycle's worth of sunlight.
The second issue with solar power is its cyclical nature. If there are no other power sources to use as backup, each Solar Panel would need 9
Smart Batteries
or 5
Jumbo Batteries
to compensate for the worst-case downtime.
More on Solar output
.
[
confirmation?
]
Mechanics
It generates
0.00053 W
for every Lux absorbed, up to a maximum output of
380 W
. This cap is reached when the panel receives a total illumination of
716,981
Lux across its tiles. Because it can absorb light from 14 tiles (a 7×2 area), maximum output occurs at an average illumination of
51,213 Lux (Brilliant)
per tile. For example, a lux of
10,000
Lux on each of the solar panel's 14 tiles sums to a total illuminance of
140,000
Lux. This produces a power output of
74.2 W
.
Illuminance from sunlight throughout a cycle
The day-night cycle results in a varying intensity of sunlight, ranging from 0 lux at night to a peak of 80,000 lux at midday (see
Light
). The illuminance on a single tile from sunlight at any given time in a 600 second cycle can be determined by the following:
E
v
=
max
⁡
(
8
0
0
0
0
l
x
⋅
sin
⁡
(
π
⋅
τ
5
2
5
s
)
,
0
)
Where
E
v
is the illuminance (in lux) that falls on each tile from sunlight, and
τ
is the time of day in seconds, ranging from 0 to 600 for each cycle.
Average power across a cycle using sunlight
For a solar panel in unobstructed direct sunlight (e.g. in the vacuum of space with no light-blockers above it), its power output will vary as solar illuminance changes with the cycle.
At night, solar panels produce no power.
During the morning, solar panels ramp up power until solar illuminance reaches 51,213 lux, at which point the maximum output of 380 W is reached.
Solar illuminance continues to rise to a peak of 80,000 lux at midday, but solar panels will continue to produce their maximum output of 380 W.
Solar illuminance begins dropping after midday. Once illuminance drops below 51,213 lux, the power produced by solar panels begins to ramp down until nighttime.
This assumes no interruption in solar illumination due to meteor showers, which is the default case in the
Spaced Out DLC
.
Combining the above formulae, the instantaneous power output as a function of time-in-cycle
τ
can be modeled as:
P
(
τ
)
=
min
⁡
(
max
⁡
(
(
0
.
0
0
7
4
2
E
max
)
sin
⁡
(
π
τ
5
2
5
s
)
,
0
)
,
3
8
0
)
where
τ
is the time (in seconds,
0
≤
τ
≤
6
0
0
) since the start of the cycle,
E
max
is the peak illuminance per cell (e.g.
8
0
,
0
0
0
lux for sunlight), and
0
.
0
0
7
4
2
=
1
4
×
0
.
0
0
0
5
3
is the combined luminance from totaling all 14 cells.
Since the game simulation typically runs every
0
.
2
s
, the total energy produced by one solar panel over a single cycle is computed as a discrete sum:
E
=
(
0
.
2
s
)
∑
n
=
0
2
9
9
9
P
(
0
.
2
n
)
Example:
A solar panel on a base game asteroid has a peak illuminance per cell
E
max
of
80,000
Lux. Thus, the power generation function is
P
(
τ
)
=
min
⁡
(
max
⁡
(
(
5
9
3
.
6
)
sin
⁡
(
π
τ
5
2
5
s
)
,
0
)
,
3
8
0
)
And taking the
(sum)
over the entire cycle gives
≈
1
5
7
2
4
8
J
A single unobstructed solar panel produces an
average power output of
∼
2
6
2
W
over a cycle
.
Average power output for various asteroids
Using the above formula, we compute the average power production depending on the differing peak illuminance for each asteroid.
Solar panel average output based on peak sunlight
Peak Sunlight
Solar Panel Average Power
10,000 lux
41.33 W
20,000 lux
82.67 W
30,000 lux
124.00 W
35.000 lux
144.66 W
40,000 lux
165.33 W
50,000 lux
206.66 W
60,000 lux
234.79 W
80,000 lux
262.08 W
100,000 lux
277.00 W
120,000 lux
286.60 W
Maximizing Harvested Solar Power
A solar panel is limited to 380 W max; which occurs when there's a total of 716,981 lux hitting it (or, 51,213 lux on each of the 14 tiles). If you have more lux hitting the solar panel then that extra light is wasted. Stacking solar panels in a stair or pyramid configuration avoids this, at the cost of extra material and space.
For the base game, sunlight gets up to
80,000
Lux. To avoid the 380 W power limit, the exposed tiles of the solar panel should be no wider than
1
2
×
7
1
6
9
8
1
8
0
0
0
0
=
4
.
4
8
tiles.
For Spaced Out, all the asteroids have different intensities of sunlight, but the calculation is the same. For Example:
for a superconductive asteroid with
100,000
Lux it's no more than
1
2
×
7
1
6
9
8
1
1
0
0
0
0
0
=
3
.
5
8
tiles.
for any asteroid with 51,213 Lux or less, no light is ever wasted, therefore stacking provides no benefit.
Determining storage capacity to last the night
A grid containing both solar panels and fuel-based electricity can automate the power generators using
Smart Batteries
to save fuel. This allows solar power to be fully integrated with very little energy storage.
If no other form of energy generation is present, then to provide a constant 262 W of power per solar panel throughout an entire cycle, power must be stored during the day so it can be released during the night when solar power production stops.
The total energy storage required (in joules) is equal to the
excess
energy generated during the day beyond the 262 W average. Or, equivalently, it is equal to the
deficit
of solar energy below the average 262 W due to the late evening / night / early morning when solar power production drops.
To compute the energy generated during the day in excess of the 262 W average, it is sufficient to consider the energy generated
within
the average 262 W envelope, and subtract it from the total energy generated during the cycle.
Using the average power generating function
P
m
(
τ
)
=
min
⁡
(
max
⁡
(
(
0
.
0
0
7
4
2
E
max
)
sin
⁡
(
π
τ
5
2
5
s
)
,
0
)
,
2
6
2
)
We have the discrete sum
E
=
(
0
.
2
s
)
∑
n
=
0
2
9
9
9
P
m
(
0
.
2
n
)
≈
1
1
7
9
2
1
Subtracting from the total power gives:
E
b
a
t
t
=
1
5
7
2
4
8
J
−
1
1
7
9
2
1
J
=
3
9
3
2
7
J
That is, for a single solar panel with a constant unobstructed view of space, you need
39.3 kJ of energy storage
to store enough energy to supply a constant 262 W throughout the cycle. Accounting for battery runoff, this is very close to the storage provided by two
Smart Batteries
, or (less optimally) one
Jumbo Battery
.
How many batteries do I need?
In the
Spaced Out DLC
, most asteroids do not experience meteor showers. Therefore, solar panels can achieve an unobstructed view of space 100% of the time. In this case, two
Smart Batteries
or one
Jumbo Battery
per solar panel should be approximately sufficient to provide a constant supply of power, even throughout the night.
In the
base game
, an unlucky meteor shower can block off access to space for an entire cycle or more. To account for a worst-case scenario (i.e. having to store an entire cycle's worth of solar power), you will need a storage capacity of at least 158 kJ per solar panel. Accounting for battery runoff, this is equivalent to 9
Smart Batteries
or 5
Jumbo Batteries
.
In both cases, these calculations assume an
unobstructed view of space
. In cases where solar panels are partially blocked (for example if vertically overlapping solar panels to maximize efficiency) you may need greater or fewer batteries to compensate, depending on the level of obstruction.
Tips
Despite the flavor text, Solar Panels convert any lux into power. Solar Panels can convert
Shine Bugs
light into electricity as well, although this only gives about 8-10W per Shine Bug, and only when they are very close to the Solar Panel. This can be used for
Shine Bug Reactors
.
Because sunlight isn't constant, a optimal solar panel placed in the sun over the course of one cycle will generate approximately
262 W
on average.
In the
Spaced Out DLC
, some asteroids lack damaging meteors making Solar Panels an easy and free power. However, the player must also take into account the max lux level for each planetoid in Spaced Out, as Lux level varies from planetoid to planetoid. Many of the DLC's POI have deconstructable
Window Tiles
to easily get the required Glass for construction. The Orbital Cargo Module can be used to transport 600 kg of Glass to a Planetoid without a Rocket Platform, allowing a new colony to build 3 solar panels right off the bat.
Solar Panels cannot be tuned up from
Microchips
.
Solar Panels can break, but cannot be destroyed. Broken solar panels do not produce power, but can still be used as protection against damaging meteors.
As it's foundation is Vacuum, it acts as a perfect, albeit unconventional, insulator between the tiles below and above the panel.
Another unconventional way of using solar panels is for placing
Heavi-Watt Conductive Wire
/
Heavi-Watt Wire
inside its foundation tile in lieu of a
Heavi-Watt Conductive Joint Plate
/
Heavi-Watt Joint Plate
.
Bugs
Solar Panels vastly over-reports its power production for
Super-Sustainable
when it is not connected to a consumer but exposed to any amounts of light. This can trivialize the achievement, as each panel reports
1.9 kW
regardless of the actual power production.
Gallery
A working solar panel
This panel is generating 380 W even if it is partially blocked. Because the light is bright enough to let the other portions work at maximum efficiency.
History
U50-582745
: Fixed missing glass string Solar Panel database entry.
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
Battery
Coal Generator
Compact Discharger
Conductive Wire
Conductive Wire Bridge
Dev Generator
Heavi-Watt Conductive Joint Plate
Heavi-Watt Conductive Wire
Heavi-Watt Joint Plate
Heavi-Watt Wire
Hydrogen Generator
Jumbo Battery
Large Discharger
Large Power Transformer
Manual Generator
Natural Gas Generator
Peat Burner
Petroleum Generator
Power Bank Charger
Power Shutoff
Power Transformer
Smart Battery
Solar Panel
Steam Turbine
Switch
Wire
Wire Bridge
Wood Burner
see all buildings
