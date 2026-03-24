# Liquid

This article may be
outdated
. It was last updated for
U40-494396
. These versions were tagged as updating the game mechanics discussed here:
U55-659901
Liquid
is a state of elements.
Mechanics
A Liquid is any matter in game that is not a
solid
entity or
gas
entity. Almost any material in the game will melt given a high enough temperature.
Liquids are affected by gravity and can flow across level floors, through open doors and down ladders.
A
Duplicant
who finds themselves submerged with no way to rise above the level of the liquid will either drown after a set period without air, burn to death in hot liquids, or die of frostbite in liquids below
-90.15 °C
/
-130.27 °F
, unless the
Duplicant
has a
Atmo Suit
.
Liquids do not mix in tiles.
Polluted Water
in
Water
does not cause the clean water to become polluted, although germs may spread.
Only 1 liquid may occupy a tile at a time, which is the same with
Gas
. This can be used for things such as a Liquid Airlock.
Elements with a lower density tends to rise above others. Mixed Liquids with different density and in sufficient amounts will form several layers after a period of time if the liquids are "mixed" by moving them around.
The density of liquids is taken to be equal to be equal to their
molecular weight
, an inaccurate assumption that causes some counterintuitive behavior like oil sinking in water.
Liquids with heavier densities cannot be displaced by liquids with lighter densities. Therefore, liquids of differing densities that are side-by-side that cannot move (such as due to being underneath more liquid) will not displace each other unless either liquid is displaced, such as by pumping a
Gas
below them. Moreover, the construction of one tile wide
Liquid Airlocks
using bottled liquids requires the heavier liquid to be poured first.
Liquids of the same densities will not displace each other. This allows the creation of arbitrarily high liquid stacks (useful for
Pacu
ranching) or
Liquid Airlocks
without needing the Max Mass of the liquid.
Nectar
and
Salt Water
is a particularly useful combination of such liquids.
Liquids exposed to
space
will be destroyed, unless protected by
Drywall
or
Tempshift Plates
Liquids can occupy two vertical tiles and touch the solid tile third above. Thus it is possible to make a
Liquid Airlock
.
Like all tiles, liquids evaporate at 3 degrees above the specified vaporization point, and freeze at 3 degrees below the specified freezing point.
If such transformation happens inside a
Liquid Pipe
, the pipe will be damaged.
Phase change will not occur if the content of the pipe is 1kg or less.
If a liquid is atop any tile with a temperature exceeding a phase change temperature, it will under go a phase change by
Flaking
. This includes tiles with virtually zero
Thermal Conductivity
, such as
Abyssalite
.
Liquids cannot exist in amounts smaller than 10g, unless if it's in pipes. Such liquids will be immediately deleted from the world.
This behavior causes liquids in quantities smaller than 40 grams to be easily partially deleted by tile creation, especially easily seen with
Manual Airlocks
and
Mechanized Airlocks
.
Default Mass
All liquids have a default mass per tile. This is used during world generation.
Above 35% of the default mass, the tile of liquid can flood
Buildings
and drown
Critters
.
Falling droplets merge into the uppermost cell of the same liquid whose mass exceeds the liquid’s default cell mass.
Freezing a liquid into a
solid
will form either
Debris
or a solid natural tile depending on the
solid's
default mass and will
not
take into account the liquid's default mass.
Max Mass
All liquids have a certain max mass per tile; above this limit, the liquid will "flow" to the tile above it (provided it's not a solid tile), pushing away the air in the tile. If several such liquid tiles are "stacked" atop each other, pressure will build in the bottom tiles, adding approximately 1% of the above mass to each tile. This pressure can eventually cause damage to surrounding solid tiles.
Pressure Damage
Sedimentary Rock
Tiles
taking pressure damage from over 962kg of
petroleum
Liquid tiles which accumulate too much mass (such as from depth stacking) will begin to visually crack the tiles containing them before causing pressure damage to the tile and seep through as droplets, until either the tile breaks entirely (losing all materials used to construct it and allowing the liquid to flow out), the pressure is released, or the tile is reinforced with more tiles. The mass at which a tile begins to take pressure damage is dependent upon the liquid's specific mass per tile (that is, the maximum amount of liquid that can be put in a tile without it overflowing), the tile's strength, the type of tile, and the thickness of the wall. Solid tiles 3 tiles or thicker are immune to pressure damage entirely. In addition, certain tiles are immune to pressure damage under any circumstances. Some pressure-immune tiles and buildings are
Airflow Tiles
,
Manual Airlocks
,
Mechanized Airlocks
,
Bunker Doors
, and
Solar Panels
.
Pressure damage to
Natural Tiles
will cause only 50% of the mass to be dropped as
Debris
, the same ratio as if a
Duplicant
dug it.
The maximum mass
(
m
m
a
x
)
a liquid tile can have before damaging its tank walls can be calculated using the following formula:
m
m
a
x
=
S
p
e
c
i
f
i
c
M
a
s
s
P
e
r
T
i
l
e
⋅
(
1
+
t
⋅
s
⋅
k
)
where
t
is the thickness of the wall (either 1 or 2),
s
is the strength of the material (located in the
solid.yaml
file) making up the wall, and
k
is the "strength multiplier", found in the table below. It should be noted that while this formula
should
give the maximum pressure a wall can withstand before taking damage, in some cases there appears to be a rounding error of less than 1 kg. Therefore, it is best to ensure the tank will be dealing with pressures less than the maximum pressure, both to avoid potential rounding errors and as a matter of general caution.
Tile Modifiers (k values)
Tile Type
Strength Multiplier (k values)
Natural Tile
tile mass / maxMass  (maxMass as defined in solid.yaml for the material)
Tile
1.50
Insulated Tile
1.00
Plastic Tile
1.00
Metal Tile
1.00
Window Tile
1.00
Bunker Tile
10.00
Carpeted Tile
1.00
Farm Tile
1.00
Hydroponic Farm
1.00
Transit Tube Crossing
1.00
Heavi-Watt Joint Plate
1.00
Heavi-Watt Conductive Joint Plate
1.00
Gantry
0.02
Common Material Strengths
Material
Strength
Aluminum
1.0
Aluminum Ore
0.7
Ceramic
1.0
Clay
0.2
Copper
0.8
Copper Ore
0.7
Diamond
2.5
Dirt
0.2
Fossil
0.2
Glass
1.0
Gold
0.7
Gold Amalgam
0.8
Granite
1.5
Igneous Rock
1.0
Insulite
2.0
Iron
1.0
Iron Ore
0.9
Isoresin
0.4
Lead
0.8
Mafic Rock
1.0
Niobium
0.8
Obsidian
1.0
Plastic
0.4
Sandstone
0.5
Sedimentary Rock
0.2
Shale
0.25
Steel
2.0
Thermium
0.8
Tungsten
0.9
Wolframite
0.8
Maximum Pressure Examples
Liquid
Mass Per Tile
(kg/tile)
Tile Type
Tile
Strength
Material
Material
Strength
Max Pressure (kg)
1 tile
2 tiles
Molten Lead
9970
Bunker Tile
10.0
Steel
2.0
209370
408770
Liquid Carbon Dioxide
2000
Bunker Tile
10.0
Steel
2.0
42000
82000
Magma
1840
Bunker Tile
10.0
Steel
2.0
38640
75440
Brine
1200
Bunker Tile
10.0
Steel
2.0
25200
49200
Water
1000
Bunker Tile
10.0
Steel
2.0
21000
41000
Crude Oil
870
Bunker Tile
10.0
Steel
2.0
18270
35670
Petroleum
740
Bunker Tile
10.0
Steel
2.0
15540
30340
Liquid Oxygen
500
Bunker Tile
10.0
Steel
2.0
10500
20500
Visco-Gel
100
Bunker Tile
10.0
Steel
2.0
2100
4100
Molten Lead
9970
Tile
1.5
Igneous Rock
1.0
24925
39880
Liquid Carbon Dioxide
2000
Tile
1.5
Igneous Rock
1.0
5000
8000
Magma
1840
Tile
1.5
Igneous Rock
1.0
4600
7360
Brine
1200
Tile
1.5
Igneous Rock
1.0
3000
4800
Water
1000
Tile
1.5
Igneous Rock
1.0
2500
4000
Crude Oil
870
Tile
1.5
Igneous Rock
1.0
2175
3480
Petroleum
740
Tile
1.5
Igneous Rock
1.0
1850
2960
Liquid Oxygen
500
Tile
1.5
Igneous Rock
1.0
1250
2000
Visco-Gel
100
Tile
1.5
Igneous Rock
1.0
250
400
Molten Lead
9970
Tile
1.5
Sedimentary Rock
0.2
12961
15952
Liquid Carbon Dioxide
2000
Tile
1.5
Sedimentary Rock
0.2
2600
3200
Magma
1840
Tile
1.5
Sedimentary Rock
0.2
2392
2944
Brine
1200
Tile
1.5
Sedimentary Rock
0.2
1560
1920
Water
1000
Tile
1.5
Sedimentary Rock
0.2
1300
1600
Crude Oil
870
Tile
1.5
Sedimentary Rock
0.2
1131
1392
Petroleum
740
Tile
1.5
Sedimentary Rock
0.2
962
1184
Liquid Oxygen
500
Tile
1.5
Sedimentary Rock
0.2
650
800
Visco-Gel
100
Tile
1.5
Sedimentary Rock
0.2
130
160
Molten Lead
9970
Metal Tile
1.0
Steel
2.0
29910
49850
Liquid Carbon Dioxide
2000
Metal Tile
1.0
Steel
2.0
6000
10000
Magma
1840
Metal Tile
1.0
Steel
2.0
5520
9200
Brine
1200
Metal Tile
1.0
Steel
2.0
3600
6000
Water
1000
Metal Tile
1.0
Steel
2.0
3000
5000
Crude Oil
870
Metal Tile
1.0
Steel
2.0
2610
4350
Petroleum
740
Metal Tile
1.0
Steel
2.0
2220
3700
Liquid Oxygen
500
Metal Tile
1.0
Steel
2.0
1500
2500
Visco-Gel
100
Metal Tile
1.0
Steel
2.0
300
500
Molten Lead
9970
Metal Tile
1.0
Gold
0.7
16949
23928
Liquid Carbon Dioxide
2000
Metal Tile
1.0
Gold
0.7
3400
4800
Magma
1840
Metal Tile
1.0
Gold
0.7
3128
4416
Brine
1200
Metal Tile
1.0
Gold
0.7
2040
2880
Water
1000
Metal Tile
1.0
Gold
0.7
1700
2400
Crude Oil
870
Metal Tile
1.0
Gold
0.7
1479
2088
Petroleum
740
Metal Tile
1.0
Gold
0.7
1258
1776
Liquid Oxygen
500
Metal Tile
1.0
Gold
0.7
850
1200
Visco-Gel
100
Metal Tile
1.0
Gold
0.7
170
240
Molten Lead
9970
Insulated Tile
1.0
Igneous Rock
1.0
19940
29910
Liquid Carbon Dioxide
2000
Insulated Tile
1.0
Igneous Rock
1.0
4000
6000
Magma
1840
Insulated Tile
1.0
Igneous Rock
1.0
3680
5520
Brine
1200
Insulated Tile
1.0
Igneous Rock
1.0
2400
3600
Water
1000
Insulated Tile
1.0
Igneous Rock
1.0
2000
3000
Crude Oil
870
Insulated Tile
1.0
Igneous Rock
1.0
1740
2610
Petroleum
740
Insulated Tile
1.0
Igneous Rock
1.0
1480
2220
Liquid Oxygen
500
Insulated Tile
1.0
Igneous Rock
1.0
1000
1500
Visco-Gel
100
Insulated Tile
1.0
Igneous Rock
1.0
200
300
Molten Lead
9970
Insulated Tile
1.0
Sedimentary Rock
0.2
11964
13958
Liquid Carbon Dioxide
2000
Insulated Tile
1.0
Sedimentary Rock
0.2
2400
2800
Magma
1840
Insulated Tile
1.0
Sedimentary Rock
0.2
2208
2576
Brine
1200
Insulated Tile
1.0
Sedimentary Rock
0.2
1440
1680
Water
1000
Insulated Tile
1.0
Sedimentary Rock
0.2
1200
1400
Crude Oil
870
Insulated Tile
1.0
Sedimentary Rock
0.2
1044
1218
Petroleum
740
Insulated Tile
1.0
Sedimentary Rock
0.2
888
1036
Liquid Oxygen
500
Insulated Tile
1.0
Sedimentary Rock
0.2
600
700
Visco-Gel
100
Insulated Tile
1.0
Sedimentary Rock
0.2
120
140
In general, higher strength multipliers correspond to more pressure-resistant tiles. Of course, the base material strength also plays an important role in the maximum pressure a tile can withstand.
Again, note that some pressures may be off by up to 0.1kg due to rounding, so exercise caution when approaching these limits.
Visual Effects
When a liquid approaches its freezing point, the liquid becomes lightly tinted.
When a liquid approaches its boiling point, the liquid gains bubble effects.
List of Liquids
Icon
Name
Freezing point
Freezes into
Vaporization point
Vaporizes into
SHC
TC
Default Mass
Light Absorption
Radiation Absorption
Density
[g/mol]
Notes
Brine
-22.50
Brine Ice
102.75
Steam
70.00%
Salt
30.00%
3.4
0.609
1200
25%
80%
22
Liquid Chlorine
-100.98
Solid Chlorine
-34.60
Chlorine Gas
0.48
0.0081
600
100%
73%
34.453
Crude Oil
-40.15
Solid Crude Oil
399.85
Petroleum
1.69
2
870
100%
80%
500
Polluted Water
-20.65
Polluted Ice
119.35
Steam
99.00%
Dirt
1.00%
4.179
0.58
1000
70%
80%
20
emits
Polluted Oxygen
Liquid Carbon Dioxide
-56.55
Solid Carbon Dioxide
-48.15
Carbon Dioxide
0.846
1.46
600
100%
80%
44.01
Gunk
-8.15
Solid Gunk
447.85
Petroleum
92.00%
Sulfur
8.00%
1.2
1.5
870
100%
90%
550
Gunk is available in the Base Game, but only with debug/sandbox features
Liquid Hydrogen
-259.15
Solid Hydrogen
-252.15
Hydrogen Gas
2.4
0.1
600
100%
90%
1.00794
Liquid Methane
-182.60
Solid Methane
-161.50
Natural Gas
2.191
0.03
600
60%
75%
16.044
Liquid Oxygen
-218.79
Solid Oxygen
-182.96
Oxygen
1.01
2
300
100%
82%
15.9994
Liquid Phosphorus
44.15
Refined Phosphorus
280.45
Phosphorus Gas
0.7697
0.236
200
100%
75%
30.973762
Liquid Sulfur
115.20
Sulfur
337.00
Sulfur Gas
0.7
0.2
190
10%
74%
32
Brackene
-16.50
Frozen Brackene
80.00
Brine
90.00%
Brackwax
10.00%
4.1
0.609
1100
80%
80%
23
Magma
1409.85
Igneous Rock
2356.85
Rock Gas
1
1
1840
100%
80%
50
Mercury
-38.85
Solid Mercury
356.75
Mercury Gas
0.14
8.3
600
100%
25%
200.59
Mercury is available in the Base Game, but only with debug/sandbox features
Molten Aluminum
660.30
Aluminum
2470.00
Aluminum Gas
0.91
20.5
1000
100%
77%
55.845
Liquid Carbon
3551.85
Refined Carbon
4826.85
Carbon Gas
0.71
2
600
100%
84%
12.0107
Molten Copper
1083.85
Copper
2560.85
Copper Gas
0.386
12
900
100%
61%
63.546
Molten Glass
1126.85
Glass
2356.85
Rock Gas
0.2
1
200
70%
65%
50
Molten Gold
1063.85
Gold
2855.85
Gold Gas
0.1291
6
870
100%
35%
196.966569
Molten Iron
1534.85
Iron
2749.85
Iron Gas
0.449
4
1000
100%
66%
55.845
Molten Cobalt
1494.85
Cobalt
2926.85
Cobalt Gas
0.42
4
1000
100%
63%
58.9
Molten Lead
327.50
Lead
1749.00
Lead Gas
0.128
11
3000
100%
85%
196.966569
Molten Niobium
2476.85
Niobium
4743.85
Niobium Gas
0.265
54
900
100%
49%
92.9
Molten Salt
799.85
Salt
1464.85
Salt Gas
0.7
0.444
190
10%
75%
32
Molten Steel
1083.85
Steel
3826.85
Steel Gas
0.386
80
900
100%
74%
63.546
Molten Tungsten
3421.85
Tungsten
5929.85
Tungsten Gas
0.134
4
200
70%
35%
183.84
Liquid Uranium
132.85
Depleted Uranium
4131.85
Rock Gas
1.69
2
3000
100%
30%
196.966569
emits 165
rads/cycle
per 1000kg mass
Liquid Naphtha
-50.15
Solid Naphtha
538.85
Sour Gas
2.191
0.2
740
80%
60%
102.2
Liquid Nuclear Waste
26.85
Solid Nuclear Waste
526.85
Nuclear Fallout
7.44
6
500
100%
30%
196.966569
emits 165
rads/cycle
per 1000kg mass
Petroleum
-57.15
Solid Petroleum
538.85
Sour Gas
1.76
2
740
80%
80%
82.2
Resin
20.00
Solid Resin
125.00
Steam
75.00%
Refined Carbon
25.00%
1.11
0.15
920
80%
75%
52.5
Sap
20.00
Solid Sap
125.00
Steam
75.00%
Isosap
25.00%
1.11
0.15
920
80%
75%
52.5
Salt Water
-7.50
Brine
23.00%
Ice
77.00%
99.69
Steam
93.00%
Salt
7.00%
4.1
0.609
1100
25%
80%
21
Nectar
-82.50
Ice
23.00%
Sucrose
77.00%
160.00
Steam
23.00%
Sucrose
77.00%
4.1
0.609
1100
50%
90%
21
Nectar is available in the Base Game, but only with debug/sandbox features
Super Coolant
-271.15
Solid Super Coolant
436.85
Super Coolant Gas
8.44
9.46
800
90%
60%
250
Visco-Gel Fluid
-30.65
Solid Visco-Gel
479.85
Liquid Naphtha
1.55
0.45
100
10%
60%
10
Water
-0.65
Ice
99.35
Steam
4.179
0.609
1000
25%
80%
18.01528
Ethanol
-114.05
Solid Ethanol
78.35
Ethanol Gas
2.46
0.171
1000
25%
70%
46.07
Phyto Oil
-33.15
Frozen Phyto Oil
75.00
Carbon Dioxide
33.34%
Algae
66.66%
0.9
2
800
30%
90%
450
Phyto Oil is available in the Base Game, but only with debug/sandbox features
Biodiesel
-10.00
Tallow
180.00
Carbon Dioxide
2.19
2
800
30%
70%
450
Liquid Sucrose
185.85
Sucrose
230.00
Carbon Dioxide
1.255
0.15
190
10%
70%
32
Molten Nickel
1454.85
Nickel
2729.85
Nickel Gas
0.44
30
900
100%
70%
58.69
Molten Iridium
2445.85
Iridium
4129.85
Iridium Gas
0.131
170
900
100%
88%
183.84
Trivia
Although Oxygen Not Included simulates liquid density using molecular mass, in real life, the density of liquids has little relationship to its molecular mass. This is because liquids have very different
molar volumes
(the amount of volume a given number of molecules take up). For example, although crude oil has the highest molecular mass in-game, its real-life density is often less than that of water and would float on water because crude oil molecules are extremely large compared to water molecules.
