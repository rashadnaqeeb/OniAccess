# World Trait

World Trait
is a new mechanism of
random
map
generation
introduced in
Launch Update
. Some traits add more terrain features to the map, and some traits replace the preset
Biomes
with other Biomes.
The Terra asteroid and its counterpart in the
Terra Cluster are guaranteed to have no world traits.
This also applies to the medium spaced out start Terrania and the
Skewed Asteroid
.
Since
LU-357226
, some world traits are mutually exclusive with others and cannot generate at the same time. This is handled by disabling the second trait once the first one is selected. Because most traits only block one other trait and these exclusions aren’t always two-way, combinations that seem impossible may still happen, depending on the order in which traits are generated. For instance,
Large Boulders excludes
Mixed Boulders, but the reverse isn’t true, so if a world picks
Mixed Boulders first, it can still end up with both traits if it then picks
Large Boulders as well.
World traits are chosen deterministically based on the Worldgen seed.
If you set the worldgen seed to "0", all asteroids in the
cluster will generate without any traits (this also applies the asteroid in the base game).
Trait List
Trait
Effect
Mutually exclusive with
Exclusive to the Starting Asteroid
Large Boulders
Randomly places 5 large
Obsidian
rocks.
Mixed Boulders
✓
Medium Boulders
Randomly places 10 medium
Obsidian
rocks.
Large Boulders
-
Small Boulders
Randomly places 20 small
Obsidian
rocks.
Medium Boulders
-
Mixed Boulders
Randomly places 2 large, 5 medium, 10 small
Obsidian
rocks.
Small Boulders
-
Frozen Core
The
Volcanic Biome
at the bottom is replaced with the Solid variant of
Frozen Biome
.
Lush Core
-
Geoactive
4 additional
Geysers
are generated. 12 -> 16.
cannot appear in some 'Spaced Out!' asteroid styles
Geodormant
✓
Geodormant
3 fewer
Geysers
are generated. 12 -> 9.
cannot appear in 'Spaced Out!' asteroid styles
Geoactive
✓
Geodes
10 random Geode deposits are added to the map. Each Geode contains a core made of
Ice
,
Polluted Ice
,
Iron Ore
,
Copper Ore
,
Gold Amalgam
,
Diamond
,
Phosphorite
,
Solid Methane
,
Solid Oxygen
,
Solid Chlorine
,
Solid Carbon Dioxide
,
Solid Hydrogen
, or
Coal
. The core is surrounded by
Diamond
,
Obsidian
and
Abyssalite
. Geodes are placed early in the worldgen, so it is not uncommon that a few of them get overwritten by other objects, leaving the world with less than 10 geodes.
None
-
Large Glaciers
5 large glaciers are randomly placed on the map. The large glacier is composed of
Ice
,
Polluted Ice
,
Wolframite
and surrounded by more Ice and Polluted Ice. It contains 5-8
Wheezewort
seeds.
None
✓
Irregular Oil
Some patch variants of
Oil Biome
are added to random areas of the map. Parts of the Oil Biome at the bottom are replaced with dry variants.
None
-
Magma Channels
The Magma Vent variant of
Volcanic Biome
is placed in the lower areas (distance 1-2 from the bottom).
If both Frozen Core and Magma Channels appears, it generates a
Frozen Biome
at the bottom and
Volcanic Biome
above it.
None
-
Metal Poor
Less
Metal Ores
. Both mass and band size reduced to 80%. It affects
Copper Ore
,
Iron Ore
,
Wolframite
,
Aluminum Ore
,
Gold Amalgam
and
Cobalt Ore
.
Metal Rich
-
Metal Rich
More
Metal Ores
. Mass is doubled and band size increases to 150%.
Metal Poor
-
Alternate Pod Location
The Starting Location is shifted both horizontally and vertically.
None
✓
Slime Molds
30
Slime
and
Algae
balls randomly added to the map.
None
-
Subsurface Ocean
Surface level Biomes under
Space Biome
are replaced with Deep variant of
Ocean Biome
.
None
-
Trapped Oil
Most of the oil is trapped within
Oil Reservoirs
None
-
Volcanic Activity
Volcano
variants of
Magma Biome
are added in the middle area of the map
None
-
Only
Trait
Effect
Mutually exclusive with
Exclusive to the Starting Asteroid
Crashed Satellites
Crashed Satellites
can be found on this planetoid
None
-
Frozen Friend
Cryotank 3000
can be found on this planetoid
None
-
Lush Core
The core is a
Forest Biome
Frozen Core
-
Metallic Caves
12 metal caves are added to the map. Each metal cave is an open area filled with
Carbon Dioxide
, the cave walls are composed of
Cobalt Ore
,
Iron Ore
and
Granite
None
-
Radioactive Crust
Uranium Ore
is scattered over the surface crust of the planetoid.
None
-
History
U52-622222
: Removed Neutronium geodes from the Geodes world trait. Folia and Quagmiris asteroids now support the Geoactive world trait.
See Also
Story Trait
Guide/Worldgen Seeds
