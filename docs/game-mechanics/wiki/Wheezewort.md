# Wheezewort

Wheezewort
Wheezeworts can be planted in Planter Boxes, Farm Tiles or Hydroponic Farms, and absorb Heat by respiring through their porous outer membranes.
Location
Regolith Biome
Feather Biome
Radioactive Biome
Tundra Biome
Seed
Wort Seed
Base Seed Chance
0%
Effect
Absorbs 1 kg of
Gas
per second (250g if wild) at the bottom tile then expels it 5 °C colder from the top tile
Spawn Temperature
19.85 °C
/
67.73 °F
Temperature
-60 °C ↔ 95 °C
/
-76 °F ↔ 203 °F
Lethal Temperature Threshold
Below
-90 °C
/
-130 °F
or above
190 °C
/
374 °F
Atmospheric Pressure
Ignored
Radiation Range
Ignored
Fertilizer
Phosphorite
4 kg/cycle
Plant Height
2 Tiles
Decor
15 (Radius: 2 Tiles)
Can be Planted in:
Code
Internal Id
ColdBreather
String Id
STRINGS.CREATURES.SPECIES.COLDBREATHER.NAME
Wheezewort
is a non-reproducing
plant
that is found in the
Frozen Biome
. Wheezeworts are extremely useful for temperature management since they destroy heat from the surrounding environment.
Seed
There is a limited amount of Wheezeworts on the map and they do not produce seeds naturally but
Printing Pod
can sometimes offer them as
Care Packages
. It can be found as buried object in
Frozen Biome
.
Growth
A Wheezewort requires temperature range of
-60 °C ↔ 95 °C
/
-76 °F ↔ 203 °F
. It has no requirement on air pressure.
Wild
A wild Wheezewort cools 250 g gas per second.
Domestic
Wheezeworts can be uprooted and replanted into a
Planter Box
,
Farm Tile
or
Hydroponic Farm
. It needs to be fertilized with 4 kg/cycle of
Phosphorite
.
A domestic Wheezewort cools 1 kg gas per second.
Technical info
Wheezeworts cool the gas around them by absorbing the gas (1000 g/s Domestic, 250 g/s wild) in the bottom block of the Wheezewort, and releasing it
-5 °C
/
-9 °F
colder. However, the Wheezewort itself weighs 400 kg, and exchanges heat relatively well with the gas, so its own temperature can significantly slow down the overall cooling effect.
Wheezewort's cooling factor is expressed in absolute temperature rather than energy. Neither heat conductivity, heat capacity, nor volume of the gas being cooled affect the change in temperature. That means that the best way to utilize it is to cool high density of high heat capacity gas - for which
Hydrogen
is best, resulting in an effective cooling of 12 kDTU/s:
(
2
.
4
D
T
U
g
K
)
×
(
5
K
)
×
(
1
0
0
0
g
s
)
=
1
2
0
0
0
D
T
U
s
Wheezewort will never cool any gas below 5°C above its condensation point, meaning it will never condense the gas being cooled, but it can cool the gas so much that the plant stifles itself.
Atmosphere Comparison
Cooling Rates for Domestic/Wild Wheezewort in Various Atmospheres
Atmosphere
SHC
Domestic Wheezewort DTU/s
Wild Wheezewort DTU/s
Minimum Operating Temperature
Notes
Hydrogen
2.400
12000
3000
-60 °C
/
-76 °F
Most cooling and highest TC of all gases in Wheezewort temperature range.
Polluted Oxygen
1.010
5050
1262.5
-60 °C
/
-76 °F
Highest-cooling
Sleet Wheat
atmosphere, but accelerates food spoilage.
Oxygen
1.005
5025
1256.25
-60 °C
/
-76 °F
Slightly less cooling than
Polluted Oxygen
but does not spoil
Sleet Wheat Grain
.
Natural Gas
2.191
10995
2748.75
-60 °C
/
-76 °F
Second-highest cooling. High SHC and low TC make it usable for insulation.
Sour Gas
1.898
9490
2372.5
-60 °C
/
-76 °F
Comparable to Natural Gas as an insulator, trades SHC and cooling for slightly lower TC.
Carbon Dioxide
0.846
4230
1057.5
-43.15 °C
/
-45.67 °F
Nosh Sprout
atmosphere.
Chlorine
0.480
2400
600
-29.6 °C
/
-21.28 °F
Least cooling of all base game gases. Lowest TC of gases in Wheezewort temperature range.
Nuclear Fallout
0.265
1325
331.25
71.9 °C
/
161.42 °F
Least cooling of all gases in Wheezewort temperature range. Produces additional
Radiation
.
Gas Ethanol
2.148
10740
2685
83.4 °C
/
182.12 °F
Third-highest cooling but highest condensation point. Useful if high minimum temperatures are desired.
Wheezewort Airpump
Due to Wheezewort gas interaction, namely, creation of
vacuum
in its bottom tile, Wheezeworts can be used as unpowered airpumps in one tile wide tunnels. However, such pump will reset itself if the tile that feeds into bottom tile of Wheezeworts gets above 2 kg of pressure or becomes vacuum too.
[
1
]
Rad Emission
In the
Spaced Out!
DLC, Wheezeworts emit
radiation
; origin from the lower half of the plant, at 480 rads per cycle in a vacuum. The radiation emission means that they are no longer entirely safe to use for cooling living spaces.
The radiation given off by Wheezeworts can be captured with
Radbolt Generators
. In general, the best practice is to have the Wheezewort as close to the generator as possible. However, since
Radiation
gets
absorbed
by the
Farm Tile
(if the Radbolt Generator is underneath the Wheezewort), one should choose a Farm Tile with as little
Radiation Absorption
as possible.
There is one more practical application for the radiation, and that is in disinfecting food storage, since it keeps the food mostly frozen and kills any
Food Poisoning
germs that may exist on the food.
Radiation Absorption Rate
for
Farm Tiles
Farm Tile
Radiation Absorption
Dirt Farm Tile
75%
Clay Farm Tile
65%
Copper Ore Hydroponic Farm
56%
Aluminum Ore Hydroponic Farm
72%
Cobalt Ore Hydroponic Farm
58%
Gold Amalgam Hydroponic Farm
30%
Iron Ore Hydroponic Farm
61%
Wolframite Hydroponic Farm
65%
Steel Hydroponic Farm
74%
Uranium Ore Hydroponic Farm
30%
Niobium Hydroponic Farm
49%
ThermiumHydroponic Farm
60%
Wheezewort radiation in a vacuum and oxygen...
...with the rads per cycle marked...
...and on a
Gold Amalgam
Hydroponic Farm
Note:
these values are not applicable to every wheezewort, and can change up to 2 rads/cycle. It is currently not known what causes this difference, but it likely has to do with their position on the map since  a Wheezewort planted in the same place will yield the same result.
Database
Wheezewort
Plant?
The Wheezewort is best known for its ability to alter the temperature of its surrounding environment, directly absorbing heat energy to maintain its bodily processes.
This environmental management also serves to enact a type of self-induced hibernation, slowing the Wheezewort's metabolism to require less nutrients over long periods of time.
Deceptive in appearance, this member of the Cnidaria phylum is in fact an animal, not a plant.
Wheezewort cells contain no chloroplasts, vacuoles or cell walls, and are incapable of photosynthesis.
Instead, the Wheezewort respires in a recently developed method similar to amphibians, using its membranous skin for cutaneous respiration.
Trivia
According to the in-game database, the Wheezewort is an animal of the Cnidaria
[
2
]
phylum - a group of creatures mainly composed of aquatic invertebrates such as jellyfish, hydroids, sea anemones, corals and some marine parasites.
Does not work in vacuum because vacuum has no heat conduct, or heat cap.
History
AU-217187
: Can now be planted in
Flower Pots
.
LU-347957
: Can't be planted in
Flower Pot
or
Wall Pot
.
LU-356355
: No longer a decor plant and requires fertiliser.
U38-486708
: Increased all radiation emission sources intensity
References
↑
Why Wheezewort pump works / when not works
on r/Oxygennotincluded
↑
https://en.wikipedia.org/wiki/Cnidaria
