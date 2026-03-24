# Thermal Conductivity

Thermal Conductivity
is the property of a material that determines how quickly it heats or cools as it comes into contact with objects of different temperatures. Although the game states that between two objects, the lowest thermal conductivity is used, this is not true for all cases.
Equations
Heat Transfer
q
, in DTU, is mainly a product of:
Δ
T
, the temperature difference in °C
Δ
t
, the time interval, which is always one tick,
0
.
2
s
, and
k
, the applicable thermal conductivity in DTU/m/s/°C
k
min
is the lower of the two:
min
⁡
(
k
1
,
k
2
)
k
geom
is the
geometric mean
of the two:
k
1
⋅
k
2
k
avg
the arithmetic average of the two:
(
k
1
+
k
2
)
/
2
k
mult
the product of the two, halved:
k
1
⋅
k
2
/
2
In some cases, heat flow depends on the
thermal mass per area
of the hotter of the two objects:
C
th max
=
s
⋅
m
hot
c
A
where:
m
hot
: mass of the hotter object
c
: specific heat capacity of the hotter object (SHC)
A
: area (the number of tiles it occupies. 1 for a single tile)
s
: mass scale factor (typically 0.2 for buildings and 1 for tiles)
Notable exceptions:
Steam Turbine
: mass scale factor 1
Fish Feeder
/
Weight Plate
/
Fish Release
/
Transit Tube Crossing
/
Farm Tile
: mass scale factor 1.2
Only the mass of the primary construction material is used. For example, the
Steam Turbine
counts only the
800 kg
of Refined Metal and ignores the
200 kg
Plastic
component.
Elements
occupying the simulation grid (natural tiles, gas and liquids) and player-built tiles such as
Tiles
use a mass scale factor 1.
Equations
[
1
]
Scenario
Formula
Cell ↔ adjacent Cell
Solid ↔ Solid
q
=
Δ
T
⋅
Δ
t
⋅
k
geom
⋅
1
0
0
0
Solid ↔ Liquid
Gas ↔ Liquid
Gas ↔ Gas
Solid ↔ Gas
q
=
Δ
T
⋅
Δ
t
⋅
k
geom
⋅
1
0
0
0
⋅
2
5
Liquid ↔ Liquid
q
=
Δ
T
⋅
Δ
t
⋅
k
geom
⋅
1
0
0
0
⋅
6
2
5
Entity lying on a Solid
q
=
Δ
T
⋅
Δ
t
⋅
k
min
⋅
6
2
.
5
Building ↔ Solid tile below it
N
/
A
pipe ↔ adjacent pipe
N
/
A
Inside a Cell
Entity ↔ Cell
q
=
Δ
T
⋅
Δ
t
⋅
k
min
⋅
1
0
0
0
Building ↔ Cell
q
=
Δ
T
⋅
Δ
t
⋅
k
mult
⋅
C
th max
Building ↔ Building
N
/
A
Building ↔
Conduction Panel
q
=
Δ
T
⋅
Δ
t
⋅
k
1
⋅
k
2
⋅
s
⋅
m
hot
c
⋅
5
⋅
1
0
0
0
⋅
1
0
−
6
Building ↔ Entity
N
/
A
Building's Contents
pipe ↔ pipe contents
q
=
Δ
T
⋅
Δ
t
⋅
k
avg
⋅
5
0
Insulated pipe ↔ contents
q
=
Δ
T
⋅
Δ
t
⋅
k
min
⋅
5
0
pipe contents ↔
Conduction Panel
q
=
Δ
T
⋅
k
avg
⋅
5
0
(heat transfer occurs when piped contents flow which occurs at most once per second)
Cell ↔ pipe contents
N
/
A
:
transfers through the pipe instead
pipe bridge ↔ bridge contents
N
/
A
:
bridges teleport elements, NO contents
Building ↔ building contents
N
/
A
Building's
Cell of Interest
↔ building contents
see Cell↔Entity
Thermal Element Categories
Category
Examples
Cell
Gas, Liquid, Solid Block, Tiles,
closed
Doors, Joint Plate (middle), Tube Crossing (middle), etc
Entity
Dupes, Creatures, Plants, Debris,
Mesh Tile
,
Airflow Tile
, etc
Building
pipes, bridges, background buildings, geysers, generators,
open
Doors,
Pneumatic doors
(open/closed), etc
Pipe
Liquid Pipe
,
Gas Pipe
,
Conveyor Rail
,
Wires
(all kinds),
Automation Wire
, and
Automation Ribbons
Bridge
Liquid
,
Gas
,
Conveyor
Wire
,
Automation
, and
Automation Ribbons
Contents
Building Production Storage (Input/Output), Reservoirs, Fridges, Compactors, etc
Special
Tempshift Plate
,
Conduction panel
,
Refrigerator
,
Compost
Certain buildings apply a modifier to their material thermal conductivity:
Insulated Tiles: divide by
(
2
5
5
2
)
2
(not correctly reflected under Properties)
Insulated Liquid Pipe and Insulated Gas Pipe: divide by 32
Power Wires: divide by 20
Some background automation buildings: divide by 20
AND Gate
OR Gate
XOR Gate
NOT Gate
FILTER Gate
BUFFER Gate
Signal Distributor
Signal Selector
Radiant Pipes, Radiant Gas Pipes and Conduction Panel: multiply by 2
a
Tempshift Plate
conducts as a building, and also conducts to all cells in a 3×3 area centered on it
a
Conduction Panel
is a (long) pipe
conducts as a building in its cells
specially conducts
building ↔ building
in its MIDDLE tile
conducts any elements passing through it via
pipe ↔ pipe contents
Entities -- debris, animals, plants -- act as if they only take up one tile of space, even if they appear to take up more than that. For example,
Duplicants
and upwards growing
Plants
exchange heat only at their bottom tile: however other tiles the entity overlaps are taken into account for some other effects like
scalding
, which does not involve actual heat exchange.
A building's contents act like they are in the building's
Cell of Interest
, and exchange heat through the Cell↔Entity Equation.
Powered
Refrigerator
and
Compost
act as normal buildings, but
their contents
will only interact with an imagined 277.15K (fridge) or 348.15K (compost) source at a locked conductivity of 1000 regardless of their material.
Bridges act as a long building, conducting along its length.
You can stack multiple bridges to increase heat transfer along the cells
You can use bridges to help stabilize a
Guide/Liquid Airlock
from evaporation or sublimation.
Heavi-Watt Joint Plates
,
Heavi-Watt Conductive Joint Plates
, &
Transit Tube Crossings
act as a cell, the connection points on the sides are cosmetic (for thermal conductivity).
Radbolt Joint Plates
acts as both a cell and a building, but the building does not conduct heat.
Fish Feeders
and
Fish Releases
conduct heat properly both as a cell and as a building.
Manual Airlocks
and
Mechanized Airlocks
behave exactly like two equal mass tiles adding up to the weight of the door (so, for example, a Steel Mechanized Airlock behaves exactly like two tiles of 200 kg Steel). The displayed temperature is that of the
Cell of Interest
but the other tile can and will likely have a different temperature. There is no heat transfer between the two tiles as a Building ↔ Cell, only heat transfer as a Cell ↔ Cell. Opening the door equalizes the temperature instantly. Closing the door causes temperature duplication.
[
2
]
Insulated Tiles reduce the thermal conductivity of their building material by (255/2)² (or 16 256) instead of 100 as stated in the game. It also uses
k
min
instead of
k
geom
for the purpose of cell to cell conductivity, which is mostly going to be the insulated tile conductivity. Solid to gas multiplier still applies.
Recap of Cell-to-Cell multipliers
Gas
Liquid
Solid
Gas
1
1
25
Liquid
1
625
1
Solid
25
1
1
Because of the gas-to-solid ×25 multiplier, it's recommended to use a double layer of tiles or a single layer of tiles plus a thin liquid layer when trying to insulate between two rooms, to instead get a ×1 multiplier.
Special cases for surface area multipliers
Neutronium
has a ×0 multiplier on all cells (separate from the fact that it has 0 thermal conductivity and 0 specific heat capacity).
Thermium
has an additional ×2 multiplier on cell-to-cell multipliers. This implies that
Thermium
-to-
Thermium
has a combined multiplier of ×4, and
Thermium
-to-gas has a combined multiplier of ×50.
Snow
,
Crushed Ice
,
Packed Snow
has a ×1000 multipler on solid-to-gas (on top of the ×25 for gas-to-solid already). This includes the
Snow Tile
, which makes it a subpar insulator against gas despite its relatively low 0.545 DTU/m/s/K thermal conductivity. (Indeed, this is actually one of the most thermally conductive element interaction in the game)
Limits of Heat Transfer
Lower Limits
Heat Transfer will not occur if:
the temperature difference is less than 1 °C
the calculated thermal flow is less than 0.1 DTU
either of the masses is less than 1 g
Upper Limits
Heat transfer between cells is capped by the following upper bound:
If the calculated heat transfer would result in a temperature jump of more than one fourth of their temperature difference
(
T
1
−
T
2
)
/
4
in either material, then the heat flow is limited to
max
⁡
(
T
1
−
T
2
4
⋅
m
1
⋅
c
1
,
T
1
−
T
2
4
⋅
m
2
⋅
c
2
)
DTU per tick.
Simply said: if the temperature difference is 40 °C, each object's temperature can change by at most 10 °C per tick
Building Limits
Heat transfer between a building and a cell has different limits. The lower limits which are applied to cells do not apply to buildings, but the upper limit is conceptually similar.
A building exchanges heat with all cells it covers simultaneously. In order to ensure that thermodynamics will not be violated, the game limits heat transfer per cell such that at most the final temperature of the building would be the equilibrium temperature, assuming that the building completely covers such cells:
T
eq
=
T
building
⋅
C
building
+
T
cell
⋅
C
cell
⋅
A
C
building
+
C
cell
⋅
A
The maximum permitted heat transfer per cell is the difference between the building's temperature and the equilibrium temperature divided by the building's area.
q
max
=
C
building
⋅
T
building
−
T
eq
A
If the thermal mass of the cell is very large relative to the building, then the maximum temperature change can be approximated as simply
Δ
T
A
Floating-Point Calculation Limits
While the above limits are deliberately implemented, it is also possible for heat exchange to fail due to limitations of the floating-point calculations used to calculate temperature changes.
Internally, ONI uses 32-bit floating-point numbers to represent temperatures, and due to the limited precision of floating-point numbers it is possible for small changes to be lost.
For example, using 32-bit floats, 300.0 + 0.00001 = 300.0
The game has a rule that if either tile fails to change temperature, then no heat exchange is allowed to take place. This prevents a large tile, especially an unnaturally large tile, from infinitely dumping heat/cold into a smaller tile without itself changing temperature.
Floating Point Calculation Limits In Insulated Tiles
Main article:
Insulated Tile
In real games, the floating point limit comes up all the time when the temperature difference between an
Insulated Tile
and a solid or liquid tile is relatively small. For example an
Igneous Rock
Insulated Tile
which is itself at
20 °C
/
68 °F
, will not exchange heat with a solid or liquid tile unless the temperature difference is at least 248.05 °C, and won't exchange heat with a gas tile unless the temperature difference is at least 9.92 °C. This makes it quite easy to achieve actually zero heat transfer without resorting to
Insulite
or
Vacuum
. The exact formulas governing this are:
Δ
T
ignorable
=
2
⌊
log
2
(
T
)
−
2
4
⌋
and
q
max
=
Δ
T
ignorable
⋅
m
⋅
c
, where temperature
T
, mass
m
, and SHC
c
are for the cell holding everything constant, and
q
max
is the relevant heat-exchange function between the two cells, which can be reversed to find
Δ
T
max
.
It is also readily observed with liquid tiles, that
Magma
and
Water
can have immense thermal masses which means that relatively large DTU inputs are required to cause a temperature change. This results in the paradoxical outcome where full magma tiles don't exchange heat with insulated tiles, but partial magma tiles can exchange heat if their masses are are sufficiently low. Using the above formula but applied to the
Magma
tile instead of the insulated tile, we can see that a cell with 715.6 kg or more magma will be unable to exchange temperature with an
Igneous Rock
Insulated Tile
at
0 °C
/
32 °F
or higher, regardless of the magma temperature. For
Mafic Rock
, which has half the conductivity, only 357.8 kg of magma are needed.
Suffice to say that, while floating-point imprecision sometimes causes heat exchange to not happen at all, when temperature changes are small it also causes the actual temperature change to deviate quite significantly from what higher precision calculations would suggest.
Thermal descriptors
There are four thermal descriptors in the game, and they are applied to elements whose thermal characteristics reach a certain threshold. These descriptors do not affect the element any further.
Thermally Reactive
: Elements with a specific heat capacity less than or equal to 0.2
Slow heating
: Elements with a specific heat capacity greater than or equal to 1.0
Insulator
: Elements with a thermal conductivity less than or equal to 1.0
High Thermal Conductivity
: Elements with a thermal conductivity greater than or equal to 10.0
Pipes list
Liquid Pipes
Liquid Pipes
Pipe
Material
Thermal Conductivity
Insulated Liquid Pipe
Insulite
0.0000003125
Liquid Pipe
Insulite
0.00001
Insulated Liquid Pipe
Ceramic
0.019375
Insulated Liquid Pipe
Shale
0.05625
Insulated Liquid Pipe
Obsidian
0.0625
Insulated Liquid Pipe
Igneous Rock
0.0625
Insulated Liquid Pipe
Sedimentary Rock
0.0625
Insulated Liquid Pipe
Sandstone
0.090625
Insulated Liquid Pipe
Granite
0.1059375
Insulated Liquid Pipe
Wolframite
0.46875
Insulated Liquid Pipe
Tungsten
0.46875
Liquid Pipe
Ceramic
0.62
Liquid Pipe
Shale
1.8
Liquid Pipe
Obsidian
2
Liquid Pipe
Igneous Rock
2
Liquid Pipe
Sedimentary Rock
2
Liquid Pipe
Sandstone
2.9
Liquid Pipe
Granite
3.39
Insulated Liquid Pipe
Thermium
6.875
Liquid Pipe
Wolframite
15
Liquid Pipe
Tungsten
60
Radiant Liquid Pipe
Lead
70
Radiant Liquid Pipe
Niobium
108
Radiant Liquid Pipe
Steel
108
Radiant Liquid Pipe
Iron
110
Radiant Liquid Pipe
Copper
120
Radiant Liquid Pipe
Tungsten
120
Radiant Liquid Pipe
Gold
120
Radiant Liquid Pipe
Nickel
182
Radiant Liquid Pipe
Cobalt
200
Liquid Pipe
Thermium
220
Radiant Liquid Pipe
Iridium
340
Radiant Liquid Pipe
Aluminum
410
Radiant Liquid Pipe
Thermium
440
Gas Pipes
Gas Pipes
Pipe
Material
Thermal Conductivity
Insulated Gas Pipe
Insulite
0.0000003125
Gas Pipe
Insulite
0.00001
Insulated Gas Pipe
Ceramic
0.019375
Insulated Gas Pipe
Mafic Rock
0.03125
Insulated Gas Pipe
Shale
0.05625
Insulated Gas Pipe
Obsidian
0.0625
Insulated Gas Pipe
Igneous Rock
0.0625
Insulated Gas Pipe
Sedimentary Rock
0.0625
Insulated Gas Pipe
Fossil
0.0625
Insulated Gas Pipe
Sandstone
0.090625
Insulated Gas Pipe
Granite
0.1059375
Gas Pipe
Ceramic
0.62
Gas Pipe
Mafic Rock
1
Gas Pipe
Shale
1.8
Gas Pipe
Obsidian
2
Gas Pipe
Igneous Rock
2
Gas Pipe
Sedimentary Rock
2
Gas Pipe
Fossil
2
Gas Pipe
Sandstone
2.9
Gas Pipe
Granite
3.39
Radiant Gas Pipe
Gold Amalgam
4
Radiant Gas Pipe
Nickel Ore
6
Radiant Gas Pipe
Iron Ore
8
Radiant Gas Pipe
Cobalt Ore
8
Radiant Gas Pipe
Copper Ore
9
Radiant Gas Pipe
Pyrite
9
Radiant Gas Pipe
Wolframite
30
Radiant Gas Pipe
Aluminum Ore
41
Radiant Gas Pipe
Niobium
108
Radiant Gas Pipe
Steel
108
Radiant Gas Pipe
Iridium
340
Radiant Gas Pipe
Thermium
440
Solid Tiles list
Important
: For
Insulated Tiles
, these numbers will not match what is seen in-game. This is because the value displayed in-game is
b
a
s
e
1
0
0
, but the actual value used by calculations (and shown here) is
b
a
s
e
⋅
(
2
2
5
5
)
2
.
Tiles
Tile
Material
Thermal Conductivity
Insulated Tile
Insulite
6.15e-10
Tile
Carpeted Tile
Insulite
0.00001
Insulated Tile
Ceramic
0.0000381
Insulated Tile
Mafic Rock
0.0000615
Insulated Tile
Shale
0.000111
Insulated Tile
Fossil
0.000123
Insulated Tile
Igneous Rock
0.000123
Insulated Tile
Obsidian
0.000123
Insulated Tile
Sedimentary Rock
0.000123
Insulated Tile
Sandstone
0.000178
Insulated Tile
Granite
0.000209
Plastic Tile
Plastic
0.150
Wood Tile
Wood
0.220
Plastic Tile
Plastium
0.250
Plastic Tile
Solid Visco-Gel
0.450
Tile
Carpeted Tile
Ceramic
0.620
Window Tile
Glass
1.110
Tile
Carpeted Tile
Mafic Rock
1.000
Tile
Carpeted Tile
Shale
1.800
Tile
Carpeted Tile
Fossil
2.000
Tile
Carpeted Tile
Igneous Rock
2.000
Tile
Carpeted Tile
Obsidian
2.000
Tile
Carpeted Tile
Sedimentary Rock
2.000
Tile
Carpeted Tile
Sandstone
2.900
Tile
Carpeted Tile
Granite
3.390
Metal Tile
Depleted Uranium
20
Metal Tile
Lead
35
Metal Tile
Bunker Tile
Steel
54
Metal Tile
Niobium
54
Metal Tile
Iron
55
Metal Tile
Copper
60
Metal Tile
Gold
60
Metal Tile
Tungsten
60
Window Tile
Diamond
80
Metal Tile
Nickel
91
Metal Tile
Cobalt
100
Metal Tile
Iridium
170
Metal Tile
Aluminum
205
Metal Tile
Thermium
220
Tips
When cooling or heating an area it's better to run pipes through tiles than through atmosphere. In both cases the equation for "Building and the cells it occupies" is used, which multiplies both thermal conductivities, and, in general, gases have a much lower thermal conductivity than liquids, which have lower conductivity than solids.
However, if drastic cooling is desired, then
Steam Turbines
and
Aquatuners
will have to be involved, which means a cavity filled with
Steam
will have to be used.
Since
Insulated Tiles
have a factor of 1/16256, and pipes a factor of 1/32, much less heat is transferred if a regular pipe goes through an insulated tile than when an insulated pipe goes through a regular tile. Of course, insulating both has an even better insulating effect.
Even though
Insulite
has a lower thermal conductivity than any
Insulated Tile
, the change in formula from
k
g
e
o
m
to
k
m
i
n
makes insulated tiles much more practical insulators than a regular
Tile
made from Insulite. Indeed, they are so good that even using regular rock is often sufficient to shut down heat transfer completely, or to practically unnoticeable levels.
References
https://forums.kleientertainment.com/forums/topic/84275-decrypting-heat-transfer/
↑
forum post on empirically derived heat transfer equations
↑
https://www.reddit.com/r/Oxygennotincluded/comments/y2r5c4/power_generation_by_heat_cloning_you_probably/?rdt=60603
