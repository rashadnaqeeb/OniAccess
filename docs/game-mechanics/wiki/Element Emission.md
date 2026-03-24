# Element Emission

Corium emitting
Liquid Nuclear Waste
that immediately solidifies.
Off-Gassing
or
Element Emission
refers to the
Game Mechanics
of one element reducing its mass to produce another element. In most cases this is a gas therefore it's also often referred as Off-Gassing. The ingame Codex categorizes some elements as
Sublimators
, however this isn't an extensive list.
The Elements that exhibit this behavior will emit their respective Element if the surrounding Atmosphere is below
1.8 kg
in pressure. As a natural tile, they will check all 4 directions and only emit in the directions that are lower than
1.8 kg
pressure without favoring any direction, as long as that condition is fulfilled.
To bypass this
1.8 kg
pressure limit, because of the
One element per cell rule
, a debris can be submerged in a puddle of liquid of less than
1.8 kg
or natural tiles surrounded by puddles of liquids of less than
1.8 kg
, allowing elements to emit constantly, even if the pressure in the nearby cells are already way above
1.8 kg
. This can also happen unintentionally when, for example, a single tile of
Carbon Dioxide
from a Dupe breathing passes by.
Natural Tiles
Natural Tiles behavior is dependant on their own probability to emit their designated element every tick (0.2 seconds)
Emission Rates in the table below are given in
one direction
. With the exception of
Polluted Water
, which can only emit to the top, Natural Tiles will calculate their emission in all 4 directions, effectively multiplying their emission by a factor of 4.
Note that
Polluted Mud
would need to rest on an Airflow Tile to emit to the bottom, since it's subject to gravity.
Element
Mass lost
Emission
Amount emitted
Probability
Real Rate
Oxylite
80 g
Oxygen
50% (
40 g
)
100%
200 g/s
Corium
80 g
Nuclear Waste
50% (
40 g
)
100%
200 g/s
Polluted Dirt
*
Polluted Mud
20 g
Polluted Oxygen
20% (
4 g
)
~6.25%**
~
1.25 g/s
Polluted Water
0.1% of mass
Polluted Oxygen
same as mass lost
1%
50 g/s
* While
Polluted Dirt
is a Element in the Vanilla Version of the game, it only ever appears as a Natural Tile in
Spaced Out!
.
** Both
Polluted Mud
and
Dirt
have their probability stated the game files as
sublimateProbability: 0.05
. However, the true/measured probability does not align with it.
Debris/Bottled
The rules for debris are rather difficult to test and find the proper equations for, and are very close approximations (to less than half a percent in deviation) to the true/measured behavior while not being too complicated to always follow
f
a
c
t
o
r
1
⋅
m
a
s
s
f
a
c
t
o
r
2
in g/s, with
m
a
s
s
is given in kg.
Element
Emission
Amount emitted
Interval
Real Rate in g/s
Rate for 20kg
Rate for 1000kg
Rate for 1000kg,
split 50x into 20kg packets
Oxylite
Oxygen
9
.
5
⋅
m
a
s
s
0
.
7
0
5
in g
every second
9
.
5
m
a
s
s
0
.
7
0
5
79.01 g/s
1,256 g/s
3,950.5 g/s
Polluted Dirt
Polluted Oxygen
50 g
every
2
5
0
0
⋅
m
a
s
s
−
0
.
5
seconds
0
.
0
2
⋅
m
a
s
s
0
.
5
0.09 g/s
0.63 g/s
4.45 g/s
Slime
Polluted Oxygen
*
125 g
every 4.8 seconds
constant
26.04 g/s
1,302.1 g/s
Polluted Water
Polluted Oxygen
varying
more frequent with higher mass
up to every second tick
0
.
0
4
⋅
m
a
s
s
1
0.8 g/s
40 g/s
40 g/s
Bleach Stone
Chlorine
0
.
2
⋅
m
a
s
s
0
.
5
0.89 g/s
6.33 g/s
44.7 g/s
* Not just for
Slimelung
with
Slime
, but any emitted elements, regardless of type and source, are always covered with germs proportional to the remaining material.
Tips
Mined
Oxylite
debris emits the same amount of
Oxygen
as it would have as a natural tile.
Since
Slime
emission rate doesn't depend on its mass, loading it on
Conveyor Rail
, which separates it into 20kg chunks, lets it emit
Polluted Oxygen
much faster than leaving it in large debris chunks.
Bottled
Polluted Waters
emission rate is linearly dependent on its mass and will not give diminishing returns. Therefore it's easy to make it emit much faster, since bottles are not limited per cell, this is useful for mass production of
Clay
from
Deodorizer
for the further production of
Ceramics
.
Transporting
Bleach Stone
is an easy way to create a
Chlorine
atmosphere for disinfection.
Trivia
Polluted Mud and Corium only emits as a natural tile, not in debris form.
Bleach Stone and Slime only emit as debris, not in natural tile form.
Corium emitting Nuclear Waste plays the animation clean Air bubbles like Oxylite emitting Oxygen.
