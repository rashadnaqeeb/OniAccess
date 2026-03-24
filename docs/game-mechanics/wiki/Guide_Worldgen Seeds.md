# Guide/Worldgen Seeds

When generating a new world in Oxygen Not Included, you can enter a seed which will determine what generates in the world.
The seed is composed of multiple sub sections, each defining a different part of the worldgen.
As an example, a seed can look like this:
SNDST-C-194068248-GSGW3-3A-0
The sub sections of this seed are as follows:
Cluster Prefix:
"SNDST-C"
This part of the seed defines what Cluster is selected. In the base game, this defines the Asteroid.
SNDST-C is the cluster prefix of the spaced out terrania cluster.
Worldgen Seed:
"194068248"
This part of the seed defines the worldgen seed. It determines the world traits and the way each asteroid generates. It can also be edited directly in the game settings of the destination selector.
World Settings:
"GSGW3"
This part of the seed describes the selected world settings. it is preselected by the difficulty (Default/No Sweat) and is adjusted when you change any game settings in the destination selector (other than worldgen seed)
Story Trait Settings:
"3A"
This part of the seed describes what story traits are enabled. By default, all story traits are currently selected. Story traits are placed after the worlgen has run, they dont alter the world layout during the generation (allthough they still replace small areas of the map).
Mixing Settings:
"0"
This part of the seed describes the current dlc scrambling settings. These are a new way of adding new content into existing clusters by partially adding new biomes or replacing asteroids with new dlc content. This setting was first introduced with the Frosty Planet Pack Dlc, which featured a new mixing asteroid (Ceres fragment) and 3 mixing biomes
The world layout, world geysers and asteroid traits are solely dependent on the Worldgen Seed section of the total seed.
If you paste in a seed and alter game- or story trait settings will change those parts of the game seed, but the worlgen will stay the same (minus potentially replacing a few materials by adding a story trait).
Adding certain mods however can alter the worldgen:
Having a mod enabled that adds a new world trait will shuffle the trait list, leading to a different asteroid trait composition with the mod enabled than vanilla.
Having the mod wgsm enabled will also alter the worldgen, as that mod breaks the way the game handles reading the seed.
Note that when a
new major update
is released, it can potentially change the result of generating worlds with these seeds if new
materials
,
vents
, or other major world objects are added or removed.
