# Metal Volcano

Metal Volcano
A highly pressurized volcano that periodically erupts with
Molten
Metals
.
Dimensions
3×3 tiles
Category
Geysers
Decor
+10 (Radius: 2 tiles)
Effects
Refined Metal
: Average 300 g/s
or
Niobium
: Average 1.2 kg/s
Deconstructible
No
Metal Volcanoes
are a special type of
geyser
and a renewable, infinite source of
Refined Metals
. In the Base Game they are always buried, but in the
Spaced Out! DLC
they can sometimes spawn uncovered when settling a new
Planetoid
.
Like a typical geyser, they alternate between an active state in which they produce their material and an inactive "dormant" state where they do nothing. During the eruption phase, output of liquid metal can be blocked by over-pressurization. The maximum pressure it will output at is 150 kg in a
Gas
, such as Steam, or when its first two layers are fully submerged in a
Liquid
, such as its own output. The tile of interest from which the emission comes out from is the exact center tile of the 3 by 3 volcano, which, on the 4 tile
neutronium
base is shifted to the left.
The
Base Game
has volcanoes for
Gold
,
Copper
and
Iron
, while the Spaced Out! DLC introduces volcanoes for
Tungsten
,
Aluminum
,
Niobium
, and the Spaced Out! exclusive element
Cobalt
. In Spaced Out!, Niobium Volcanoes are the only renewable source of Niobium aside from converting
Tungsten
to
Niobium
using a
Molecular Forge
and a
Metal Refinery
.
Name
Produced Element
Temp
Copper Volcano
Molten Copper
2226.85 °C
/
4040.33 °F
Iron Volcano
Molten Iron
2526.85 °C
/
4580.33 °F
Gold Volcano
Molten Gold
2626.85 °C
/
4760.33 °F
Aluminum Volcano
Molten Aluminum
1726.85 °C
/
3140.33 °F
Tungsten Volcano
Molten Tungsten
3726.85 °C
/
6740.33 °F
Niobium Volcano
Molten Niobium
3226.85 °C
/
5840.33 °F
Cobalt Volcano
Molten Cobalt
2226.85 °C
/
4040.33 °F
Metal Volcano Taming
Metal Volcanoes eject their metals in molten form at appropriately hot temperatures. Although
all Metal Volcanoes except Niobium
follow the same rules for their eruption periods and ejection rates, the metals have different ejection temperatures, freezing points, and specific heat capacities, therefore it is unlikely to have a one-size fits all solution which is not wasteful for most volcanoes.
Any geyser, vent, or, in this case, volcano, cycles through 3 phases. The dormant phase, and the active phase which contains the ejection phase, and the idle phase. And it's important to view it as such, since merely calculating the volcano's average output over its lifetime can still lead to equipment overheating.
During the
Ejection Phase
, Metal and Heat is rapidly introduced into the environment, therefore one needs a
Buffer
to catch the Heat in.
During the
Idle Phase
, all the produced heat from the
Ejection Phase
, stored in the
Buffer
must be moved away to be ready for the next ejection.
And during the
Dormant Phase
, nothing happens, but it can not be relied upon as a large period of time to let the entirety of the setup cool down, that's what the
Idle Phase
should be for.
By far the best buffer and heat deletion combination is
Water
(or rather,
Steam
) and the
Steam Turbine
. In perfect conditions, a
Self-Cooled Steam Turbine
can delete 292.53kDTU per second.
To calculate how big of a water buffer must be in place, it is handy to know that the ejected metal exchanges its temperature with the environment much more readily as a
Liquid
than as debris.
To get the Ratio of Ejection amount to buffer size, you first calculate the total amount of heat to be removed from the ejected metal until it solidifies, which is the difference of its Output and Freezing temperature multiplied by its SHC. Then, you divide that number by the amount of heat your buffer medium (water) can take before its temperature leaves the permissible range. In other words, you calculate the difference between the high and the low end of your permissible range and multiply it with the SHC of you buffer medium (which is 4.179 for water). The permissible range for a
self-cooled Steam Turbine
is
138 °C
/
280.4 °F
-
125 °C
/
257 °F
. For a Steam Turbine cooled by
Aquatuners
it is
275 °C
/
527 °F
-
125 °C
/
257 °F
because steel equipment will break above that temperature. The latter version cuts down the amount of water needed by a factor of 10.
R
=
S
H
C
m
e
t
a
l
⋅
(
T
o
u
t
p
u
t
−
T
f
r
e
e
z
i
n
g
)
S
H
C
w
a
t
e
r
⋅
(
1
3
8
−
1
2
5
)
The Ejection amount can be simply multiplied with this ratio (different for every metal) to get the size of the water buffer needed.
Examples:
A Gold Volcano that spews 11kg/s for 27 seconds, with gold's ratio of 3.71, needs about 1150kg of Water.
An Iron Volcano that spews 17kg/s for 22 seconds, with iron's ratio of 8.2, needs about 3100kg of Water.
An Aluminum Volcano that spews 8.2kg/s for 32 seconds, with aluminum's ratio of 17.87, needs about 4700kg of Water.
To calculate how many
Self-Cooled Steam Turbines
are required, first calculate the output that one turbine can take care of by dividing the amount of cooling the Turbine can do (292,53kDTU) by the amount of Heat produced by one gram of metal, assuming the metal would be cooled to 398.15 K (
125 °C
/
257 °F
). Then take the average output over an activity period (not average over lifetime) and round up to the next whole number of turbines:
η
=
2
9
2
5
3
0
S
H
C
m
e
t
a
l
⋅
(
T
e
m
p
m
e
t
a
l
−
3
9
8
.
1
5
)
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
*
t
e
r
u
p
t
i
n
g
t
p
e
r
i
o
d
n
T
u
r
b
i
n
e
s
=
⌈
Δ
a
c
t
i
v
e
η
⌉
Note that since all metal volcanoes (except Niobium) produce between 200 and 400 kg/cycle, the bounds on Δ
active
will always be between 333.33 and 666.67 g/s; Niobium volcanoes instead produce 800-1600 kg/cycle, so Δ
active
will be between 1333.33-2666.67 g/s. Also note that Temp
metal
must be in Kelvin, not in °C (e.g. iron emits at 2800 K (
2526.85 °C
/
4580.33 °F
), so η = 292530 / (0.449 * (2800 - 398.15) ) = about 271).
Examples:
A Gold Volcano that spews 11kg/s for 27 seconds every 570 seconds, or 521g/s. One Turbine is needed since it can handle up to 906 g/s:
η
=
2
9
2
5
3
0
0
.
1
2
9
⋅
(
2
9
0
0
−
3
9
8
.
1
5
)
≈
9
0
6
.
4
0
Δ
a
c
t
i
v
e
=
1
1
0
0
0
*
2
7
5
7
0
≈
5
2
1
.
0
5
n
T
u
r
b
i
n
e
s
≈
⌈
5
2
1
.
0
5
9
0
6
.
4
0
⌉
=
1
An Iron Volcano that spews 17kg/s for 22 seconds every 780 seconds, or 479.5g/s. Two Turbines are needed since one can handle up to 271 g/s, two can handle 543 g/s:
η
=
2
9
2
5
3
0
0
.
4
4
9
⋅
(
2
8
0
0
−
3
9
8
.
1
5
)
≈
2
7
1
.
2
6
Δ
a
c
t
i
v
e
=
1
7
0
0
0
*
2
2
7
8
0
≈
4
7
9
.
4
9
n
T
u
r
b
i
n
e
s
≈
⌈
4
7
9
.
4
9
2
7
1
.
2
6
⌉
=
2
An Aluminum Volcano that spews 8.2kg/s for 32 seconds every 450 seconds, or 583.1g/s. Three Turbines are needed since one can handle up to 201 g/s, so three would be 602 g/s:
η
=
2
9
2
5
3
0
0
.
9
1
⋅
(
2
0
0
0
−
3
9
8
.
1
5
)
≈
2
0
0
.
6
8
Δ
a
c
t
i
v
e
=
8
2
0
0
*
3
2
4
5
0
≈
5
8
3
.
1
1
n
T
u
r
b
i
n
e
s
≈
⌈
5
8
3
.
1
1
2
0
0
.
6
8
⌉
=
3
Niobium
volcanoes are a unique exception to the eruption behavior exhibited by other metal volcanoes; their eruption patterns resemble that of regular,
magma
-producing volcanoes- long intervals between eruptions, but emitting massive quantities of very hot, very conductive
Liquid Niobium
during eruptions- potentially several hundred kilograms per second. Further complicating things,
Liquid Niobium
will form a tile of solid
Niobium
when cooled at a mere 50kg of mass, well below the volume emitted per second while erupting. This will result in the volcano quickly entombing itself in tiles of
Niobium
if conventional methods are used to tame it, providing a unique challenge and necessitating an alternative strategy.
Metal
Output
Temperature
Freezing
Temperature
Temperature Range
SHC
Heat in DTU/g
Ratio
Amount of Metal Handled (g/s)
to solid
to
125 °C
/
257 °F
to solid
to
125 °C
/
257 °F
exact
rounded
by 1 Turbine
by 2
by 3
by 4
Gold
2626.85 °C
/
4760.33 °F
1063.85 °C
/
1946.93 °F
1563
~2500
0.129
201.627
322.739
3.71
4
906
Tungsten
3726.85 °C
/
6740.33 °F
3421.85 °C
/
6191.33 °F
305
~3600
0.134
40.870
482.648
0.75
1
606
1212
Niobium
3226.85 °C
/
5840.33 °F
2476.85 °C
/
4490.33 °F
750
~3100
0.265
198.750
821.990
3.66
4
356
712
1068
1424
Copper
2226.85 °C
/
4040.33 °F
1083.85 °C
/
1982.93 °F
1143
~2100
0.386
441.198
811.314
8.12
10
361
721
Iron
2526.85 °C
/
4580.33 °F
1534.85 °C
/
2794.73 °F
992
~2400
0.449
445.408
1078.431
8.20
10
271
543
814
Cobalt
2626.85 °C
/
4760.33 °F
1494.9 °C
/
2722.82 °F
1131.95
~2500
0.420
475.419
1050.777
8.75
10
278
557
835
Aluminum
1726.85 °C
/
3140.33 °F
660.3 °C
/
1220.54 °F
1066.55
~1600
0.910
970.561
1457.684
17.87
20
201
401
602
803
These calculations are for
self-cooled
steam turbines only. The color of the cells in the last four columns indicate the viability of that many self-cooled turbines for a volcano of that type, with red indicating entirely insufficient for all sizes, orange indicating sufficient for some but not all volcanoes, and green indicating sufficient to handle even the highest-output volcanoes. For turbines being cooled by
Thermo Aquatuners
instead (located in the same area as the volcano) then the heat deletion per Turbine can be raised up to 1538.05 kDTU/s (at
300 °C
/
572 °F
, where a
Steel
aquatuner won't overheat) at which point a single Turbine is enough to handle any non-
Geotuned
Metal Volcano except for
Niobium
Volcano.
