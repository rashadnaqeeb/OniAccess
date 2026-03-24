# Geyser

Geysers
is the umbrella term used for Vents/Fumaroles, Geysers, Volcanoes and Fissures. They emit elements at variable intervals, and provide a sustainable source of
material
, albeit in typically low volume.
When active, they alternate between erupting and idle. The types of geysers spawned on the map ultimately decide the economy of the colony.
A
Duplicant
with the Field Research
skill
can analyze a geyser in order to figure out its activity duration and dormancy duration. This will also produce
Data Banks
.
The
Starmap
displays which geysers are located on each
planetoid
, including the starting one.
Geysers can be boosted by one or multiple
Geotuner
stations. This will increase the mass and temperature outputs of the geyser at the cost of various resources.
Dealing with Geysers
Many geysers periodically output a large volume of either
heat
or troublesome elements when uncovered.
A geyser is considered covered if the tile two tiles above the middle left
Neutronium
tile of the geyser's base is covered. In this state it does not emit anything and cannot be analyzed. Digging out other geyser tiles is safe and can be used to determine the type of the geyser.
A Geyser's type can also be checked by setting their priority to yellow alert with the Priority Tool, then hovering over the yellow alert.
A buried geyser in the frozen biome.
An uncovered geyser in the frozen biome.
A geyser blocked by the only block checked.
Low temperature geysers can be contained with a simple
Insulated Tile
box. Volcanoes require at least inner
obsidian
box (most other materials will eventually melt) and some form of
vacuum
heat isolation in the long-term.
All geysers' eruptions can be halted by submerging their second bottom row in liquid. Low pressure geysers can be drowned even by gases, usually they do it to themselves. Partial drowning can also help cool down and recover metals.
Output Distribution
All geyser types randomly select an average active yield, eruption periods and percentages, and active periods and percentages; these values are picked independently.
These values are then remapped based on this formula (
graph by Google
):
f
(
x
)
=
−
log
e
(
1
0
.
9
9
5
0
5
4
7
5
4
x
+
0
.
0
0
2
4
7
2
6
2
3
−
1
)
+
6
1
2
where x is a random value from 0 to 1. This leads to a probability density distribution of on this formula:
p
X
(
x
)
=
1
2
⋅
e
1
2
x
−
6
0
.
9
9
5
0
5
4
7
5
4
⋅
(
e
1
2
x
−
6
+
1
)
2
A randomly generated value has an 50% chance of being in the middle 20% (exactly between 0.409 and 0.591), and a 90% chance of falling in the middle 50% (exactly between 0.258 and 0.742).
The remapped random value is then applied to the range of a geyser. For example: the
Cool Slush Geyser
has a range between 1000 and 2000 kg/cycle while active, but half of all cool slush geysers will fall between 1409 and 1591 kg/cycle while active.
Geyser variants
Possible geyser variants and their parameters are listed in the table below, along with the element they produce at which temperature, at which atmospheric or liquid pressure they clog. As well as expected average yield and their
activity cycle
. From these values the emission rate can be calculated as multiplying by the ratio of activity cycle over activity percentage (and dividing by
600
s
cycle
).
For example:
a
Gold Volcano
with
an
activity cycle
lasting
800 s
,
a
5 %
activity percentage
, and
an
average active yield
of
1,000
kg
cycle
Its emission rate will come to
26.67
kg
s
for
40 s
every
800 s
.
Meanwhile all geysers (with the exception of the leaky oil fissure) will have a dormancy cycle between
15000 s
and
135000 s
, or
25 cycles
and
225 cycles
in which they are active between
40 %
and
80 %
of the time and
dormant
for the remainder. Therefore, to calculate a geyser's lifetime average output one can simply multiply the average active yield by the active percentage of the dormancy cycle. The calculation of derived values is further discussed
below
.
Displaystyle range:
Unit style:
Chance of encountering such a geyser:
Name
Produced Element
Temp
P
MAX
Average Active Yield
kg/cycle
Average Lifetime Yield
kg/cycle
Activity Cycle (sec)
Active Percentage
Water Geyser
Water
95 °C
/
203 °F
500
3000 (±1000)
1800 (±1200)
600 (±540)
50% (±40%)
Cool Slush Geyser
Polluted Water
-10 °C
/
14 °F
500
1500 (±500)
900 (±600)
600 (±540)
50% (±40%)
Cool Steam Vent
Steam
110 °C
/
230 °F
5
1500 (±500)
900 (±600)
600 (±540)
50% (±40%)
Salt Water Geyser
Salt Water
95 °C
/
203 °F
500
3000 (±1000)
1800 (±1200)
600 (±540)
50% (±40%)
Steam Vent
Steam
500 °C
/
932 °F
5
750 (±250)
450 (±300)
600 (±540)
50% (±40%)
Polluted Water Vent
Polluted Water
with
Food Poisoning
30 °C
/
86 °F
500
3000 (±1000)
1800 (±1200)
600 (±540)
50% (±40%)
Cool Salt Slush Geyser
Brine
-10 °C
/
14 °F
500
1500 (±500)
900 (±600)
600 (±540)
50% (±40%)
Copper Volcano
Molten Copper
2226.85 °C
/
4040.33 °F
150
300 (±100)
180 (±120)
780 (±300)
5.83% (±4.17%)
Iron Volcano
Molten Iron
2526.85 °C
/
4580.33 °F
150
300 (±100)
180 (±120)
780 (±300)
5.83% (±4.17%)
Gold Volcano
Molten Gold
2626.85 °C
/
4760.33 °F
150
300 (±100)
180 (±120)
780 (±300)
5.83% (±4.17%)
Aluminum Volcano
Molten Aluminum
1726.85 °C
/
3140.33 °F
150
300 (±100)
180 (±120)
780 (±300)
5.83% (±4.17%)
Tungsten Volcano
Molten Tungsten
3726.85 °C
/
6740.33 °F
150
300 (±100)
180 (±120)
780 (±300)
5.83% (±4.17%)
Cobalt Volcano
Molten Cobalt
2226.85 °C
/
4040.33 °F
150
300 (±100)
180 (±120)
780 (±300)
5.83% (±4.17%)
Niobium Volcano
Molten Niobium
3226.85 °C
/
5840.33 °F
150
1200 (±400)
720 (±480)
9000 (±3000)
0.75% (±0.25%)
Volcano
Magma
1726.85 °C
/
3140.33 °F
150
1200 (±400)
720 (±480)
9000 (±3000)
0.75% (±0.25%)
Minor Volcano
Magma
1726.85 °C
/
3140.33 °F
150
600 (±200)
360 (±240)
9000 (±3000)
0.75% (±0.25%)
Hot Polluted Oxygen Vent
Polluted Oxygen
500 °C
/
932 °F
5
105 (±35)
63 (±42)
600 (±540)
50% (±40%)
Liquid Sulfur Geyser
Liquid Sulfur
165.2 °C
/
329.36 °F
500
1500 (±500)
900 (±600)
600 (±540)
50% (±40%)
Hydrogen Vent
Hydrogen Gas
500 °C
/
932 °F
5
105 (±35)
63 (±42)
600 (±540)
50% (±40%)
Natural Gas Geyser
Natural Gas
150 °C
/
302 °F
5
105 (±35)
63 (±42)
600 (±540)
50% (±40%)
Leaky Oil Fissure
*
Crude Oil
326.85 °C
/
620.33 °F
50
126 (±124)
75 (±100)
300 (±200)
60% (±20%)
Infectious Polluted Oxygen Vent
Polluted Oxygen
with
Slimelung
60 °C
/
140 °F
5
105 (±35)
63 (±42)
600 (±540)
50% (±40%)
Chlorine Gas Vent
Chlorine Gas
60 °C
/
140 °F
5
105 (±35)
63 (±42)
600 (±540)
50% (±40%)
Cool Chlorine Gas Vent
Chlorine Gas
5 °C
/
41 °F
5
105 (±35)
63 (±42)
600 (±540)
50% (±40%)
Carbon Dioxide Vent
Carbon Dioxide
500 °C
/
932 °F
5
105 (±35)
63 (±42)
600 (±540)
50% (±40%)
Carbon Dioxide Geyser
Liquid Carbon Dioxide
-55.15 °C
/
-67.27 °F
50
150 (±50)
90 (±60)
600 (±540)
50% (±40%)
* The
Leaky Oil Fissure
works differently than other geysers as it is 100% active during an activity cycle. The values stated for it's activity cycle here are instead the dormancy cycle, which is unique for the Leaky Oil Fissure.
Derived Calculations
A geyser's
average active yield
(one of their main randomized parameters) is not shown directly to players; nonetheless, it can be derived quite easily using only the geyser's average overall yield (shown in a fully-analyzed geyser's information window under "average output") and its active amount percentage (which is shown under "active period" - e.g. a geyser active for 48.9 cycles every 76.8 cycles has an active amount percentage of roughly 63.7%):
Δ
a
c
t
i
v
e
=
Δ
a
v
g
%
a
c
t
i
v
e
For example: a Copper Volcano which is active for 48.9 cycles every 76.8 cycles and emits 311.8 g/s on average will have an active yield of 311.8 / 0.637 ≈ 490 g/s; multiplying by 600 gives about 293.7 kg/cycle.
Similarly, it is equally easy to go in reverse: if you have a geyser's average active yield and its active amount percentage, its
average overall yield
is:
Δ
a
v
g
=
Δ
a
c
t
i
v
e
⋅
%
a
c
t
i
v
e
Using the same example as above, a Copper Volcano which is active for 48.9 cycles every 76.8 cycles and emits 490 g/s while active on average will have an average overall yield of 490 * 0.637 ≈ 312 g/s; multiplying by 600 gives about 187.2 kg/cycle.
Calculating the
eruption rate
of a geyser, which is the rate at which the geyser actually emits its contents, is done similarly using the average active yield (see above) and the eruption amount (which is shown under "eruption period" - e.g. a geyser which erupts for 349 seconds every 719 seconds has an active amount percentage of roughly 48.5%):
Δ
a
c
t
i
v
e
=
Δ
e
r
u
p
t
i
n
g
⋅
%
e
r
u
p
t
i
n
g
Δ
e
r
u
p
t
i
n
g
=
Δ
a
c
t
i
v
e
%
e
r
u
p
t
i
n
g
For example, a Steam Vent which emits 3070.9 g/s of Steam for 349s every 719s will have an active yield of 3070.9 * 0.485 ≈ 1489 g/s (893 kg/cycle) - notably higher than the middle 20% for that parameter. If that same geyser is active for 77.5 cycles every 117.6 cycles (≈ 66%), then we can calculate its average overall yield to be 1489 * 0.66 ≈ 982 g/s. (The real geyser these values were taken from had an in-game average overall yield of 982.3 g/s.)
Geyser Emission Rules
Entombment Check
Check whether the geyser’s primary cell of interest is entombed by a solid tile. If so, the geyser will not emit elements and reports as
Overpressured
.
Direct Emission Check
Check the primary cell and adjacent cells (above, below, left, right):
If any contain the same element as emitted by the geyser and are below the geyser’s overpressure limit, the element is emitted directly into that cell.
If all cells exceed the overpressure limit, the geyser will not emit elements and reports as
Overpressured
.
Cells of interest around a geyser
Displacement Check
If no cells satisfy the previous step, determine if displacement is possible:
A cell can be displaced if:
It is under the geyser’s overpressure limit.
It is a gas (Gas geysers only). Liquid geysers can displace both gases and liquids.
An adjacent cell of the same element exists for displacement.
Vacuum can always be displaced; solid cells cannot.
If no displacement is possible, the geyser will neither emit nor report as
Overpressured
.
The geyser checks cells in this order:
Primary cell (0,0)
Below (0,-1)
Left (-1,0)
Right (1,0)
Above (0,1)
Geyser Spawning Rules - Basic Asteroids
The swamp biome's cool steam vent, exposed to space.
The caustic biome's chlorine geyser (left), next to a covered geyser (right).
All geysers are spawned as part of
biomes
, with a specified amount for each biome.
Open Geysers
Swamp Biome
:
One open cool steam vent
One open natural gas vent
Caustic Biome
:
One open cool steam vent
One open natural gas OR chlorine vent
Ocean Biome
:
One open saltwater geyser
Volcanic Biome
on
Rime Asteroid
:
5 open volcanoes
Volcanoes
World Trait
8 open volcanoes
Buried Geysers
There are 12 randomly spawned buried geysers in each game. The
Geoactive
World Trait
increases the number to 16, and the
Geodormant
World Trait decreases the number to 9.
These buried geysers are randomly spawned all over the map, except in the start Biome,
Volcanic Biome
, and
Space Biome
. These geysers start covered with
granite
and
obsidian
. In some cases, maps will contain less than what is listed due to points of interest overwriting each-other due to close proximity, especially in the Oil Biome.
As a convenient rule of thumb, each biome type will only have at most two buried geysers.
Geyser Spawning Rules - Asteroid Clusters (Spaced Out DLC)
Each asteroid within a cluster has a specific list of open geysers, which can be seen by selecting the asteroid in the starmap.
The list of geysers for each asteroid type is as follows:
Terrania Asteroid
2 Cool Steam Vent (exposed, 1
Jungle
, 1
Marsh
)
1 Cool Salt Slush Geyser (exposed)
1 Cool Slush Geyser (exposed)
0-3 Minor Volcano
Squelchy Asteroid
1 Cool Salt Slush Geyser (exposed)
1 Cool Slush Geyser (exposed)
1 Cool Steam Vent (exposed)
1 Hydrogen or Chlorine Gas Vent (exposed)
0-3 Minor Volcano
Folia Asteroid
1 Cool Salt Slush Geyser (exposed)
1 Cool Slush Geyser (exposed)
1 Cool Steam Vent (exposed)
1 Hydrogen or Chlorine Gas Vent (exposed)
0-3 unique of:
Chlorine Gas Vent
Hydrogen Vent
Minor Volcano
Liquid Sulfur Geyser
0-3 Minor Volcano
Oily Swamp Asteroid
0-5 unique of:
Chlorine Gas Vent
Natural Gas Geyser
Carbon Dioxide Vent
Hydrogen Vent
Minor Volcano
Carbon Dioxide Geyser
Liquid Sulfur Geyser
1 Liquid Sulfur Geyser
2-5
Oil Reservoir
Rusty Oil Asteroid
0-4 unique of:
Chlorine Gas Vent
Natural Gas Geyser
Carbon Dioxide Vent
Minor Volcano
Carbon Dioxide Geyser
Liquid Sulfur Geyser
1 Liquid Sulfur Geyser
2-5
Oil Reservoir
Irradiated Asteroids
1-2 Gold Volcano
Irradiated Forest and Irradiated Marsh Asteroids
1-2 Aluminum Volcano
Irradiated Swampy Asteroid
1-2 Cobalt Volcano
0-4 unique of (0-5 on Irradiated Forest Asteroid):
Chlorine Gas Vent
Natural Gas Geyser
Hot Polluted Oxygen Vent
Minor Volcano
Hydrogen Vent
Polluted Water Vent
Salt Water Geyser (only on Irradiated Forest Asteroid)
Tundra Asteroid
2-5 Iron Volcano
Marshy Asteroid
1 Tungsten Volcano
0-2 Tungsten Volcano (
Magma
)
0-5 unique of:
Infectious Polluted Oxygen Vent
Natural Gas Geyser
Chlorine Gas Vent
Carbon Dioxide Vent
Hydrogen Vent
Hot Polluted Oxygen Vent
Superconductive Asteroid
1 Niobium Volcano
Moo Asteroid
1 Chlorine Gas Vent
Water Asteroid
0-2 unique of:
Polluted Water Geyser
Cool Slush Geyser
Salt Water Geyser
Water Geyser
Regolith Asteroid
1-2 of:
Cool Steam Vent
Hot Steam Vent
Geyser Probabilities
There are 25 unique geysers to pick from for each of the 12 randomly chosen geysers, each having the same chance of selection. This can be broken down into groups:
Water-producing: 7 (polluted water can be sieved, steam can be cooled, salt water and brine can be desalinated)
Oxygen-producing: 2 (polluted O2 can be deorderized)
Volcanic: 2
Metal-producing: 7
Combustion Power: 3 (natural gas, leaky oil, hydrogen)
Other: 4 (sulfur, chlorine, CO2)
Ignoring geysers forced by the biome type or planetoid type (i.e. cool steam vent in swamp), and assuming 9 of the 12 randomly chosen geysers exist on a given world (oil biome geysers often get overwritten by other POIs), the odds for getting at least a certain number of a preferred geyser or group of geysers are as follows:
Number of types preferred
1
2
3
4
5
6
7
Min
amount
wanted
1
30.75%
52.78%
68.35%
79.18%
86.58%
91.54%
94.8%
2
4.78%
15.83%
29.51%
43.48%
56.38%
67.5%
76.6%
3
0.45%
2.98%
8.33%
16.29%
26.18%
37.13%
45.03%
4
0.03%
0.37%
1.58%
4.2%
8.56%
14.75%
22.6%
5
0.001%
0.03%
0.21%
0.75%
1.96%
4.16%
7.62%
6
0%
0.002%
0.02%
0.09%
0.31%
0.81%
1.79%
All listed values assume repetition within the set of preferred geysers doesn't matter (ex. getting two Gold volcanoes, rather than a Gold and Tungsten).  The odds of getting multiple distinct geysers will be worse.  Note that on world with the Geoactive trait, or where fewer geysers are overwritten by other POIs, the resulting odds may be greater than listed.
As an example, if a world with at least two volcanoes is desired, it should have a 15.83% chance per world (2 preferred, 2 wanted). If at least one metal volcano is desired, it should have a 94.8% chance per world (7 preferred, 1 wanted).
As a corollary, ignoring forced-spawn geysers, any given world has at least a 94.8% chance of having at least one water geyser source, a 94.8% chance of having at least one metal geyser, a 68.35% chance of having a geyser producing a combustion power resource, a 52.78% chance of having at least one volcano, and a 52.78% chance of having a geyser directly producing oxygen.
Average Output Geyser Percentiles
A common question is: “How does my geyser’s average output compare to others?”
Because the underlying values are resampled and the output depends on a product distribution (which behaves differently from a simple average), answering this is non-trivial. A table is computed below listing 1, 5, 50, 90, 95, and 99 percentiles of geysers. Exact figures can be computed via this
Wolfram Cloud notebook
, or through the Geyser calculation mode on
Professor Oakshell
.
Geyser Average Output Percentiles
Geyser Type(s)
1%
5%
50%
90%
95%
99%
Carbon Dioxide Vent
Hydrogen Vent
Hot Polluted Oxygen Vent
Infectious Polluted Oxygen Vent
Chlorine Gas Vent
Natural Gas Geyser
72.56 g/s
81.843 g/s
104.58 g/s
123.514 g/s
129.526 g/s
141.622 g/s
Oil Reservoir
34.077 g/s
63.773 g/s
124.5 g/s
174.063 g/s
190.569 g/s
224.396 g/s
Carbon Dioxide Geyser
103.657 g/s
116.918 g/s
149.4 g/s
176.449 g/s
185.037 g/s
202.316 g/s
Copper Volcano
Iron Volcano
Gold Volcano
Aluminum Volcano
Tungsten Volcano
Cobalt Volcano
207.314 g/s
233.837 g/s
298.799 g/s
352.897 g/s
370.075 g/s
404.633 g/s
Minor Volcano
414.628 g/s
467.673 g/s
597.598 g/s
705.794 g/s
740.15 g/s
809.266 g/s
Steam Vent
518.285 g/s
584.592 g/s
746.998 g/s
882.243 g/s
925.187 g/s
1011.582 g/s
Volcano
Niobium Volcano
829.256 g/s
935.347 g/s
1195.196 g/s
1411.589 g/s
1.48 kg/s
1618.531 g/s
Cool Steam Vent
Cool Slush Geyser
Cool Salt Slush Geyser
Sulfur Geyser
1036.571 g/s
1169.183 g/s
1493.995 g/s
1764.486 g/s
1.85 kg/s
2023.164 g/s
Water Geyser
Polluted Water Vent
Salt Water Geyser
2073.141 g/s
2338.367 g/s
2987.991 g/s
3528.972 g/s
3.701 kg/s
4046.328 g/s
